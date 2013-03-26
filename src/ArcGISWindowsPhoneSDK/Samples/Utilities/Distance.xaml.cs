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
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Distance : PhoneApplicationPage
    {
        private Draw MyDrawObject;
        GraphicsLayer graphicsLayer;
        GraphicsLayer lineLayer;

        public Distance()
        {
            InitializeComponent();

            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            lineLayer = MyMap.Layers["LineGraphicsLayer"] as GraphicsLayer;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Point,
                IsEnabled = false
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            MyDrawObject.DrawBegin += MyDrawObject_DrawBegin;
        }

        private void MyDrawObject_DrawBegin(object sender, EventArgs args)
        {
            if (graphicsLayer.Graphics.Count >= 2)
            {
                graphicsLayer.ClearGraphics();
                lineLayer.ClearGraphics();
            }
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            //ESRI.ArcGIS.Client.Geometry.Polyline polyline = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;
            args.Geometry.SpatialReference = MyMap.SpatialReference;
            Graphic graphic = new Graphic() { Geometry = args.Geometry };
            graphic.Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;

            graphicsLayer.Graphics.Add(graphic);

            if (graphicsLayer.Graphics.Count == 2)
            {
                MyDrawObject.IsEnabled = false;
                GeometryService geometryService =
                            new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
                geometryService.DistanceCompleted += GeometryService_DistanceCompleted;
                geometryService.Failed += GeometryService_Failed;

                DistanceParameters distanceParameters = new DistanceParameters()
                {
                    DistanceUnit = LinearUnit.SurveyMile,
                    Geodesic = true
                };

                geometryService.DistanceAsync(graphicsLayer.Graphics[0].Geometry, graphicsLayer.Graphics[1].Geometry, distanceParameters);
                ResponseTextBlock.Text = "The distance between the points is... ";

                ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                points.Add(graphicsLayer.Graphics[0].Geometry as MapPoint);
                points.Add(graphicsLayer.Graphics[1].Geometry as MapPoint);
                ESRI.ArcGIS.Client.Geometry.Polyline polyline = new ESRI.ArcGIS.Client.Geometry.Polyline();
                polyline.Paths.Add(points);
                Graphic linegraphic = new Graphic()
                {
                    Geometry = polyline,
                    Symbol = LayoutRoot.Resources["DefaultLineSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol
                };
                lineLayer.Graphics.Add(linegraphic);

            }
            MyDrawObject.IsEnabled = true;
        }

        void GeometryService_DistanceCompleted(object sender, DistanceEventArgs e)
        {
            ResponseTextBlock.Text =
                String.Format("The distance between the points is {0} miles", Math.Round(e.Distance, 3));
            MyDrawObject.IsEnabled = false;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
            MyDrawObject.IsEnabled = true;
        }

        private void DrawPointButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Point;
            MyDrawObject.IsEnabled = true;
        }

    }
}