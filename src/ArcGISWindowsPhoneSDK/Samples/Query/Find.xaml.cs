﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Find : PhoneApplicationPage
    {
        public Find()
        {
            InitializeComponent();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            FindTask findTask = new FindTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer");
            findTask.Failed += FindTask_Failed;

            FindParameters findParameters = new FindParameters();
            // Layer ids to search
            findParameters.LayerIds.AddRange(new int[] { 0, 1, 2 });
            // Fields in layers to search
            findParameters.SearchFields.AddRange(new string[] { "CITY_NAME", "NAME", "SYSTEM", "STATE_ABBR", "STATE_NAME" });

            // Bind data grid to find results.  Bind to the LastResult property which returns a list
            // of FindResult instances.  When LastResult is updated, the ItemsSource property on the 
            // will update.  
            Binding resultFeaturesBinding = new Binding("LastResult");
            resultFeaturesBinding.Source = findTask;
            DataListBox.SetBinding(ListBox.ItemsSourceProperty, resultFeaturesBinding);

            findParameters.SearchText = FindText.Text;
            findTask.ExecuteAsync(findParameters);

            // Since binding to DataGrid, handling the ExecuteComplete event is not necessary.
        }

        private void FindDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Highlight the graphic feature associated with the selected row
            ListBox dataGrid = sender as ListBox;

            int selectedIndex = dataGrid.SelectedIndex;
            if (selectedIndex > -1)
            {
                FindResult findResult = (FindResult)DataListBox.SelectedItem;
                Graphic graphic = findResult.Feature;

                switch (graphic.Attributes["Shape"].ToString())
                {
                    case "Polygon":
                        graphic.Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                        break;
                    case "Polyline":
                        graphic.Symbol = LayoutRoot.Resources["DefaultLineSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                        break;
                    case "Point":
                        graphic.Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                        break;
                }

                GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
                graphicsLayer.ClearGraphics();
                graphicsLayer.Graphics.Add(graphic);
            }
        }

        private void FindTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Find failed: " + args.Error);
        }

    
    }
}