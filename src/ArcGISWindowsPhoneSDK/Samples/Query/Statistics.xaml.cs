using Microsoft.Phone.Controls;
using System.Windows;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Statistics : PhoneApplicationPage
    {
        QueryTask queryTask;

        public Statistics()
        {
            InitializeComponent();
            queryTask = new QueryTask("http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2");
            queryTask.ExecuteCompleted += queryTask_ExecuteCompleted;
        }

        private void OutStatisticsListBox_Loaded(object sender, RoutedEventArgs e)
        {
            Query query = new Query()
            {
                GroupByFieldsForStatistics = new List<string> { "sub_region" },
                OutStatistics = new List<OutStatistic> { 
                    new OutStatistic(){
                        OnStatisticField = "pop2000",
                        OutStatisticFieldName = "subregionpopulation",
                        StatisticType = StatisticType.Sum
                    },
                    new OutStatistic(){
                        OnStatisticField = "sub_region",
                        OutStatisticFieldName = "numberofstates",
                        StatisticType = StatisticType.Count
                    }
                 }
            };
            queryTask.ExecuteAsync(query);
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            FeatureSet featureSet = e.FeatureSet;

            if (featureSet != null && featureSet.Features.Count > 0)
            {
                OutStatisticsListBox.ItemsSource = featureSet.Features;
            }
        }
    }
}