using Microsoft.Phone.Controls;
using System;
using System.Windows;
using ESRI.ArcGIS.Client;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Coordinates : PhoneApplicationPage
    {
        public Coordinates()
        {
            InitializeComponent();
        }

        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Hold && MyMap.Extent != null)
            {
                System.Windows.Point screenPoint = e.GetPosition(MyMap);
                ScreenCoordsTextBlock.Text = string.Format("Screen Coordinates:\r\n    X = {0}, Y = {1}",
                    screenPoint.X, screenPoint.Y);

                ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = MyMap.ScreenToMap(screenPoint);
                if (MyMap.WrapAroundIsActive)
                    mapPoint = ESRI.ArcGIS.Client.Geometry.Geometry.NormalizeCentralMeridian(mapPoint) as ESRI.ArcGIS.Client.Geometry.MapPoint;
                MapCoordsTextBlock.Text = string.Format("Map Coordinates:\r\n    X = {0}, Y = {1}",
                    Math.Round(mapPoint.X, 4), Math.Round(mapPoint.Y, 4));
            }
        }

        private void ShowCoordsButton_Click(object sender, System.EventArgs e)
        {
            CoordsGrid.Visibility = CoordsGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowHintButton_Click(object sender, System.EventArgs e)
        {
            HintTextGrid.Visibility = HintTextGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}