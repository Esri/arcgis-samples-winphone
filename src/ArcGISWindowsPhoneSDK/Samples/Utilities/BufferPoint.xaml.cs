using System;
using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class BufferPoint : PhoneApplicationPage
    {

        private Draw myDrawObject;
        
        public BufferPoint()
        {
            InitializeComponent();
            myDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Point,
                IsEnabled = false
            };

            myDrawObject.DrawComplete += myDrawObject_DrawComplete;
        }

        void myDrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            myDrawObject.IsEnabled = false;
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = e.Geometry,
                Symbol = LayoutRoot.Resources["DefaultClickSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol
            };
            graphic.SetZIndex(1);
            graphicsLayer.Graphics.Add(graphic);

            GeometryService geometryService =
              new GeometryService("http://serverapps101.esri.com/arcgis/rest/services/Geometry/GeometryServer");
            geometryService.BufferCompleted += GeometryService_BufferCompleted;
            geometryService.Failed += GeometryService_Failed;

            BufferParameters bufferParams = new BufferParameters()
            {
                Unit = LinearUnit.StatuteMile,
                OutSpatialReference = MyMap.SpatialReference,
                Geodesic = true
            };
            bufferParams.Features.Add(graphic);
            bufferParams.Distances.Add(5);

            geometryService.BufferAsync(bufferParams);
        }

        void GeometryService_BufferCompleted(object sender, GraphicsEventArgs args)
        {
            IList<Graphic> results = args.Results;
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            foreach (Graphic graphic in results)
            {
                graphic.Symbol = LayoutRoot.Resources["DefaultBufferSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                graphicsLayer.Graphics.Add(graphic);
            }
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }

        private void DrawPointButton_Click(object sender, EventArgs e)
        {
            myDrawObject.DrawMode = DrawMode.Point;
            myDrawObject.IsEnabled = true;
        }

 
    }
}