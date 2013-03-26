using System.Windows;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class ElementLayer : PhoneApplicationPage
    {
        public ElementLayer()
        {
            InitializeComponent();
        }

        private void RedlandsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You found Redlands");
        }
    }
}