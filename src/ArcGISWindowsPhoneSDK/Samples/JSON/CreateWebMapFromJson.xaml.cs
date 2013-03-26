using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISWindowsPhoneSDK
{
    public partial class CreateWebMapFromJson : PhoneApplicationPage
    {
        ESRI.ArcGIS.Client.Projection.WebMercator mercator =
          new ESRI.ArcGIS.Client.Projection.WebMercator();

        public CreateWebMapFromJson()
        {
            InitializeComponent();
            CreateWebMapJson();
        }

        private void CreateWebMapJson()
        {

            string jsonInput = @"{                                         
""baseMap"": {
    ""baseMapLayers"": [
        {
            ""opacity"": 1,
            ""url"": ""http://services.arcgisonline.com/ArcGIS/rest/services/World_Terrain_Base/MapServer"",
            ""visibility"": true
        },
        {
            ""isReference"": true,
            ""opacity"": 1,
            ""url"": ""http://services.arcgisonline.com/ArcGIS/rest/services/Reference/World_Reference_Overlay/MapServer"",
            ""visibility"": true
        }
        ],
        ""title"": ""World_Terrain_Base""
    },
    ""operationalLayers"": [
    {
        ""itemId"": ""204d94c9b1374de9a21574c9efa31164"",
        ""opacity"": 0.75,
        ""title"": ""Soil Survey Map"",
        ""url"": ""http://server.arcgisonline.com/ArcGIS/rest/services/Specialty/Soil_Survey_Map/MapServer"",
        ""visibility"": true
    }
    ],
""version"": ""1.1""}";
            JsonTextBox.Text = jsonInput;

        }

        private void Button_Load(object sender, System.Windows.RoutedEventArgs e)
        {
            Document webMapDocument = new Document();
            webMapDocument.GetMapCompleted += webMapDocument_GetMapCompleted;
            webMapDocument.GetMapFromJsonAsync(JsonTextBox.Text);
        }

        void webMapDocument_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                e.Map.Extent = mercator.FromGeographic(new Envelope(-139.4916, 20.7191, -52.392, 59.5199)) as Envelope;
                MyMapGrid.Children.Add(e.Map);
            }
        }

        private void Button_ClearMap(object sender, System.Windows.RoutedEventArgs e)
        {
            MyMapGrid.Children.Clear();
            MyMapGrid.UpdateLayout();
        }
    }
}
