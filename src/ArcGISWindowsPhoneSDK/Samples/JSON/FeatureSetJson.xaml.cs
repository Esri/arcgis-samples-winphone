using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;

namespace ArcGISWindowsPhoneSDK
{
    public partial class FeatureSetJson : PhoneApplicationPage
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
                new ESRI.ArcGIS.Client.Projection.WebMercator();

        InfoWindow myInfoWindow;

        public FeatureSetJson()
        {
            InitializeComponent();
            CreateFeatureSetJson();
        }

        private void Button_Load(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                FeatureSet featureSet = FeatureSet.FromJson(JsonTextBox.Text);

                GraphicsLayer graphicsLayerFromFeatureSet = new GraphicsLayer()
                {
                    Graphics = new GraphicCollection(featureSet)
                };

                if (!featureSet.SpatialReference.Equals(MyMap.SpatialReference))
                {
                    if (MyMap.SpatialReference.Equals(new SpatialReference(102100)) &&
                        featureSet.SpatialReference.Equals(new SpatialReference(4326)))
                        foreach (Graphic g in graphicsLayerFromFeatureSet.Graphics)
                            g.Geometry = _mercator.FromGeographic(g.Geometry);

                    else if (MyMap.SpatialReference.Equals(new SpatialReference(4326)) &&
                        featureSet.SpatialReference.Equals(new SpatialReference(102100)))
                        foreach (Graphic g in graphicsLayerFromFeatureSet.Graphics)
                            g.Geometry = _mercator.ToGeographic(g.Geometry);

                    else
                    {
                        GeometryService geometryService =
                            new GeometryService("http://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");

                        geometryService.ProjectCompleted += (s, a) =>
                        {
                            for (int i = 0; i < a.Results.Count; i++)
                                graphicsLayerFromFeatureSet.Graphics[i].Geometry = a.Results[i].Geometry;
                        };

                        geometryService.Failed += (s, a) =>
                        {
                            MessageBox.Show("Projection error: " + a.Error.Message);
                        };

                        geometryService.ProjectAsync(graphicsLayerFromFeatureSet.Graphics, MyMap.SpatialReference);
                    }
                }

                SimpleRenderer simpleRenderer = new SimpleRenderer();
                SolidColorBrush symbolColor = new SolidColorBrush(Colors.Blue);
                graphicsLayerFromFeatureSet.Renderer = simpleRenderer;

                if (featureSet.GeometryType == GeometryType.Polygon || featureSet.GeometryType == GeometryType.Polygon)
                {
                    simpleRenderer.Symbol = new SimpleFillSymbol()
                    {
                        Fill = symbolColor
                    };
                }
                else if (featureSet.GeometryType == GeometryType.Polyline)
                {
                    simpleRenderer.Symbol = new SimpleLineSymbol()
                    {
                        Color = symbolColor
                    };
                }
                else // Point
                {
                    simpleRenderer.Symbol = new SimpleMarkerSymbol()
                    {
                        Color = symbolColor,
                        Size = 12
                    };
                }

                myInfoWindow = new InfoWindow()
                {
                    Background = new SolidColorBrush(Colors.Black),  
                    BorderBrush = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(1),
                    CornerRadius = 10,
                    Map = MyMap,
                };
                myInfoWindow.MouseLeftButtonDown += myInfoWindow_MouseLeftButtonDown;

                StackPanel stackPanelParent = new StackPanel()
                {
                    Orientation = System.Windows.Controls.Orientation.Vertical,
                    Margin = new Thickness(12)
                };

                foreach (KeyValuePair<string, string> keyvalue in featureSet.FieldAliases)
                {
                    StackPanel stackPanelChild = new StackPanel()
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new Thickness(0, 0, 0, 6)
                    };
                    TextBlock textValue = new TextBlock()
                    {
                        Text = keyvalue.Value + ": "
                    };

                    TextBlock textKey = new TextBlock();
                    Binding keyBinding = new Binding(string.Format("[{0}]", keyvalue.Key));
                    textKey.SetBinding(TextBlock.TextProperty, keyBinding);

                    stackPanelChild.Children.Add(textValue);
                    stackPanelChild.Children.Add(textKey);

                    if (keyvalue.Key == featureSet.DisplayFieldName)
                    {
                        textKey.FontWeight = textValue.FontWeight = FontWeights.Bold;
                        stackPanelParent.Children.Insert(0, stackPanelChild);
                    }
                    else
                        stackPanelParent.Children.Add(stackPanelChild);

                }

