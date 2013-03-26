using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class SelectGraphics : PhoneApplicationPage
    {
        public SelectGraphics()
        {
            InitializeComponent();
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, ESRI.ArcGIS.Client.GraphicMouseButtonEventArgs e)
        {
            e.Graphic.Selected = !e.Graphic.Selected;
        }

    }
}