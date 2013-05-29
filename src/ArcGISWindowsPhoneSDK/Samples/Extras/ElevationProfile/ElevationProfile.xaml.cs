using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;
using System.IO;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Geometry;
using GraphSample.CustomControls;
using GraphSample;
using System.ComponentModel;
using Microsoft.Phone.Shell;

namespace ElevationProfile
{
    public partial class MainPage : PhoneApplicationPage
    {

        private Draw _drawObject;
        private Symbol _activeSymbol = null;
        private const string soeName = "ElevationsSOE_NET";
        private Uri mapServiceUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer/");
        private int resourceIndex = 0;
        private delegate void OperationResultHandler(string jsonResponse);
        private List<IGraphData> graphData = new List<IGraphData>();
        private ESRI.ArcGIS.Client.Geometry.Polyline _polyline = null;
        private double _length;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            _drawObject = new Draw(MyMap)
            {
                LineSymbol = LayoutRoot.Resources["DrawLineSymbol"] as LineSymbol,
                //FillSymbol = LayoutRoot.Resources["DrawFillSymbol"] as FillSymbol
            };

            _drawObject.DrawBegin += MyDrawObject_DrawBegin;
            _drawObject.DrawComplete += MyDrawObject_DrawComplete;
            _drawObject.DrawMode = DrawMode.Polyline;
            _activeSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as Symbol;
            _drawObject.IsEnabled = true;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false; // clear
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false; // show graph
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false; // back
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            GraphPage.IsOpen = false;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false; // clear
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false; // show graph
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false; // back

            if (_polyline != null)
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true; // clear
                ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true; // show graph
            }
        }


        private void DrawChartButton_Click(object sender, EventArgs e)
        {
            GraphPage.IsOpen = true;

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false; // clear
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false; // show graph
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true; // back


            if (_polyline != null)
             GetElevations(_polyline);
        }

        private void MyDrawObject_DrawBegin(object sender, EventArgs args)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = args.Geometry,
                Symbol = _activeSymbol,
            };
            graphicsLayer.Graphics.Add(graphic);

            if (args.Geometry is ESRI.ArcGIS.Client.Geometry.Polyline)
                _polyline = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;
            
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true; // clear
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true; // show graph
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer; 
            graphicsLayer.ClearGraphics();
            _polyline = null;

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false; // clear
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false; // show graph
        }

        public void GetElevations(ESRI.ArcGIS.Client.Geometry.Polyline p)
        {
            string jsonGeometries = SerializePolyline(p);
            string opParams = string.Format(System.Globalization.CultureInfo.InvariantCulture, "geometries={0}&f=json", jsonGeometries);
            this.invokeOperation("GetElevations", opParams, this._GetElevationsHandler);
        }

        private string SerializePolyline(ESRI.ArcGIS.Client.Geometry.Polyline p)
        {

            _length = ESRI.ArcGIS.Client.Geometry.Euclidian.Length(p) * 0.000621371192;

            string s = @"[{""spatialreference"":{""wkid"":";
            if (p.SpatialReference != null)
                s = s + p.SpatialReference.WKID + "},";

            s = s + "\"hasZ\":" + p.HasZ +",";
            s = s + "\"hasM\":" + p.HasM + ",";
            s = s + "\"paths\":[[";

            for (int i=0; i < p.Paths.Count; i++ )
            {
                ESRI.ArcGIS.Client.Geometry.PointCollection path = p.Paths[i];
                foreach (MapPoint c in path)
                    s = s + "[" + c.X + "," + c.Y + "],";
                  
            }
            s = s + "]]}]";

            return s;
        }

       private void _GetElevationsHandler(string s)
        {
            int first = s.IndexOf("[[");
            int last = s.LastIndexOf("]]");
            if ((first < 0) || (last < 0))
                return;
            string path = s.Substring(first + 2, last - first -2);
            string[] coords = path.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
           
            graphData.Clear();

            int currentIndex = 0;
            foreach (string coord in coords)
            {
           
                if (coord != ",")
                {
                    string[] ords = coord.Split(',');
                    double x = Convert.ToDouble(ords[0]);
                    double y = Convert.ToDouble(ords[1]);
                    double z = Convert.ToDouble(ords[2]);
                    currentIndex++;
                    graphData.Add(new BasicGraphData((int)(currentIndex * _length/100.0), (float)z));
                }
            }

            graph1.LineGraph =false;
            graph1.GraphData = null;
            graph1.GraphData = graphData;
            graph1.ChartTitle = "Elevation Profile";
        }

        private void invokeOperation(string operationName, string operationParameters, OperationResultHandler operationHandler)
        {
            Uri requestUri = new Uri(this.mapServiceUri,
                                     string.Format("exts/{0}/ElevationLayers/{1}/{2}?{3}",
                                     soeName,
                                     this.resourceIndex,
                                     operationName,
                                     operationParameters));

            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += (s, a) =>
            {
                StreamReader sr = new StreamReader(a.Result);
                string jsonResponse = sr.ReadToEnd();
                sr.Close();

                operationHandler(jsonResponse);
            };


            webClient.OpenReadAsync(requestUri);
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            double Height = graph1.ActualHeight;
            double Width = graph1.ActualWidth;

            if (GraphPage.IsOpen == true)
            {
                if (e.Orientation == PageOrientation.Landscape ||
                    e.Orientation == PageOrientation.LandscapeLeft ||
                    e.Orientation == PageOrientation.LandscapeRight)
                {
                    graph1.Width = Math.Max(Height, Width);
                    graph1.Height = Math.Min(Height, Width);
                }
                else
                {
                    graph1.Width = Math.Min(Height, Width);
                    graph1.Height = Math.Max(Height, Width);
                }
                base.OnOrientationChanged(e);
            }
        }


    }
}