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
    public partial class Densify : PhoneApplicationPage
    {
        private Draw MyDrawObject;

        public Densify()
        {
            InitializeComponent();
            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Polygon,
                IsEnabled = false,
                FillSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            MyDrawObject.DrawBegin += MyDrawObject_DrawBegin;
        }

        private void MyDrawObject_DrawBegin(object sender, EventArgs args)
        {
            GraphicsLayer graphicsLayerPolygon = MyMap.Layers["PolygonGraphicsLayer"] as GraphicsLayer;
            graphicsLayerPolygon.ClearGraphics();
            GraphicsLayer graphicsLayerVertices = MyMap.Layers["VerticesGraphicsLayer"] as GraphicsLayer;
            graphicsLayerVertices.ClearGraphics();
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = true;

            ESRI.ArcGIS.Client.Geometry.Polygon polygon = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
            polygon.SpatialReference = MyMap.SpatialReference;
            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = polygon
            };

            GraphicsLayer graphicsLayerPolygon = MyMap.Layers["PolygonGraphicsLayer"] as GraphicsLayer;
            graphicsLayerPolygon.Graphics.Add(graphic);

            GraphicsLayer graphicsLayerVertices = MyMap.Layers["VerticesGraphicsLayer"] as GraphicsLayer;
            foreach (MapPoint point in polygon.Rings[0])
            {
                Graphic vertice = new Graphic()
                {
                    Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                    Geometry = point
                };
                graphicsLayerVertices.Graphics.Add(vertice);
            }
            DensifyButton.IsEnabled = true;
        }

        private void DensifyButton_Click(object sender, RoutedEventArgs e)
        {
            DensifyButton.IsEnabled = false;

            GraphicsLayer graphicsLayerPolygon = MyMap.Layers["PolygonGraphicsLayer"] as GraphicsLayer;

            GeometryService geometryService =
                        new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.DensifyCompleted += GeometryService_DensifyCompleted;
            geometryService.Failed += GeometryService_Failed;

            DensifyParameters densityParameters = new DensifyParameters()
            {
                LengthUnit = LinearUnit.SurveyMile,
                Geodesic = true,
                MaxSegmentLength = MyMap.Resolution * 1000
            };

            geometryService.DensifyAsync(graphicsLayerPolygon.Graphics.ToList(), densityParameters);
        }

        void GeometryService_DensifyCompleted(object sender, GraphicsEventArgs e)
        {
            GraphicsLayer graphicsLayerVertices = MyMap.Layers["VerticesGraphicsLayer"] as GraphicsLayer;
            foreach (Graphic g in e.Results)
            {
                ESRI.ArcGIS.Client.Geometry.Polygon p = g.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;

                foreach (ESRI.ArcGIS.Client.Geometry.PointCollection pc in p.Rings)
                {
                    foreach (MapPoint point in pc)
                    {
                        Graphic vertice = new Graphic()
                        {
                            Symbol = LayoutRoot.Resources["NewMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                            Geometry = point
                        };
                        graphicsLayerVertices.Graphics.Add(vertice);
                    }
                }
            }
            DensifyButton.IsEnabled = true;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }

        private void DrawPolygonButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.IsEnabled = true;
        }
    }
}
