using System;
using ESRI.ArcGIS.Client;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows;
using System.Windows.Media;

namespace ArcGISWindowsPhoneSDK
{
    public partial class SwitchMap : PhoneApplicationPage
    {
        public SwitchMap()
        {
            InitializeComponent();
        }
        
        private void Menu_ItemSelected(object sender, EventArgs e)
        {
            ArcGISTiledMapServiceLayer arcgisLayer = MyMap.Layers["AGOLayer"] as ArcGISTiledMapServiceLayer;
            ApplicationBarMenuItem menuItem = sender as ApplicationBarMenuItem;
            switch (menuItem.Text) 
            {
                case "Aerial":
                    arcgisLayer.Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer";
                    break;
                case "Road":
                    arcgisLayer.Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
                    break;
                case "Topo":
                    arcgisLayer.Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer";
                    break;
            }            
        }

        private void MyMap_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Height == 0)
            {
                MyMap.Width = e.NewSize.Width / (MyMap.RenderTransform as ScaleTransform).ScaleX;
                MyMap.Height = e.NewSize.Height / (MyMap.RenderTransform as ScaleTransform).ScaleY;
            }
        }
    }
}