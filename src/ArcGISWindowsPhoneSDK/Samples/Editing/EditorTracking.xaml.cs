using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.FeatureService;

namespace ArcGISWindowsPhoneSDK
{
    public partial class EditorTracking : PhoneApplicationPage
    {
        Editor editor;
        FeatureLayer featureLayer;
        bool isAdd = false;

        Dictionary<string, int> challengeAttemptsPerUrl = new Dictionary<string, int>();

        public EditorTracking()
        {
            InitializeComponent();

            editor = LayoutRoot.Resources["MyEditor"] as Editor;
            featureLayer = MyMap.Layers["WildfireLayer"] as FeatureLayer;

            // Activate identity manager
            IdentityManager.Current.ChallengeMethod = Challenge;
        }

        private void Challenge(string url, Action<IdentityManager.Credential, Exception> callback, 
                               IdentityManager.GenerateTokenOptions options)
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

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            FeatureLayer fl = sender as FeatureLayer;
            IdentityManager.Credential credential = IdentityManager.Current.FindCredential(fl.Url);
            
            if (credential != null || fl.InitializationFailure != null)
            {
                LoginGrid.Visibility = System.Windows.Visibility.Collapsed;
                ShadowGrid.Visibility = System.Windows.Visibility.Collapsed;
                LoggedInGrid.Visibility = System.Windows.Visibility.Visible;

                #region populate the FeatureTypeListBox with the possible templates
                FeatureTypeListBox.Items.Clear();
                IDictionary<object, FeatureType> featureTypes = fl.LayerInfo.FeatureTypes;
                if (fl.Renderer != null)
                {
                    Symbol defaultSymbol = fl.Renderer.GetSymbol(null);
                    if (featureTypes != null && featureTypes.Count > 0)
                    {
                        foreach (KeyValuePair<object, FeatureType> featureTypePairs in featureTypes)
                        {
                            if (featureTypePairs.Value != null && featureTypePairs.Value.Templates != null && featureTypePairs.Value.Templates.Count > 0)
                            {
                                foreach (KeyValuePair<string, FeatureTemplate> featureTemplate in featureTypePairs.Value.Templates)
                                {
                                    string name = featureTypePairs.Value.Name;
                                    if (featureTypePairs.Value.Templates.Count > 1)
                                        name = string.Format("{0}-{1}", featureTypePairs.Value.Name, featureTemplate.Value.Name);
                                    Symbol symbol = featureTemplate.Value.GetSymbol(fl.Renderer) ?? defaultSymbol;

                                    FeatureTypeListBox.Items.Add(new TemplateItem(name, symbol, Convert.ToInt32(featureTypePairs.Value.Id)));
                                }
                            }
                        }
                    }
                }
                #endregion

                if (credential != null)
                {
                    LoggedInUserTextBlock.Text = credential.UserName;
                    fl.EditUserName = credential.UserName;
                }

                //enable the app bar and context menu items
                for (int i = 0; i < ApplicationBar.Buttons.Count; ++i)
                    (ApplicationBar.Buttons[i] as IApplicationBarIconButton).IsEnabled = true;
            }
        }

        private void FeatureLayer_InitializationFailed(object sender, EventArgs e) { }

        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (isAdd)
                return;

            // open the feature dialog on tap of a feature
            if (e.Gesture == GestureType.Tap)
            {
                IEnumerable<Graphic> graphics = e.DirectlyOver(10, new GraphicsLayer[] { featureLayer } );
                foreach (Graphic graphic in graphics)
                {
                    // if editable, make the editable fields enabled. Otherwise disable them
                    if (featureLayer.IsUpdateAllowed(graphic))
                    {
                        RotationTB.IsEnabled = true;
                        DescriptionTB.IsEnabled = true;
                        DateTB.IsEnabled = true;
                        TypeButton.IsEnabled = true;
                    }
                    else
                    {
                        RotationTB.IsEnabled = false;
                        DescriptionTB.IsEnabled = false;
                        DateTB.IsEnabled = false;
                        TypeButton.IsEnabled = false;
                    }

                    // show the attribute page for the first of the graphics returned
                    FeatureInfoPage.IsOpen = true;
                    ApplicationBar.IsVisible = false;

                    FeatureInfoPage.DataContext = graphic;

                    // select the type value in the ListBoxes to match the graphic's attributes
                    var typeMatches = FeatureTypeListBox.Items.Where(a => (a as TemplateItem).ID.Equals(graphic.Attributes["eventtype"]));
                    if (typeMatches.Any())
                        FeatureTypeListBox.SelectedItem = typeMatches.First();

                    return;
                }
            }

