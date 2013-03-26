using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using System.Windows.Media;

namespace ArcGISWindowsPhoneSDK
{
    public partial class CSVLayer : PhoneApplicationPage
    {
        public CSVLayer()
        {
            InitializeComponent();
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            CsvLayer csvLayer = MyMap.Layers["MyCSVLayer"] as CsvLayer;
            System.Windows.Point screenPnt = MyMap.MapToScreen(e.MapPoint);

            // Account for difference between Map and application origin
            GeneralTransform generalTransform = MyMap.TransformToVisual(null);
            System.Windows.Point transformScreenPnt = generalTransform.Transform(screenPnt);

            int tolerance = 20;
            Rect screenRect = new Rect(new Point(transformScreenPnt.X - tolerance / 2, transformScreenPnt.Y - tolerance / 2),
                new Point(transformScreenPnt.X + tolerance / 2, transformScreenPnt.Y + tolerance / 2));
            IEnumerable<Graphic> selected =
                csvLayer.FindGraphicsInHostCoordinates(screenRect);

            foreach (Graphic g in selected)
            {

                MyInfoWindow.Anchor = e.MapPoint;
                MyInfoWindow.IsOpen = true;
                //Since a ContentTemplate is defined, Content will define the DataContext for the ContentTemplate
                MyInfoWindow.Content = g.Attributes;
                return;
            }
        }

        private void MyInfoWindow_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MyInfoWindow.IsOpen = false;
        }

    }
}