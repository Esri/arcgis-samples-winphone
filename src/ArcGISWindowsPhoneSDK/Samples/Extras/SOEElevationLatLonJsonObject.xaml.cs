using System;
using System.Net;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Geometry;
using Microsoft.Phone.Controls;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ArcGISWindowsPhoneSDK
{
    public partial class SOEElevationLatLonJsonObject : PhoneApplicationPage
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public SOEElevationLatLonJsonObject()
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
                var sr = new StreamReader(a.Result);
                string str = sr.ReadToEnd();


                JObject jsonResponse = JObject.Parse(str);
                double elevation = (double)jsonResponse["elevation"];
                a.Result.Close();

                MyInfoWindow.Anchor = e.MapPoint;
                MyInfoWindow.Content = string.Format("Elevation: {0} meters", elevation.ToString("0"));
                MyInfoWindow.IsOpen = true;
            };

            webClient.OpenReadAsync(new Uri(SOEurl));
        }
    }
}