            // move a held feature
            if (e.Gesture == GestureType.Hold)
            {
                IEnumerable<Graphic> graphics = e.DirectlyOver(10, new GraphicsLayer[] { featureLayer } );
                foreach (Graphic graphic in graphics)
                {
                    if (graphic != null && !graphic.Selected && featureLayer.IsUpdateAllowed(graphic))
                    {
                        Editor editor = LayoutRoot.Resources["MyEditor"] as Editor;
                        if (featureLayer.IsUpdateAllowed(graphic))
                        {
                            if (editor.EditVertices.CanExecute(null))
                                editor.EditVertices.Execute(null);
                        }
                        else
                            if (editor.CancelActive.CanExecute(null))
                                editor.CancelActive.Execute(null);
                    }
                    featureLayer.ClearSelection();
                    graphic.Select();
                }
            }

        }

        private void AddItemButton_Click(object sender, System.EventArgs e)
        {
            isAdd = true;
            FeatureTypeChoicesPage.IsOpen = true;
        }
        private void TypeButton_Click(object sender, RoutedEventArgs e)
        {
            isAdd = false;
            FeatureTypeChoicesPage.IsOpen = true;
        }
        private void FeatureTypeListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (isAdd)
            {
                int index = FeatureTypeListBox.SelectedIndex;
                if (index > -1)
                {
                    TemplateItem selectedTemplate = FeatureTypeListBox.SelectedItem as TemplateItem;
                    if (editor.Add.CanExecute(selectedTemplate.ID))
                        editor.Add.Execute(selectedTemplate.ID);
                    FeatureTypeChoicesPage.IsOpen = false;
                }
            }
            else
            {
                // set type
                (FeatureInfoPage.DataContext as Graphic).Attributes["eventtype"] = ((TemplateItem)e.AddedItems[0]).ID;
                FeatureTypeChoicesPage.IsOpen = false;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            FeatureInfoPage.IsOpen = false;
            ApplicationBar.IsVisible = true;
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Graphic graphic = (sender as Button).DataContext as Graphic;
            featureLayer.Graphics.Remove(graphic);

            FeatureInfoPage.IsOpen = false;
            ApplicationBar.IsVisible = true;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (FeatureInfoPage.IsOpen)
            {
                FeatureInfoPage.IsOpen = false;
                ApplicationBar.IsVisible = true;

                e.Cancel = true;
            }
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            var featureLayer = MyMap.Layers["WildfireLayer"] as FeatureLayer;
            var credential = IdentityManager.Current.FindCredential(featureLayer.Url, LoggedInUserTextBlock.Text);
            if (credential == null) return;

            //disable the app bar and context menu items
            for (int i = 0; i < ApplicationBar.Buttons.Count; ++i)
                (ApplicationBar.Buttons[i] as IApplicationBarIconButton).IsEnabled = false;

            LoggedInGrid.Visibility = System.Windows.Visibility.Collapsed;
            IdentityManager.Current.RemoveCredential(credential);
            MyMap.Layers.Remove(featureLayer);
            featureLayer = new FeatureLayer()
            {
                ID = "WildfireLayer",
                DisplayName = "Wildfire Layer",
                Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire_secure/FeatureServer/0",
                Mode = FeatureLayer.QueryMode.OnDemand
            };
            featureLayer.OutFields.Add("*");
            featureLayer.Initialized += FeatureLayer_Initialized;
            featureLayer.InitializationFailed += FeatureLayer_InitializationFailed;
            MyMap.Layers.Add(featureLayer);
        }

        private void Editor_EditCompleted(object sender, Editor.EditEventArgs e)
        {
            isAdd = false;
        }
    }

    public class TemplateItem
    {
        public string Name { get; set; }
        public Symbol Symbol { get; set; }
        public int ID { get; set; }

        public TemplateItem(string name, Symbol symbol, int id)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.ID = id;
        }
    }
}