using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class CustomSymbols : PhoneApplicationPage
    {
        public CustomSymbols()
        {
            InitializeComponent();
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, ESRI.ArcGIS.Client.GraphicMouseButtonEventArgs e)
        {
            if (e.Graphic.Selected)
                e.Graphic.UnSelect();
            else
                e.Graphic.Select();
        }    
    }
}