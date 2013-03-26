using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Windows;
using ESRI.ArcGIS.Client.Printing;

namespace ArcGISWindowsPhoneSDK
{
    public partial class ExportWebMap : PhoneApplicationPage
    {
        PrintTask printTask;

        public ExportWebMap()
        {
            InitializeComponent();

            printTask = new PrintTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task");
            printTask.DisableClientCaching = true;
            printTask.ExecuteCompleted += printTask_PrintCompleted;
            printTask.GetServiceInfoCompleted += printTask_GetServiceInfoCompleted;
            printTask.GetServiceInfoAsync();
        }

        private void printTask_GetServiceInfoCompleted(object sender, ServiceInfoEventArgs e)
        {
            LayoutTemplates.ItemsSource = e.ServiceInfo.LayoutTemplates;
            Formats.ItemsSource = e.ServiceInfo.Formats;
        }

        private void printTask_PrintCompleted(object sender, PrintEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = e.PrintResult.Url;
            webBrowserTask.Show();
        }

        private void ExportMap_Click(object sender, RoutedEventArgs e)
        {
            if (printTask == null || Formats.SelectedIndex < 0 || printTask.IsBusy) return;

            PrintParameters printParameters = new PrintParameters(MyMap)
            {
                ExportOptions = new ExportOptions() { Dpi = 96, OutputSize = new Size(MyMap.ActualWidth, MyMap.ActualHeight) },
                LayoutTemplate = (string)LayoutTemplates.SelectedItem ?? string.Empty,
                Format = (string)Formats.SelectedItem,

            };
            printTask.ExecuteAsync(printParameters);
        }

        private void LayoutTemplatesButton_Click(object sender, RoutedEventArgs e)
        {
            LayoutTemplatesPage.IsOpen = true;
        }
        private void LayoutTemplates_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = LayoutTemplates.SelectedIndex;
            if (index > -1)
            {
                LayoutTemplatesPage.IsOpen = false;
                LayoutTemplatesButton.Content = (string)LayoutTemplates.SelectedItem;
            }
        }

        private void FormatsButton_Click(object sender, RoutedEventArgs e)
        {
            FormatsPage.IsOpen = true;
        }
        private void Formats_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = Formats.SelectedIndex;
            if (index > -1)
            {
                FormatsPage.IsOpen = false;
                FormatsButton.Content = (string)Formats.SelectedItem;
            }
        }

        private void ShowOptionsButton_Click(object sender, System.EventArgs e)
        {
            ExportOptions.Visibility = ExportOptions.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}