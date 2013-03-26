using System;
using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Project : PhoneApplicationPage
    {
        GeometryService geometryService;
        GraphicsLayer graphicsLayer;

        public Project()
        {
            InitializeComponent();

            geometryService = new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.ProjectCompleted += geometryService_ProjectCompleted;
            geometryService.Failed += geometryService_Failed;

            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
        }

        private void ProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            double x = double.NaN;
            double y = double.NaN;

            double.TryParse(XTextBox.Text, out x);
            double.TryParse(YTextBox.Text, out y);

            if (double.IsNaN(x) || double.IsNaN(y))
            {
                MessageBox.Show("Enter valid coordinate values.");
                return;
            }

            MapPoint inputMapPoint = new MapPoint(x, y, new SpatialReference(4326));

            geometryService.ProjectAsync(new List<Graphic>() { new Graphic() { Geometry = inputMapPoint } }, MyMap.SpatialReference, inputMapPoint);
        }

        void geometryService_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            Graphic resultGraphic = e.Results[0];
            resultGraphic.Symbol = LayoutRoot.Resources["RoundMarkerSymbol"] as SimpleMarkerSymbol;

            MapPoint resultMapPoint = resultGraphic.Geometry as MapPoint;
            resultGraphic.Attributes.Add("Output_Coordinate", resultMapPoint.X + "," + resultMapPoint.Y);

            MapPoint inputMapPoint = e.UserState as MapPoint;
            resultGraphic.Attributes.Add("Input_Coordinate", inputMapPoint.X + "," + inputMapPoint.Y);

            graphicsLayer.Graphics.Add(resultGraphic);
        }

        void geometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geometry Service error: " + e.Error);
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            InfoGrid.Visibility = InfoGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

 
    }
}