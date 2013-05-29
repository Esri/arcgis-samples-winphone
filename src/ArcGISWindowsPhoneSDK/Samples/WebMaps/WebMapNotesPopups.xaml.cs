using Microsoft.Phone.Controls;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISWindowsPhoneSDK
{
    public partial class WebMapNotesPopups : PhoneApplicationPage
    {
        MapPoint lastPoint;

        public WebMapNotesPopups()
        {
            InitializeComponent();

            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("2ccf901c5b414e5c98a346edb75e3c13");
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
                    if (layer is GroupLayer) // the graphicslayer we are interested in is in a grouplayer
                        foreach (Layer childLayer in (layer as GroupLayer).ChildLayers)
                            if (childLayer is GraphicsLayer)
                                  (childLayer as GraphicsLayer).MouseLeftButtonUp += WebMapMapNotesPopups_MouseLeftButtonUp;
                    i++;
                }

                e.Map.Layers.Clear();
                MyMap.Layers = layerCollection;
            }
        }

        void WebMapMapNotesPopups_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            GraphicsLayer glayer = sender as GraphicsLayer;
            MapPoint clickPoint = MyMap.ScreenToMap(e.GetPosition(MyMap));

            if (clickPoint != lastPoint)
            {
                if (glayer.GetValue(Document.PopupTemplateProperty) != null)
                {
                    DataTemplate dt = glayer.GetValue(Document.PopupTemplateProperty) as DataTemplate;

                    MyInfoWindow.Anchor = clickPoint;
                    MyInfoWindow.ContentTemplate = dt;
                    MyInfoWindow.Content = e.Graphic.Attributes;
                    MyInfoWindow.IsOpen = true;
                    lastPoint = clickPoint;
                }
            }
        }

        private void MyMap_MouseLeftButtonUp_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}