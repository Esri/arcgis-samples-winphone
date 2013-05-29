using Microsoft.Phone.Controls;
using System;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Threading;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Coordinates : PhoneApplicationPage
    {
        DispatcherTimer _dispatcherTimer;
        System.Windows.Point _currentScreenPoint;

        public Coordinates()
        {
            InitializeComponent();

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 350);
            _dispatcherTimer.Tick += (s, ex) =>
            {
                ScreenCoordsTextBlock.Text = string.Format("Screen Coordinates:\r\n    X = {0}, Y = {1}",
                    _currentScreenPoint.X, _currentScreenPoint.Y);

                ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = MyMap.ScreenToMap(_currentScreenPoint);
                if (MyMap.WrapAroundIsActive)
                    mapPoint = ESRI.ArcGIS.Client.Geometry.Geometry.NormalizeCentralMeridian(mapPoint) as ESRI.ArcGIS.Client.Geometry.MapPoint;
                MapCoordsTextBlock.Text = string.Format("Map Coordinates:\r\n    X = {0}, Y = {1}",
                    Math.Round(mapPoint.X, 4), Math.Round(mapPoint.Y, 4));

                GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
                graphicsLayer.Graphics[0].Geometry = mapPoint;

                _dispatcherTimer.Stop();
            };
        }

        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {            
            if (e.Gesture == GestureType.Tap && MyMap.Extent != null && !_dispatcherTimer.IsEnabled)
            {
                _currentScreenPoint = e.GetPosition(MyMap);
                _dispatcherTimer.Start();
            }
            else if (e.Gesture == GestureType.DoubleTap)
            {
                _dispatcherTimer.Stop();
            }
        }

        private void ShowCoordsButton_Click(object sender, System.EventArgs e)
        {
            CoordsGrid.Visibility = CoordsGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }       
    }
}