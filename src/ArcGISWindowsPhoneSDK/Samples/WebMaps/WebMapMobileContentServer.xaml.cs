using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISWindowsPhoneSDK
{
    public partial class WebMapMobileContentServer : PhoneApplicationPage
    {
        public WebMapMobileContentServer()
        {
            InitializeComponent();
            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;
            webMap.ServerBaseUrl = "http://arcgismobile.esri.com/arcgis/mobile/content";

            webMap.GetMapAsync("00ab0becb052428485a8d25e62afb86d");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
                ContentPanel.Children.Add(e.Map);
        }
    }
}