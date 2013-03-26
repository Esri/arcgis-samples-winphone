using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows;

namespace ArcGISWindowsPhoneSDK
{
    public partial class FeatureLayerOnDemand : PhoneApplicationPage
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator = new ESRI.ArcGIS.Client.Projection.WebMercator();

        ESRI.ArcGIS.Client.Geometry.Envelope initialExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(
            mercator.FromGeographic(new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.190346717, 34.0514888762)) as ESRI.ArcGIS.Client.Geometry.MapPoint,
            mercator.FromGeographic(new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.160305976, 34.072946548)) as ESRI.ArcGIS.Client.Geometry.MapPoint)
            {
                SpatialReference = new SpatialReference(102100)
            };

        public FeatureLayerOnDemand()
        {
            InitializeComponent();

            MyMap.Extent = initialExtent;
        }

        private void MyMap_MapGesture(object sender, ESRI.ArcGIS.Client.Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Hold)
            {
                FeatureLayer featureLayer = MyMap.Layers["MyFeatureLayer"] as FeatureLayer;
                IEnumerable<Graphic> selected = e.DirectlyOver(10, new GraphicsLayer[] { featureLayer });
                foreach (Graphic g in selected)
                {
                    MyInfoWindow.Anchor = e.MapPoint;
                    MyInfoWindow.IsOpen = true;
                    MyInfoWindow.Content = g;
                    return;
                }
            }
            else if (e.Gesture == GestureType.Tap)
                MyInfoWindow.IsOpen = false;
        }
    }
}