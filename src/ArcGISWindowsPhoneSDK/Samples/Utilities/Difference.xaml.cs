using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Difference : PhoneApplicationPage
    {
        private Draw MyDrawObject;
        GeometryService geometryService;
        GraphicsLayer inputGraphicsLayer;
        GraphicsLayer outputGraphicsLayer;

        public Difference()
        {
            InitializeComponent();

            geometryService =
                        new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");

            geometryService.DifferenceCompleted += GeometryService_DifferenceCompleted;
            geometryService.SimplifyCompleted += GeometryService_SimplifyCompleted;
            geometryService.Failed += GeometryService_Failed;

            inputGraphicsLayer = MyMap.Layers["InputGraphicsLayer"] as GraphicsLayer;

            MyDrawObject = new Draw(MyMap)
                {
                    DrawMode = DrawMode.Polygon,
                    IsEnabled = false,
                    FillSymbol = LayoutRoot.Resources["DrawFillSymbol"] as ESRI.ArcGIS.Client.Symbols.FillSymbol
                };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = false;

            ESRI.ArcGIS.Client.Geometry.Polygon polygon = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
            polygon.SpatialReference = MyMap.SpatialReference;

            geometryService.SimplifyAsync(new List<Graphic>() { new Graphic() { Geometry = polygon } });
        }

        void GeometryService_SimplifyCompleted(object sender, GraphicsEventArgs e)
        {
            geometryService.DifferenceAsync(inputGraphicsLayer.Graphics.ToList(), e.Results[0].Geometry);
        }

        void GeometryService_DifferenceCompleted(object sender, GraphicsEventArgs e)
        {
            outputGraphicsLayer = MyMap.Layers["OutputGraphicsLayer"] as GraphicsLayer;
            outputGraphicsLayer.ClearGraphics();
            foreach (Graphic g in e.Results)
            {
                if (g.Geometry is ESRI.ArcGIS.Client.Geometry.Polygon)
                    g.Symbol = LayoutRoot.Resources["DifferenceFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                outputGraphicsLayer.Graphics.Add(g);
            }

            MyDrawObject.IsEnabled = true;
            ResetButton.IsEnabled = true;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
            MyDrawObject.IsEnabled = true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            GraphicsLayer outputGraphicsLayer = MyMap.Layers["OutputGraphicsLayer"] as GraphicsLayer;
            outputGraphicsLayer.ClearGraphics();

            ResetButton.IsEnabled = false;
        }

        private void DrawPolygonButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.IsEnabled = true;
        }
        
        private void Menu_List_Click(object sender, EventArgs e)
        {
            Instructions.Visibility = Instructions.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}