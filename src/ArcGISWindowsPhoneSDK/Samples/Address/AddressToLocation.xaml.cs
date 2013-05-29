using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class AddressToLocation : PhoneApplicationPage
    {
        GraphicsLayer graphicsLayer;

        public AddressToLocation()
        {
            InitializeComponent();
            graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
        }

        private void FindAddressButton_Click(object sender, RoutedEventArgs e)
        {
            graphicsLayer.ClearGraphics();
            AddressBorder.Visibility = Visibility.Collapsed;
            ResultsTextBlock.Visibility = Visibility.Collapsed;

            Locator locatorTask = new Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");
            locatorTask.AddressToLocationsCompleted += LocatorTask_AddressToLocationsCompleted;
            locatorTask.Failed += LocatorTask_Failed;

            AddressToLocationsParameters addressParams = new AddressToLocationsParameters()
            {
                OutSpatialReference = MyMap.SpatialReference
            };

            Dictionary<string, string> address = addressParams.Address;

            if (!string.IsNullOrEmpty(Address.Text))
                address.Add("Address", Address.Text);
            if (!string.IsNullOrEmpty(City.Text))
                address.Add("City", City.Text);
            if (!string.IsNullOrEmpty(StateAbbrev.Text))
                address.Add("Region", StateAbbrev.Text);
            if (!string.IsNullOrEmpty(Zip.Text))
                address.Add("Postal", Zip.Text);

            locatorTask.AddressToLocationsAsync(addressParams);
        }

        private void LocatorTask_AddressToLocationsCompleted(object sender, ESRI.ArcGIS.Client.Tasks.AddressToLocationsEventArgs args)
        {
            List<AddressCandidate> returnedCandidates = args.Results;            

            AddressBorder.Visibility = System.Windows.Visibility.Collapsed;

            if (returnedCandidates.Count > 0)
            {
                AddressCandidate candidate = returnedCandidates[0];

                Graphic graphic = new Graphic()
                {
                    Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as Symbol,
                    Geometry = candidate.Location
                };

                graphic.Attributes.Add("Address", candidate.Address);

                graphicsLayer.Graphics.Add(graphic);

                ResultsTextBlock.Visibility = System.Windows.Visibility.Visible;
                ResultsTextBlock.Text = candidate.Address;

                double displaySize = MyMap.MinimumResolution * 30;
                ESRI.ArcGIS.Client.Geometry.Envelope displayExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(
                    candidate.Location.X - (displaySize / 2),
                    candidate.Location.Y - (displaySize / 2),
                    candidate.Location.X + (displaySize / 2),
                    candidate.Location.Y + (displaySize / 2));
                MyMap.ZoomTo(displayExtent);
            }
        }

        private void LocatorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Geocode service failed: " + e.Error);
        }

        private void Menu_List_Click(object sender, System.EventArgs e)
        {
            AddressBorder.Visibility = AddressBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}