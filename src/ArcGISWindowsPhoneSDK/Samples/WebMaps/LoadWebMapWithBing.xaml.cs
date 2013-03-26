using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Net;
using System.Runtime.Serialization.Json;

namespace ArcGISWindowsPhoneSDK
{
    public partial class LoadWebMapWithBing : PhoneApplicationPage
    {
        public LoadWebMapWithBing()
        {
            InitializeComponent();
        }

        #region support entry of a Bing Key
        private void BingKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length >= 64)
                LoadMapButton.IsEnabled = true;
            else
                LoadMapButton.IsEnabled = false;
        }
        [DataContract]
        public class BingAuthentication
        {
            [DataMember(Name = "authenticationResultCode")]
            public string authenticationResultCode { get; set; }
        }
        private void LoadMapButton_Click(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();
            string uri = string.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text);

            webClient.OpenReadCompleted += (s, a) =>
            {
                if (a.Error == null)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BingAuthentication));
                    BingAuthentication bingAuthentication = serializer.ReadObject(a.Result) as BingAuthentication;
                    a.Result.Close();

                    if (bingAuthentication.authenticationResultCode == "ValidCredentials")
                    {
                        Document webMap = new Document();
                        webMap.BingToken = BingKeyTextBox.Text;
                        webMap.GetMapCompleted += webMap_GetMapCompleted;

                        webMap.GetMapAsync("75e222ec54b244a5b73aeef40ce76adc");

                        BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed;
                        InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
                }
                else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
            };

            webClient.OpenReadAsync(new System.Uri(uri));
        }
        #endregion

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
                ContentPanel.Children.Add(e.Map);
        } 
    }
}