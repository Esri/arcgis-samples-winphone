using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK.Samples
{
    public partial class LocationToAddress : PhoneApplicationPage
    {
        private ESRI.ArcGIS.Client.Geometry.MapPoint _mapClick;

        public LocationToAddress()
        {
            InitializeComponent();
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            Locator locatorTask = new Locator("http://tasks.arcgisonline.com/ArcGIS/rest/services/Locators/TA_Streets_US/GeocodeServer");
            locatorTask.LocationToAddressCompleted += LocatorTask_LocationToAddressCompleted;
            locatorTask.Failed += LocatorTask_Failed;

            // Tolerance (distance) specified in meters
            double tolerance = 20;
            locatorTask.LocationToAddressAsync(e.MapPoint, tolerance);
        }

        private void LocatorTask_LocationToAddressCompleted(object sender, AddressEventArgs args)
        {
            Address address = args.Address;
            Dictionary<string, object> attributes = address.Attributes;

            Graphic graphic = new Graphic()
            {
                Symbol = DefaultMarkerSymbol,
                Geometry = address.Location
            };
            string locAddress = string.Format("{0}\n{1}, {2} {3}",attributes["Street"].ToString(),attributes["City"], attributes["State"], attributes["ZIP"]);
            TextSymbol textSymbol = new TextSymbol()
            {
                Text = locAddress,
                OffsetX = 0,
                OffsetY = 0,
                Foreground = new SolidColorBrush(Colors.Red),
                
            };

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.Graphics.Clear();
            graphicsLayer.Graphics.Add(graphic);
            AddressText.Text = locAddress;
            AddressBorder.Visibility = Visibility.Visible;
        }

        private void LocatorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Locator service failed: " + e.Error);
        }

        private void Menu_LocateMeClick(object sender, EventArgs e)
        {
            ClickScreen.Visibility = Visibility.Visible;
        }

        private void ClickScreen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(ContentPanel);
            _mapClick = MyMap.ScreenToMap(pt);

            ClickScreen.Visibility = Visibility.Collapsed;
            Locator locatorTask = new Locator("http://tasks.arcgisonline.com/ArcGIS/rest/services/Locators/TA_Streets_US/GeocodeServer");
            locatorTask.LocationToAddressCompleted += LocatorTask_LocationToAddressCompleted;
            locatorTask.Failed += LocatorTask_Failed;

            // Tolerance (distance) specified in meters
            double tolerance = 30;
            locatorTask.LocationToAddressAsync(_mapClick, tolerance);
        }

  
    }
}