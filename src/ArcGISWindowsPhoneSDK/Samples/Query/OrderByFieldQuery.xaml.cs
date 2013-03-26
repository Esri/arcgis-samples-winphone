using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISWindowsPhoneSDK
{
    public partial class OrderByFieldQuery : PhoneApplicationPage
    {
        GraphicsLayer parcelsGraphicsLayer; 

        public OrderByFieldQuery()
        {
            InitializeComponent();

            parcelsGraphicsLayer = MyMap.Layers["MontgomeryParcels"] as GraphicsLayer;
        }

        private void MyMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (MyMap.SpatialReference != null)
            {
                MyMap.PropertyChanged -= MyMap_PropertyChanged;

                RunQuery();
            }
        }
        
        private void GraphicsLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            (sender as GraphicsLayer).ClearSelection();
            e.Graphic.Select();

            // select the corresponding item in the ListBox
            FeatureListBox.SelectedItem = e.Graphic;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RunQuery();
        }

        private void RunQuery()
        {
            parcelsGraphicsLayer.Graphics.Clear();

            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
            {
                ReturnGeometry = true,
                OutSpatialReference = MyMap.SpatialReference,
                Where = string.Format("OWNER_NAME LIKE '%{0}%'", SearchTextBox.Text),
                OrderByFields = new List<OrderByField>() { new OrderByField("OWNER_NAME", SortOrder.Ascending) }
            };

            query.OutFields.Add("OWNER_NAME,PARCEL_ID,ZONING,DEED_DATE");

            QueryTask queryTask = new QueryTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/MontgomeryQuarters/MapServer/1");
            queryTask.ExecuteCompleted += (s, a) =>
            {
                foreach (Graphic g in a.FeatureSet.Features)
                {
                    parcelsGraphicsLayer.Graphics.Add(g);
                    FeatureListBox.Items.Add(g);
                }

                (ApplicationBar.Buttons[0] as IApplicationBarIconButton).IsEnabled = true;
                FeatureChoicesPage.Visibility = Visibility.Visible;
            };
            FeatureListBox.Items.Clear();
            queryTask.ExecuteAsync(query);
        }

        private void ShowFeaturesButton_Click(object sender, System.EventArgs e)
        {
            FeatureChoicesPage.Visibility = FeatureChoicesPage.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
        private void FeatureListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            (MyMap.Layers["MontgomeryParcels"] as GraphicsLayer).ClearSelection();

            int index = FeatureListBox.SelectedIndex;
            if (index > -1)
            {
                (FeatureListBox.SelectedItem as Graphic).Selected = true;
            }
        }
    }
}