using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit.Primitives;

namespace ArcGISWindowsPhoneSDK
{
    public partial class DynamicLayerThematic : PhoneApplicationPage
    {
        GenerateRendererTask generateRendererTask;
        ColorRamp colorRamp;
        GenerateRendererParameters generateClassesParameters;

        public DynamicLayerThematic()
        {
            InitializeComponent();

            PopulateListBoxes();

            InitializeRenderingInfo();
        }

        private void FeatureLayer_Initialized(object sender, System.EventArgs e)
        {
            FeatureLayer featureLayer = sender as FeatureLayer;
            IEnumerable<Field> intandDoublefields =
              from fld in featureLayer.LayerInfo.Fields
              where fld.Type == Field.FieldType.Integer || fld.Type == Field.FieldType.Double
              select fld;
            if (intandDoublefields != null && intandDoublefields.Count() > 0)
            {
                ClassificationFieldListBox.ItemsSource = intandDoublefields;
                ClassificationFieldListBox.SelectedIndex = 1;
                NormalizationFieldListBox.ItemsSource = intandDoublefields;
                NormalizationFieldListBox.SelectedIndex = -1;
                RenderButton.IsEnabled = true;
            }
        }

        private void PopulateListBoxes()
        {
            AlgorithmListBox.ItemsSource = new object[] 
			{
			    Algorithm.CIELabAlgorithm, 
			    Algorithm.HSVAlgorithm, 
			    Algorithm.LabLChAlgorithm 
			};
            AlgorithmListBox.SelectedIndex = 1;

            ClassificationMethodListBox.ItemsSource = new object[] 
			{ 
			    ClassificationMethod.EqualInterval, 
			    ClassificationMethod.NaturalBreaks,
			    ClassificationMethod.Quantile, 
			    ClassificationMethod.StandardDeviation
			};
            ClassificationMethodListBox.SelectedIndex = 0;

            NormalizationTypeListBox.ItemsSource = new object[]
			{
			    NormalizationType.Field,
			    NormalizationType.Log,          
                NormalizationType.PercentOfTotal,
                NormalizationType.None
			};
            NormalizationTypeListBox.SelectedIndex = 3;

            IntervalListBox.ItemsSource = new object[]
			{
			    StandardDeviationInterval.One,
			    StandardDeviationInterval.OneHalf,
			    StandardDeviationInterval.OneQuarter,
			    StandardDeviationInterval.OneThird
			};
            IntervalListBox.SelectedIndex = 0;
        }

        private void InitializeRenderingInfo()
        {
            FeatureLayer statesFeatureLayer = MyMap.Layers["StatesFeatureLayer"] as FeatureLayer;

            generateRendererTask = new GenerateRendererTask(statesFeatureLayer.Url);

            generateClassesParameters = new GenerateRendererParameters();
            generateClassesParameters.Source = new LayerMapSource() { MapLayerID = 3 };

            generateRendererTask.Failed += (s, e) =>
            {
                MessageBox.Show(string.Format("GenerateRendererTask Failed: {0}", e.Error.Message));
            };
            generateRendererTask.ExecuteCompleted += (s, e) =>
            {
                statesFeatureLayer.Renderer = e.GenerateRendererResult.Renderer;
            };
        }

        private void RenderButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableCollection<ColorRamp> colorRamps = new ObservableCollection<ColorRamp>();
            colorRamp = new ColorRamp()
            {
                From = ((StartColorListBox.SelectedItem as ListBoxItem).Background as SolidColorBrush).Color,
                To = ((EndColorListBox.SelectedItem as ListBoxItem).Background as SolidColorBrush).Color,
                Algorithm = (Algorithm)AlgorithmListBox.SelectedItem,
            };
            colorRamps.Add(colorRamp);

            generateClassesParameters.ClassificationDefinition = new ClassBreaksDefinition()
            {
                BaseSymbol = new ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol()
                {
                    Fill = (ColorRampListBox.SelectedItem as ListBoxItem).Background
                },
                ClassificationField = ((ClassificationFieldListBox.SelectedItem) as Field).Name,
                ClassificationMethod = (ClassificationMethod)ClassificationMethodListBox.SelectedItem,

                BreakCount = int.Parse(BreakCountTB.Text.Trim()),
                ColorRamps = colorRamps,
            };

            ClassBreaksDefinition classBreakDef =
                generateClassesParameters.ClassificationDefinition as ClassBreaksDefinition;

