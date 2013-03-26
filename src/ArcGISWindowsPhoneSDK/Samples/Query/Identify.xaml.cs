using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Identify : PhoneApplicationPage
    {
        private List<DataItem> _dataItems = null;
        Draw MyDrawObject;

        public Identify()
        {
            InitializeComponent();

            MyDrawObject = new Draw(MyMap)
            {
                IsEnabled = false,
                DrawMode = DrawMode.Point
            };

            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            MyDrawObject.IsEnabled = true;
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs e)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint clickPoint = e.Geometry as MapPoint;

            ESRI.ArcGIS.Client.Tasks.IdentifyParameters identifyParams = new IdentifyParameters()
            {
                SpatialReference = MyMap.SpatialReference,
                Geometry = clickPoint,
                MapExtent = MyMap.Extent,
                Width = (int)MyMap.ActualWidth,
                Height = (int)MyMap.ActualHeight,
                LayerOption = LayerOption.visible
            };

            IdentifyTask identifyTask = new IdentifyTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer");
            identifyTask.ExecuteCompleted += IdentifyTask_ExecuteCompleted;
            identifyTask.Failed += IdentifyTask_Failed;
            identifyTask.ExecuteAsync(identifyParams);

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();
            ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = clickPoint,
                Symbol = LayoutRoot.Resources["DefaultPictureSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol
            };
            graphicsLayer.Graphics.Add(graphic);
        }

        public void ShowFeatures(List<IdentifyResult> results)
        {
            _dataItems = new List<DataItem>();

            if (results != null && results.Count > 0)
            {
                IdentifyListBox.Items.Clear();
                foreach (IdentifyResult result in results)
                {
                    Graphic feature = result.Feature;
                    string title = result.Value.ToString() + " (" + result.LayerName + ")";
                    _dataItems.Add(new DataItem()
                    {
                        Title = title,
                        Data = feature.Attributes
                    });
                    IdentifyListBox.Items.Add(title);
                }
                IdentifyListBox.SelectedIndex = 0;
            }
        }

        void IdentifyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = IdentifyListBox.SelectedIndex;
            if (index > -1)
            {
                IdentifyButton.Content = IdentifyListBox.SelectedItem.ToString();
                DataListBox.ItemsSource = _dataItems[index].Data;
                IdentifyChoicesPage.IsOpen = false;
            }
        }

        private void IdentifyTask_ExecuteCompleted(object sender, IdentifyEventArgs args)
        {
            DataListBox.ItemsSource = null;

            if (args.IdentifyResults != null && args.IdentifyResults.Count > 0)
            {
                IdentifyResultsPanel.Visibility = Visibility.Visible;
                IdentifyBorder.Visibility = Visibility.Visible;
                ShowFeatures(args.IdentifyResults);
            }
            else
            {
                IdentifyListBox.Items.Clear();
                IdentifyListBox.UpdateLayout();

                IdentifyResultsPanel.Visibility = Visibility.Collapsed;
            }
        }

        public class DataItem
        {
            public string Title { get; set; }
            public IDictionary<string, object> Data { get; set; }
        }

        void IdentifyTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Identify failed. Error: " + e.Error);
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            IdentifyBorder.Visibility = IdentifyBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void IdentifyButton_Click(object sender, RoutedEventArgs e)
        {
            IdentifyChoicesPage.IsOpen = true;
        }
    }
}
