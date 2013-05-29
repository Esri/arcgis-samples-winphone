using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client;
using Microsoft.Phone.Controls;
    

namespace ArcGISWindowsPhoneSDK
{
    public partial class FeatureLayerChangeVersion : PhoneApplicationPage
    {
        FeatureLayer featureLayer;

        public FeatureLayerChangeVersion()
        {
            InitializeComponent();

            featureLayer = (MyMap.Layers["ServiceConnections"] as FeatureLayer);

            Geoprocessor gp_ListVersions = new Geoprocessor("http://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions");

            gp_ListVersions.Failed += (s, a) =>
            {
                MessageBox.Show("Geoprocessing service failed: " + a.Error);
            };

            gp_ListVersions.ExecuteCompleted += (c, d) =>
            {
                VersionsCombo.DataContext = (d.Results.OutParameters[0] as GPRecordSet).FeatureSet;

                foreach (Graphic g in (d.Results.OutParameters[0] as GPRecordSet).FeatureSet.Features)
                {
                    if ((g.Attributes["name"] as string) == featureLayer.GdbVersion)
                    {
                        VersionsCombo.SelectedValue = g;
                        break;
                    }
                }                
            };

            List<GPParameter> gpparams = new List<GPParameter>();
            gpparams.Add(new GPRecordSet("Versions", new FeatureSet()));
            gp_ListVersions.ExecuteAsync(gpparams);
        }

        private void VersionsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            featureLayer.GdbVersion = (e.AddedItems[0] as Graphic).Attributes["name"].ToString();
            featureLayer.Update();
            FieldChoicesPage.IsOpen = false;
        }

        private void Menu_Dialog_Click(object sender, System.EventArgs e)
        {
            FieldChoicesPage.IsOpen = true;
        }
    }
}
