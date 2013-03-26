using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client;
using System.Windows;

namespace ArcGISWindowsPhoneSDK
{
    public partial class LoadWebMapDynamically : PhoneApplicationPage
    {
        public LoadWebMapDynamically()
        {
            InitializeComponent();
        }

        private void LoadWebMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateMapFromID(WebMapTextBox.Text);
        }

        public void CreateMapFromID(string webMapID)
        {
            if (!string.IsNullOrEmpty(webMapID))
            {
                Document webMap = new Document();
                webMap.GetMapCompleted += (s, e) =>
                {
                    MyMap.Extent = e.Map.Extent;

                    LayerCollection layerCollection = new LayerCollection();
                    foreach (Layer layer in e.Map.Layers)
                        layerCollection.Add(layer);

                    e.Map.Layers.Clear();
                    MyMap.Layers = layerCollection;
                    WebMapPropertiesTextBox.DataContext = e.ItemInfo;
                };

                webMap.GetMapAsync(webMapID);
            }
        }

        private void Menu_List_Click(object sender, System.EventArgs e)
        {
            InfoGrid.Visibility = InfoGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
