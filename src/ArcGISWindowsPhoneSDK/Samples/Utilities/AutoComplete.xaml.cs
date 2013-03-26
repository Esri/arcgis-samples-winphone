﻿using System;
using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class AutoComplete : PhoneApplicationPage
    {
        private Draw MyDrawObject;

        public AutoComplete()
        {
            InitializeComponent();

            MyMap.MinimumResolution = double.Epsilon;

            QueryTask queryTask =
                new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1");
            Query query = new Query();
            query.Geometry = MyMap.Extent;
            query.ReturnGeometry = true;
            queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;
            queryTask.Failed += queryTask_Failed;
            queryTask.ExecuteAsync(query);

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Polyline,
                IsEnabled = false,
                LineSymbol = LayoutRoot.Resources["RedLineSymbol"] as ESRI.ArcGIS.Client.Symbols.LineSymbol
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
        }

        void queryTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Query error: " + e.Error);
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            GraphicsLayer parcelGraphicsLayer = MyMap.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
            foreach (Graphic g in e.FeatureSet.Features)
            {
                g.Symbol = LayoutRoot.Resources["BlueFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                parcelGraphicsLayer.Graphics.Add(g);
            }
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = false;
            ESRI.ArcGIS.Client.Geometry.Polyline polyline = args.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;
            polyline.SpatialReference = MyMap.SpatialReference;

            Graphic polylineGraphic = new Graphic()
            {
                Geometry = polyline
            };
            List<Graphic> polylineList = new List<Graphic>();
            polylineList.Add(polylineGraphic);

            GeometryService geometryService =
              new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.AutoCompleteCompleted += GeometryService_AutoCompleteCompleted;
            geometryService.Failed += GeometryService_Failed;

            GraphicsLayer graphicsLayer = MyMap.Layers["ParcelsGraphicsLayer"] as GraphicsLayer;
            List<Graphic> polygonList = new List<Graphic>();
            foreach (Graphic g in graphicsLayer.Graphics)
            {
                g.Geometry.SpatialReference = MyMap.SpatialReference;
                polygonList.Add(g);
            }

            geometryService.AutoCompleteAsync(polygonList, polylineList);
        }

        void GeometryService_AutoCompleteCompleted(object sender, GraphicsEventArgs e)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["CompletedGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            foreach (Graphic g in e.Results)
            {
                g.Symbol = LayoutRoot.Resources["RedFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                graphicsLayer.Graphics.Add(g);
            }

            if (e.Results.Count > 0)
                (MyMap.Layers["ConnectDotsGraphicsLayer"] as GraphicsLayer).ClearGraphics();

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