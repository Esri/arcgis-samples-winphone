using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class RoutingBarriers : PhoneApplicationPage
    {
        RouteTask _routeTask;
        List<Graphic> _stops = new List<Graphic>();
        List<Graphic> _barriers = new List<Graphic>();
        RouteParameters _routeParams = new RouteParameters();
	Draw myDrawObject;
	GraphicsLayer stopsLayer = null;
	GraphicsLayer barriersLayer = null;


        public RoutingBarriers()
        {
            InitializeComponent();

            myDrawObject = new Draw(MyMap)
            {
                DrawMode = DrawMode.Point,
                IsEnabled = true
            };
            myDrawObject.DrawComplete += myDrawObject_DrawComplete;

            _routeTask =
                new RouteTask("http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route");
            _routeTask.SolveCompleted += routeTask_SolveCompleted;
            _routeTask.Failed += routeTask_Failed;

            _routeParams.Stops = _stops;
            _routeParams.Barriers = _barriers;
            _routeParams.UseTimeWindows = false;

	     barriersLayer = MyMap.Layers["MyBarriersGraphicsLayer"] as GraphicsLayer;
	     stopsLayer = MyMap.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
        }

        void myDrawObject_DrawComplete(object sender, DrawEventArgs e)
        {
            if (StopsRadioButton.IsChecked.Value)
            {
                Graphic stop = new Graphic() { Geometry = e.Geometry, Symbol = LayoutRoot.Resources["StopSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol };
                stop.Attributes.Add("StopNumber", stopsLayer.Graphics.Count + 1);
                stopsLayer.Graphics.Add(stop);
                _stops.Add(stop);
            }
            else if (BarriersRadioButton.IsChecked.Value)
            {
                Graphic barrier = new Graphic() { Geometry = e.Geometry, Symbol = LayoutRoot.Resources["BarrierSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol };
		barriersLayer.Graphics.Add(barrier);
                _barriers.Add(barrier);
            }
            if (_stops.Count > 1)
            {
                if (_routeTask.IsBusy)
                    _routeTask.CancelAsync();
                _routeParams.OutSpatialReference = MyMap.SpatialReference;
                _routeTask.SolveAsync(_routeParams);
            }
        }

        private void routeTask_Failed(object sender, TaskFailedEventArgs e)
        {
            string errorMessage = "Routing error: ";
            errorMessage += e.Error.Message;
            foreach (string detail in (e.Error as ServiceException).Details)
                errorMessage += "," + detail;

						MessageBox.Show(errorMessage);

						if ((_stops.Count) > 10)
						{
							stopsLayer.Graphics.RemoveAt(stopsLayer.Graphics.Count - 1);
							_stops.RemoveAt(_stops.Count - 1);
						}
        }

        private void routeTask_SolveCompleted(object sender, RouteEventArgs e)
        {
            GraphicsLayer routeLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;
            RouteResult routeResult = e.RouteResults[0];
            routeResult.Route.Symbol = LayoutRoot.Resources["RouteSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;

            routeLayer.Graphics.Clear();
            Graphic lastRoute = routeResult.Route;

            routeLayer.Graphics.Add(lastRoute);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _stops.Clear();
            _barriers.Clear();

            foreach (Layer layer in MyMap.Layers)
                if (layer is GraphicsLayer)
                    (layer as GraphicsLayer).ClearGraphics();
        }

    }
}
