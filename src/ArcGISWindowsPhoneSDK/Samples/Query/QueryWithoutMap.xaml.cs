using System.Windows;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class QueryWithoutMap : PhoneApplicationPage
    {
        public QueryWithoutMap()
        {
            InitializeComponent();
        }

        void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            QueryTask queryTask =
                new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.Failed += QueryTask_Failed;

            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.Text = StateNameTextBox.Text;

            query.OutFields.Add("*");
            queryTask.ExecuteAsync(query);
        }

        void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            if (featureSet != null && featureSet.Features.Count > 0)
                DataListBox.ItemsSource = featureSet.Features;
            else
                MessageBox.Show("No features returned from query");
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query execute error: " + args.Error);
        }
    }
}