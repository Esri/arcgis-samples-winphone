using System;
using ESRI.ArcGIS.Client.Bing;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Net;
using System.Windows;
using System.Runtime.Serialization.Json;

namespace ArcGISWindowsPhoneSDK
{
    public partial class BingImagery : PhoneApplicationPage
    {
        public BingImagery()
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
                        ESRI.ArcGIS.Client.Bing.TileLayer tileLayer = new TileLayer()
                        {
                            ID = "BingLayer",
                            LayerStyle = TileLayer.LayerType.Road,
                            ServerType = ServerType.Production,
                            Token = BingKeyTextBox.Text
                        };

                        MyMap.Layers.Insert(0, tileLayer);

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

        #region Button/Menu methods

        private void Menu_AerialSelected(object sender, EventArgs e)
        {
            SetLayerType(TileLayer.LayerType.Aerial);
        }

        private void Menu_RoadSelected(object sender, EventArgs e)
        {
            SetLayerType(TileLayer.LayerType.Road);
        }

        private void Menu_HybridSelected(object sender, EventArgs e)
        {
            SetLayerType(TileLayer.LayerType.AerialWithLabels);
        }
        private void SetLayerType(TileLayer.LayerType type)
        {
            (MyMap.Layers[0] as TileLayer).LayerStyle = type;
        }

        #endregion
    
    }
}