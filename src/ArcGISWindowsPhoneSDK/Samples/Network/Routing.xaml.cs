using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Routing : PhoneApplicationPage
    {
        Draw MyDrawObject;
        public Routing()
        {
            InitializeComponent();
            MyDrawObject = new Draw(MyMap)
            {
                IsEnabled = false,
                DrawMode = DrawMode.Point
            };

            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs e)
        {            
            GraphicsLayer stopsGraphicsLayer = MyMap.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            Graphic stop = new Graphic() { Geometry = e.Geometry, Symbol = LayoutRoot.Resources["StopSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol };
            stopsGraphicsLayer.Graphics.Add(stop);

            if (stopsGraphicsLayer.Graphics.Count > 1)
            {
                RouteTask routeTask = LayoutRoot.Resources["MyRouteTask"] as RouteTask;
                if (routeTask.IsBusy)
                {
                    routeTask.CancelAsync();
                    stopsGraphicsLayer.Graphics.RemoveAt(stopsGraphicsLayer.Graphics.Count - 1);
                }
                routeTask.SolveAsync(new RouteParameters() { Stops = stopsGraphicsLayer, UseTimeWindows = false, OutSpatialReference = MyMap.SpatialReference });
            }
        }

        private void MyRouteTask_Failed(object sender, TaskFailedEventArgs e)
        {
            string errorMessage = "Routing error: ";
            errorMessage += e.Error.Message;
            foreach (string detail in (e.Error as ServiceException).Details)
                errorMessage += "," + detail;

            MessageBox.Show(errorMessage);
            GraphicsLayer stopsGraphicsLayer = MyMap.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            stopsGraphicsLayer.Graphics.RemoveAt(stopsGraphicsLayer.Graphics.Count - 1);
        }

        private void MyRouteTask_SolveCompleted(object sender, RouteEventArgs e)
        {
            GraphicsLayer routeGraphicsLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
            routeGraphicsLayer.Graphics.Clear();

            RouteResult routeResult = e.RouteResults[0];
            routeResult.Route.Symbol = LayoutRoot.Resources["RouteSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;

            Graphic lastRoute = routeResult.Route;

            decimal totalTime = (decimal)lastRoute.Attributes["Total_Time"];
            string tip = string.Format("Total Time: {0} minutes", totalTime.ToString("#0.00"));
            TimeText.Text = tip;

            routeGraphicsLayer.Graphics.Add(lastRoute);
        }

        private void Menu_List_Clear(object sender, System.EventArgs e)
        {
            GraphicsLayer stopsGraphicsLayer = MyMap.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            stopsGraphicsLayer.Graphics.Clear();

            GraphicsLayer routeGraphicsLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
            routeGraphicsLayer.Graphics.Clear();
        }

        private void DrawPointButton_Click(object sender, System.EventArgs e)
        {
            MyDrawObject.DrawMode = DrawMode.Point;
            MyDrawObject.IsEnabled = true;
        }

    }
}