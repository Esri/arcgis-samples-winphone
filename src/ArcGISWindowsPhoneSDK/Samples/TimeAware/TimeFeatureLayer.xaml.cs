using System.Device.Location;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class TimeFeatureLayer : PhoneApplicationPage
    {
        List<DateTime> years;
            
        public TimeFeatureLayer()
        {
            InitializeComponent();

            MyMap.TimeExtent = new TimeExtent();
        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            years = new List<DateTime>();
            DateTime dt = (sender as FeatureLayer).TimeExtent.Start;
            while (dt < (sender as FeatureLayer).TimeExtent.End)
            {
                years.Add(dt);
                dt = dt.AddYears(1);
            }
            StartYearListBox.ItemsSource = years;
            StartYearListBox.SelectedIndex = 0;
            EndYearListBox.ItemsSource = years;
            EndYearListBox.SelectedIndex = years.Count - 1;
        }

        private void StartYearListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListBox yearCB = (ListBox)sender;

            if (MyMap.TimeExtent.End != new DateTime() && (DateTime)((sender as ListBox).Items[(sender as ListBox).SelectedIndex]) > MyMap.TimeExtent.End)
            {
                MessageBox.Show("End year must be after start year.");
                return;
            }

            MyMap.TimeExtent.Start = (DateTime)(yearCB.Items[yearCB.SelectedIndex]);
            (MyMap.Layers["EarthquakesLayer"] as FeatureLayer).Refresh();

            StartYearChoicesPage.IsOpen = false;
        }

        private void EndYearListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListBox yearCB = (ListBox)sender;
            if (MyMap.TimeExtent.Start != new DateTime() && (DateTime)((sender as ListBox).Items[(sender as ListBox).SelectedIndex]) < MyMap.TimeExtent.Start)
            {
                MessageBox.Show("End year must be after start year.");
                return;
            }

            MyMap.TimeExtent.End = (DateTime)(yearCB.Items[yearCB.SelectedIndex]);
            (MyMap.Layers["EarthquakesLayer"] as FeatureLayer).Refresh();

            EndYearChoicesPage.IsOpen = false;
        }

        private void StartYearButton_Click(object sender, RoutedEventArgs e)
        {
            StartYearChoicesPage.IsOpen = true;
        }

        private void EndYearButton_Click(object sender, RoutedEventArgs e)
        {
            EndYearChoicesPage.IsOpen = true;
        }
    }
}