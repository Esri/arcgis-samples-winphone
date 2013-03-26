using System;
using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class ConvexHull : PhoneApplicationPage
    {
        private Draw MyDrawObject;
        private GraphicsLayer outputGraphicsLayer;
        private GraphicsLayer inputGraphicsLayer;

        public ConvexHull()
        {
            InitializeComponent();

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Point,
                IsEnabled = false
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;

            outputGraphicsLayer = MyMap.Layers["ConvexHullGraphicsLayer"] as GraphicsLayer;
            inputGraphicsLayer = MyMap.Layers["InputGraphicsLayer"] as GraphicsLayer;
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            outputGraphicsLayer.ClearGraphics();

            ESRI.ArcGIS.Client.Geometry.MapPoint point = args.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint;
            point.SpatialReference = MyMap.SpatialReference;
            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = point
            };

            inputGraphicsLayer.Graphics.Add(graphic);

            if (inputGraphicsLayer.Graphics.Count >= 3)
                ConvexButton.IsEnabled = true;
        }

        private void ConvexButton_Click(object sender, RoutedEventArgs e)
        {
            ConvexButton.IsEnabled = false;
            outputGraphicsLayer.ClearGraphics();

            GeometryService geometryService =
                        new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.ConvexHullCompleted += GeometryService_ConvexHullCompleted;
            geometryService.Failed += GeometryService_Failed;
            
            List<Graphic> graphicsList = new List<Graphic>();
            foreach (Graphic g in inputGraphicsLayer.Graphics)
            {
                graphicsList.Add(g);
            }
            geometryService.ConvexHullAsync(graphicsList);
        }

        void GeometryService_ConvexHullCompleted(object sender, GeometryEventArgs e)
        {
            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = e.Result
            };
            outputGraphicsLayer.Graphics.Add(graphic);

            ConvexButton.IsEnabled = true;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }

        private void DrawPointButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Point;
            MyDrawObject.IsEnabled = true;
        }

 
    }
}