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
            Locator locatorTask = new Locator("http://tasks.arcgisonline.com/ArcGIS/rest/services/Locators/TA_Streets_US_10/GeocodeServer");
            locatorTask.AddressToLocationsCompleted += LocatorTask_AddressToLocationsCompleted;
            locatorTask.Failed += LocatorTask_Failed;
            AddressToLocationsParameters addressParams = new AddressToLocationsParameters();
            Dictionary<string, string> address = addressParams.Address;

            if (!string.IsNullOrEmpty(StateAbbrev.Text))
                address.Add("Street", Address.Text);
            if (!string.IsNullOrEmpty(City.Text))
                address.Add("City", City.Text);
            if (!string.IsNullOrEmpty(StateAbbrev.Text))
                address.Add("State", StateAbbrev.Text);
            if (!string.IsNullOrEmpty(Zip.Text))
                address.Add("ZIP", Zip.Text);

            locatorTask.AddressToLocationsAsync(addressParams);
        }

        private void LocatorTask_AddressToLocationsCompleted(object sender, ESRI.ArcGIS.Client.Tasks.AddressToLocationsEventArgs args)
        {
            List<AddressCandidate> returnedCandidates = args.Results;
            graphicsLayer.ClearGraphics();

            AddressGrid.Visibility = System.Windows.Visibility.Collapsed;

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
                
                ResultsTextBlock.Text = candidate.Address;

                double displaySize = MyMap.MinimumResolution * 30;
                ESRI.ArcGIS.Client.Geometry.Envelope displayExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(
                    candidate.Location.X - (displaySize / 2),
                    candidate.Location.Y - (displaySize / 2),
                    candidate.Location.X + (displaySize / 2),
                    candidate.Location.Y + (displaySize / 2));
                MyMap.ZoomTo(displayExtent);
            }
            else
            {
                CandidatePanel.Visibility = System.Windows.Visibility.Visible;
                MessageTextBlock.Text = "Address not found";
                CandidatePanel.SetValue(Canvas.LeftProperty,
                    ((MyMap.ActualWidth - MessageTextBlock.ActualWidth) / 2) - MessageTextBlock.Margin.Left);
                CandidatePanel.SetValue(Canvas.TopProperty, (MyMap.ActualHeight / 2));
            }
        }

        private void LocatorTask_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Locator service failed: " + e.Error);
        }

        private void Menu_List_Click(object sender, System.EventArgs e)
        {
            AddressGrid.Visibility = AddressGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (AddressGrid.Visibility == Visibility.Visible)
                CandidatePanel.Visibility = Visibility.Collapsed;
        }

        private void MyMap_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Height == 0)
            {
                MyMap.Width = e.NewSize.Width / (MyMap.RenderTransform as ScaleTransform).ScaleX;
                MyMap.Height = e.NewSize.Height / (MyMap.RenderTransform as ScaleTransform).ScaleY;
            }
        }

        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture != GestureType.Completed && e.Gesture != GestureType.Started)
                CandidatePanel.Visibility = Visibility.Collapsed;

            if (e.Gesture == GestureType.Tap)
            {
                GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
                IEnumerable<Graphic> graphics = e.DirectlyOver(10, new GraphicsLayer[] { graphicsLayer });

                foreach (Graphic g in graphics)
                {
                    MessageTextBlock.Text = g.Attributes["Address"] as string;

                    double left = ((MyMap.MapToScreen(g.Geometry as MapPoint).X *
                        (MyMap.RenderTransform as ScaleTransform).ScaleX)) - (MessageTextBlock.ActualWidth / 2);
                    double top = (MyMap.MapToScreen(g.Geometry as MapPoint).Y *
                        (MyMap.RenderTransform as ScaleTransform).ScaleY);

                    CandidatePanel.SetValue(Canvas.LeftProperty, left);
                    CandidatePanel.SetValue(Canvas.TopProperty, top);
                    CandidatePanel.Visibility = System.Windows.Visibility.Visible;
                    break;
                }
            }
        }
    }
}