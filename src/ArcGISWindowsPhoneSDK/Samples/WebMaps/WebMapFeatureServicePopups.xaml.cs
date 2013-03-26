using Microsoft.Phone.Controls;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISWindowsPhoneSDK
{
    public partial class WebMapFeatureServicePopups : PhoneApplicationPage
    {
        MapPoint lastPoint;

        public WebMapFeatureServicePopups()
        {
            InitializeComponent();
            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("921e9016d2a5423da8bd08eb306e4e0e");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MyMap.Extent = e.Map.Extent;
                int i = 0;

                LayerCollection layerCollection = new LayerCollection();
                foreach (Layer layer in e.Map.Layers)
                {
                    layer.ID = i.ToString();
                    layerCollection.Add(layer);
                    if (layer is FeatureLayer)
                        (layer as FeatureLayer).MouseLeftButtonUp += WebMapFeatureServicePopups_MouseLeftButtonUp;
                    i++;
                }

                e.Map.Layers.Clear();
                MyMap.Layers = layerCollection;
            }
        }

        void WebMapFeatureServicePopups_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            FeatureLayer flayer = sender as FeatureLayer;
            MapPoint clickPoint = MyMap.ScreenToMap(e.GetPosition(MyMap));

            if (clickPoint != lastPoint)
            {
                if (flayer.GetValue(Document.PopupTemplateProperty) != null)
                {
                    DataTemplate dt = flayer.GetValue(Document.PopupTemplateProperty) as DataTemplate;

                    MyInfoWindow.Anchor = clickPoint;
                    MyInfoWindow.ContentTemplate = dt;
                    MyInfoWindow.Content = e.Graphic.Attributes;
                    MyInfoWindow.IsOpen = true;
                    lastPoint = clickPoint;
                }
            }
        }
    }
}