using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class SpatialQuery : PhoneApplicationPage
    {
        private Draw MyDrawObject;
        GraphicsLayer selectionGraphicsLayer;

        public SpatialQuery()
        {
            InitializeComponent();

            selectionGraphicsLayer = MyMap.Layers["MySelectionGraphicsLayer"] as GraphicsLayer;

            MyDrawObject = new Draw(MyMap)
            {
                LineSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as SimpleLineSymbol,
                FillSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as FillSymbol
            };
            MyDrawObject.DrawComplete += MyDrawSurface_DrawComplete;
        }

        private void DataListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Graphic g in e.AddedItems)
                g.Select();

            foreach (Graphic g in e.RemovedItems)
                g.UnSelect();
        }

        private void MyDrawSurface_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            selectionGraphicsLayer.ClearGraphics();

            QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.Failed += QueryTask_Failed;

            Query query = new ESRI.ArcGIS.Client.Tasks.Query()
            {
                Geometry = args.Geometry,
                OutSpatialReference = MyMap.SpatialReference,
                ReturnGeometry = true
            };

            // Specify fields to return from query
            query.OutFields.AddRange(new string[] { "STATE_NAME", "POP2000", "POP2007" });

            queryTask.ExecuteAsync(query);
        }

        private void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;
            if (featureSet == null || featureSet.Features.Count < 1)
            {
                MessageBox.Show("No features returned from query");
                return;
            }

            if (featureSet != null && featureSet.Features.Count > 0)
            {
                foreach (Graphic feature in featureSet.Features)
                {
                    feature.Symbol = LayoutRoot.Resources["ResultsFillSymbol"] as FillSymbol;
                    selectionGraphicsLayer.Graphics.Insert(0, feature);
                }
            }

            DataListBox.ItemsSource = featureSet;
            MyDrawObject.IsEnabled = false;
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query failed: " + args.Error);
        }

        private void DrawPointButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Point;
            MyDrawObject.IsEnabled = true;
        }

        private void DrawPolylineButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Polyline;
            MyDrawObject.IsEnabled = true;
        }

        private void DrawPolygonButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Polygon;
            MyDrawObject.IsEnabled = true;
        }

        private void DrawRectangleButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Rectangle;
            MyDrawObject.IsEnabled = true;
        }


        private void ShapeChoicesLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ListBoxItem selectedShapeLBI = e.AddedItems[0] as ListBoxItem;

                if (selectedShapeLBI.Name.Equals("RectLBI"))
                    MyDrawObject.DrawMode = DrawMode.Rectangle;
                else if (selectedShapeLBI.Name.Equals("ArrowLBI"))
                    MyDrawObject.DrawMode = DrawMode.Arrow;
                else if (selectedShapeLBI.Name.Equals("TriLBI"))
                    MyDrawObject.DrawMode = DrawMode.Triangle;
                else if (selectedShapeLBI.Name.Equals("CircleLBI"))
                    MyDrawObject.DrawMode = DrawMode.Circle;
                else if (selectedShapeLBI.Name.Equals("EllipLBI"))
                    MyDrawObject.DrawMode = DrawMode.Ellipse;
                else
                    return;

                UseOtherShapeChoice.Visibility = System.Windows.Visibility.Collapsed;

                MyDrawObject.IsEnabled = true;
            }
        }

        private void UseOtherMenuItem_Click(object sender, EventArgs e)
        {
            UseOtherShapeChoice.Visibility = System.Windows.Visibility.Visible;
        }

        private void ClearGraphicsMenuItem_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.None;
            GraphicsLayer graphicsLayer = MyMap.Layers["MySelectionGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();
            DataListBox.ItemsSource = null;
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            DataListBox.Focus();
            DataListBox.SelectedItem = e.Graphic;
            DataListBox.ScrollIntoView(DataListBox.SelectedItem);
        }
    }
}