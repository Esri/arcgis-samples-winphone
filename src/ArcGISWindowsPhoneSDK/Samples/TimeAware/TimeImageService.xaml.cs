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
    public partial class TimeImageService : PhoneApplicationPage
    {
        public TimeImageService()
        {
            InitializeComponent();

            MyMap.TimeExtent = new TimeExtent(); 
            MonthSlider.ValueChanged += Month_ValueChanged;
            MonthSlider.Value = 1;
        }

        private void Month_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider monthSlider = sender as Slider;
            DateTime selected = new DateTime(2004, Convert.ToInt32(monthSlider.Value), 1, 0, 0, 0, DateTimeKind.Utc);
            MyMap.TimeExtent.Start = selected;
            MyMap.TimeExtent.End = selected;
            (MyMap.Layers["ImageSrvLayer"] as ArcGISImageServiceLayer).Refresh();
        }
    }

    public class MonthIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                double val = (double)value;
                if (val < 1 || val > 12)
                    return "";

                DateTimeFormatInfo info = DateTimeFormatInfo.GetInstance(null);
                return info.GetMonthName(System.Convert.ToInt32(val));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}