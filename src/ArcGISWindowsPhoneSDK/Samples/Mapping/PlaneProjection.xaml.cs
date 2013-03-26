using System.Windows;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class PlaneProjection : PhoneApplicationPage
    {
        public PlaneProjection()
        {
            InitializeComponent();
        }

        private void MySlantSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyMap != null && MyPlaneProjection != null)
                MyPlaneProjection.RotationX = e.NewValue;
        }
    }
}