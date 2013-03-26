using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Bing.RouteService;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ArcGISWindowsPhoneSDK
{
    public partial class BingRouting : PhoneApplicationPage
    {
        private Draw myDrawObject;
        private GraphicsLayer waypointGraphicsLayer;
        private GraphicsLayer routeResultsGraphicsLayer;
        private ESRI.ArcGIS.Client.Bing.Routing routing;
        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator = new ESRI.ArcGIS.Client.Projection.WebMercator();

        public BingRouting()
        {
            InitializeComponent();

            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.IsVisible = true;
            ApplicationBar.Opacity = 1.0;
        }

        private void BingKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length >= 64)
                LoadMapButton.IsEnabled = true;
            else
                LoadMapButton.IsEnabled = false;
        }
        [DataContract]
        public class BingAuthentication
        {
            [DataMember(Name = "authenticationResultCode")]
            public string authenticationResultCode { get; set; }
        }
        private void LoadMapButton_Click(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();
            string uri = string.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text);

            webClient.OpenReadCompleted += (s, a) =>
            {
                if (a.Error == null)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BingAuthentication));
                    BingAuthentication bingAuthentication = serializer.ReadObject(a.Result) as BingAuthentication;
                    a.Result.Close();

                    if (bingAuthentication.authenticationResultCode == "ValidCredentials")
                    {
                        ESRI.ArcGIS.Client.Bing.TileLayer tileLayer = new TileLayer()
                        {
                            ID = "BingLayer",
                            LayerStyle = TileLayer.LayerType.Road,
                            ServerType = ServerType.Production,
                            Token = BingKeyTextBox.Text
                        };

                        MyMap.Layers.Insert(0, tileLayer);

                        // Add your Bing Maps key in the constructor for the Routing class. Use http://www.bingmapsportal.com to generate a key.  
                        routing = new ESRI.ArcGIS.Client.Bing.Routing(BingKeyTextBox.Text);
                        routing.ServerType = ServerType.Production;

                        myDrawObject = new Draw(MyMap)
                        {
                            DrawMode = DrawMode.Point,
                            IsEnabled = true
                        };

                        myDrawObject.DrawComplete += MyDrawObject_DrawComplete;
                        waypointGraphicsLayer = MyMap.Layers["WaypointGraphicsLayer"] as GraphicsLayer;
                        routeResultsGraphicsLayer = MyMap.Layers["RouteResultsGraphicsLayer"] as GraphicsLayer;

                        ESRI.ArcGIS.Client.Geometry.Envelope initialExtent =
                            new ESRI.ArcGIS.Client.Geometry.Envelope(-13064142.2218876, 3831722.30101942, -13023209.4358361, 3878454.82787388);

                        initialExtent.SpatialReference = new SpatialReference(102100);

                        MyMap.Extent = initialExtent;

                        BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed;
                        DialogsGrid.Visibility = System.Windows.Visibility.Visible;

                        InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed;

                    }
                    else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
                }
                else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
            };

            webClient.OpenReadAsync(new System.Uri(uri));
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            myDrawObject.IsEnabled = false;
            Graphic waypointGraphic = new Graphic()
            {
                Geometry = args.Geometry as MapPoint,
                Symbol = LayoutRoot.Resources["UserStopSymbol"] as Symbol
            };
            waypointGraphic.Attributes.Add("StopNumber", waypointGraphicsLayer.Graphics.Count + 1);
            waypointGraphicsLayer.Graphics.Add(waypointGraphic);

            if (waypointGraphicsLayer.Graphics.Count > 1)
                RouteButton.IsEnabled = true;
            else
                RouteButton.IsEnabled = false;
        }

        private void RouteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            myDrawObject.IsEnabled = false;
            routing.Optimization = RouteOptimization.MinimizeTime;
            routing.TrafficUsage = TrafficUsage.None;
            routing.TravelMode = TravelMode.Driving;
            routing.Route(GraphicsToMapPoints(), Route_Complete);
        }

        private List<MapPoint> GraphicsToMapPoints()
        {
            List<MapPoint> mapPoints = new List<MapPoint>();
            foreach (Graphic g in waypointGraphicsLayer.Graphics)
                mapPoints.Add(g.Geometry as MapPoint);

            return mapPoints;
        }

        private void Route_Complete(object sender, CalculateRouteCompletedEventArgs args)
        {
            myDrawObject.IsEnabled = true;
            routeResultsGraphicsLayer.ClearGraphics();
            waypointGraphicsLayer.ClearGraphics();

            StringBuilder directions = new StringBuilder();

            ObservableCollection<RouteLeg> routeLegs = args.Result.Result.Legs;
            int numLegs = routeLegs.Count;
            int instructionCount = 0;
            for (int n = 0; n < numLegs; n++)
            {
                if ((n % 2) == 0)
                {
                    AddStopPoint(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(routeLegs[n].ActualStart.Longitude, routeLegs[n].ActualStart.Latitude)));
                    AddStopPoint(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(routeLegs[n].ActualEnd.Longitude, routeLegs[n].ActualEnd.Latitude)));
                }
                else if (n == (numLegs - 1))
                {
                    AddStopPoint(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(routeLegs[n].ActualEnd.Longitude, routeLegs[n].ActualEnd.Latitude)));
                }

                directions.Append(string.Format("--Leg #{0}--\n", n + 1));

                foreach (ItineraryItem item in routeLegs[n].Itinerary)
                {
                    instructionCount++;
                    directions.Append(string.Format("{0}. {1}\n", instructionCount, item.Text));
                }
            }

            Regex regex = new Regex("<[/a-zA-Z:]*>",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            DirectionsContentTextBlock.Text = regex.Replace(directions.ToString(), string.Empty);
            DirectionsGrid.Visibility = Visibility.Visible;

            RoutePath routePath = args.Result.Result.RoutePath;

            Polyline line = new ESRI.ArcGIS.Client.Geometry.Polyline();
            line.Paths.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection());

            foreach (ESRI.ArcGIS.Client.Bing.RouteService.Location location in routePath.Points)
                line.Paths[0].Add(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(location.Longitude, location.Latitude)));

            Graphic graphic = new Graphic()
            {
                Geometry = line,
                Symbol = LayoutRoot.Resources["RoutePathSymbol"] as Symbol
            };
            routeResultsGraphicsLayer.Graphics.Add(graphic);
        }

        private void AddStopPoint(MapPoint mapPoint)
        {
            Graphic graphic = new Graphic()
            {
                Geometry = mapPoint,
                Symbol = LayoutRoot.Resources["ResultStopSymbol"] as Symbol
            };
            graphic.Attributes.Add("StopNumber", waypointGraphicsLayer.Graphics.Count + 1);
            waypointGraphicsLayer.Graphics.Add(graphic);
        }

        private void ClearRouteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            waypointGraphicsLayer.ClearGraphics();
            routeResultsGraphicsLayer.ClearGraphics();
            DirectionsContentTextBlock.Text = "";
            DirectionsGrid.Visibility = System.Windows.Visibility.Collapsed;
            RouteButton.IsEnabled = false;
        }

        #region Button/Menu methods

        private void DrawPointButton_Click(object sender, EventArgs e)
        {
            myDrawObject.DrawMode = DrawMode.Point;
            myDrawObject.IsEnabled = true;
        }

        private void Menu_Dialog_Click(object sender, EventArgs e)
        {
            InputGrid.Visibility = InputGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            DirectionsGrid.Visibility = DirectionsGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        private void CloseInputGridGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InputGrid.Visibility = Visibility.Collapsed;
        }

        private void CloseDirectionsGridGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DirectionsGrid.Visibility = Visibility.Collapsed;
        }
    }
}