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
    public partial class TemporalRendererPoints : PhoneApplicationPage
    {

        public static readonly DependencyProperty CurrentDateProperty =
            DependencyProperty.Register("CurrentDate", typeof (DateTime), typeof (TemporalRendererPoints), new PropertyMetadata(default(DateTime)));

        public DateTime CurrentDate
        {
            get { return (DateTime) GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }

        public TemporalRendererPoints()
        {
            InitializeComponent();

            MyMap.TimeExtent = new TimeExtent(); 
            }

        // slider increment is hours and the slider range is 0-168 hours (7 days)
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            DateTime StartDate = DateTime.UtcNow.AddDays(-7);
            //DateTime StartDate= new DateTime(2010, 02, 23, 17, 56, 52, DateTimeKind.Utc); // time extent of layer starts here and is 7 days long
            CurrentDate = StartDate.AddHours(e.NewValue);
            if (MyMap == null)
                return;
            MyMap.TimeExtent.Start = StartDate;
            MyMap.TimeExtent.End = CurrentDate;
            (MyMap.Layers["MyEarthquakeFeatureLayer"] as FeatureLayer).Refresh();
        }

    }
}