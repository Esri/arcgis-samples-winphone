using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class ElementLayer_Media : PhoneApplicationPage
    {
        public ElementLayer_Media()
        {
            InitializeComponent();
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs args)
        {
            // Repeat play of the video
            MediaElement media = sender as MediaElement;
            media.Position = TimeSpan.FromSeconds(0);
            media.Play();
        }
     }
}