using Microsoft.Phone.Controls;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;

namespace ArcGISWindowsPhoneSDK
{
    public partial class HostedFeatureServiceSimple : PhoneApplicationPage
    {
        public HostedFeatureServiceSimple()
        {
            InitializeComponent();
        }

        private void MyMap_MapGesture(object sender, ESRI.ArcGIS.Client.Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap)
            {
                MyInfoWindow.IsOpen = false;

                FeatureLayer featureLayer = MyMap.Layers["MyFeatureLayer"] as FeatureLayer;

                IEnumerable<Graphic> selected = e.DirectlyOver(10, new GraphicsLayer[] { featureLayer });
                foreach (Graphic g in selected)
                {
                    MyInfoWindow.Anchor = e.MapPoint;
                    MyInfoWindow.IsOpen = true;
                    MyInfoWindow.DataContext = g.Attributes;
                    return;
                }
            }
        }
    }
}