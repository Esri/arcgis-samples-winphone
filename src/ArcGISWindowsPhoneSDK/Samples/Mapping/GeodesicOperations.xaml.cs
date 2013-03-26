using Microsoft.Phone.Controls;
using System;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISWindowsPhoneSDK
{
    public partial class GeodesicOperations : PhoneApplicationPage
    {
        Draw MyDrawObject;
        GraphicsLayer featureGraphicsLayer;
        GraphicsLayer verticesGraphicsLayer;

        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator = new ESRI.ArcGIS.Client.Projection.WebMercator();

        public GeodesicOperations()
        {
            InitializeComponent();

            featureGraphicsLayer = MyMap.Layers["GeodesicResultGraphicsLayer"] as GraphicsLayer;
            verticesGraphicsLayer = MyMap.Layers["VerticesGraphicsLayer"] as GraphicsLayer;

            MyDrawObject = new Draw(MyMap)
            {
                LineSymbol = LayoutRoot.Resources["DrawLineSymbol"] as LineSymbol,
                FillSymbol = LayoutRoot.Resources["DrawFillSymbol"] as FillSymbol
            };

            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;

        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            Type geometryType = args.Geometry.GetType();
            Geometry wgs84Geometry = _mercator.ToGeographic(args.Geometry);
            Geometry mercatorDensifiedGeometry = null;
            Geometry densifiedGeometry = null;
            int originalVerticeCount = 0;
            int densifiedVerticeCount = 0;

            if (geometryType == typeof(Polygon))
            {
                // Values returned in meters
                if (RadioButtonGeodesic.IsChecked.Value)
                {
                    TextBlockLength.Text = (ESRI.ArcGIS.Client.Geometry.Geodesic.Length(wgs84Geometry as Polygon)
                        * 0.000621371192).ToString("#0.000") + " mi";
                    TextBlockArea.Text = (Math.Abs(ESRI.ArcGIS.Client.Geometry.Geodesic.Area(wgs84Geometry as Polygon))
                        * 3.86102159e-7).ToString("#0.000") + " sq mi";
                }
                else
                {
                    TextBlockLength.Text = (ESRI.ArcGIS.Client.Geometry.Euclidian.Length(args.Geometry as Polygon)
                        * 0.000621371192).ToString("#0.000") + " mi";
                    TextBlockArea.Text = (Math.Abs(ESRI.ArcGIS.Client.Geometry.Euclidian.Area(args.Geometry as Polygon))
                        * 3.86102159e-7).ToString("#0.000") + " sq mi";
                }

                foreach (PointCollection ring in (args.Geometry as Polygon).Rings)
                    foreach (MapPoint mp in ring)
                    {
                        originalVerticeCount++;
                        verticesGraphicsLayer.Graphics.Add(new ESRI.ArcGIS.Client.Graphic()
                        {
                            Geometry = mp,
                            Symbol = LayoutRoot.Resources["OriginalMarkerSymbol"] as Symbol
                        });
                    }

                if (RadioButtonGeodesic.IsChecked.Value)
                {
                    densifiedGeometry =
                        ESRI.ArcGIS.Client.Geometry.Geodesic.Densify(wgs84Geometry, MyMap.Resolution * 10);
                    mercatorDensifiedGeometry = _mercator.FromGeographic(densifiedGeometry);
                }
                else
                    mercatorDensifiedGeometry =
                        ESRI.ArcGIS.Client.Geometry.Euclidian.Densify(args.Geometry, MyMap.Resolution * 10);

                foreach (PointCollection ring in (mercatorDensifiedGeometry as Polygon).Rings)
                    foreach (MapPoint mp in ring)
                    {
                        densifiedVerticeCount++;
                        verticesGraphicsLayer.Graphics.Add(new ESRI.ArcGIS.Client.Graphic()
                        {
                            Geometry = mp,
                            Symbol = LayoutRoot.Resources["NewMarkerSymbol"] as Symbol
                        });
                    }
            }
            else  // Polyline
            {
                // Value returned in meters
                if (RadioButtonGeodesic.IsChecked.Value)
                    TextBlockLength.Text = (ESRI.ArcGIS.Client.Geometry.Geodesic.Length(wgs84Geometry as Polyline)
                      * 0.000621371192).ToString("#0.000") + " mi";
                else
                    TextBlockLength.Text = (ESRI.ArcGIS.Client.Geometry.Euclidian.Length(args.Geometry as Polyline)
                      * 0.000621371192).ToString("#0.000") + " mi";

                TextBlockArea.Text = "NA";

                foreach (PointCollection path in (args.Geometry as Polyline).Paths)
                    foreach (MapPoint mp in path)
                    {
                        originalVerticeCount++;
                        verticesGraphicsLayer.Graphics.Add(new ESRI.ArcGIS.Client.Graphic()
                        {
                            Geometry = mp,
                            Symbol = LayoutRoot.Resources["OriginalMarkerSymbol"] as Symbol
                        });
                    }

                if (RadioButtonGeodesic.IsChecked.Value)
                {
                    densifiedGeometry =
                        ESRI.ArcGIS.Client.Geometry.Geodesic.Densify(wgs84Geometry, MyMap.Resolution * 10);
                    mercatorDensifiedGeometry = _mercator.FromGeographic(densifiedGeometry);
                }
                else
                    mercatorDensifiedGeometry =
                        ESRI.ArcGIS.Client.Geometry.Euclidian.Densify(args.Geometry, MyMap.Resolution * 10);

                foreach (PointCollection path in (mercatorDensifiedGeometry as Polyline).Paths)
                    foreach (MapPoint mp in path)
                    {
                        densifiedVerticeCount++;
                        verticesGraphicsLayer.Graphics.Add(new ESRI.ArcGIS.Client.Graphic()
                        {
                            Geometry = mp,
                            Symbol = LayoutRoot.Resources["NewMarkerSymbol"] as Symbol
                        });
                    }
            }

            featureGraphicsLayer.Graphics.Add(new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = mercatorDensifiedGeometry,
                Symbol = geometryType == typeof(Polygon) ?
                LayoutRoot.Resources["ResultFillSymbol"] as Symbol :
                LayoutRoot.Resources["ResultLineSymbol"] as Symbol
            });

            TextBlockVerticesBefore.Text = originalVerticeCount.ToString();
            TextBlockVerticesAfter.Text = densifiedVerticeCount.ToString();
        }


        private void DrawLineButton_Click(object sender, System.EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Polyline;
            MyDrawObject.IsEnabled = true;
        }

        private void DrawPolygonButton_Click(object sender, System.EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Polygon;
            MyDrawObject.IsEnabled = true;
        }

        private void ClearButton_Click(object sender, System.EventArgs e)
        {
            MyDrawObject.IsEnabled = false;
            featureGraphicsLayer.ClearGraphics();
            verticesGraphicsLayer.ClearGraphics();
        }

        private void ShowInfoButton_Click(object sender, EventArgs e)
        {
            InfoDialog.Visibility = InfoDialog.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}