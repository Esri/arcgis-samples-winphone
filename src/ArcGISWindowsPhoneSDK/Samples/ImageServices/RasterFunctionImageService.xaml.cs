using Microsoft.Phone.Controls;
using System.Windows;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using System;
using System.Windows.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class RasterFunctionImageService : PhoneApplicationPage
    {
        public RasterFunctionImageService()
        {
            InitializeComponent();
        }

        private void ArcGISImageServiceLayer_Initialized(object sender, EventArgs e)
        {
            RasterFunctionsComboBox.ItemsSource =
                (sender as ArcGISImageServiceLayer).RasterFunctionInfos;
            RasterFunctionsComboBox.SelectedIndex = 0;
        }

        private void RasterFunctionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArcGISImageServiceLayer imageLayer = MyMap.Layers["MyImageLayer"] as ArcGISImageServiceLayer;
            var rasterFunction = (sender as ListPicker).SelectedItem as RasterFunctionInfo;
            if (rasterFunction != null)
            {
                RenderingRule renderingRule = new RenderingRule() { RasterFunctionName = rasterFunction.Name };
                imageLayer.RenderingRule = renderingRule;
                imageLayer.Refresh();
            }
        }
    }
}
