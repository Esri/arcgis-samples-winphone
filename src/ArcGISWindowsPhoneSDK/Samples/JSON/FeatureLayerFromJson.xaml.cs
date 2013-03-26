using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using ESRI.ArcGIS.Client;

namespace ArcGISWindowsPhoneSDK
{
    public partial class FeatureLayerFromJson : PhoneApplicationPage
    {
        string jsonInput;

        public FeatureLayerFromJson()
        {
            InitializeComponent();

            CreateFeatureLayerJson();
        }

        private void CreateFeatureLayerJson()
        {
            jsonInput = @"{
  ""layerDefinition"": {
    ""name"": ""Feature Notes - Points"",
    ""type"": ""Feature Layer"",
    ""displayField"": ""TITLE"",
    ""geometryType"": ""esriGeometryPoint"",
    ""drawingInfo"": {
      ""renderer"": {
        ""type"": ""uniqueValue"",
        ""field1"": ""TYPEID"",
        ""uniqueValueInfos"": [
          {
            ""value"": ""0"",
            ""label"": ""Stickpin"",
            ""description"": """",
            ""symbol"": {
              ""type"": ""esriPMS"",
              ""url"": ""http://static.arcgis.com/images/Symbols/Basic/GreenStickpin.png"",
              ""contentType"": ""image/png"",
              ""width"": 24,
              ""height"": 24,
              ""xoffset"": 0,
              ""yoffset"": 12
            }
          },
          {
            ""value"": ""1"",
            ""label"": ""Pushpin"",
            ""symbol"": {
              ""type"": ""esriPMS"",
              ""url"": ""http://static.arcgis.com/images/Symbols/Basic/GreenShinyPin.png"",
              ""contentType"": ""image/png"",
              ""width"": 24,
              ""height"": 24,
              ""xoffset"": 2,
              ""yoffset"": 8
            }
          }
        ]
      }
    },
    ""objectIdField"": ""OBJECTID"",
    ""fields"": [
      {
        ""name"": ""OBJECTID"",
        ""type"": ""esriFieldTypeOID"",
        ""alias"": ""OBJECTID"",
        ""editable"": false
      },
      {
        ""name"": ""TITLE"",
        ""type"": ""esriFieldTypeString"",
        ""alias"": ""Title"",
        ""editable"": true,
        ""length"": 50
      },
      {
        ""name"": ""DESCRIPTION"",
        ""type"": ""esriFieldTypeString"",
        ""alias"": ""Description"",
        ""editable"": true,
        ""length"": 1073741822
      },
      {
        ""name"": ""TYPEID"",
        ""type"": ""esriFieldTypeInteger"",
        ""alias"": ""Type ID"",
        ""editable"": true
      }
    ],
    ""types"": [
      {
        ""id"": 0,
        ""name"": ""Stickpin"",
        ""domains"": {
          
        },
        ""templates"": [
          {
            ""name"": ""Stickpin"",
            ""description"": """",
            ""drawingTool"": ""esriFeatureEditToolPoint"",
            ""prototype"": {
              ""attributes"": {
                ""TYPEID"": 0,
                ""VISIBLE"": 1,
                ""TITLE"": ""Point""
              }
            }
          }
        ]
      },
      {
        ""id"": 1,
        ""name"": ""Pushpin"",
        ""domains"": {
          
        },
        ""templates"": [
          {
            ""name"": ""Pushpin"",
            ""description"": """",
            ""drawingTool"": ""esriFeatureEditToolPoint"",
            ""prototype"": {
              ""attributes"": {
                ""TYPEID"": 1,
                ""VISIBLE"": 1,
                ""TITLE"": ""Point""
              }
            }
          }
        ]
      }
    ]
  },
  ""featureSet"": {
    ""geometryType"": ""esriGeometryPoint"",
    ""features"": [
      {
        ""geometry"": {
          ""x"": -1.359478966190899E7,
          ""y"": 6040655.3174108695,
          ""spatialReference"": {
            ""wkid"": 102100
          }
        },
        ""attributes"": {
          ""TYPEID"": 0,
          ""TITLE"": ""Seattle"",
          ""OBJECTID"": 0,
          ""DESCRIPTION"": ""The Emerald City""
        },
        ""symbol"": {
          ""angle"": 0,
          ""xoffset"": 12,
          ""yoffset"": 12,
          ""type"": ""esriPMS"",
          ""url"": ""http://static.arcgis.com/images/Symbols/Basic/GreenFlag.png"",
          ""width"": 24,
          ""height"": 24
        }
      },
      {
        ""geometry"": {
          ""x"": -1.0356305647523504E7,
          ""y"": 4059407.5442596273,
          ""spatialReference"": {
            ""wkid"": 102100
          }
        },
        ""attributes"": {
          ""TYPEID"": 0,
          ""TITLE"": ""Cajun Capital"",
          ""OBJECTID"": 1,
          ""DESCRIPTION"": ""Mardi Gras!""
        },
        ""symbol"": {
          ""angle"": 0,
          ""xoffset"": 12,
          ""yoffset"": 12,
          ""type"": ""esriPMS"",
          ""url"": ""http://static.arcgis.com/images/Symbols/Basic/RedFlag.png"",
          ""width"": 24,
          ""height"": 24
        }
      },
      {
        ""geometry"": {
          ""x"": -1.3531196670179205E7,
          ""y"": 5908575.52289088,
          ""spatialReference"": {
            ""wkid"": 102100
          }
        },
        ""attributes"": {
          ""TYPEID"": 1,
          ""TITLE"": ""Mt St. Helens"",
          ""OBJECTID"": 2,
          ""DESCRIPTION"": ""Fantastic View!""
        }
      },
      {
        ""geometry"": {
          ""x"": -1.2406043613821708E7,
          ""y"": 5497650.058829881,
          ""spatialReference"": {
            ""wkid"": 102100
          }
        },
        ""attributes"": {
          ""TYPEID"": 1,
          ""TITLE"": ""Yellowstone Park"",
          ""OBJECTID"": 3,
          ""DESCRIPTION"": ""America's first national park""
        }
      },
      {
        ""geometry"": {
          ""x"": -1.1231970859361714E7,
          ""y"": 4299117.455318636,
          ""spatialReference"": {
            ""wkid"": 102100
          }
        },
        ""attributes"": {
          ""TYPEID"": 1,
          ""TITLE"": ""Dust bowl"",
          ""OBJECTID"": 4,
          ""DESCRIPTION"": ""Time to go to California""
        }
      }
    ]
  }
}";

            JsonBrowser.NavigateToString("<html><head><title>JSON Input</title></head><body>" + jsonInput.Replace("\r\n", "<br/>").Replace(" ", "&nbsp;") + "</body></html>");
        }

        private void ClearMapButton_Click(object sender, EventArgs e)
        {
            List<FeatureLayer> featureLayers = new List<FeatureLayer>();

            foreach (Layer layer in MyMap.Layers)
                if (layer is FeatureLayer)
                    featureLayers.Add(layer as FeatureLayer);

            for (int i = 0; i < featureLayers.Count; i++)
                MyMap.Layers.Remove(featureLayers[i]);
        }

        private void CreateFeatureLayerButton_Click(object sender, EventArgs e)
        {
            try
            {
                FeatureLayer featureLayer = FeatureLayer.FromJson(jsonInput);
                featureLayer.RendererTakesPrecedence = false;
                MyMap.Layers.Add(featureLayer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "FeatureLayer creation failed", MessageBoxButton.OK);
            }
        }

        private void JsonButton_Click(object sender, EventArgs e)
        {
            JsonBrowser.Visibility = JsonBrowser.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}