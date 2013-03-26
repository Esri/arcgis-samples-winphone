using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class AttributeQuery : PhoneApplicationPage
    {
        public AttributeQuery()
        {
            InitializeComponent();

            QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.Failed += QueryTask_Failed;

            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            // Specify fields to return from initial query
            query.OutFields.AddRange(new string[] { "STATE_NAME" });

            // This query will just populate the drop-down, so no need to return geometry
            query.ReturnGeometry = false;

            // Return all features
            query.Where = "1=1";
            queryTask.ExecuteAsync(query, "initial");
        }

        private void QueryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QueryButton.Content = QueryListBox.SelectedItem.ToString();

            if (QueryListBox.SelectedItem.ToString().Contains("Select..."))
                return;

            QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.Failed += QueryTask_Failed;

            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.OutSpatialReference = MyMap.SpatialReference;
            query.Text = QueryListBox.SelectedItem.ToString();
			query.ReturnGeometry = true;
            query.OutFields.Add("*");

            queryTask.ExecuteAsync(query);

            QueryChoicesPage.IsOpen = false;
        }

        private void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            // If initial query to populate states list box
            if ((args.UserState as string) == "initial")
            {
                // Just show on initial load
                QueryListBox.Items.Add("Select...");

                foreach (Graphic graphic in args.FeatureSet.Features)
                {
                    QueryListBox.Items.Add(graphic.Attributes["STATE_NAME"].ToString());
                }

                QueryListBox.SelectedIndex = 0;
                return;
            }

            // Remove the first entry if "Select..."
            if (QueryListBox.Items[0].ToString().Contains("Select..."))
                QueryListBox.Items.RemoveAt(0);

            // If an item has been selected            
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            if (featureSet != null && featureSet.Features.Count > 0)
            {
                // Show selected feature attributes in DataGrid
                Graphic selectedFeature = featureSet.Features[0];

                ResultsListBox.Items.Clear();
                //QueryDetailsDataGrid.ItemsSource = selectedFeature.Attributes;
                foreach (KeyValuePair<string, object> pair in selectedFeature.Attributes)
                {
                    TextBlock tb1 = new TextBlock()
                    {
                        FontSize = 30,
                        FontWeight = FontWeights.Bold,
                        Text = string.Format("{0}: ", pair.Key)
                    };
                    TextBlock tb2 = new TextBlock()
                    {
                        FontSize = 30,
                        Text = string.Format(" {0}", pair.Value)
                    };
                    StackPanel sp = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };
                    sp.Children.Add(tb1);
                    sp.Children.Add(tb2);
                    ListBoxItem item = new ListBoxItem();
                    item.Content = sp;
                    ResultsListBox.Items.Add(item);
                }

                // Highlight selected feature
                selectedFeature.Symbol = DefaultFillSymbol;
                graphicsLayer.Graphics.Add(selectedFeature);

                // Zoom to selected feature (define expand percentage)
                ESRI.ArcGIS.Client.Geometry.Envelope selectedFeatureExtent = selectedFeature.Geometry.Extent;

                double expandPercentage = 30;

                double widthExpand = selectedFeatureExtent.Width * (expandPercentage / 100);
                double heightExpand = selectedFeatureExtent.Height * (expandPercentage / 100);

                ESRI.ArcGIS.Client.Geometry.Envelope displayExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(
                selectedFeatureExtent.XMin - (widthExpand / 2),
                selectedFeatureExtent.YMin - (heightExpand / 2),
                selectedFeatureExtent.XMax + (widthExpand / 2),
                selectedFeatureExtent.YMax + (heightExpand / 2));

                MyMap.ZoomTo(displayExtent);

                // If DataGrid not visible (initial load), show it
                if (ResultsListBox.Visibility == Visibility.Collapsed)
                {
                    ResultsListBox.Visibility = Visibility.Visible;
                    QueryGrid.Height = Double.NaN;
                    QueryGrid.UpdateLayout();
                }
            }
            else
            {
                //QueryDetailsDataGrid.ItemsSource = null;
                ResultsListBox.Visibility = Visibility.Collapsed;
                QueryGrid.Height = Double.NaN;
                QueryGrid.UpdateLayout();
            }
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query failed: " + args.Error);
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            QueryBorder.Visibility = QueryBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            QueryChoicesPage.IsOpen = true;
        }
    }
}