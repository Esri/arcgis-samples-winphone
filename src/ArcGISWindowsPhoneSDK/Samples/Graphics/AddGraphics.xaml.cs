using System;
using System.Collections.Generic;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class AddGraphics : PhoneApplicationPage
    {
        private ESRI.ArcGIS.Client.Projection.WebMercator wm = new ESRI.ArcGIS.Client.Projection.WebMercator();

        public AddGraphics()
        {
            InitializeComponent();
 
            AddMarkerGraphics();
            AddPictureMarkerAndTextGraphics();
            AddLineGraphics();
            AddPolygonGraphics();
        }

        void AddMarkerGraphics()
        {
            List<string> coordList = new List<string>(){"1346270,7357522", "8181982,2045495", "6011252,4383204"};
            
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            for (int i = 0; i < coordList.Count; i++)
            {
                string[] xy = coordList[i].Split(',');
                MapPoint point = new MapPoint(Convert.ToDouble(xy[0]), Convert.ToDouble(xy[1]));
                Graphic graphic = new Graphic()
                    {
                        Geometry = point,
                        Symbol = i > 0 ? RedMarkerSymbol : BlackMarkerSymbol
                    };
                graphicsLayer.Graphics.Add(graphic);
            }

        }

        private void AddPictureMarkerAndTextGraphics()
        {
            string gpsNMEASentences = "$GPGGA, 92204.9, -35.6334, N, -60.2343, W, 1, 04, 2.4, 25.7, M,,,,*75\r\n" +
                                     "$GPGGA, 92510.5, -49.9334, N, -65.2131, W, 1, 04, 2.6, 1.7, M,,,,*75\r\n";
            string[] gpsNMEASentenceArray = gpsNMEASentences.Split('\n');

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            for (int i = 0; i < gpsNMEASentenceArray.Length - 1; i++)
            {
                string[] gpsNMEASentence = gpsNMEASentenceArray[i].Split(',');
                MapPoint mp = new MapPoint(Convert.ToDouble(gpsNMEASentence[4]), Convert.ToDouble(gpsNMEASentence[2]));
                MapPoint conv_mp = wm.FromGeographic(mp) as MapPoint;

                Graphic graphic = new Graphic()
                {
                    Geometry = conv_mp,
                    Symbol = GlobePictureSymbol
                };

                graphicsLayer.Graphics.Add(graphic);


                TextSymbol textSymbol = new TextSymbol()
                {
                    FontFamily = new System.Windows.Media.FontFamily("Arial"),
                    Foreground = new System.Windows.Media.SolidColorBrush(Colors.Black),
                    FontSize = 18,
                    Text = gpsNMEASentence[9]
                };

                Graphic graphicText = new Graphic()
                {
                    Geometry = conv_mp,
                    Symbol = textSymbol
                };

                graphicsLayer.Graphics.Add(graphicText);
            }
        }

        private void AddLineGraphics()
        {
            string geoRSSLine = @"<?xml version='1.0' encoding='utf-8'?>
                                    <feed xmlns='http://www.w3.org/2005/Atom' xmlns:georss='http://www.georss.org/georss'>
                                    <georss:line>-118.169, 34.016, -104.941, 39.7072, -96.724, 32.732</georss:line>
                                    <georss:line>-28.69, 14.16, -14.91, 23.702, -1.74, 13.72</georss:line>
                                </feed>";

            List<ESRI.ArcGIS.Client.Geometry.Polyline> polylineList = new List<ESRI.ArcGIS.Client.Geometry.Polyline>();

            using (System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(new System.IO.StringReader(geoRSSLine)))
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case System.Xml.XmlNodeType.Element:
                            string nodeName = xmlReader.Name;
                            if (nodeName == "georss:line")
                            {
                                string lineString = xmlReader.ReadElementContentAsString();

                                string[] lineCoords = lineString.Split(',');

                                ESRI.ArcGIS.Client.Geometry.PointCollection pointCollection = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                                for (int i = 0; i < lineCoords.Length; i += 2)
                                {
                                    MapPoint mp = new MapPoint(Convert.ToDouble(lineCoords[i]), Convert.ToDouble(lineCoords[i + 1]));
                                    MapPoint conv_mp = wm.FromGeographic(mp) as MapPoint;
                                    pointCollection.Add(conv_mp);
                                }

                                ESRI.ArcGIS.Client.Geometry.Polyline polyline = new ESRI.ArcGIS.Client.Geometry.Polyline();
                                polyline.Paths.Add(pointCollection);

                                polylineList.Add(polyline);

                            }
                            break;
                    }
                }
            }

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            foreach (ESRI.ArcGIS.Client.Geometry.Polyline polyline in polylineList)
            {
                Graphic graphic = new Graphic()
                {
                    Symbol = DefaultLineSymbol,
                    Geometry = polyline
                };

                graphicsLayer.Graphics.Add(graphic);
            }
        }


        private void AddPolygonGraphics()
        {
            string coordinateString1 = "14819406,1294088 13066124,751406 13191358,2880391 15570812,2713412 14819406,1294088";

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            PointCollectionConverter pointConverter = new PointCollectionConverter();
            ESRI.ArcGIS.Client.Geometry.PointCollection pointCollection1 =
                pointConverter.ConvertFromString(coordinateString1) as ESRI.ArcGIS.Client.Geometry.PointCollection;

            ESRI.ArcGIS.Client.Geometry.Polygon polygon1 = new ESRI.ArcGIS.Client.Geometry.Polygon();
            polygon1.Rings.Add(pointCollection1);

            Graphic graphic = new Graphic()
            {
                Geometry = polygon1,
                Symbol = DefaultFillSymbol
            };

            graphicsLayer.Graphics.Add(graphic);
        }
    }
}