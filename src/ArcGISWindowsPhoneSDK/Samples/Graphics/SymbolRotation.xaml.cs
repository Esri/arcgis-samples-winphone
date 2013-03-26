using System;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using Microsoft.Phone.Controls;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows;
using System.Windows.Media;

namespace ArcGISWindowsPhoneSDK
{
    public partial class SymbolRotation : PhoneApplicationPage
    {
        public SymbolRotation()
        {
            InitializeComponent();
        }

               
       private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
             FeatureLayer featureLayer = MyMap.Layers["WindLayer"] as FeatureLayer;
            System.Windows.Point screenPnt = MyMap.MapToScreen(e.MapPoint);

            // Account for difference between Map and application origin
            GeneralTransform generalTransform = MyMap.TransformToVisual(null);
            System.Windows.Point transformScreenPnt = generalTransform.Transform(screenPnt);

            int tolerance = 20;
            Rect screenRect = new Rect(new Point(transformScreenPnt.X - tolerance / 2, transformScreenPnt.Y - tolerance / 2),
                new Point(transformScreenPnt.X + tolerance / 2, transformScreenPnt.Y + tolerance / 2));
            IEnumerable<Graphic> selected =
                featureLayer.FindGraphicsInHostCoordinates(screenRect);

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