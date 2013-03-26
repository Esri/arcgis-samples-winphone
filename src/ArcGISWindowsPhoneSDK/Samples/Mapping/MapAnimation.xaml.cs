using Microsoft.Phone.Controls;
using System;
using System.Windows;

namespace ArcGISWindowsPhoneSDK
{
    public partial class MapAnimation : PhoneApplicationPage
    {
        public MapAnimation()
        {
            InitializeComponent();
        }

        private void ZoomAnimation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int seconds = Convert.ToInt32(e.NewValue);
            MyMap.ZoomDuration = new TimeSpan(0, 0, seconds);
            ZoomValueLabel.Text = string.Format("Value: {0}", seconds);
        }

        private void PanAnimation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int seconds = Convert.ToInt32(e.NewValue);
            MyMap.PanDuration = new TimeSpan(0, 0, seconds);
            PanValueLabel.Text = string.Format("Value: {0}", seconds);
        }

        private void InfoButton_Click(object sender, System.EventArgs e)
        {
            InfoCanvas.Visibility = InfoCanvas.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}