using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Symbols;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Controls;
using System.IO;
using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;
using System.Runtime.Serialization.Json;

namespace ArcGISWindowsPhoneSDK
{
    public partial class SOEElevationDataContract : PhoneApplicationPage
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public SOEElevationDataContract()
        {
            InitializeComponent();
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            MapPoint geographicPoint = _mercator.ToGeographic(e.MapPoint) as MapPoint; 

            string SOEurl = "http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationAtLonLat";
            SOEurl += string.Format(System.Globalization.CultureInfo.InvariantCulture, "?lon={0}&lat={1}&f=json", geographicPoint.X, geographicPoint.Y);

            WebClient webClient = new WebClient();

            webClient.OpenReadCompleted += (s, a) =>
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ElevationSOELatLon));
                ElevationSOELatLon elevationSOELatLon = serializer.ReadObject(a.Result) as ElevationSOELatLon;
                a.Result.Close();

                MyInfoWindow.Anchor = e.MapPoint;
                MyInfoWindow.Content = string.Format("Elevation: {0} meters", elevationSOELatLon.Elevation.ToString("0"));
                MyInfoWindow.IsOpen = true;
            };

            webClient.OpenReadAsync(new Uri(SOEurl));
        }

        [DataContract]
        public class ElevationSOELatLon
        {
            [DataMember(Name = "elevation")]
            public double Elevation { get; set; }
        }
    }
}