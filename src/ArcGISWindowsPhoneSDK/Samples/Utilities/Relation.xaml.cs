using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Relation : PhoneApplicationPage
    {
        private Draw MyDrawObject;
        private GraphicsLayer polygonLayer;
        private GraphicsLayer pointLayer;
        private GeometryService geometryService;

        public Relation()
        {
            InitializeComponent();

            polygonLayer = MyMap.Layers["MyPolygonGraphicsLayer"] as GraphicsLayer;
            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
            pointLayer = MyMap.Layers["MyPointGraphicsLayer"] as GraphicsLayer;

            MyDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Point,
                IsEnabled = false,
            };
            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            for (int i = 0; i < polygonLayer.Graphics.Count; i++)
            {
                Graphic graphic = polygonLayer.Graphics[i];
                graphic.Geometry.SpatialReference = MyMap.SpatialReference;
                if (!graphic.Attributes.ContainsKey("Name"))
                {
                    graphic.Attributes.Add("Name", String.Format("Polygon_{0}", i));
                    graphic.Attributes.Add("Relation", null);
                }
            }
        }

        private void MyDrawObject_DrawComplete(object sender, DrawEventArgs args)
        {
            MyDrawObject.IsEnabled = false;

            string name = String.Format("Point_{0}", pointLayer.Graphics.Count);

            MapPoint mapPoint = args.Geometry as MapPoint;

            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultPointMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol,
                Geometry = mapPoint
            };

            graphic.Attributes.Add("Name", name);
            graphic.Attributes.Add("Relation", null);

            pointLayer.Graphics.Add(graphic);
        }

        private void ExecuteRelationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MyDrawObject.IsEnabled = false;
            ExecuteRelationButton.Visibility = Visibility.Collapsed;

            geometryService = new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.RelationCompleted += GeometryService_RelationCompleted;
            geometryService.SimplifyCompleted += GeometryService_SimplifyCompleted;
            geometryService.Failed += GeometryService_Failed;

            if (pointLayer.Graphics.Count < 1)
            {
                MessageBox.Show("No points available");
                ExecuteRelationButton.Visibility = Visibility.Visible;
                return;
            }

            foreach (Graphic graphic in pointLayer.Graphics)
                graphic.Attributes["Relation"] = null;

            foreach (Graphic graphic in polygonLayer.Graphics)
                graphic.Attributes["Relation"] = null;

            // Call simplify operation to correct orientation of rings in a polygon (clockwise = ring, counterclockwise = hole)
            geometryService.SimplifyAsync(polygonLayer.Graphics);
        }

        void GeometryService_SimplifyCompleted(object sender, GraphicsEventArgs e)
        {
            geometryService.RelationAsync(
              pointLayer.Graphics,
              e.Results,
              GeometryRelation.esriGeometryRelationWithin, null);
        }

        private void GeometryService_RelationCompleted(object sender, RelationEventArgs args)
        {
            List<GeometryRelationPair> results = args.Results;
            foreach (GeometryRelationPair pair in results)
            {
                if (pointLayer.Graphics[pair.Graphic1Index].Attributes["Relation"] == null)
                {
                    pointLayer.Graphics[pair.Graphic1Index].Attributes["Relation"] =
                    string.Format("Within Polygon {0}", pair.Graphic2Index);
                }
                else
                {
                    pointLayer.Graphics[pair.Graphic1Index].Attributes["Relation"] +=
                    "," + pair.Graphic2Index.ToString();
                }

                if (polygonLayer.Graphics[pair.Graphic2Index].Attributes["Relation"] == null)
                {
                    polygonLayer.Graphics[pair.Graphic2Index].Attributes["Relation"] =
                    string.Format("Contains Point {0}", pair.Graphic1Index);
                }
                else
                {
                    polygonLayer.Graphics[pair.Graphic2Index].Attributes["Relation"] +=
                    "," + pair.Graphic1Index.ToString();
                }
            }

            ExecuteRelationButton.Visibility = Visibility.Visible;
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Geometry Service error: " + args.Error);
        }

        private void MyMap_MapGesture(object sender, ESRI.ArcGIS.Client.Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap && !MyDrawObject.IsEnabled)
            {
                MyInfoWindow.IsOpen = false;

                GraphicsLayer pointsGraphicsLayer = MyMap.Layers["MyPointGraphicsLayer"] as GraphicsLayer;
                GraphicsLayer polygonsGraphicsLayer = MyMap.Layers["MyPolygonGraphicsLayer"] as GraphicsLayer;
                IEnumerable<Graphic> selected = e.DirectlyOver(10, new GraphicsLayer[] { pointsGraphicsLayer, polygonsGraphicsLayer });
                foreach (Graphic g in selected)
                {
                    MyInfoWindow.Anchor = e.MapPoint;
                    MyInfoWindow.IsOpen = true;
                    MyInfoWindow.Content = g;
                    return;
                }
            }
        }

        private void AddPoint_Clicked(object sender, EventArgs e)
        {
            MyDrawObject.IsEnabled = true;
            MyInfoWindow.IsOpen = false;
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            InfoGrid.Visibility = InfoGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}