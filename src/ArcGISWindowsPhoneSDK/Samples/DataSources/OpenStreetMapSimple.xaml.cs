using Microsoft.Phone.Controls;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace ArcGISWindowsPhoneSDK
{
    public partial class OpenStreetMapSimple : PhoneApplicationPage
    {
        public OpenStreetMapSimple()
        {
            InitializeComponent();
        }

        private void RadioButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenStreetMapLayer osmLayer = MyMap.Layers["OSMLayer"] as OpenStreetMapLayer;
            string layerTypeTag = (string)((RadioButton)sender).Content;
            OpenStreetMapLayer.MapStyle newLayerType = (OpenStreetMapLayer.MapStyle)System.Enum.Parse(typeof(OpenStreetMapLayer.MapStyle),
                layerTypeTag, true);
            osmLayer.Style = newLayerType;
        }
    }
}