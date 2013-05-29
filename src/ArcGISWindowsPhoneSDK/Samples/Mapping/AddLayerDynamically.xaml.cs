using Microsoft.Phone.Controls;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISWindowsPhoneSDK
{
    public partial class AddLayerDynamically : PhoneApplicationPage
    {
        public AddLayerDynamically()
        {
            InitializeComponent();
        }

        private void AddLayerButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MyMap.Layers.Clear();

            ArcGISTiledMapServiceLayer NewTiledLayer = new ArcGISTiledMapServiceLayer();

            MyMap.Layers.LayersInitialized += (evtsender, args) =>
            {
                MyMap.ZoomTo(NewTiledLayer.InitialExtent);
            };

            NewTiledLayer.Url = UrlTextBox.Text;
            MyMap.Layers.Add(NewTiledLayer);
        }

        private void MyMap_ExtentChange(object sender, ESRI.ArcGIS.Client.ExtentEventArgs e)
        {
            setExtentText(e.NewExtent);
        }
        private void setExtentText(Envelope newExtent)
        {
            ExtentTextBlock.Text = string.Format("MinX: {0}\nMinY: {1}\nMaxX: {2}\nMaxY: {3}",
                newExtent.XMin, newExtent.YMin, newExtent.XMax, newExtent.YMax);
            ExtentGrid.Visibility = Visibility.Visible;
        }

        private void ShowInfo_Clicked(object sender, System.EventArgs e)
        {
            ExtentGrid.Visibility = ExtentGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            UrlEntry.Visibility = UrlEntry.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;       
        }
    }
}