using System;
using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class LabelPoints : PhoneApplicationPage
    {

        private Draw MyDrawObject;
        private GeometryService geometryService;
        private GraphicsLayer graphicsLayer;

        public LabelPoints()
        {
            InitializeComponent();

            MyDrawObject = new Draw(MyMap)
            {
                FillSymbol = LayoutRoot.Resources["DefaultPolygonSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol,
                DrawMode = DrawMode.Polygon,
                IsEnabled = false
            };

            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;

            geometryService = new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.SimplifyCompleted += GeometryService_SimplifyCompleted;
            geometryService.LabelPointsCompleted += GeometryService_LabelPointsCompleted;
            geometryService.Failed += GeometryService_Failed;

            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
        }

        private void ClearGraphicsButton_Click(object sender, RoutedEventArgs e)
        {            
            graphicsLayer.ClearGraphics();
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = false;

            ESRI.ArcGIS.Client.Geometry.Polygon polygon = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
            polygon.SpatialReference = new SpatialReference(4326);
            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultPolygonSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = polygon
            };
            graphic.Attributes.Add("X", "Label Point Polygon");
            graphicsLayer.Graphics.Add(graphic);

            List<Graphic> graphicsList = new List<Graphic>();
            graphicsList.Add(graphic);

            geometryService.SimplifyAsync(graphicsList);
        }

        void GeometryService_SimplifyCompleted(object sender, GraphicsEventArgs e)
        {
            geometryService.LabelPointsAsync(e.Results);
        }

        private void GeometryService_LabelPointsCompleted(object sender, GraphicsEventArgs args)
        {
            foreach (Graphic graphic in args.Results)
            {
                graphic.Symbol = LayoutRoot.Resources["DefaultRasterSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                MapPoint mapPoint = graphic.Geometry as MapPoint;
                graphic.Attributes.Add("X", mapPoint.X);
                graphic.Attributes.Add("Y", mapPoint.Y);
                graphicsLayer.Graphics.Add(graphic);
            }            
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }
  
        private void DrawPolygonButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Polygon;
            MyDrawObject.IsEnabled = true;
        }

 
    }
}