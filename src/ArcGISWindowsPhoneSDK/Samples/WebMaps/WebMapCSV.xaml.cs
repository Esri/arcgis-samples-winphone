using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISWindowsPhoneSDK
{
    public partial class WebMapCSV : PhoneApplicationPage
    {
        public WebMapCSV()
        {
            InitializeComponent();

            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            webMap.GetMapAsync("e64c82296b5a48acb0a7f18e3f556607");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
                ContentPanel.Children.Add(e.Map);
        }

    }
}