            if (classBreakDef.ClassificationMethod == ClassificationMethod.StandardDeviation)
                classBreakDef.StandardDeviationInterval = (StandardDeviationInterval)IntervalListBox.SelectedItem;

            if (NormalizationTypeListBox.SelectedItem != null)
            {
                classBreakDef.NormalizationType = (NormalizationType)NormalizationTypeListBox.SelectedItem;

                if (classBreakDef.NormalizationType == NormalizationType.Field)
                {
                    if (NormalizationFieldListBox.SelectedItem != null)
                        classBreakDef.NormalizationField = ((NormalizationFieldListBox.SelectedItem) as Field).Name;
                    else
                    {
                        MessageBox.Show("Normalization Field must be selected");
                        return;
                    }
                }
            }
            if (generateRendererTask.IsBusy)
                generateRendererTask.CancelAsync();
            generateRendererTask.ExecuteAsync(generateClassesParameters);

            ThematicPropertiesPage.IsOpen = false;
        }

        private void PropertyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (((Button)sender).Tag.ToString())
            {
                case "BaseSymbol":
                    ColorRampChoicesPage.IsOpen = true;
                    break;
                case "StartColor":
                    StartColorChoicesPage.IsOpen = true;
                    break;
                case "EndColor":
                    EndColorChoicesPage.IsOpen = true;
                    break;
                case "Algorithm":
                    AlgorithmChoicesPage.IsOpen = true;
                    break;
                case "ClassificationField":
                    ClassificationFieldChoicesPage.IsOpen = true;
                    break;
                case "ClassificationMethod":
                    ClassificationMethodChoicesPage.IsOpen = true;
                    break;
                case "Interval":
                    IntervalChoicesPage.IsOpen = true;
                    break;
                case "NormalizationType":
                    NormalizationTypeChoicesPage.IsOpen = true;
                    break;
                case "NormalizationField":
                    NormalizationFieldChoicesPage.IsOpen = true;
                    break;
            }
        }

        private void ColorRampListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorRampListBox != null)
            {
                ColorRampChoicesPage.IsOpen = false;
            }
        }
        private void StartColorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartColorListBox != null)
            {
                StartColorChoicesPage.IsOpen = false;
            }
        }
        private void EndColorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EndColorListBox != null)
            {
                EndColorChoicesPage.IsOpen = false;
            }
        }

        private void PageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null)
            {
                ((ChildPage)(((ListBox)sender).Parent)).IsOpen = false;

                if (((ListBox)sender).Tag != null)
                {
                    if (((ListBox)sender).Tag.ToString().Equals("ClassificationMethod"))
                    {
                        ClassificationMethod method = (ClassificationMethod)(sender as ListBox).SelectedItem;
                        IntervalButton.IsEnabled = (method == ClassificationMethod.StandardDeviation) ? true : false;
                        BreakCountTB.IsEnabled = (method == ClassificationMethod.StandardDeviation) ? false : true;
                    }
                    else if (((ListBox)sender).Tag.ToString().Equals("NormalizationType"))
                    {
                        NormalizationType normType = (NormalizationType)(sender as ListBox).SelectedItem;
                        NormalizationFieldButton.IsEnabled = (normType == NormalizationType.Field) ? true : false;
                    }
                }
            }
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (ColorRampChoicesPage.IsOpen || StartColorChoicesPage.IsOpen || EndColorChoicesPage.IsOpen ||
                AlgorithmChoicesPage.IsOpen || ClassificationFieldChoicesPage.IsOpen || ClassificationMethodChoicesPage.IsOpen ||
                IntervalChoicesPage.IsOpen || NormalizationTypeChoicesPage.IsOpen || NormalizationFieldChoicesPage.IsOpen)
            {
                ColorRampChoicesPage.IsOpen = false;
                StartColorChoicesPage.IsOpen = false;
                EndColorChoicesPage.IsOpen = false;
                AlgorithmChoicesPage.IsOpen = false;
                ClassificationFieldChoicesPage.IsOpen = false;
                ClassificationMethodChoicesPage.IsOpen = false;
                IntervalChoicesPage.IsOpen = false;
                NormalizationTypeChoicesPage.IsOpen = false;
                NormalizationFieldChoicesPage.IsOpen = false;
                e.Cancel = true;
            }
            
        }

        private void ShowLegendButton_Click(object sender, System.EventArgs e)
        {
            Legend.Visibility = Legend.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        private void ShowThematicPropertiesButton_Click(object sender, System.EventArgs e)
        {
            ThematicPropertiesPage.IsOpen = true;
        }
    }
}