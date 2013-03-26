using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Portal;
using ESRI.ArcGIS.Client.WebMap;

namespace ArcGISWindowsPhoneSDK
{
    public partial class PortalMetadata : PhoneApplicationPage
    {
        public PortalMetadata()
        {
            InitializeComponent();
        }

        private void LoadPortalInfo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PropertiesListBox.Items.Clear();
            if (String.IsNullOrEmpty(PortalUrltextbox.Text))
                return;
            InitializePortal(PortalUrltextbox.Text);
        }

        public void InitializePortal(string PortalUrl)
        {
            ArcGISPortal arcgisPortal = new ArcGISPortal();
            arcgisPortal.InitializeAsync(PortalUrl, (p, ex) =>
            {
                if (ex == null)
                {
                    ArcGISPortalInfo portalInfo = p.ArcGISPortalInfo;
                    if (portalInfo == null)
                    {
                        MessageBox.Show("Portal Information could not be retrieved");
                        return;
                    }
                    PropertiesListBox.Items.Add(string.Format("Current Version: {0}", p.CurrentVersion));
                    PropertiesListBox.Items.Add(string.Format("Access: {0}", portalInfo.Access));
                    PropertiesListBox.Items.Add(string.Format("Host Name: {0}", portalInfo.PortalHostname));
                    PropertiesListBox.Items.Add(string.Format("Name: {0}", portalInfo.PortalName));
                    PropertiesListBox.Items.Add(string.Format("Mode: {0}", portalInfo.PortalMode));

                    ESRI.ArcGIS.Client.WebMap.BaseMap basemap = portalInfo.DefaultBaseMap;

                    PropertiesListBox.Items.Add(string.Format("Default BaseMap Title: {0}", basemap.Title));
                    PropertiesListBox.Items.Add(string.Format("WebMap Layers ({0}):", basemap.Layers.Count));

                    foreach (WebMapLayer webmapLayer in basemap.Layers)
                    {
                        PropertiesListBox.Items.Add(webmapLayer.Url);
                    }

                    portalInfo.GetFeaturedGroupsAsync((portalgroup, exp) =>
                    {
                        if (exp == null)
                        {                            
                            GroupsList.ItemsSource = portalgroup;
                            (ApplicationBar.Buttons[1] as IApplicationBarIconButton).IsEnabled = true;
                        }
                    });

                    portalInfo.SearchFeaturedItemsAsync(new SearchParameters() { Limit = 15 }, (result, err) =>
                    {
                        if (err == null)
                        {
                            FeaturedMapsList.ItemsSource = result.Results;
                            (ApplicationBar.Buttons[0] as IApplicationBarIconButton).IsEnabled = true;
                        }
                    });
                }
                else
                    MessageBox.Show("Failed to initialize" + ex.Message.ToString());
            });
        }

        private void ShowMapsButton_Click(object sender, EventArgs e)
        {
            MapsPage.IsOpen = true;
            ApplicationBar.IsVisible = false;
        }

        private void ShowGroupsButton_Click(object sender, EventArgs e)
        {
            GroupsPage.IsOpen = true;
            ApplicationBar.IsVisible = false;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (MapsPage.IsOpen || GroupsPage.IsOpen)
            {
                ApplicationBar.IsVisible = true;
                e.Cancel = true;
            }
        }
    }
}