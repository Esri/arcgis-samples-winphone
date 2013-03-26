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
    public partial class MessageInABottle : PhoneApplicationPage
    {
        Draw MyDrawObject;

        public MessageInABottle()
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
            MyDrawObject.IsEnabled = false;
            
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            Graphic graphic = new Graphic()
            {
                Symbol = LayoutRoot.Resources["StartMarkerSymbol"] as Symbol,
                Geometry = e.Geometry as MapPoint
            };
            graphicsLayer.Graphics.Add(graphic);

            Geoprocessor geoprocessorTask = new Geoprocessor("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/" +
                "Specialty/ESRI_Currents_World/GPServer/MessageInABottle");
            geoprocessorTask.ExecuteCompleted += GeoprocessorTask_ExecuteCompleted;
            geoprocessorTask.Failed += GeoprocessorTask_Failed;

            List<GPParameter> parameters = new List<GPParameter>();
            parameters.Add(new GPFeatureRecordSetLayer("Input_Point", e.Geometry as MapPoint));
            parameters.Add(new GPDouble("Days", Convert.ToDouble(DaysTextBox.Text)));

            geoprocessorTask.ExecuteAsync(parameters);
        }

        private void GeoprocessorTask_ExecuteCompleted(object sender, GPExecuteCompleteEventArgs e)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            foreach (GPParameter gpParameter in e.Results.OutParameters)
            {
                if (gpParameter is GPFeatureRecordSetLayer)
                {
                    GPFeatureRecordSetLayer gpLayer = gpParameter as GPFeatureRecordSetLayer;
                    foreach (Graphic graphic in gpLayer.FeatureSet.Features)
                    {
                        graphic.Symbol = LayoutRoot.Resources["PathLineSymbol"] as Symbol;
                        graphicsLayer.Graphics.Add(graphic);
                    }
                }
            }
        }

        private void GeoprocessorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geoprocessor service failed: " + e.Error);
        }

        #region Button/Menu methods

        private void Menu_List_Click(object sender, EventArgs e)
        {
            QueryDialog.Visibility = QueryDialog.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Menu_PointClick(object sender, EventArgs e)
        {
            MyDrawObject.IsEnabled = true;
        }

        #endregion
    }
}