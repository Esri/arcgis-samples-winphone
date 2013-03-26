using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using Microsoft.Phone.Controls;



namespace ArcGISWindowsPhoneSDK
{
    public partial class ArcGISWebClientSimple : PhoneApplicationPage
    {
        Uri _serverUri;
        ArcGISWebClient _webclient;

        public ArcGISWebClientSimple()
        {
            InitializeComponent();

            _webclient = new ArcGISWebClient();
            _webclient.OpenReadCompleted += webclient_OpenReadCompleted;
            _webclient.DownloadStringCompleted += webclient_DownloadStringCompleted;
        }

        // Get a list of services for an ArcGIS Server site
        private void GetServicesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _serverUri = new Uri(MySvrTextBox.Text);

            // Add the parameter f=json to return a response in json format
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("f", "json");

            // If another request is in progress, cancel it
            if (_webclient.IsBusy)
                _webclient.CancelAsync();

            _webclient.OpenReadAsync(_serverUri, parameters);
        }

        // Show a list of map services in the Listbox
        void webclient_OpenReadCompleted(object sender, ArcGISWebClient.OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                    throw new Exception(e.Error.Message);

                // Deserialize response using classes defined by a data contract, included in this class definition below
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MySvcs));
                MySvcs mysvcs = serializer.ReadObject(e.Result) as MySvcs;

                if (mysvcs.Services.Count == 0)
                    throw new Exception("No services returned");

                // Use LINQ to return all map services
                var mapSvcs = from s in mysvcs.Services
                              where s.Type == "MapServer"
                              select s;

                // If map services are returned, show the Listbox with items as map services
                if (mapSvcs.Count() > 0)
                {
                    MySvcTreeView.ItemsSource = mapSvcs;
                    MySvcTreeView.Visibility = System.Windows.Visibility.Visible;
                    NoMapServicesTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    MySvcTreeView.Visibility = System.Windows.Visibility.Collapsed;
                    NoMapServicesTextBlock.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (e.Result != null)
                    e.Result.Close();
            }
        }

        // Stores site list of map services
        [DataContract]
        public class MySvcs
        {
            [DataMember(Name = "services")]
            public IList<MySvc> Services { get; set; }
        }

        // Defines service item properties in a site list
        [DataContract]
        public class MySvc
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "type")]
            public string Type { get; set; }
        }

        private void MySvcTreeView_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] != null)
            {
                if (_webclient.IsBusy)
                    _webclient.CancelAsync();

                // Clear layers and set Extent to null (reset spatial reference)
                MyMap.Layers.Clear();
                MyMap.Extent = null;

                // Get the service item selected
                MySvc svc = e.AddedItems[0] as MySvc;

                // Construct the url to the map service
                string svcUrl = string.Format("{0}/{1}/{2}", _serverUri, svc.Name, svc.Type);

                IDictionary<string, string> svcParameters = new Dictionary<string, string>();
                svcParameters.Add("f", "json");

                // Pass the map service url as an user object for the handler
                _webclient.DownloadStringAsync(new Uri(svcUrl), svcParameters,
                    ArcGISWebClient.HttpMethods.Auto, svcUrl);
            }
        }

        // When map service item selected in Listbox, choose appropriate type and add to the map
        void webclient_DownloadStringCompleted(object sender, ArcGISWebClient.DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                    throw new Exception(e.Error.Message);

                // Get the service url from the user object
                string svcUrl = e.UserState as string;

                //only available in Silverlight or .Net 4.5
                // Abstract JsonValue holds json response
                // JsonValue serviceInfo = JsonObject.Parse(e.Result);

                string[] jsonPairs = e.Result.Split(',');
                string mapCachePair = jsonPairs.Where(json => json.Contains("singleFusedMapCache")).FirstOrDefault();
                string[] mapCacheKeyAndValue = mapCachePair.Split(':');
                bool isTiledMapService = Boolean.Parse(mapCacheKeyAndValue[1]);

                // Use "singleFusedMapCache" to determine if a tiled or dynamic layer should be added to the map
                //bool isTiledMapService = Boolean.Parse(serviceInfo["singleFusedMapCache"].ToString());

                Layer lyr = null;

                if (isTiledMapService)
                    lyr = new ArcGISTiledMapServiceLayer() { Url = svcUrl };
                else
                    lyr = new ArcGISDynamicMapServiceLayer() { Url = svcUrl };

                if (lyr != null)
                {
                    lyr.InitializationFailed += (a, b) =>
                    {
                        throw new Exception(lyr.InitializationFailure.Message);
                    };
                    MyMap.Layers.Add(lyr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Stores site list of map services
        [DataContract]
        public class FeatureCollectionClass
        {
            [DataMember(Name = "featureCollection")]
            public Dictionary<String, Object>FeatureCollection { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            ToggleShowHidebutton(b);
        }

        private void ToggleShowHidebutton(Button b)
        {
            if (ControlPanel.Visibility == System.Windows.Visibility.Visible)
            {
                ControlPanel.Visibility = System.Windows.Visibility.Collapsed;
                b.Content = "Show";
            }
            else
            {
                ControlPanel.Visibility = System.Windows.Visibility.Visible;
                b.Content = "Hide";
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleShowHidebutton(HideShowButton);
        }

    }
}
