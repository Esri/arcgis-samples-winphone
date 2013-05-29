using Microsoft.Phone.Controls;
using System;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Controls;


namespace ArcGISWindowsPhoneSDK
{
    public partial class SymbolJson : PhoneApplicationPage
    {
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
                new ESRI.ArcGIS.Client.Projection.WebMercator();

        public SymbolJson()
        {
            InitializeComponent();

            SimpleMarkerMenuItem_Click(null, null);
        }

        private void GraphicsLayer_Initialized(object sender, EventArgs e)
        {
            GraphicsLayer graphicsLayer = sender as GraphicsLayer;
            foreach (Graphic g in graphicsLayer.Graphics)
            {
                g.Geometry = _mercator.FromGeographic(g.Geometry);

                if (g.Geometry is Polygon || g.Geometry is Envelope)
                    JsonTextBoxFillCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
                else if (g.Geometry is Polyline)
                    JsonTextBoxLineCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
                else
                    JsonTextBoxMarkerCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();

                g.PropertyChanged += g_PropertyChanged;
            }
        }

        void g_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Symbol")
            {
                Graphic g = sender as Graphic;
                if (g.Geometry is Polygon || g.Geometry is Envelope)
                    JsonTextBoxFillCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
                else if (g.Geometry is Polyline)
                    JsonTextBoxLineCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
                else
                    JsonTextBoxMarkerCurrent.Text = (g.Symbol as IJsonSerializable).ToJson();
            }
        }

        private void SimpleMarkerMenuItem_Click(object sender, EventArgs e)
        {
            string jsonString = @"{
    ""type"": ""esriSMS"",
    ""style"": ""esriSMSSquare"",
    ""color"": [76,115,0,255],
    ""size"": 8,
    ""angle"": 0,
    ""xoffset"": 0,
    ""yoffset"": 0,
    ""outline"": 
    {
        ""color"": [152,230,0,255],
        ""width"": 1
    }
}
";
            UseSymbol(jsonString);
        }

        private void PictureMarkerMenuItem_Click(object sender, EventArgs e)
        {
            string jsonString = @"{
	""type"" : ""esriPMS"", 
	""url"" : ""http://static.arcgis.com/images/Symbols/Basic/GreenStickpin.png"", 
	""contentType"" : ""image/png"", 
	""color"" : null, 
	""width"" : 28, 
	""height"" : 28, 
	""angle"" : 0, 
	""xoffset"" : 0, 
	""yoffset"" : 0
}
";
            UseSymbol(jsonString);
        }

        private void SimpleLineMenuItem_Click(object sender, EventArgs e)
        {
            string jsonString = @"{
    ""type"": ""esriSLS"",
    ""style"": ""esriSLSDot"",
    ""color"": [115,76,0,255],
    ""width"": 2
}
";
            UseSymbol(jsonString);
        }

        private void SimpleFillMenuItem_Click(object sender, EventArgs e)
        {
            string jsonString = @"{
    ""type"": ""esriSFS"",
    ""style"": ""esriSFSSolid"",
    ""color"": [250,76,0,150],
    ""outline"": 
    {
        ""type"": ""esriSLS"",
        ""style"": ""esriSLSSolid"",
        ""color"": [110,110,110,255],
        ""width"": 2
    }
}
";
            UseSymbol(jsonString);
        }

        private void PictureFillMenuItem_Click(object sender, EventArgs e)
        {
            string jsonString = @"{
	""type"" : ""esriPFS"", 
	""url"" : ""http://static.arcgis.com/images/Symbols/Transportation/AmberBeacon.png"", 
	""contentType"" : ""image/png"", 
	""color"" : null, 
	""outline"" : 
	{
		""type"" : ""esriSLS"", 
		""style"" : ""esriSLSSolid"", 
		""color"" : [110,110,110,255], 
		""width"" : 1
	}, 
	""width"" : 12, 
	""height"" : 12, 
	""angle"" : 0, 
	""xoffset"" : 0, 
	""yoffset"" : 0, 
	""xscale"" : 1, 
	""yscale"" : 1
  }

";
            UseSymbol(jsonString);
        }

        private void UseSymbol(string symbolJson)
        {
            try
            {
                Symbol symbol = Symbol.FromJson(symbolJson);

                GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

                foreach (Graphic g in graphicsLayer.Graphics)
                {
                    if ((g.Geometry is Polygon || g.Geometry is Envelope) && symbol is FillSymbol)
                        g.Symbol = symbol;
                    else if (g.Geometry is Polyline && symbol is LineSymbol)
                        g.Symbol = symbol;
                    else if (g.Geometry is MapPoint && symbol is MarkerSymbol)
                        g.Symbol = symbol;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Deserializing JSON failed", MessageBoxButton.OK);
            }
        }

        private void JsonButton_Click(object sender, EventArgs e)
        {
            TextBox textbox = ((JsonPivot.SelectedItem as ContentControl).Content as ScrollViewer).Content as TextBox;
            UseSymbol(textbox.Text);
        }
    }
}