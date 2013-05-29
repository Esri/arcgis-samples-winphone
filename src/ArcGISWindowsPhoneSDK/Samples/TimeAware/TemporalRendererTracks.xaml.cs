using System.Device.Location;
using System.Windows;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using ESRI.ArcGIS.Client;

namespace ArcGISWindowsPhoneSDK
{
    public partial class TemporalRendererTracks : PhoneApplicationPage
    {

        public static readonly DependencyProperty CurrentDateProperty =
            DependencyProperty.Register("CurrentDate", typeof (DateTime), typeof (TemporalRendererTracks), new PropertyMetadata(default(DateTime)));

        public DateTime CurrentDate
        {
            get { return (DateTime) GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }

        public TemporalRendererTracks()
        {
            InitializeComponent();

            MyMap.TimeExtent = new TimeExtent(); 
            }

        // slider increment is hours and the slider range is 0-168 hours (7 days)
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            DateTime StartTime= new DateTime(2000, 08, 04, 02, 00, 00, DateTimeKind.Utc);
            //DateTime EndTime= new DateTime(2000, 10, 22, 08, 00, 00, DateTimeKind.Utc);

            //TimeSpan diff = EndTime.Subtract(StartTime);
            //int days = (int)Math.Floor(diff.TotalDays);

            CurrentDate = StartTime.AddDays(e.NewValue);
            if (MyMap == null)
                return;
            MyMap.TimeExtent.Start = StartTime;
            MyMap.TimeExtent.End = CurrentDate;
            (MyMap.Layers["MyHurricaneFeatureLayer"] as FeatureLayer).Refresh();
        }

    }
}