using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using Microsoft.Phone.Shell;

namespace ArcGISWindowsPhoneSDK
{
    public partial class OwnershipBasedEditing : PhoneApplicationPage
    {
        Dictionary<string, int> challengeAttemptsPerUrl = new Dictionary<string, int>();

        public OwnershipBasedEditing()
        {
            InitializeComponent();
            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
            IdentityManager.Current.ChallengeMethod = Challenge;
            MyEditor.MoveEnabled = true;
        }

        private void Challenge(string url,
            Action<IdentityManager.Credential, Exception> callback, IdentityManager.GenerateTokenOptions options)
        {
            LoginGrid.Visibility = System.Windows.Visibility.Visible;

            TitleTextBlock.Text = string.Format("Login to access: \n{0}", url);

            if (!challengeAttemptsPerUrl.ContainsKey(url))
                challengeAttemptsPerUrl.Add(url, 0);

            RoutedEventHandler handleClick = null;
            handleClick = (s, e) =>
            {
                IdentityManager.Current.GenerateCredentialAsync(url, UserTextBox.Text, PasswordTextBox.Text,
                (credential, ex) =>
                {
                    challengeAttemptsPerUrl[url]++;
                    if (ex == null || challengeAttemptsPerUrl[url] == 3)
                    {
                        LoginLoadLayerButton.Click -= handleClick;
                        callback(credential, ex);
                        LoggedInUserButton.Content = credential.UserName;
                    }

                }, options);
            };
            LoginLoadLayerButton.Click += handleClick;

            System.Windows.Input.KeyEventHandler handleEnterKeyDown = null;
            handleEnterKeyDown = (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    PasswordTextBox.KeyDown -= handleEnterKeyDown;
                    handleClick(null, null);
                }
            };
            PasswordTextBox.KeyDown += handleEnterKeyDown;
        }


        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            LoginGrid.Visibility = System.Windows.Visibility.Collapsed;

            FeatureLayer l = MyMap.Layers["SaveTheBayMarineLayer"] as FeatureLayer;
            if (l == null) return;

            #region Build TemplatePicker
            // Use LayerInfo.FeatureTypes and FeatureTemplates with Editor.Add and SymbolDisplay to build TemplatePicker
            if (l.LayerInfo.FeatureTypes != null && l.LayerInfo.FeatureTypes.Count > 0)
            {
                foreach (var featureType in l.LayerInfo.FeatureTypes)
                {
                    if (featureType.Value.Templates != null && featureType.Value.Templates.Count > 0)
                    {
                        foreach (var featureTemplate in featureType.Value.Templates)
                        {
                            var sp = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal };
                            sp.Children.Add(new Button()
                            {
                                Content = new SymbolDisplay()
                                {
                                    Height = 25,
                                    Width = 25,
                                    Symbol = featureTemplate.Value.GetSymbol(l.Renderer)
                                },
                                DataContext = MyEditor,
                                CommandParameter = featureType.Value.Id,
                                Command = MyEditor.Add
                            });
                            sp.Children.Add(new TextBlock() { Text = featureTemplate.Value.Name, VerticalAlignment = System.Windows.VerticalAlignment.Center });
                            TemplatePicker.Children.Add(sp);
                        }
                    }
                }
            }
            #endregion
        }

        private void Layer_InitializationFailed(object sender, EventArgs e) { }


        private void FeatureLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            // enable the delete icon if user is the owner, otherwise disable it
            if (e.Graphic != null && (sender as FeatureLayer).IsUpdateAllowed(e.Graphic))
                    (ApplicationBar.Buttons[1] as IApplicationBarIconButton).IsEnabled = true;
            else
                (ApplicationBar.Buttons[1] as IApplicationBarIconButton).IsEnabled = false;

            (sender as FeatureLayer).ClearSelection();
            e.Graphic.Select();
            (ApplicationBar.Buttons[2] as IApplicationBarIconButton).IsEnabled = true;
        }

        private void UsernameButton_Click(object sender, RoutedEventArgs e)
        {
            FeatureLayer featureLayer = MyMap.Layers["SaveTheBayMarineLayer"] as FeatureLayer;
            if (featureLayer == null)
                return;

            TemplatePickerGrid.Visibility = Visibility.Collapsed;

            ESRI.ArcGIS.Client.IdentityManager.Credential c = IdentityManager.Current.FindCredential(featureLayer.Url, featureLayer.EditUserName);
            if (c == null)
                return;

            IdentityManager.Current.RemoveCredential(c);

            var cloneLayer = new FeatureLayer()
                {
                    Url = featureLayer.Url,
                    Mode = featureLayer.Mode,
                    ID = featureLayer.ID,
                    DisplayName = featureLayer.DisplayName,
                    OutFields = featureLayer.OutFields,
                    
                };

            cloneLayer.MouseLeftButtonDown += FeatureLayer_MouseLeftButtonDown;
            featureLayer.MouseLeftButtonDown -= FeatureLayer_MouseLeftButtonDown;
            MyMap.Layers.Remove(featureLayer);               
            MyMap.Layers.Add(cloneLayer);
        }


        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
           if (e.Gesture == GestureType.Hold)
           {
               FeatureLayer featureLayer = MyMap.Layers["SaveTheBayMarineLayer"] as FeatureLayer;
                IEnumerable<Graphic> selected = e.DirectlyOver(10, new GraphicsLayer[] { featureLayer });
                foreach (Graphic g in selected)
                {
                    InfoWindow.Anchor = e.MapPoint;
                    InfoWindow.IsOpen = true;
                    InfoWindow.Content = g;
                    return;
                }
            }
            else 
                InfoWindow.IsOpen = false;
            
        }

        private void AddIconButton_Click(object sender, EventArgs e)
        {
            ShowElement(TemplatePickerGrid);
        }

        private void DeleteIconButton_Click(object sender, EventArgs e)
        {
            if (MyEditor.DeleteSelected.CanExecute(null))
            {
                MyEditor.DeleteSelected.Execute(null);
                (ApplicationBar.Buttons[2] as IApplicationBarIconButton).IsEnabled = true;
            }
        }

        private void CancelIconButton_Click(object sender, EventArgs e)
        {
            if (MyEditor.ClearSelection.CanExecute(null))
            {
                MyEditor.ClearSelection.Execute(null);
                (ApplicationBar.Buttons[1] as IApplicationBarIconButton).IsEnabled = false;
            }
        }

        private void ShowElement(UIElement element)
        {
            if (TemplatePickerGrid == element)
              if (TemplatePickerGrid.Visibility == Visibility.Visible)
                TemplatePickerGrid.Visibility =  Visibility.Collapsed;
              else
                  TemplatePickerGrid.Visibility = Visibility.Visible;
        }

    }
}
