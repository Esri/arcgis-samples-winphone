using Microsoft.Phone.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISWindowsPhoneSDK
{
    public partial class WebMapCharts : PhoneApplicationPage
    {
        static Color currentAccentColor = (Color)Application.Current.Resources["PhoneAccentColor"];
        private UIElement highlightedChartPiece;
        private readonly Brush highlightColor = new SolidColorBrush(currentAccentColor);
        private Brush origColor;

        public WebMapCharts()
        {
            InitializeComponent();

            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("3fafddcb23ee41cf9c597054f0da6bd6");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MyMap.Extent = e.Map.Extent;

                LayerCollection layerCollection = new LayerCollection();
                foreach (Layer layer in e.Map.Layers)
                    layerCollection.Add(layer);

                GraphicsLayer selectedGraphics = new GraphicsLayer()
                {
                    RendererTakesPrecedence = false,
                    ID = "MySelectionGraphicsLayer"
                };
                layerCollection.Add(selectedGraphics);

                e.Map.Layers.Clear();
                MyMap.Layers = layerCollection;
            }
        }

        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap)
            {
                GraphicsLayer glayer = MyMap.Layers["MySelectionGraphicsLayer"] as GraphicsLayer;
                glayer.Graphics.Clear();

                MyInfoWindow.IsOpen = false;
                chartDetailsContent.Content = null;

                double mapScale = MyMap.Scale;

                ArcGISTiledMapServiceLayer alayer = null;
                DataTemplate dt = null;
                int layid = 0;

                foreach (Layer layer in MyMap.Layers)
                {
                    if (layer.GetValue(Document.PopupTemplatesProperty) != null)
                    {
                        alayer = layer as ArcGISTiledMapServiceLayer;
                        IDictionary<int, DataTemplate> idict = layer.GetValue(Document.PopupTemplatesProperty) as IDictionary<int, DataTemplate>;

                        foreach (LayerInfo linfo in alayer.Layers)
                        {
                            if (((mapScale > linfo.MaxScale // in scale range
                                && mapScale < linfo.MinScale) ||
                                (linfo.MaxScale == 0.0 // no scale dependency
                                && linfo.MinScale == 0.0) ||
                                (mapScale > linfo.MaxScale // minscale = 0.0 = infinity
                                && linfo.MinScale == 0.0)) &&
                                idict.ContainsKey(linfo.ID)) // id present in dictionary
                            {
                                layid = linfo.ID;
                                dt = idict[linfo.ID];
                                break;
                            }
                        }
                        if (dt != null)
                        {
                            QueryTask qt = new QueryTask(string.Format("{0}/{1}", alayer.Url, layid));
                            qt.ExecuteCompleted += (s, qe) =>
                            {
                                if (qe.FeatureSet.Features.Count > 0)
                                {
                                    Graphic g = qe.FeatureSet.Features[0];
                                    MyInfoWindow.Anchor = e.MapPoint;
                                    popupContent.ContentTemplate = dt;
                                    popupContent.Content = g.Attributes;
                                    MyInfoWindow.IsOpen = true;

                                    SolidColorBrush symbolColor = new SolidColorBrush(Colors.Cyan);

                                    if (g.Geometry is ESRI.ArcGIS.Client.Geometry.Polygon || g.Geometry is Envelope)
                                    {
                                        g.Symbol = new SimpleFillSymbol()
                                        {
                                            BorderBrush = symbolColor,
                                            BorderThickness = 2
                                        };
                                    }
                                    else if (g.Geometry is ESRI.ArcGIS.Client.Geometry.Polyline)
                                    {
                                        g.Symbol = new SimpleLineSymbol()
                                        {
                                            Color = symbolColor
                                        };
                                    }
                                    else // Point
                                    {
                                        g.Symbol = new SimpleMarkerSymbol()
                                        {
                                            Color = symbolColor,
                                            Size = 12
                                        };
                                    }
                                    glayer.Graphics.Add(g);
                                }
                            };

                            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
                            {
                                Geometry = e.MapPoint,
                                OutSpatialReference = MyMap.SpatialReference,
                                ReturnGeometry = true
                            };
                            query.OutFields.Add("*");

                            qt.ExecuteAsync(query);
                        }
                    }
                }
            }
        }

        // Highlight piece of chart
        private void Highlight(UIElement elt)
        {
            if (elt is Shape) // PieChart, LineChart
            {
                origColor = (elt as Shape).Fill;
                (elt as Shape).Fill = highlightColor;
            }
            else if (elt is Border) // BarChart, ColumnChart
            {
                origColor = (elt as Border).Background;
                (elt as Border).Background = highlightColor;
            }
            highlightedChartPiece = elt;
        }
        private void Unhighlight(UIElement elt)
        {
            if (elt is Shape) // PieChart, LineChart
            {
                (elt as Shape).Fill = origColor;
            }
            else if (elt is Border) // BarChart, ColumnChart
            {
                (elt as Border).Background = origColor;
            }
            highlightedChartPiece = null;
            origColor = null;
            chartDetailsContent.Content = null;
        }
        private void MyInfoWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Display tooltip of the clicked piece of chart

            // reset previous result
            if (highlightedChartPiece != null)
                Unhighlight(highlightedChartPiece);

            // get clicked element having tooltip
            var point = e.GetPosition(null);
            var element = VisualTreeHelper.FindElementsInHostCoordinates(point, popupContent).FirstOrDefault(elt => elt.GetValue(ToolTipService.ToolTipProperty) != null);

            if (element == null)
            {
                // Add a tolerance since the piece of chart can be small
                const double tolerance = 40;
                Rect rect = new Rect(point.X - tolerance / 2, point.Y - tolerance / 2, tolerance, tolerance);
                element = VisualTreeHelper.FindElementsInHostCoordinates(rect, popupContent).FirstOrDefault(elt => elt.GetValue(ToolTipService.ToolTipProperty) != null);
            }

            if (element != null)
            {
                // Display tooltip in chartDetails
                chartDetailsContent.Content = element.GetValue(ToolTipService.ToolTipProperty);
                Highlight(element);
            }
        }
    }
}