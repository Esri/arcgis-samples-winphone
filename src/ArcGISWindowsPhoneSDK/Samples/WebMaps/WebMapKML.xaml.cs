using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISWindowsPhoneSDK
{
    public partial class WebMapKML : PhoneApplicationPage
    {
        InfoWindow MyInfoWindow;
        Map MyMap;

        public WebMapKML()
        {
            InitializeComponent();

            MyInfoWindow = new InfoWindow()
            {
                Padding = new Thickness(5),
                CornerRadius = 5,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Colors.Black)
            };

            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("d2cb7cac8b1947c7b57ed8edd6b045bb");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                foreach (Layer layer in e.Map.Layers)
                    if (layer is KmlLayer)
                        (layer as KmlLayer).Initialized += kmllayer_Initialized;

                MyMap = e.Map;
                MyMap.WrapAround = true;
                ContentPanel.Children.Add(MyMap);
                ContentPanel.Children.Add(MyInfoWindow);
                MyInfoWindow.Map = MyMap;
            }
        }

        void kmllayer_Initialized(object sender, System.EventArgs e)
        {
            foreach (Layer layer in (sender as KmlLayer).ChildLayers)
            {
                layer.Visible = true;

                AddEventsToGraphicsLayers(layer);
            }
        }
        void AddEventsToGraphicsLayers(Layer layer)
        {
            // a KmlLayer is a group layer containing other KmlLayers or the basic KML group layer, which contains 
            // GraphicsLayer for points, a GraphicsLayer for Polylines, a GraphicsLayer for Polygons, and an 
            // ElementLayer for GroundOverlays. For each of the GraphicsLayers, we will add pop-ups on tap 

            if (layer is GraphicsLayer)
                (layer as GraphicsLayer).MouseLeftButtonUp += WebMapKML_MouseLeftButtonUp;

            else if (layer is KmlLayer)
                foreach (Layer childlayer in (layer as KmlLayer).ChildLayers)
                    AddEventsToGraphicsLayers(childlayer);
        }

        void WebMapKML_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            GraphicsLayer glayer = sender as GraphicsLayer;
            MapPoint clickPoint = MyMap.ScreenToMap(e.GetPosition(MyMap));

            MyInfoWindow.Anchor = clickPoint;
            MyInfoWindow.Content = e.Graphic.Attributes["name"];
            MyInfoWindow.IsOpen = true;
        }
    }
}