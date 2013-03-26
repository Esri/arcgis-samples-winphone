using System;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class DrawGraphics : PhoneApplicationPage
    {
        private Draw MyDrawObject;
        private Symbol _activeSymbol = null;
        
        public DrawGraphics()
        {
            InitializeComponent();

            MyDrawObject = new Draw(MyMap)
            {
                LineSymbol = LayoutRoot.Resources["DrawLineSymbol"] as LineSymbol,
                FillSymbol = LayoutRoot.Resources["DrawFillSymbol"] as FillSymbol
            };

            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
        }

        private void DrawPointButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Point;
            _activeSymbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as Symbol;
            MyDrawObject.IsEnabled = true;
        }

        private void DrawPolylineButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Polyline;
            _activeSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as Symbol;
            MyDrawObject.IsEnabled = true;
        }

        private void DrawPolygonButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Polygon;
            _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
            MyDrawObject.IsEnabled = true;
        }

        private void DrawRectangleButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Rectangle;
            _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
            MyDrawObject.IsEnabled = true;
        }

        private void DrawFreehandButton_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Freehand;
            _activeSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as Symbol;
            MyDrawObject.IsEnabled = true;
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = args.Geometry,
                Symbol = _activeSymbol,
            };
            graphicsLayer.Graphics.Add(graphic);
        }

        private void ClearGraphicsMenuItem_Click(object sender, EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.None;
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();
        }

        private void StopGraphicsMenuItem_Click(object sender, EventArgs e)
        {
            MyDrawObject.IsEnabled = false;
        }

        private void DrawOtherMenuItem_Click(object sender, EventArgs e)
        {
            DrawOtherShapeChoice.Visibility = System.Windows.Visibility.Visible;
        }

        private void DrawButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DrawOtherShapeChoice.Visibility = System.Windows.Visibility.Collapsed;

            if (LineSegRB.IsChecked == true)
            {
                MyDrawObject.DrawMode = DrawMode.LineSegment;
                _activeSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as Symbol;
                MyDrawObject.IsEnabled = true;
                return;
            }
            
            if (RectRB.IsChecked == true)
                MyDrawObject.DrawMode = DrawMode.Rectangle;
            else if (ArrowRB.IsChecked == true)
                MyDrawObject.DrawMode = DrawMode.Arrow;
            else if (TriRB.IsChecked == true)
                MyDrawObject.DrawMode = DrawMode.Triangle;
            else if (CircleRB.IsChecked == true)
                MyDrawObject.DrawMode = DrawMode.Circle;
            else if (EllipRB.IsChecked == true)
                MyDrawObject.DrawMode = DrawMode.Ellipse;
            else
                return;

            _activeSymbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol;
            MyDrawObject.IsEnabled = true;
        }

        private void GraphicsLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            if (EnableEditVerticesScaleRotate.IsChecked.Value)
            {
                MyDrawObject.DrawMode = DrawMode.None;
                Editor editor = LayoutRoot.Resources["MyEditor"] as Editor;
                if (e.Graphic != null && !(e.Graphic.Geometry is ESRI.ArcGIS.Client.Geometry.MapPoint))
                    editor.EditVertices.Execute(e.Graphic);
            }
        }
    
    }
}