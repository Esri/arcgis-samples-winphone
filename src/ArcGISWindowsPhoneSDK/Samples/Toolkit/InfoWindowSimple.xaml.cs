using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class InfoWindowSimple : PhoneApplicationPage
    {
        public InfoWindowSimple()
        {
            InitializeComponent();
        }

        private void MyMap_MapGesture(object sender, ESRI.ArcGIS.Client.Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap)
            {                
                FeatureLayer featureLayer = MyMap.Layers["MyFeatureLayer"] as FeatureLayer;
                IEnumerable<Graphic> selected = e.DirectlyOver(10,  new GraphicsLayer[] { featureLayer });
                foreach (Graphic g in selected)
                {                     
                    MyInfoWindow.Anchor = e.MapPoint;
                    MyInfoWindow.IsOpen = true;
                    //Since a ContentTemplate is defined (in XAML), Content will define the DataContext for the ContentTemplate
                    MyInfoWindow.Content = g;                                                          
                    return;
                }

                InfoWindow window = new InfoWindow()
                {
                    Anchor = e.MapPoint,
                    Padding = new Thickness(3),
                    Map = MyMap,
                    IsOpen = true,
                    Placement = InfoWindow.PlacementMode.Auto,
                    ContentTemplate = LayoutRoot.Resources["LocationInfoWindowTemplate"] as System.Windows.DataTemplate,
                    //Since a ContentTemplate is defined, Content will define the DataContext for the ContentTemplate                    
                    Content = new ESRI.ArcGIS.Client.Geometry.MapPoint(
                        double.Parse(e.MapPoint.X.ToString("0.000")),
                        double.Parse(e.MapPoint.Y.ToString("0.000")))
                };
                LayoutRoot.Children.Add(window);
            }
        }
    }
}