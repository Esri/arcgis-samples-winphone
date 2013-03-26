using System;
using System.Linq;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Lengths : PhoneApplicationPage
    {
        private Draw MyDrawObject;

        public Lengths()
        {
            InitializeComponent();
            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Polyline,
                LineSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as ESRI.ArcGIS.Client.Symbols.LineSymbol,
                IsEnabled = false
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
            MyDrawObject.DrawBegin += MyDrawObject_DrawBegin;
        }

        private void MyDrawObject_DrawBegin(object sender, EventArgs args)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = false;
            ESRI.ArcGIS.Client.Geometry.Polyline polyline = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;
            polyline.SpatialReference = MyMap.SpatialReference;
            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultLineSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = polyline
            };

            GeometryService geometryService =
                            new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.LengthsCompleted += GeometryService_LengthsCompleted;
            geometryService.Failed += GeometryService_Failed;

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.Graphics.Add(graphic);
            geometryService.LengthsAsync(graphicsLayer.Graphics.ToList(), LinearUnit.SurveyMile, CalculationType.Geodesic, null);
        }

        private void GeometryService_LengthsCompleted(object sender, ESRI.ArcGIS.Client.Tasks.LengthsEventArgs args)
        {
            ResponseTextBlock.Text =
                String.Format("The distance of the polyline is {0} miles", Math.Round(args.Results[0], 3));
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }

        private void DrawPolylineButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.IsEnabled = true;
        }
    }
}