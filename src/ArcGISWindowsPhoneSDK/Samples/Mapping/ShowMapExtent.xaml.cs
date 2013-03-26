using System;
using System.Windows;
using ESRI.ArcGIS.Client.Geometry;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class ShowMapExtent : PhoneApplicationPage
    {
        public ShowMapExtent()
        {
            InitializeComponent();
        }

        private void MyMap_Loaded(object sender, RoutedEventArgs e)
        {
            MyMap.ExtentChanging += MyMap_ExtentChanging;
            MyMap.ExtentChanged += MyMap_ExtentChanging;
        }

        void MyMap_ExtentChanging(object sender, ESRI.ArcGIS.Client.ExtentEventArgs e)
        {
            Envelope ext = e.NewExtent;
            XMinText.Text = String.Format("MinX: {0}", Math.Round(ext.XMin, 3));
            YMinText.Text = String.Format("MinY: {0}", Math.Round(ext.YMin, 3));
            XMaxText.Text = String.Format("MaxX: {0}", Math.Round(ext.XMax, 3));
            YMaxText.Text = String.Format("MaxY: {0}", Math.Round(ext.YMax, 3));
        }
    }
}