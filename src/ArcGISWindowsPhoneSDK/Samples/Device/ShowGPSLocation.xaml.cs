using System.Device.Location;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class ShowGPSLocation : PhoneApplicationPage
    {
        GeoCoordinateWatcher _watcher;
        Graphic _graphicLocation;
        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator =
          new ESRI.ArcGIS.Client.Projection.WebMercator();
        bool initialLoad = true;

        public ShowGPSLocation()
        {
            InitializeComponent();

            _graphicLocation = (MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer).Graphics[0];

            _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            _watcher.MovementThreshold = 20;
            
            _watcher.StatusChanged += watcher_StatusChanged;
            _watcher.PositionChanged += watcher_PositionChanged;

            // Start data acquisition
            _watcher.Start();
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    // The location service is disabled or unsupported.
                    // Alert the user
                    StatusTextBlock.Text = "Location is unsupported on this device";
                    break;
                case GeoPositionStatus.Initializing:
                    // The location service is initializing.
                    // Disable the Start Location button
                    StatusTextBlock.Text = "Initializing location service";
                    break;
                case GeoPositionStatus.NoData:
                    // The location service is working, but it cannot get location data
                    // Alert the user and enable the Stop Location button
                    StatusTextBlock.Text = "Data unavailable";
                    break;
                case GeoPositionStatus.Ready:
                    // The location service is working and is receiving location data
                    // Show the current position and enable the Stop Location button
                    StatusTextBlock.Text = "Ready - retrieving data";
                    break;
            }
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            _graphicLocation.Geometry = mercator.FromGeographic(new MapPoint(e.Position.Location.Longitude, e.Position.Location.Latitude));

            // Use horizontal accuracy (returned in meters) to zoom to the location
            if (initialLoad)
            {
                Envelope rect = new Envelope(
                    (_graphicLocation.Geometry as MapPoint).X - (e.Position.Location.HorizontalAccuracy / 2),
                    (_graphicLocation.Geometry as MapPoint).Y - (e.Position.Location.HorizontalAccuracy / 2),
                    (_graphicLocation.Geometry as MapPoint).X + (e.Position.Location.HorizontalAccuracy / 2),
                    (_graphicLocation.Geometry as MapPoint).Y + (e.Position.Location.HorizontalAccuracy / 2));

                MyMap.ZoomTo(rect.Expand(20));

                initialLoad = false;
            }
            else
            {
                MyMap.PanTo(_graphicLocation.Geometry);
            }
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _watcher.Stop();
        }
    }
}