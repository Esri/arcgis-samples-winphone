using Microsoft.Phone.Controls;
using System.Windows;
using ESRI.ArcGIS.Client;
using Microsoft.Phone.Shell;

namespace ArcGISWindowsPhoneSDK
{
    public partial class FeatureLayerSelection : PhoneApplicationPage
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator = new ESRI.ArcGIS.Client.Projection.WebMercator();

        ESRI.ArcGIS.Client.Geometry.Envelope initialExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(
            mercator.FromGeographic(new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.190346717, 34.0514888762)) as 
            ESRI.ArcGIS.Client.Geometry.MapPoint,
            mercator.FromGeographic(new ESRI.ArcGIS.Client.Geometry.MapPoint(-117.160305976, 34.072946548)) as 
            ESRI.ArcGIS.Client.Geometry.MapPoint)
        {
            SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(102100)
        };

        Editor editor;

        public FeatureLayerSelection()
        {
            InitializeComponent();

            MyMap.Extent = initialExtent;
            editor = LayoutRoot.Resources["MyEditor"] as Editor;
        }

        private void InfoButton_Click(object sender, System.EventArgs e)
        {
            if (InformationGrid.Visibility == Visibility.Visible)       
                InformationGrid.Visibility = Visibility.Collapsed;          
            else
                InformationGrid.Visibility = Visibility.Visible;           
        }

        private void NewButton_Click(object sender, System.EventArgs e)
        {
            if (editor.Select.CanExecute("New"))
                editor.Select.Execute("New");
        }

        private void ClearButton_Click(object sender, System.EventArgs e)
        {
            if (editor.ClearSelection.CanExecute(null))
                editor.ClearSelection.Execute(null);
        }
    }
}