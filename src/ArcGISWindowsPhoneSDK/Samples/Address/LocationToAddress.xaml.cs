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


        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap && MyMap.Extent != null)
            {
                Locator locatorTask = new Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");
                locatorTask.LocationToAddressCompleted += LocatorTask_LocationToAddressCompleted;
                locatorTask.Failed += LocatorTask_Failed;

                // Tolerance (distance) specified in meters
                double tolerance = 30;
                locatorTask.LocationToAddressAsync(e.MapPoint, tolerance);

                Graphic graphic = new Graphic()
                {
                    Symbol = DefaultMarkerSymbol,
                    Geometry = e.MapPoint
                };
                GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
                graphicsLayer.Graphics.Clear();
                graphicsLayer.Graphics.Add(graphic);
            }
        }

        private void LocatorTask_LocationToAddressCompleted(object sender, AddressEventArgs args)
        {
            Address address = args.Address;
            Dictionary<string, object> attributes = address.Attributes;
            
            string locAddress = string.Format("{0}\n{1}, {2} {3}",attributes["Address"],attributes["City"], 
                attributes["Region"], attributes["Postal"]);    

            AddressText.Text = locAddress;
            AddressBorder.Visibility = Visibility.Visible;
        }

        private void LocatorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            AddressBorder.Visibility = Visibility.Collapsed;
            MessageBox.Show("Locator service failed: " + e.Error);
        }
    }
}