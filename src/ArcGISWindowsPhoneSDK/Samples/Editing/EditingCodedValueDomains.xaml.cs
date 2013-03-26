using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISWindowsPhoneSDK
{
    public partial class EditingCodedValueDomains : PhoneApplicationPage
    {
        Editor editor;
        FeatureLayer featureLayer;
        CodedValueDomain facilityFieldDomain;
        CodedValueDomain qualityFieldDomain;

        public EditingCodedValueDomains()
        {
            InitializeComponent();

            editor = LayoutRoot.Resources["MyEditor"] as Editor;
            featureLayer = MyMap.Layers["RecreationFacilities"] as FeatureLayer;
        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            FeatureLayer fl = sender as FeatureLayer;

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

                                FeatureTypeListBox.Items.Add(new CVDTemplateItem(name, symbol, Convert.ToInt32(featureTypePairs.Value.Id)));
                            }
                        }
                    }
                }
            }
            #endregion

            #region get coded value codes and descriptions
            var facilityField = fl.LayerInfo.Fields.Where(f => f.Name == "facility").First();
            facilityFieldDomain = facilityField.Domain as CodedValueDomain;
            FacilityChoicesListBox.ItemsSource = facilityFieldDomain.CodedValues;

            var qualityField = fl.LayerInfo.Fields.Where(f => f.Name == "quality").First();
            qualityFieldDomain = qualityField.Domain as CodedValueDomain;
            QualityChoicesListBox.ItemsSource = qualityFieldDomain.CodedValues;
            #endregion

            //enable the app bar and context menu items
            for (int i = 0; i < ApplicationBar.Buttons.Count; ++i)
                (ApplicationBar.Buttons[i] as IApplicationBarIconButton).IsEnabled = true;
        }

        private void FeatureLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            // show the attribute page for the first of the graphics returned
            FeatureInfoPage.IsOpen = true;
            ApplicationBar.IsVisible = false;

            FeatureInfoPage.DataContext = e.Graphic;
            // select the type and confirm values in the ListBoxes to match the graphic's attributes
            var facilityMatches = facilityFieldDomain.CodedValues.Where(a => a.Key.Equals(e.Graphic.Attributes["facility"]));
            if (facilityMatches.Any())
                FacilityChoicesListBox.SelectedItem = facilityMatches.First();

            var qualityMatches = qualityFieldDomain.CodedValues.Where(a => a.Key.Equals(e.Graphic.Attributes["quality"]));
            if (qualityMatches.Any())
                QualityChoicesListBox.SelectedItem = qualityMatches.First();
        }
        

        private void AddItemButton_Click(object sender, System.EventArgs e)
        {
            FeatureTypeChoicesPage.IsOpen = true;
        }
        private void FeatureTypeListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = FeatureTypeListBox.SelectedIndex;
            if (index > -1)
            {
                CVDTemplateItem selectedTemplate = FeatureTypeListBox.SelectedItem as CVDTemplateItem;
                if (editor.Add.CanExecute(selectedTemplate.ID))
                    editor.Add.Execute(selectedTemplate.ID);
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

        private void FacilityButton_Click(object sender, RoutedEventArgs e)
        {
            FacilityChoicesPage.IsOpen = true;
        }
        private void FacilityChoicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (FeatureInfoPage.DataContext as Graphic).Attributes["facility"] = ((KeyValuePair<object, string>)e.AddedItems[0]).Key;
            FacilityChoicesPage.IsOpen = false;
        }

        private void QualityButton_Click(object sender, RoutedEventArgs e)
        {
            QualityChoicesPage.IsOpen = true;
        }
        private void QualityChoicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (FeatureInfoPage.DataContext as Graphic).Attributes["quality"] = ((KeyValuePair<object, string>)e.AddedItems[0]).Key;
            QualityChoicesPage.IsOpen = false;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (FacilityChoicesPage.IsOpen)
            {
                FacilityChoicesPage.IsOpen = false;
                e.Cancel = true;
            }
            if (QualityChoicesPage.IsOpen)
            {
                QualityChoicesPage.IsOpen = false;
                e.Cancel = true;
            }
            if (FeatureInfoPage.IsOpen)
            {
                FeatureInfoPage.IsOpen = false;
                ApplicationBar.IsVisible = true;

                e.Cancel = true;
            }
        }
    }

    public class CVDTemplateItem
    {
        public string Name { get; set; }
        public Symbol Symbol { get; set; }
        public int ID { get; set; }

        public CVDTemplateItem(string name, Symbol symbol, int id)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.ID = id;
        }
    }
}