                myInfoWindow.Content = stackPanelParent;
                ContentPanel.Children.Add(myInfoWindow);

                MyMap.Layers.Add(graphicsLayerFromFeatureSet);
                graphicsLayerFromFeatureSet.MouseLeftButtonDown += graphicsLayerFromFeatureSet_MouseLeftButtonDown;
                JsonPivot.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GraphicsLayer creation failed", MessageBoxButton.OK);
            }
        }

        void myInfoWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (sender as InfoWindow).IsOpen = false;
        }

        private void graphicsLayerFromFeatureSet_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(ContentPanel);
            myInfoWindow.DataContext = e.Graphic.Attributes;
            myInfoWindow.Anchor = MyMap.ScreenToMap(pt);
            myInfoWindow.IsOpen = true;
        }

        private void Button_ClearMap(object sender, RoutedEventArgs e)
        {
            List<GraphicsLayer> graphicsLayers = new List<GraphicsLayer>();

            foreach (Layer layer in MyMap.Layers)
                if (layer is GraphicsLayer)
                    graphicsLayers.Add(layer as GraphicsLayer);

            for (int i = 0; i < graphicsLayers.Count; i++)
                MyMap.Layers.Remove(graphicsLayers[i]);

            DisplayInBrowser(string.Empty);
        }

        private void CreateFeatureSetJson()
        {
            string jsonInput = @"{
    ""displayFieldName"" : ""AREANAME"",
    ""geometryType"" : ""esriGeometryPoint"",
    ""spatialReference"" : {""wkid"" : 4326},
    ""fieldAliases"" : {
        ""ST"" : ""State Name"",
        ""POP2000"" : ""Population"",
        ""AREANAME"" : ""City Name"" 
        },     
    ""features"" : [
        {
            ""attributes"" : {
            ""ST"" : ""CA"",
            ""POP2000"" : 3694820,
            ""AREANAME"" : ""Los Angeles""
            },
        ""geometry"" : { ""x"" : -118.4, ""y"" : 34.1 }
        },
        {
            ""attributes"" : {
            ""ST"" : ""WA"",
            ""POP2000"" : 563374,
            ""AREANAME"" : ""Seattle""
        },
        ""geometry"" : { ""x"" : -122.3, ""y"" : 47.5 }
        }
    ]
}";
            // string.Replace necessary here to handle double-quotes escaped for the '@' string literal 
            JsonTextBox.Text = jsonInput;
        }

        private void Button_Show(object sender, RoutedEventArgs e)
        {
            DisplayInBrowser(string.Empty);
            FeatureLayer featureLayer = new FeatureLayer()
            {
                Url = FeatureLayerUrlTextBox.Text
            };

            featureLayer.InitializationFailed += featureLayer_InitializationFailed;

            featureLayer.UpdateCompleted += featureLayer_UpdateCompleted;
            featureLayer.UpdateFailed += featureLayer_UpdateFailed;
            MyMap.Layers.Add(featureLayer);
        }

        void featureLayer_InitializationFailed(object sender, EventArgs e)
        {
            MessageBox.Show("Url must reference a feature layer in a map or feature service.",
               "FeatureLayer initialization failed", MessageBoxButton.OK);
        }

        private void featureLayer_UpdateCompleted(object sender, EventArgs e)
        {
            FeatureSet featureSet = new FeatureSet((sender as FeatureLayer).Graphics);
            string jsonOutput = featureSet.ToJson();
            // show the json in a web browser since too long for a Windows Phone TextBox
            DisplayInBrowser(jsonOutput);
        }
        void DisplayInBrowser(string json)
        {
            OutWebBrowser.NavigateToString("<html><head><title>JSON Output</title></head><body>" + json + "</body></html>");
        }

        void featureLayer_UpdateFailed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show(e.Error.Message,
                "FeatureLayer update failed", MessageBoxButton.OK);
        }

        private void PivotButton_Click(object sender, EventArgs e)
        {
            JsonPivot.Visibility = (JsonPivot.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
            if (myInfoWindow != null && myInfoWindow.IsOpen && JsonPivot.Visibility == Visibility.Visible)
                myInfoWindow.IsOpen = false;
        }
    }
}