using System;
using System.Linq;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;
using System.Collections.Generic;

namespace ArcGISWindowsPhoneSDK
{
    public partial class TrimExtend : PhoneApplicationPage
    {
        private Draw MyDrawObject;
        private GraphicsLayer polylineLayer;
        private GraphicsLayer resultsLayer;
        private GeometryService geometryService;

        public TrimExtend()
        {
            InitializeComponent();

            polylineLayer = MyMap.Layers["MyPolylineGraphicsLayer"] as GraphicsLayer;
            resultsLayer = MyMap.Layers["MyResultsGraphicsLayer"] as GraphicsLayer;
            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Polyline,
                IsEnabled = false,
                LineSymbol = LayoutRoot.Resources["DrawLineSymbol"] as ESRI.ArcGIS.Client.Symbols.LineSymbol
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            foreach (Graphic g in polylineLayer.Graphics)
                g.Geometry.SpatialReference = MyMap.SpatialReference;
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = false;

            resultsLayer.ClearGraphics();

            Polyline polyline = args.Geometry as Polyline;
            polyline.SpatialReference = MyMap.SpatialReference;

            geometryService =
            new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.TrimExtendCompleted += GeometryService_TrimExtendCompleted;
            geometryService.Failed += GeometryService_Failed;

            List<Polyline> polylineList = new List<Polyline>();
            foreach (Graphic g in polylineLayer.Graphics)
                polylineList.Add(g.Geometry as Polyline);

            geometryService.TrimExtendAsync(polylineList, polyline, CurveExtension.DefaultCurveExtension);
        }

        void GeometryService_TrimExtendCompleted(object sender, GraphicsEventArgs e)
        {
            foreach (Graphic g in e.Results)
            {
                g.Symbol = LayoutRoot.Resources["ResultsLineSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                resultsLayer.Graphics.Add(g);
            }
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Geometry Service error: " + args.Error);
        }

        private void DrawPolylineButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.IsEnabled = true;
        }
    }
}