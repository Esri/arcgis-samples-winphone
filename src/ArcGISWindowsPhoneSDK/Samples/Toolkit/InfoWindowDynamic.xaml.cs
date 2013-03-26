using System;
using System.Windows.Controls;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using Microsoft.Phone.Controls;
using System.Windows;

namespace ArcGISWindowsPhoneSDK
{
    public partial class InfoWindowDynamic : PhoneApplicationPage
    {
        InfoWindow _infoWindow;
        MapPoint _tapPoint;
        DispatcherTimer _dispatcherTimer;

        public InfoWindowDynamic()
        {
            InitializeComponent();

            _infoWindow = new InfoWindow()
            {
                Map = MyMap,
                Padding = new Thickness(3),
                IsOpen = false,
                Content = new TextBlock(),
            };
            LayoutRoot.Children.Add(_infoWindow);
            _dispatcherTimer = new DispatcherTimer();
        }

        private void Map_MapGesture(object sender, ESRI.ArcGIS.Client.Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap)
            {
                _tapPoint = e.MapPoint;
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 350);
                _dispatcherTimer.Tick -= dispatcherTimer_Tick;
                _dispatcherTimer.Tick += dispatcherTimer_Tick;
                _dispatcherTimer.Start();
            }
            else if (e.Gesture == GestureType.DoubleTap)
            {
                _dispatcherTimer.Stop();
            }
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _infoWindow.IsOpen = false;

            QueryTask queryTask =
                new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/4");
            Query query = new Query()
            {
                Geometry = _tapPoint,
                OutSpatialReference = MyMap.SpatialReference
            };
            query.OutFields.Add("NAME");

            queryTask.ExecuteCompleted += (s, evt) =>
            {
                if (evt.FeatureSet.Features.Count > 0)
                {
                    _infoWindow.Anchor = _tapPoint;
                    (_infoWindow.Content as TextBlock).Text = evt.FeatureSet.Features[0].Attributes["NAME"] as string;
                    _infoWindow.IsOpen = true;
                }
            };
            queryTask.ExecuteAsync(query);
            _dispatcherTimer.Stop();
        }
    }
}