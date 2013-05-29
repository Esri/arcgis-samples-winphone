using Microsoft.Phone.Controls;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using System.Windows;

namespace ArcGISWindowsPhoneSDK
{
    public partial class GroupLayers : PhoneApplicationPage
    {
        public GroupLayers()
        {
            InitializeComponent();
        }

        private void Legend_Refreshed(object sender, ESRI.ArcGIS.Client.Toolkit.Legend.RefreshedEventArgs e)
        {
            if (e.LayerItem.LayerItems != null)
                foreach (LayerItemViewModel layerItemVM in e.LayerItem.LayerItems)
                    if (layerItemVM.IsExpanded)
                        layerItemVM.IsExpanded = false;
        }

        private void InfoButton_Click(object sender, System.EventArgs e)
        {
            LegendGrid.Visibility = LegendGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;      
        }
    }
}