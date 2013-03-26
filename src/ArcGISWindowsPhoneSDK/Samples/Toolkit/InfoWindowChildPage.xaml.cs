using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class InfoWindowChildPage : PhoneApplicationPage
    {
        Graphic _tapGraphic;
        DispatcherTimer _dispatcherTimer;

        public InfoWindowChildPage()
        {
            InitializeComponent();
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
        }

        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + e.Gesture.ToString());
            
            if (e.Gesture == GestureType.Tap || e.Gesture == GestureType.Hold)
            {
                List<Graphic> results = new List<Graphic>(e.DirectlyOver(10));
                if (results.Count == 0)
                 MyInfoWindow.IsOpen = false; 
                else
                {
                    _tapGraphic = results[0];
                    _dispatcherTimer.Start();
                }                             
            }
            else if (e.Gesture == GestureType.DoubleTap)
            {
                _dispatcherTimer.Stop();
            }
        }

        private void ShowDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            MyDetailsPage.DataContext = (sender as FrameworkElement).DataContext;
            MyDetailsPage.IsOpen = true;
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {          
            MyMap.PanTo(_tapGraphic.Geometry);

            MyInfoWindow.DataContext = _tapGraphic;
            MyInfoWindow.Anchor = _tapGraphic.Geometry.Extent.GetCenter();
            MyInfoWindow.IsOpen = true;

            _dispatcherTimer.Stop();
        }

        private void MyInfoWindow_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MyInfoWindow.IsOpen = false;
        }
    }
}