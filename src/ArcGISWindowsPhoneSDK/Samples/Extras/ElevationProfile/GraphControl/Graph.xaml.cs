using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using ArcGISWindowsPhoneSDK;

namespace GraphSample.CustomControls
{
    /// <summary>
    /// Implement this interface to pass data to the graph control.
    /// </summary>
    public interface IGraphData
    {
        double GetX();
        double GetY();
        string GetXText(double x);
        string GetYText(double y);
    };

    /// <summary>
    /// Use a simple Point struct since Silverlight blows up when you operate on
    /// the internal Point class outside of the Drawing thread.
    /// </summary>
    struct PointF
    {
        public float X;
        public float Y;
        public PointF(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// NOTE FROM THE DEVELOPER:
    /// 
    /// This is a custom control designed for Windows Phone 7 to provide a chart that accepts custom user data
    /// and allows the user of the graph to scale and translate the graph along it's X-Axis.  Various design
    /// decisions were made along the way that focused on performance, as such I implemented, and later abandoned
    /// scaling along the Y-Axis.  Partly because I was seeking smoother touch feedback and partly because it
    /// was difficult to control and did not add a lot of value for my application (a stock price chart).
    /// 
    /// This was my very first attempt at Silverlight, and one of my first trips to the UI as I am primarily a backend
    /// developer.  Because I am unfortunately not a great designer, I likely did a few hackish things in code that
    /// should have been done in the XAML.  There may also be internal properties that you would like exposed to the
    /// XAML which I kept as public member variables as I am more comfortable in C# than XAML.
    /// 
    /// Finally, I did not intend to release this publicly and various functions could use refactoring - some are very long
    /// like CreateGraph() and should be broken up.  Along the same lines, some functions perform several tasks and you will
    /// probably also hate me for various variable names :).  I sprinkled comments throughout the code to help you along,
    /// and performed a bit of refactoring before making this publicly available.  Now, I want to move on to other projects, 
    /// but also share this component with the community.  If I come across bugs or improve functionality in this control for
    /// my own applications, I will update the source online and pass these on to you.  Hopefully you can get some value from
    /// it in its current state, or modified for your own applications.  Happy coding!
    /// 
    /// You can see this control in use in two of my applications already on the WP7 marketplace: "Stock Portfolio" and "Car Log."
    /// 
    /// Please contact me at blalger@gmail.com for bugs, comments or inquiries.  You can find additional details at www.bryanalger.com/projects
    /// 
    /// -Bryan Alger 4/24/2011
    /// </summary>
    public partial class Graph : UserControl
    {
        /// <summary>
        /// Controls how many points to draw while scrolling, lowering this improves performance
        /// at cost of detail \ drawing accuracy
        /// </summary>
        private const int MaxPointsScrolling = 20;

        private LinearGradientBrush fillBrush;
        private Polygon graphPoly;
        private int     MaxNumPoints;
        private int     spacePerGridLine;
        private bool    computedSpacePerGridLine;
        private bool    lineGraph;
        private bool    drawFullResolution; 
        private double  graphWidth;
        private double  graphHeight;
        private double  minX;
        private double  maxX;
        private double  minY;
        private double  maxY;
        private double  minWorldX;
        private double  maxWorldX;
        private ScaleTransform  scale;
        private ScaleTransform  lastScale;
        private DispatcherTimer drawTimer;
        private Collection<UIElement> horizontalGrid;
        private List<PointF>[] graphData;
        private DateTime manipulationStart;
        private DateTime lastTap;
        private PointF highlightedPoint;
        private bool   pointHighlighted;
        private Canvas highlightedPointCanvas;

        public static readonly DependencyProperty ChartTitleProperty      = DependencyProperty.Register("ChartTitle",      typeof(string),           typeof(Graph), null);
        public static readonly DependencyProperty DarkAccentColorProperty = DependencyProperty.Register("DarkAccentColor", typeof(Color),            typeof(Graph), null);
        public static readonly DependencyProperty AccentColorProperty     = DependencyProperty.Register("AccentColor",     typeof(Color),            typeof(Graph), null);
        public static readonly DependencyProperty GraphDataProperty       = DependencyProperty.Register("GraphData",       typeof(List<IGraphData>), typeof(Graph), new PropertyMetadata(null, OnGraphDataChanged));
        
        /// <summary>
        /// The actual data to graph - set this value to trigger a full graph redraw.  This must be called on the
        /// Drawing thead.  Avoid excessive calling as it is fairly CPU intensive.
        /// </summary>
        public List<IGraphData> GraphData
        {
            get { return (List<IGraphData>)GetValue(GraphDataProperty); }
            set { SetValue(GraphDataProperty, value); }
        }

        /// <summary>
        /// Get or set the title of the chart.  If no title is set, or NULL or "" is the title value,
        /// then screen real estate will not be reserved for the title, and more will be given to the actual chart.
        /// </summary>
        public string ChartTitle
        {
            get
            {
                return (string)GetValue(ChartTitleProperty);
            }
            set
            {
                if (graphTitle != null)
                {
                    double oldHeight = rootElement.RowDefinitions[0].Height.Value;
                    graphTitle.Text = value;
                    if (String.IsNullOrEmpty(value))
                    {
                        rootElement.RowDefinitions[0].Height = new GridLength(4);
                    }
                    else
                    {
                        rootElement.RowDefinitions[0].Height = new GridLength(40);
                    }
                    if (oldHeight != rootElement.RowDefinitions[0].Height.Value)
                    {
                        graphHeight = Height - rootElement.RowDefinitions[0].Height.Value
                                - rootElement.RowDefinitions[2].Height.Value;
                        FullRedraw();
                    }
                }
                SetValue(ChartTitleProperty, value);
            }
        }

        /// <summary>
        /// When LineGraph is false, this is the color for the graph fill area beginning at the bottom of the graph.
        /// This will likely be your screen background color.
        /// </summary>
        public Color DarkAccentColor
        {
            get
            {
                return (Color)GetValue(DarkAccentColorProperty);
            }
            set
            {
                SetValue(DarkAccentColorProperty, value);
                UpdateGraphFillGradient();
            }
        }

        /// <summary>
        /// This is the accent color used throughout the Chart control.  For a line graph, this is the color of the line
        /// or the color of the highest peaks of the graph fill area for non-line graphs.
        /// </summary>
        public Color AccentColor
        {
            get
            {
                return (Color)GetValue(AccentColorProperty);
            }
            set
            {
                SetValue(AccentColorProperty, value);
                UpdateGraphFillGradient();
            }
        }

        /// <summary>
        /// If set to true, we will draw a line graph, else a fill graph will be used.
        /// </summary>
        public bool LineGraph
        {
            get
            {
                return lineGraph;
            }
            set
            {
                if (value != lineGraph)
                {
                    lineGraph = value;
                    TriggerGraphRefresh();
                }
            }
        }

        public Graph()
        {
            InitializeComponent();
            AccentColor        = (Color)App.Current.Resources["PhoneAccentColor"];
            DarkAccentColor    = (Color)App.Current.Resources["PhoneBackgroundColor"];
            MaxNumPoints       = MaxPointsScrolling;
            scale              = new ScaleTransform();
            lastScale          = new ScaleTransform();
            drawTimer          = new DispatcherTimer();
            drawTimer.Interval = TimeSpan.FromMilliseconds(50);
            Unloaded          += new RoutedEventHandler(Graph_Unloaded);
            Loaded            += new RoutedEventHandler(Graph_Loaded);
            drawTimer.Tick    += new EventHandler(drawTimer_Tick);
            drawTimer.Start();
            UpdateGraphFillGradient();
            ClearGraph();
        }

        void Graph_Loaded(object sender, RoutedEventArgs e)
        {
            drawTimer.Start();
        }

        void Graph_Unloaded(object sender, RoutedEventArgs e)
        {
            drawTimer.Stop();
        }

        /// <summary>
        /// This is the graph drawing loop.  It will draw low resolution graphs while the graph is being
        /// manipulated, and progressively higher resolution when content is static.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void drawTimer_Tick(object sender, EventArgs e)
        {
            if (graphData != null)
            {
                bool draw = true;

                if (drawFullResolution)
                {
                    if (MaxNumPoints >= Math.Max(Math.Floor(graphWidth / 2), MaxPointsScrolling * 2))
                    {
                        draw = false;   // Already drawing full resolution graph
                    }
                    else
                    {
                        // Draw a higher resolution version of the current graph
                        MaxNumPoints = (int)Math.Min(2 * MaxNumPoints, Math.Floor(graphWidth / 2));
                        MaxNumPoints = (int)Math.Max(MaxPointsScrolling, MaxNumPoints);
                    }
                }
                else
                {
                    if (lastScale.CenterX != scale.CenterX ||
                        lastScale.ScaleX  != scale.ScaleX)
                    {
                        MaxNumPoints = MaxPointsScrolling;  // User actively manipulated graph content since last draw
                    }
                    else
                    {
                        draw = false;   // User is touching graph because drawFullResolution is false, but did not move since last draw
                    }
                }
                
                if (draw)
                {
                    CreateGraph();
                }
            }
        }

        /// <summary>
        /// Trigger a full refresh of the graph starting with a lower resolution and working up
        /// </summary>
        private void TriggerGraphRefresh()
        {
            drawFullResolution = true;
            MaxNumPoints = 0;
        }

        /// <summary>
        /// Called when the GraphData property was set by the user
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnGraphDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Graph graph = (Graph)d;

            graph.pointHighlighted = false;
            if (graph.GraphData != null)
            {
                graph.FullRedraw();
            }
            else
            {
                graph.ClearGraph(); // GraphData was set to null, warn the user there is no data to display
            }
        }

        private void FullRedraw()
        {
            ComputeGraphBounds();
            ZoomOut(); 
            GenerateSubGraphs();
            GenerateLeftBorder();
            TriggerGraphRefresh();
        }

        /// <summary>
        /// Draw an empty graph with "No data to display" text
        /// </summary>
        private void ClearGraph()
        {
            leftBorder.Children.Clear();
            bottomBorder.Children.Clear();
            graphArea.Children.Clear();
            TextBlock noData = new TextBlock()
            {
                Text = "No data to display"
            };
            double width  = noData.ActualWidth;
            double height = noData.ActualHeight;
            if (width < graphWidth && height < graphHeight)
            {
                noData.SetValue(Canvas.LeftProperty, (double)(graphWidth  / 2 - width /  2) - rootElement.ColumnDefinitions[0].Width.Value / 2);
                noData.SetValue(Canvas.TopProperty,  (double)(graphHeight / 2 - height / 2) + rootElement.RowDefinitions[2].Height.Value   / 2);
                graphArea.Children.Add(noData);
            }
        }

        /// <summary>
        /// Iterates over all of the points and determines the range of values (min, max for x and y coordinates)
        /// </summary>
        private void ComputeGraphBounds()
        {
            if (GraphData != null)
            {
                minY = double.MaxValue;
                maxY = double.MinValue;
                minX = double.MaxValue;
                maxX = double.MinValue;

                computedSpacePerGridLine = false;
                horizontalGrid = null;
                leftBorder.Children.Clear();

                if (GraphData.Count == 0)
                {
                    graphData = null;
                    return;
                }

                // Find min and max values of the graph data
                foreach (IGraphData point in GraphData)
                {
                    float X = (float)point.GetX();
                    float Y = (float)point.GetY();
                    minY = Y < minY ? Y : minY;
                    maxY = Y > maxY ? Y : maxY;
                    minX = X < minX ? X : minX;
                    maxX = X > maxX ? X : maxX;
                }

                minY = (minY - (maxY - minY) * .1);
                maxY = (maxY + (maxY - minY) * .1);
            }
        }

        /// <summary>
        /// Generates several sub graphs with progressively fewer number of data points
        /// This allows us to redraw the graph quickly during manipulation
        /// </summary>
        private void GenerateSubGraphs()
        {
            if (GraphData != null)
            {
                // Determine how many sub graphs we need
                int numGraphs = 1;
                int currNumPoints = GraphData.Count;
                while (currNumPoints > 64)
                {
                    currNumPoints = currNumPoints >> 1;
                    numGraphs++;
                }

                // Create empty sub graphs
                graphData = new List<PointF>[numGraphs];
                currNumPoints = GraphData.Count;
                for (int i = 0; i < graphData.Length; i++)
                {
                    graphData[i] = new List<PointF>(currNumPoints);
                    currNumPoints = currNumPoints % 2 == 0 ? currNumPoints >> 1 : currNumPoints >> 1 + 1;
                }

                // This is the source of the graph's efficiency, here we take the users input and generate several other
                // data inputs with progressively less accurate (due to rounding) depictions of the graph data, but each
                // with 1/2 as many points as the last graph we generated.
                float[] totalX = new float[graphData.Length];
                float[] totalY = new float[graphData.Length];
                float lastX = float.MinValue;
                for (int i = 0; i < GraphData.Count; i++)
                {
                    IGraphData point = GraphData[i];
                    float x = (float)point.GetX();
                    float y = (float)WorldToScreenYCoord(point.GetY());
                    bool isLastPoint = i == GraphData.Count - 1;

                    if (x < lastX)
                    {
                        throw new ArgumentException("GraphData must be sorted by ascending X values");
                    }
                    lastX = x;

                    // Add the first and last points to all graphData sets
                    if (i == 0)
                    {
                        for (int j = 1; j < graphData.Length; j++)
                        {
                            graphData[j].Add(new PointF(x, y));
                        }
                    }

                    graphData[0].Add(new PointF(x, y));         // Add all data points to the first graphData set
                    for (int j = 1; j < graphData.Length; j++)  // For the rest of the sets, add averages of the points
                    {
                        totalX[j] += x;
                        totalY[j] += y;

                        // We are dealing with powers of 2, so we can do a more efficient mod operation '%'
                        int mask = (1 << j) - 1;
                        int remainder = (mask & i);
                        if (remainder == 0 || isLastPoint)
                        {
                            int divisor = remainder == 0 ? mask + 1 : remainder;
                            graphData[j].Add(new PointF(totalX[j] / divisor, totalY[j] / divisor));
                            totalX[j] = totalY[j] = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Completely zoom out to all of the graph data
        /// </summary>
        public void ZoomOut()
        {
            ZoomToPoints(0, GraphData == null ? 0 : GraphData.Count - 1);
        }

        /// <summary>
        /// Zoom the graph onto a subset of its points
        /// </summary>
        /// <param name="minX">The desired minimum x parameter of the drawn graph</param>
        /// <param name="maxX">The desired maximum x parameter of the drawn graph</param>
        public void ZoomToPoints(double minX, double maxX)
        {
            if (graphData != null && graphData[0] != null && graphData[0].Count > 0)
            {
                int start = FindNearestPoint(minX, 0);
                int end   = FindNearestPoint(maxX, 0);
                ZoomToPoints(start, end);
            }
        }

        /// <summary>
        /// Zooms the graph onto a subset of points
        /// </summary>
        /// <param name="minPoint">The INDEX of the full-data GraphData array of the minimum x of the desired graph</param>
        /// <param name="maxPoint">The INDEX of the full-data GraphData array of the maximum x of the desired graph</param>
        private void ZoomToPoints(int minPoint, int maxPoint)
        {
            if (maxX <= minX || maxY <= minY || minPoint >= maxPoint || GraphData == null || maxPoint >= GraphData.Count)
            {
                return;
            }

            double currMaxX = GraphData[maxPoint].GetX();
            double currMinX = GraphData[minPoint].GetX();

            double leftX = 0;
            double topX  = 0;
            double scaleX = graphWidth  / (currMaxX == currMinX ? 1 : currMaxX - currMinX);
            double scaleY = graphHeight / (maxY     == minY     ? 1 : maxY     - minY);
            scale = new ScaleTransform()
            {
                CenterX = leftX - (currMinX * scaleX),
                CenterY = topX  - (minY     * scaleY),
                ScaleX  = scaleX,
                ScaleY  = scaleY
            };

            TriggerGraphRefresh();
        }

        /// <summary>
        /// Small helper function that generates the Brush that comprises the gradient for the fill graph
        /// </summary>
        private void UpdateGraphFillGradient()
        {
            GradientStop stop1 = new GradientStop()
            {
                Offset = 0,
                Color  = AccentColor
            };
            GradientStop stop2 = new GradientStop()
            {
                Offset = 1,
                Color  = DarkAccentColor
            };
            fillBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            fillBrush.GradientStops.Add(stop1);
            fillBrush.GradientStops.Add(stop2);

            if (graphPoly != null)
            {
                graphPoly.Fill = fillBrush;
            }
        }

        /// <summary>
        /// Resize the graph.
        /// </summary>
        /// <param name="newWidth">New width of the graph</param>
        /// <param name="newHeight">New height of the graph</param>
        private void ChangeBounds(double newWidth, double newHeight)
        {
            if (rootElement.Width != newWidth || rootElement.Height != newHeight)
            {
                Width  = newWidth;
                Height = newHeight;
                rootElement.Width  = newWidth;
                rootElement.Height = newHeight;
                graphTitle.Width   = Width - rootElement.ColumnDefinitions[0].Width.Value;
                graphTitle.Height  = rootElement.RowDefinitions[0].Height.Value;
                graphTitle.Text    = ChartTitle;

                graphWidth  = Width  - rootElement.ColumnDefinitions[0].Width.Value - 2;
                graphHeight = Height - rootElement.RowDefinitions[0].Height.Value
                                     - rootElement.RowDefinitions[2].Height.Value;
                MaxNumPoints  = (int)Math.Floor(graphWidth / 2);
                scale.ScaleX  = 1.0;
                scale.ScaleY  = 1.0;
                scale.CenterX = graphWidth / 2 + (double)graphArea.GetValue(Canvas.LeftProperty);
                FullRedraw();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            ChangeBounds(availableSize.Width, availableSize.Height);
            return base.MeasureOverride(availableSize); 
        }

        /// <summary>
        /// Recompute the graph's scale transformation given a user manipulation.
        /// </summary>
        /// <param name="CenterX">The center screen x coordinate of the scale manipulation.</param>
        /// <param name="ScaleX">The scale factor applied</param>
        private void CombineScaleTransform(double CenterX, double ScaleX)
        {
            double scaleX  = scale.ScaleX * ScaleX;
            double centerX = CenterX - (((CenterX - scale.CenterX) * scaleX) / scale.ScaleX);
            scale.CenterX  = centerX;
            scale.ScaleX   = scaleX;
        }

        /// <summary>
        /// Transform a world y-coordinate to a screen-y coordinate.
        /// </summary>
        /// <param name="y">THe world y-coordinate to transform</param>
        /// <returns>The equivalent screen-y coordinate of the transformed location.</returns>
        private double WorldToScreenYCoord(double y)
        {
            return graphHeight - (y * scale.ScaleY + scale.CenterY);
        }

        /// <summary>
        /// Transforms world coordinates to screen coordinates, noting that the Graph actually computed
        /// the y coordinate in advance (when GraphData was set) since we only scale on the X-Axis
        /// </summary>
        /// <param name="x">The world x coordinate to transform</param>
        /// <param name="y">The y coordinate (assumed to already have been transformed)</param>
        /// <returns>A drawable Point object with the transformed screen coordinates</returns>
        private Point WorldToScreenOptimized(double x, double y)
        {
            return new Point()
            {
                X = x * scale.ScaleX + scale.CenterX,
                Y = y
            };
        }

        /// <summary>
        /// Transforms screen coordinates to world coordinates.
        /// </summary>
        /// <param name="x">Screen x coordinate to transform</param>
        /// <param name="y">Screen y coordinate to transform</param>
        /// <returns>The transformed screen to world coordinates as a Point object</returns>
        private Point ScreenToWorld(double x, double y)
        {
            return new Point()
            {
                X = (x - scale.CenterX) / (scale.ScaleX == 0 ? 1 : scale.ScaleX),
                Y = (graphHeight - y - scale.CenterY) / (scale.ScaleY == 0 ? 1 : scale.ScaleY)
            };
        }

        /// <summary>
        /// Transforms only a screen x coordinate to world x coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private double ScreenToWorldX(double x)
        {
            return (x - scale.CenterX) / (scale.ScaleX == 0 ? 1 : scale.ScaleX);
        }

        /// <summary>
        /// Transforms only a screen y coordinate to world y coordinates
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private double ScreenToWorldY(double y)
        {
            return (graphHeight - y - scale.CenterY) / (scale.ScaleY == 0 ? 1 : scale.ScaleY);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);
            PageOrientation orientation = (App.Current.RootVisual as PhoneApplicationFrame).Orientation;
            bool  reverseCoords = orientation == PageOrientation.Landscape || orientation == PageOrientation.LandscapeLeft || orientation == PageOrientation.LandscapeRight;
            double scaleDelta   = reverseCoords ? e.DeltaManipulation.Scale.Y : e.DeltaManipulation.Scale.X;

            //Process scale
            if (scaleDelta != 1)
            {
                // Dont let user zoom out further than the width of the graph
                double scaleX = scaleDelta == 0 ? 1 : scaleDelta;
                if (scaleX * scale.ScaleX * (maxX - minX) < graphWidth)
                {
                    scaleX = graphWidth / (scale.ScaleX * (maxX - minX));
                }

                if (scaleX != scale.ScaleX)
                {
                    CombineScaleTransform(e.ManipulationOrigin.X, scaleX);
                }
            }

            // Process translation
            if (e.DeltaManipulation.Translation.X != 0)
            {
                scale.CenterX += e.DeltaManipulation.Translation.X;
            }

            double maxTranslate = -minX * scale.ScaleX;
            double minTranslate = graphWidth - maxX * scale.ScaleX;
            scale.CenterX = Math.Min(maxTranslate, scale.CenterX);
            scale.CenterX = Math.Max(minTranslate, scale.CenterX);
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            base.OnManipulationStarted(e);
            drawFullResolution = false;     // Tell the graph drawing timer to draw a low resolution chart since we are actively manipulating it
            manipulationStart  = DateTime.Now;
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);
            drawFullResolution = true;  // We are done manipulating the graph, so tell the chart drawing timer to draw a full resolution graph
            double leftBorderWidth = rootElement.ColumnDefinitions[0].Width.Value;

            // Is this a tap event?
            if (Math.Abs(e.TotalManipulation.Translation.X) <= 5 &&
                Math.Abs(e.TotalManipulation.Translation.Y) <= 5 &&
                e.TotalManipulation.Scale.X == 0 &&
                e.TotalManipulation.Scale.Y == 0 &&
                e.ManipulationOrigin.X >= leftBorderWidth &&
                manipulationStart != null &&
                (DateTime.Now - manipulationStart).TotalMilliseconds < 250)
            {
                //Is this a double tap event?
                if ((DateTime.Now - lastTap).TotalMilliseconds < 350)
                {
                    pointHighlighted = false;
                    ZoomOut();
                }
                else    // single tap
                {
                    // There is already a highlighted point - clear the display of it
                    if (pointHighlighted)
                    {
                        pointHighlighted = false;
                        graphArea.Children.Remove(highlightedPointCanvas);
                        highlightedPointCanvas = null;
                    }
                    else if (graphData != null)
                    {
                        // Determine the closest point to the user tap, and add an information overlay for that point
                        double worldPointX = ScreenToWorldX(e.ManipulationOrigin.X - leftBorderWidth);
                        int    graphIndex  = FindNearestPoint(worldPointX, 0);
                        if (Math.Abs(graphData[0][graphIndex].Y - e.ManipulationOrigin.Y) < 80)
                        {
                            highlightedPoint = graphData[0][graphIndex];
                            pointHighlighted = true;
                            DrawHighlightedPoint();
                        }
                    }
                }
                lastTap = DateTime.Now;
            }
        }

        /// <summary>
        /// Uses binary search to find the point with the given targetX or the point immediately preceding that x coordinate.
        /// </summary>
        /// <param name="targetX">The target x coordinate of the point to find in the graph expressed in World X Coordinates</param>
        /// <param name="graphIndex">The index of the graph to source (remember we computed multiple arrays of graphdata, each with fewer points?)</param>
        /// <returns></returns>
        private int FindNearestPoint(double targetX, int graphIndex)
        {
            int startIndex = 0;
            int endIndex   = graphData[graphIndex].Count - 1;
            int mid        = 0;
            while (startIndex < endIndex)
            {
                mid = (endIndex + startIndex) / 2;
                double currVal = graphData[graphIndex][mid].X;

                if (startIndex + 1 == endIndex)
                {
                    if (graphData[graphIndex][endIndex].X > targetX)
                        return startIndex;
                    return endIndex;
                }

                if (currVal == targetX)
                {
                    return (mid > 0 ? mid - 1 : 0); // Exact match!
                }
                else if (currVal > targetX)
                {
                    endIndex = mid;
                }
                else
                {
                    startIndex = mid;
                }
            }

#if DEBUG
            if (graphData[graphIndex][mid].X > targetX ||
                (mid + 1 < graphData[graphIndex].Count && graphData[graphIndex][mid + 1].X < targetX))
            {
                throw new Exception();
            }
#endif

            return mid;
        }

        /// <summary>
        /// Generate the Y-Axis border of the graph.  This can be done once for the graph since the Y-Axis does not scale or transform in this version.
        /// </summary>
        private void GenerateLeftBorder()
        {
            // The Y-Axis stays constant, so we only need to generate the y-grid a single time and save it to the 'horizontalGrid' List
            if (GraphData != null && horizontalGrid == null && !double.IsNaN(graphWidth))
            {
                //rootElement.ColumnDefinitions[0].SetValue(ColumnDefinition.WidthProperty, new GridLength(0.0));
                const int spacePerElem = 40;
                double maxWidth  = 0;
                int    numElems  = (int)graphHeight / spacePerElem;
                double startY    = (graphHeight - numElems * spacePerElem) / 2 + spacePerElem / 2;
                double colWidth  = rootElement.ColumnDefinitions[0].Width.Value;
                double rowHeight = rootElement.RowDefinitions[0].Height.Value;
                double fontSize  = (double)this.Resources["PhoneFontSizeSmall"];
                horizontalGrid   = new Collection<UIElement>();

                // Two Passes;
                //  Pass 0: Determine the maximum width required to draw the y-axis scale 
                //  Pass 1: Actually draw the Y Scale
                for (int pass = 0; pass < 2; pass++)
                {
                    for (int i = 0; i < numElems; i++)
                    {
                        double screenY = startY + i * spacePerElem;
                        double worldY = ((graphHeight - screenY) / graphHeight) * (maxY - minY) + minY;

                        TextBlock yKey = new TextBlock()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Top,
                            FontSize = fontSize,
                            Text = GraphData[0].GetYText(worldY)
                        };
                        maxWidth = Math.Max(maxWidth, yKey.ActualWidth);

                        // Do the actual drawing
                        if (pass == 1)
                        {
                            yKey.SetValue(Canvas.LeftProperty, colWidth - yKey.ActualWidth - 2);
                            yKey.SetValue(Canvas.TopProperty, rowHeight - yKey.ActualHeight / 2 + screenY);
                            leftBorder.Children.Add(yKey);

                            Line line = new Line()
                            {
                                Y1 = screenY,
                                Y2 = screenY,
                                X1 = 0,
                                X2 = graphWidth,
                                Stroke = (SolidColorBrush)this.Resources["PhoneContrastBackgroundBrush"],
                                StrokeThickness = 1
                            };
                            graphArea.Children.Add(line);
                            horizontalGrid.Add(line);
                        }
                    }

                    // Resize elements inside the graph to hold the y-scale
                    if (pass == 0)
                    {
                        rootElement.ColumnDefinitions[0].SetValue(ColumnDefinition.WidthProperty, new GridLength(maxWidth + 10.0));
                        colWidth = rootElement.ColumnDefinitions[0].Width.Value;
                        graphWidth = Width - colWidth - 2;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the highlighted point (if any) that the user selected when he touched the chart.
        /// </summary>
        private void DrawHighlightedPoint()
        {
            if (pointHighlighted)
            {
                Point point = WorldToScreenOptimized(highlightedPoint.X, highlightedPoint.Y);
                if (point.X >= 0 && point.X < graphWidth)
                {
                    // We might have to create the canvas that holds a circle for the point the user tapped as well
                    // as a text box with the information of that point.  This canvas might already exist from a previous drawing
                    if (highlightedPointCanvas == null)
                    {
                        highlightedPointCanvas = new Canvas();
                        double fontSize = (double)this.Resources["PhoneFontSizeSmall"]; ;
                        SolidColorBrush contrastBgBrush = (SolidColorBrush)this.Resources["PhoneContrastBackgroundBrush"];
                        Ellipse circle = new Ellipse()
                        {
                            Fill = new SolidColorBrush(Colors.Transparent),
                            Height = 10,
                            Width = 10,
                            StrokeThickness = 2,
                            Stroke = contrastBgBrush
                        };

                        highlightedPointCanvas.SetValue(Canvas.LeftProperty, point.X);
                        highlightedPointCanvas.SetValue(Canvas.TopProperty, point.Y);
                        circle.SetValue(Canvas.LeftProperty, -5.0);
                        circle.SetValue(Canvas.TopProperty, -5.0);
                        highlightedPointCanvas.Children.Add(circle);

                        TextBlock pointText = new TextBlock()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Top,
                            FontSize = fontSize,
                            Text = String.Format("({0}, {1})", GraphData[0].GetXText(highlightedPoint.X), GraphData[0].GetYText(ScreenToWorldY(point.Y)))
                        };
                        highlightedPointCanvas.Children.Add(pointText);
                    }

                    // If the canvas exists, we just need to shift the X-coordinate of the TextBlock inside the canvas as the user translates the graph
                    if (highlightedPointCanvas != null)
                    {
                        highlightedPointCanvas.SetValue(Canvas.LeftProperty, point.X);
                        highlightedPointCanvas.SetValue(Canvas.TopProperty,  point.Y);
                        if (highlightedPointCanvas.Children.Count == 2 && highlightedPointCanvas.Children[1].GetType() == typeof(TextBlock))
                        {
                            TextBlock pointText = (TextBlock)highlightedPointCanvas.Children[1];
                            double textHeight = pointText.ActualHeight + 12;
                            double textLocation = textHeight - point.Y < 12 ? -textHeight : textHeight;
                            pointText.SetValue(Canvas.LeftProperty, -((point.X / graphWidth) * pointText.ActualWidth));
                            pointText.SetValue(Canvas.TopProperty, textLocation);
                            graphArea.Children.Add(highlightedPointCanvas);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Meat and potatoes, draw the graph
        /// </summary>
        private void CreateGraph()
        {
            if (graphData == null || rootElement == null)
            {
                return;
            }

            // Determine which of the graphData sets we should use for this creation
            // IE, what quality of detail we need (less during manipulation by user)
            double scaleX       = graphWidth / (maxX == minX ? 1 : maxX - minX);
            double zoom         = scaleX == 0 ? 1 : scale.ScaleX / scaleX;
            int    targetPoints = (int)(MaxNumPoints * zoom);
            int    graphIndex   = graphData.Length - 1;

            // Determine which graph to draw from.  targetPoints is our estimate for the target number of points in the graph
            // we want to select.  It unfairly assumes the distribution of points is pretty regular (which was true for me when
            // I created this custom control, but may not be for you)
            while (graphIndex > 0 && graphData[graphIndex].Count < targetPoints) { graphIndex--; }

            List<Point>  dataPoints  = new List<Point>();
            List<PointF> targetGraph = graphData[graphIndex];
            graphArea.Children.Clear();            

            // Compute boundaries of the data to graph
            minWorldX = ScreenToWorldX(0);
            maxWorldX = ScreenToWorldX(graphWidth);
            int startIndex = FindNearestPoint(minWorldX, graphIndex);
            int endIndex   = Math.Min(FindNearestPoint(maxWorldX, graphIndex) + 1, targetGraph.Count - 1);

            // Only one point to draw, no graph here, I suppose we could draw the single point...
            if (endIndex == startIndex)
            {
                return;
            }

            // First point may be drawn off screen, if so, do some simple math to add an intermediate point at x = 0
            Point point1 = WorldToScreenOptimized(targetGraph[startIndex].X, targetGraph[startIndex].Y);
            if (point1.X < 0)
            {
                Point point2 = WorldToScreenOptimized(targetGraph[startIndex + 1].X, targetGraph[startIndex + 1].Y);
                double slope = (point2.Y - point1.Y) / (point2.X - point1.X);
                point1.Y    -= slope * point1.X;
                point1.X     = 0;
            }
            dataPoints.Add(point1);

            // Also add all of the interior points
            for (int i = startIndex + 1; i < endIndex; i ++)
            {
                dataPoints.Add(WorldToScreenOptimized(targetGraph[i].X, targetGraph[i].Y));
            }
            
            // Add the last point and check to see if it is off the screen, if so, again do some scaling
            // to put the last point on the line that connects to it on the graph.
            dataPoints.Add(WorldToScreenOptimized(targetGraph[endIndex].X, targetGraph[endIndex].Y));
            int numPoints = dataPoints.Count;
            Point prevPoint = dataPoints[numPoints - 2];
            Point endPoint  = dataPoints[numPoints - 1];
            if (endPoint.X > graphWidth)
            {
                dataPoints.RemoveAt(numPoints - 1);
                double slope = (endPoint.Y - prevPoint.Y) / (endPoint.X - prevPoint.X);
                endPoint.Y   = prevPoint.Y + slope * (graphWidth - prevPoint.X);
                endPoint.X   = graphWidth;
                dataPoints.Add(endPoint);
            }

            // Create the polygon that will be our graph if we are drawing a fill graph
            if (!lineGraph)
            {
                graphPoly = new Polygon()
                {
                    Stroke = (SolidColorBrush)App.Current.Resources["PhoneBorderBrush"],
                    Fill   = this.fillBrush,
                    StrokeThickness = 0
                };
            }

            // Draw the actual graph data - the lines for the line graph or add the points to plot the fill graph
            Point lastPoint = dataPoints[0];
            SolidColorBrush accentBrush = new SolidColorBrush(AccentColor);
            for (int i = 0; i < numPoints - 1; i++)
            {
                Point currPoint = dataPoints[i + 1];
#if DEBUG
                // A data point outside the bounds of the graph area at this point indicates an
                // error in assumptions I made about the data for optimizations.  This would be
                // a poor user experience.
                if ((int)lastPoint.X > graphWidth  ||
                    (int)currPoint.X > graphWidth  ||
                    (int)lastPoint.X < 0           ||
                    (int)currPoint.X < 0           ||
                    (int)lastPoint.Y < 0           ||
                    (int)currPoint.Y < 0           ||
                    (int)lastPoint.Y > graphHeight ||
                    (int)currPoint.Y > graphHeight)
                {
                    throw new Exception("Graph tried to add a point outside of graph bounds.");
                }
#endif
                // Create a line for a line graph
                if (lineGraph)
                {
                    if (lastPoint.X != currPoint.X)
                    {
                        Line line = new Line()
                        {
                            Stroke = accentBrush,
                            StrokeThickness = 3,
                            X1 = lastPoint.X,
                            X2 = currPoint.X
                        };
                    
                        line.Y1 = lastPoint.Y;
                        line.Y2 = currPoint.Y;
                        graphArea.Children.Add(line);
                    }
                }
                else
                {
                    if (graphPoly.Points.Count == 0)
                    {
                        graphPoly.Points.Add(new Point(0, graphHeight));
                        graphPoly.Points.Add(lastPoint);
                    }
                    graphPoly.Points.Add(currPoint);
                }
                lastPoint = currPoint;
            }

            if (!lineGraph)
            {
                graphPoly.Points.Add(new Point(graphWidth, graphHeight));
                graphArea.Children.Add(graphPoly);
            }

            double fontSize = (double)this.Resources["PhoneFontSizeSmall"]; ;
            SolidColorBrush contrastBgBrush = (SolidColorBrush)this.Resources["PhoneContrastBackgroundBrush"];
            
            // Draw the labels on left side bar to show the y scale of the graph
            GenerateLeftBorder();
            if (horizontalGrid != null)
            {
                int len = horizontalGrid.Count;
                for (int i = 0; i < len; i++)
                {
                    graphArea.Children.Add(horizontalGrid[i]);
                }
            }

            // Only need to recompute the X-Axis labels if the x axis changed by translating or scaling
            if (scale.CenterX != lastScale.CenterX ||
                scale.ScaleX  != lastScale.ScaleX)
            {
                bottomBorder.Children.Clear();
                // Draw the labels on bottom bar to show the x scale of the graph
                if (!computedSpacePerGridLine)
                {
                    // Step through a few dozen points determining the average and standard deviation of the width
                    // required to draw their X-Axis label
                    const int targetNumCompute = 40;
                    int numCompute = GraphData.Count <= targetNumCompute ? GraphData.Count : targetNumCompute;
                    if (numCompute > 0)
                    {
                        double   stdDev     = 0;
                        double   totalWidth = 0;
                        double   avg        = 0;
                        double[] widths     = new double[numCompute];
                        int      step       = GraphData.Count / numCompute;
                        for (int i = 0; i < numCompute; i++)
                        {
                            TextBlock xKey1 = new TextBlock()
                            {
                                FontSize = fontSize,
                                Text = GraphData[0].GetXText(GraphData[i * step].GetX())
                            };
                            widths[i] = xKey1.ActualWidth;
                            totalWidth += widths[i];
                        }

                        avg = totalWidth / numCompute;
                        if (numCompute > 1)
                        {
                            for (int i = 0; i < numCompute; i++)
                            {
                                stdDev += Math.Pow(widths[i] - avg, 2);
                            }
                            stdDev = Math.Sqrt(stdDev / (numCompute - 1));
                            spacePerGridLine = (int)(Math.Ceiling(avg + stdDev * 4) + 12);
                        }
                        else
                        {
                            spacePerGridLine = (int)(Math.Ceiling(avg * .65) + 12);
                        }

                        computedSpacePerGridLine = true;
                    }
                }

                // Good - we have computed the avg and deviation of the space required to draw a label on the
                // x-axis, now lets add some labels and grid lines!
                double screenX = spacePerGridLine;
                double endLastX = 0;
                while (true)
                {
                    double x = ScreenToWorldX(screenX);
                    TextBlock xKey = new TextBlock()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontSize = fontSize,
                        Text = GraphData[0].GetXText(x)
                    };

                    double actualWidth = xKey.ActualWidth;
                    xKey.SetValue(Canvas.LeftProperty, screenX - (actualWidth / 2));
                    xKey.SetValue(Canvas.TopProperty, 2.0);

                    double endX = screenX + actualWidth / 2;
                    if (endX >= graphWidth)
                    {   // No space left in the bottom grid area for another label
                        break;
                    }
                    if (endX - actualWidth >= endLastX + 10)
                    {
                        endLastX = endX;
                        bottomBorder.Children.Add(xKey);
                    }
                    screenX += spacePerGridLine;
                }
            }

            // We computed the X-Axis labels which are TextBlocks that live inside "bottomBorder", now draw the corresponding grid lines for the labels
            int lastBottomIndex = bottomBorder.Children.Count;
            for (int i = 0; i < lastBottomIndex; i++)
            {
                UIElement elem = bottomBorder.Children[i];
                double screenX = (double)elem.GetValue(Canvas.LeftProperty) + (double)elem.GetValue(TextBlock.ActualWidthProperty) / 2;
                Line line = new Line()
                {
                    X1 = screenX,
                    X2 = screenX,
                    Y1 = 0,
                    Y2 = graphHeight,
                    Stroke = contrastBgBrush,
                    StrokeThickness = 1
                };
                graphArea.Children.Add(line);
            }

            // Draw the highlighted point if there is one
            DrawHighlightedPoint();
            lastScale.ScaleX  = scale.ScaleX;
            lastScale.CenterX = scale.CenterX;
        }
    }
}
