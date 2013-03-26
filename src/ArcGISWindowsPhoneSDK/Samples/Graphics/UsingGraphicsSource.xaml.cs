using Microsoft.Phone.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISWindowsPhoneSDK
{
    public partial class UsingGraphicsSource : PhoneApplicationPage
    {
        public UsingGraphicsSource()
        {
            InitializeComponent();
        }
    }

    public class Customers : ObservableCollection<Graphic>
    {
        Random random;

        private static ESRI.ArcGIS.Client.Projection.WebMercator mercator =
            new ESRI.ArcGIS.Client.Projection.WebMercator();

        public Customers()
        {
            DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(4) };
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            ClearItems();

            random = new Random();

            for (int i = 0; i < 10; i++)
            {
                Graphic g = new Graphic()
                {
                    Geometry = mercator.FromGeographic(new MapPoint(random.Next(-180, 180), random.Next(-90, 90)))
                };

                g.Symbol = new SimpleMarkerSymbol()
                {
                    Color = new SolidColorBrush(Color.FromArgb(255, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255))),
                    Size = 24
                };

                Add(g);
            }
        }
    }

}