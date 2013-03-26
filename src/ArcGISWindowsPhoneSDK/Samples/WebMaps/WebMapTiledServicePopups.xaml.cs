﻿using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISWindowsPhoneSDK
{
    public partial class WebMapTiledServicePopups : PhoneApplicationPage
    {
        public WebMapTiledServicePopups()
        {
            InitializeComponent();
            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("0e8aa0cc8dcb47d3b9a46e3c6c7c0f8f");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MyMap.Extent = e.Map.Extent;

                LayerCollection layerCollection = new LayerCollection();
                foreach (Layer layer in e.Map.Layers)
                    layerCollection.Add(layer);

                GraphicsLayer selectedGraphics = new GraphicsLayer()
                {
                    RendererTakesPrecedence = false,
                    ID = "MySelectionGraphicsLayer"
                };
                layerCollection.Add(selectedGraphics);

                e.Map.Layers.Clear();
                MyMap.Layers = layerCollection;
            }
        }

        private void MyMap_MapGesture(object sender, ESRI.ArcGIS.Client.Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap)
            {
                GraphicsLayer glayer = MyMap.Layers["MySelectionGraphicsLayer"] as GraphicsLayer;
                glayer.Graphics.Clear();

                MyInfoWindow.IsOpen = false;

                double mapScale = MyMap.Scale;

                ArcGISTiledMapServiceLayer alayer = null;
                DataTemplate dt = null;
                int layid = 0;

                foreach (Layer layer in MyMap.Layers)
                {
                    if (layer.GetValue(Document.PopupTemplatesProperty) != null)
                    {
                        alayer = layer as ArcGISTiledMapServiceLayer;
                        IDictionary<int, DataTemplate> idict = layer.GetValue(Document.PopupTemplatesProperty) as IDictionary<int, DataTemplate>;

                        foreach (LayerInfo linfo in alayer.Layers)
                        {
                            if (((mapScale > linfo.MaxScale // in scale range
                                && mapScale < linfo.MinScale) ||
                                (linfo.MaxScale == 0.0 // no scale dependency
                                && linfo.MinScale == 0.0) ||
                                (mapScale > linfo.MaxScale // minscale = 0.0 = infinity
                                && linfo.MinScale == 0.0)) &&
                                idict.ContainsKey(linfo.ID)) // id present in dictionary
                            {
                                layid = linfo.ID;
                                dt = idict[linfo.ID];
                                break;
                            }
                        }
                    }
                }

                if (dt != null)
                {
                    QueryTask qt = new QueryTask(string.Format("{0}/{1}", alayer.Url, layid));
                    qt.ExecuteCompleted += (s, qe) =>
                    {
                        if (qe.FeatureSet.Features.Count > 0)
                        {
                            Graphic g = qe.FeatureSet.Features[0];
                            MyInfoWindow.Anchor = e.MapPoint;
                            MyInfoWindow.ContentTemplate = dt;
                            MyInfoWindow.Content = g.Attributes;
                            MyInfoWindow.IsOpen = true;

                            SolidColorBrush symbolColor = new SolidColorBrush(Colors.Cyan);

                            if (g.Geometry is Polygon || g.Geometry is Envelope)
                            {
                                g.Symbol = new SimpleFillSymbol()
                                {
                                    BorderBrush = symbolColor,
                                    BorderThickness = 2
                                };
                            }
                            else if (g.Geometry is Polyline)
                            {
                                g.Symbol = new SimpleLineSymbol()
                                {
                                    Color = symbolColor
                                };
                            }
                            else // Point
                            {
                                g.Symbol = new SimpleMarkerSymbol()
                                {
                                    Color = symbolColor,
                                    Size = 12
                                };
                            }
                            glayer.Graphics.Add(g);
                        }
                    };

                    ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
                    query.Geometry = e.MapPoint;
                    query.OutSpatialReference = MyMap.SpatialReference;
                    query.OutFields.Add("*");
                    query.ReturnGeometry = true;

                    qt.ExecuteAsync(query);
                }
            }
        }
    }
}