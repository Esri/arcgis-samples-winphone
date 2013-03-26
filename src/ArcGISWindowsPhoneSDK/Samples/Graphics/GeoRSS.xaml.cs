using System;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISWindowsPhoneSDK
{
    public partial class GeoRSS : PhoneApplicationPage
    {
        private ESRI.ArcGIS.Client.Projection.WebMercator wm = new ESRI.ArcGIS.Client.Projection.WebMercator();

        public GeoRSS()
        {
            InitializeComponent();
        }

        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap)
            {
                MyInfoWindow.IsOpen = false;

                GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
                IEnumerable<Graphic> selected = e.DirectlyOver(10, new GraphicsLayer[] { graphicsLayer });
                foreach (Graphic g in selected)
                {
                    MyInfoWindow.Anchor = e.MapPoint;
                    MyInfoWindow.IsOpen = true;
                    MyInfoWindow.DataContext = g;
                    return;
                }
            }
        }

        private void FetchRssButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (FeedLocationTextBox.Text != String.Empty)
            {
                RSSFeedGrid.Visibility = Visibility.Collapsed;
                
                LoadRSS(FeedLocationTextBox.Text.Trim());
                DispatcherTimer UpdateTimer = new System.Windows.Threading.DispatcherTimer();
                UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 60000);
                UpdateTimer.Tick += (evtsender, args) =>
                {
                    LoadRSS(FeedLocationTextBox.Text.Trim());
                };
                UpdateTimer.Start();
            }
        }

        protected void LoadRSS(string uri)
        {
            WebClient wc = new WebClient();
            wc.OpenReadCompleted += wc_OpenReadCompleted;
            Uri feedUri = new Uri(uri, UriKind.Absolute);
            wc.OpenReadAsync(feedUri);
        }

        private void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {

            if (e.Error != null)
            {
                FeedLocationTextBox.Text = "Error in Reading Feed. Try Again later!!";
                return;
            }

            // use LINQ to query GeoRSS feed.
            UseLinq(e.Result);
        }

        private void UseLinq(Stream s)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            XDocument doc = XDocument.Load(s);
            XNamespace geo = "http://www.w3.org/2003/01/geo/wgs84_pos#";

            var rssGraphics = from rssgraphic in doc.Descendants("item")
                              select new RssGraphic
                              {
                                  Geometry = new MapPoint(
                                      Convert.ToDouble(rssgraphic.Element(geo + "long").Value, System.Globalization.CultureInfo.InvariantCulture),
                                      Convert.ToDouble(rssgraphic.Element(geo + "lat").Value, System.Globalization.CultureInfo.InvariantCulture),
                                      new SpatialReference(4326)),
                                  Symbol = LayoutRoot.Resources["QuakePictureSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                                  RssAttributes = new Dictionary<string, object>() { { "MAGNITUDE", rssgraphic.Element("title").Value } }
                              };

            foreach (RssGraphic rssGraphic in rssGraphics)
            {
                foreach (KeyValuePair<string, object> rssAttribute in rssGraphic.RssAttributes)
                {
                    rssGraphic.Attributes.Add(rssAttribute.Key, rssAttribute.Value);
                }
                graphicsLayer.Graphics.Add((Graphic)rssGraphic);
            }
        }

        internal class RssGraphic : Graphic
        {
            public Dictionary<string, object> RssAttributes { get; set; }
        }

        private void ShowRSSEntryButton_Click(object sender, EventArgs e)
        {
            RSSFeedGrid.Visibility = RSSFeedGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}