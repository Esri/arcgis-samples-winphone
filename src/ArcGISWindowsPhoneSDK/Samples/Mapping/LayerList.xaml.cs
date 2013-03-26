using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class LayerList : PhoneApplicationPage
    {
        public LayerList()
        {
            InitializeComponent();
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            LayerListBorder.Visibility = LayerListBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}