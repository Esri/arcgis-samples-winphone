using System;
using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISWindowsPhoneSDK
{
    public partial class DriveTimes : PhoneApplicationPage
    {
        Geoprocessor _geoprocessorTask;
        Draw MyDrawObject;

        public DriveTimes()
        {
            InitializeComponent();

            _geoprocessorTask = new Geoprocessor("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/" +
                "Network/ESRI_DriveTime_US/GPServer/CreateDriveTimePolygons");
            _geoprocessorTask.ExecuteCompleted += GeoprocessorTask_ExecuteCompleted;
            _geoprocessorTask.Failed += GeoprocessorTask_Failed;

            MyDrawObject = new Draw(MyMap)
            {
                IsEnabled = false,
                DrawMode = DrawMode.Point
            };

            MyDrawObject.DrawComplete += MyDrawObject_DrawComplete;
        }

        private void MyDrawObject_DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs e)
        {
            MyDrawObject.IsEnabled = false;

            _geoprocessorTask.CancelAsync();

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as Symbol,
                Geometry = e.Geometry as MapPoint,
            };
            graphic.Attributes.Add("Info", "Start location");
            string latlon = String.Format("{0}, {1}", (e.Geometry as MapPoint).X, (e.Geometry as MapPoint).Y);
            graphic.Attributes.Add("LatLon", latlon);
            graphic.SetZIndex(1);
            graphicsLayer.Graphics.Add(graphic);

            List<GPParameter> parameters = new List<GPParameter>();
            parameters.Add(new GPFeatureRecordSetLayer("Input_Location", e.Geometry as MapPoint));
            parameters.Add(new GPString("Drive_Times", "1 2 3"));

            _geoprocessorTask.ExecuteAsync(parameters);
        }

        private void GeoprocessorTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.GPExecuteCompleteEventArgs args)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            foreach (GPParameter parameter in args.Results.OutParameters)
            {
                if (parameter is GPFeatureRecordSetLayer)
                {
                    GPFeatureRecordSetLayer gpLayer = parameter as GPFeatureRecordSetLayer;

                    List<FillSymbol> bufferSymbols = new List<FillSymbol>(
                    new FillSymbol[] { LayoutRoot.Resources["FillSymbol1"] as FillSymbol, LayoutRoot.Resources["FillSymbol2"] as FillSymbol, LayoutRoot.Resources["FillSymbol3"] as FillSymbol });

                    int count = 0;
                    foreach (Graphic graphic in gpLayer.FeatureSet.Features)
                    {
                        graphic.Symbol = bufferSymbols[count];
                        graphic.Attributes.Add("Info", String.Format("{0} minute buffer ", 3 - count));
                        graphicsLayer.Graphics.Add(graphic);
                        count++;
                    }
                }
            }
        }

        private void GeoprocessorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geoprocessing service failed: " + e.Error);
        }

        private void Menu_PointClick(object sender, EventArgs e)
        {
            MyDrawObject.IsEnabled = true;
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            InfoDialog.Visibility = InfoDialog.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}