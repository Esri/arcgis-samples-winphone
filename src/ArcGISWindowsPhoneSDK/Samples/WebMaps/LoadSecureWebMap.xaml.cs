using Microsoft.Phone.Controls;
using System;
using System.Net;
using System.Windows;
using System.Runtime.Serialization.Json;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using System.Runtime.Serialization;

namespace ArcGISWindowsPhoneSDK
{
    public partial class LoadSecureWebMap : PhoneApplicationPage
    {
        public LoadSecureWebMap()
        {
            InitializeComponent();
        }

        private void LoadWebMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IdentityManager.Current.GenerateCredentialAsync("https://www.arcgis.com", UsernameTextBox.Text, PasswordTextBox.Password,
                (credential, ex) =>
                {
                    if (ex == null)
                    {
                        Document webMap = new Document();
                        webMap.Token = credential.Token;
                        webMap.GetMapCompleted += webMap_GetMapCompleted;
                        webMap.GetMapAsync(WebMapTextBox.Text);
                    }
                }, null);
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(string.Format("Unable to load webmap. {0}", e.Error.Message));
            else
                ContentPanel.Children.Insert(0, e.Map);
        }

        [DataContract]
        public class TokenDataContract
        {
            [DataMember(Name = "token")]
            public string Token { get; set; }
        }
    }
}