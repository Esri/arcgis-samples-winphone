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
    public partial class TimeMapService : PhoneApplicationPage
    {
        DateTime startDT = new DateTime();
        DateTime endDT = new DateTime();
        List<int> years;

        public TimeMapService()
        {
            InitializeComponent();

            MyMap.TimeExtent = new TimeExtent();
        }

        private void ArcGISDynamicMapServiceLayer_Initialized(object sender, EventArgs e)
        {
            List<int> months = new List<int>();
            for (int i = 1; i <= 12; i++)
            {
                months.Add(i);
            }
            StartMonthListBox.ItemsSource = months;
            EndMonthListBox.ItemsSource = months;
            StartMonthListBox.SelectedIndex = 0;
            EndMonthListBox.SelectedIndex = 0;

            years = new List<int>();
            DateTime dt = (sender as ArcGISDynamicMapServiceLayer).TimeExtent.Start;
            while (dt < (sender as ArcGISDynamicMapServiceLayer).TimeExtent.End)
            {
                years.Add(dt.Year);
                dt = dt.AddYears(1);
            }
            StartYearListBox.ItemsSource = years;
            EndYearListBox.ItemsSource = years;
            StartYearListBox.SelectedIndex = 0;
            EndYearListBox.SelectedIndex = 10;

            List<int> days = new List<int>();  // set this when a month is selected
            for (int i = 1; i <= 31; i++)
            {
                days.Add(i);
            }
            StartDayListBox.ItemsSource = days;
            EndDayListBox.ItemsSource = days;
            StartDayListBox.SelectedIndex = 0;
            EndDayListBox.SelectedIndex = 0;

            MyMap.TimeExtent = new TimeExtent(startDT, endDT);
        }

        private void StartMonthListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            startDT = new DateTime(startDT.Year, ((ListBox)sender).SelectedIndex + 1, startDT.Day);
            StartMonthChoicesPage.IsOpen = false;
        }

        private void StartDayListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            startDT = new DateTime(startDT.Year, startDT.Month, ((ListBox)sender).SelectedIndex + 1);
            StartDayChoicesPage.IsOpen = false;
        }

        private void StartYearListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            startDT = new DateTime(years[((ListBox)sender).SelectedIndex], startDT.Month, startDT.Day);
            StartYearChoicesPage.IsOpen = false;
        }

        private void EndMonthListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            endDT = new DateTime(endDT.Year, ((ListBox)sender).SelectedIndex + 1, endDT.Day);
            EndMonthChoicesPage.IsOpen = false;
        }

        private void EndDayListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            endDT = new DateTime(endDT.Year, endDT.Month, ((ListBox)sender).SelectedIndex + 1);
            EndDayChoicesPage.IsOpen = false;
        }

        private void EndYearListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            endDT = new DateTime(years[((ListBox)sender).SelectedIndex], endDT.Month, endDT.Day);
            EndYearChoicesPage.IsOpen = false;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (startDT > endDT)
            {
                MessageBox.Show("End date must be after start date.");
                return;
            }
            MyMap.TimeExtent = new TimeExtent(startDT, endDT);
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            LegendBorder.Visibility = LegendBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Menu_Dialog_Click(object sender, EventArgs e)
        {
            TimeInfoGrid.Visibility = TimeInfoGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void DateButton_Click(object sender, RoutedEventArgs e)
        {
            string sendingButton = (sender as Button).Tag.ToString();
            switch (sendingButton)
            {
                case "StartMonth":
                    StartMonthChoicesPage.IsOpen = true;
                    return;
                case "StartDay":
                    StartDayChoicesPage.IsOpen = true;
                    return;
                case "StartYear":
                    StartYearChoicesPage.IsOpen = true;
                    return;
                case "EndMonth":
                    EndMonthChoicesPage.IsOpen = true;
                    return;
                case "EndDay":
                    EndDayChoicesPage.IsOpen = true;
                    return;
                case "EndYear":
                    EndYearChoicesPage.IsOpen = true;
                    return;
                default:
                    return;
            }
        }
    }
}