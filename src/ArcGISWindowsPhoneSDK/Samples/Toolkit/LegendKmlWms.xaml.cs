using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class LegendKmlWms : PhoneApplicationPage
    {
        public LegendKmlWms()
        {
            InitializeComponent();
        }

        private void ShowLegend_Click(object sender, System.EventArgs e)
        {
            LegendPage.IsOpen = true;
            ApplicationBar.IsVisible = false;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ApplicationBar.IsVisible = true;
        }
    }
}