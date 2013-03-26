﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class Thematic_Interactive : PhoneApplicationPage
    {

        List<ThematicItem> ThematicItemList = new List<ThematicItem>();
        List<List<SolidColorBrush>> ColorList = new List<List<SolidColorBrush>>();
        int _colorShadeIndex = 0;
        int _thematicListIndex = 0;
        FeatureSet _featureSet = null;
        int _classType = 0; // EqualInterval = 1; Quantile = 0;
        int _classCount = 6;
        int _lastGeneratedClassCount = 0;    
    
        public Thematic_Interactive()
        {
            InitializeComponent();        

            // Get start value for number of classifications in XAML.
            _lastGeneratedClassCount = Convert.ToInt32(((ListBoxItem)ClassCountListBox.SelectedItem).Content);

            // Set query where clause to include features with an area greater than 70 square miles.  This 
            // will effectively exclude the District of Columbia from attributes to avoid skewing classifications.
            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
            {
                Where = "SQMI > 70",
                ReturnGeometry=true
            };
            query.OutFields.Add("*");

            QueryTask queryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/" +
                "Demographics/ESRI_Census_USA/MapServer/5");

            queryTask.ExecuteCompleted += (evtsender, args) =>
            {
                if (args.FeatureSet == null)
                    return;
                _featureSet = args.FeatureSet;
                SetRangeValues();
                RenderButton.IsEnabled = true;
            };

            queryTask.ExecuteAsync(query);

            CreateColorList();
            CreateThematicList();
        }

        public struct ThematicItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string CalcField { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
            public string MinName { get; set; }
            public string MaxName { get; set; }
            public List<double> RangeStarts { get; set; }

        }

        private void CreateColorList()
        {
            ColorList = new List<List<SolidColorBrush>>();

            List<SolidColorBrush> BlueShades = new List<SolidColorBrush>();
            List<SolidColorBrush> RedShades = new List<SolidColorBrush>();
            List<SolidColorBrush> GreenShades = new List<SolidColorBrush>();
            List<SolidColorBrush> YellowShades = new List<SolidColorBrush>();
            List<SolidColorBrush> MagentaShades = new List<SolidColorBrush>();
            List<SolidColorBrush> CyanShades = new List<SolidColorBrush>();

            int rgbFactor = 255 / _classCount;

            for (int j = 0; j < 256; j = j + rgbFactor)
            {
                BlueShades.Add(new SolidColorBrush(Color.FromArgb(192, (byte)j, (byte)j, 255)));
                RedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, (byte)j, (byte)j)));
                GreenShades.Add(new SolidColorBrush(Color.FromArgb(192, (byte)j, 255, (byte)j)));
                YellowShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 255, (byte)j)));
                MagentaShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, (byte)j, 255)));
                CyanShades.Add(new SolidColorBrush(Color.FromArgb(192, (byte)j, 255, 255)));
            }

            ColorList.Add(BlueShades);
            ColorList.Add(RedShades);
            ColorList.Add(GreenShades);
            ColorList.Add(YellowShades);
            ColorList.Add(MagentaShades);
            ColorList.Add(CyanShades);

            foreach (List<SolidColorBrush> brushList in ColorList)
            {
                brushList.Reverse();
            }

            List<SolidColorBrush> MixedShades = new List<SolidColorBrush>();
            if (_classCount > 5) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 255, 255)));
            if (_classCount > 4) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 0, 255)));
            if (_classCount > 3) MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 255, 0)));
            MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 255, 0)));
            MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 0, 0, 255)));
            MixedShades.Add(new SolidColorBrush(Color.FromArgb(192, 255, 0, 0)));
            ColorList.Add(MixedShades);

            _lastGeneratedClassCount = _classCount;
        }

        private void CreateThematicList()
        {
            ThematicItemList.Add(new ThematicItem() { Name = "POP2007", Description = "2007 Population ", CalcField = "" });
            ThematicItemList.Add(new ThematicItem() { Name = "POP07_SQMI", Description = "2007 Pop per Sq Mi", CalcField = "" });
            ThematicItemList.Add(new ThematicItem() { Name = "MALES", Description = "%Males", CalcField = "POP2007" });
            ThematicItemList.Add(new ThematicItem() { Name = "FEMALES", Description = "%Females", CalcField = "POP2007" });
            ThematicItemList.Add(new ThematicItem() { Name = "MED_AGE", Description = "Median age", CalcField = "" });
            ThematicItemList.Add(new ThematicItem() { Name = "SQMI", Description = "Total SqMi", CalcField = "" });
            foreach (ThematicItem items in ThematicItemList)
            {
                FieldListBox.Items.Add(new ListBoxItem(){
                    Content = items.Description,
                    Margin = new Thickness(5)
                });
            }
            FieldListBox.SelectedIndex = 0;
        }

        private void SetRangeValues()
        {
            Style smallStyle = Application.Current.Resources["PhoneTextSmallStyle"] as Style;
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            // if necessary, update ColorList based on current number of classes.
            if (_lastGeneratedClassCount != _classCount) CreateColorList();

            // Field on which to generate a classification scheme.  
            ThematicItem thematicItem = ThematicItemList[_thematicListIndex];

            // Calculate value for classification scheme
            bool useCalculatedValue = !string.IsNullOrEmpty(thematicItem.CalcField);

            // Store a list of values to classify
            List<double> valueList = new List<double>();

            // Get range, min, max, etc. from features
            for (int i = 0; i < _featureSet.Features.Count; i++)
            {
                Graphic graphicFeature = _featureSet.Features[i];

                double graphicValue = Convert.ToDouble(graphicFeature.Attributes[thematicItem.Name]);
                string graphicName = graphicFeature.Attributes["STATE_NAME"].ToString();

                if (useCalculatedValue)
                {
                    double calcVal = Convert.ToDouble(graphicFeature.Attributes[thematicItem.CalcField]);
                    graphicValue = Math.Round(graphicValue / calcVal * 100, 2);
                }

                if (i == 0)
                {
                    thematicItem.Min = graphicValue;
                    thematicItem.Max = graphicValue;
                    thematicItem.MinName = graphicName;
                    thematicItem.MaxName = graphicName;
                }
                else
                {
                    if (graphicValue < thematicItem.Min) { thematicItem.Min = graphicValue; thematicItem.MinName = graphicName; }
                    if (graphicValue > thematicItem.Max) { thematicItem.Max = graphicValue; thematicItem.MaxName = graphicName; }
                }

                valueList.Add(graphicValue);
            }

            // Set up range start values
            thematicItem.RangeStarts = new List<double>();

            double totalRange = thematicItem.Max - thematicItem.Min;
            double portion = totalRange / _classCount;

            thematicItem.RangeStarts.Add(thematicItem.Min);
            double startRangeValue = thematicItem.Min;

            // Equal Interval
            if (_classType == 1)
            {
                for (int i = 1; i < _classCount; i++)
                {
                    startRangeValue += portion;
                    thematicItem.RangeStarts.Add(startRangeValue);
                }
            }
            // Quantile
            else
            {
                // Enumerator of all values in ascending order
                IEnumerable<double> valueEnumerator =
                from aValue in valueList
                orderby aValue //"ascending" is default
                select aValue;

                int increment = Convert.ToInt32(Math.Ceiling(_featureSet.Features.Count / _classCount));
                for (int i = increment; i < valueList.Count; i += increment)
                {
                    double value = valueEnumerator.ElementAt(i);
                    thematicItem.RangeStarts.Add(value);
                }
            }

            // Create graphic features and set symbol using the class range which contains the value 
            List<SolidColorBrush> brushList = ColorList[_colorShadeIndex];
            if (_featureSet != null && _featureSet.Features.Count > 0)
            {
                // Clear previous graphic features
                graphicsLayer.ClearGraphics();

                for (int i = 0; i < _featureSet.Features.Count; i++)
                {
                    Graphic graphicFeature = _featureSet.Features[i];

                    double graphicValue = Convert.ToDouble(graphicFeature.Attributes[thematicItem.Name]);
                    if (useCalculatedValue)
                    {
                        double calcVal = Convert.ToDouble(graphicFeature.Attributes[thematicItem.CalcField]);
                        graphicValue = Math.Round(graphicValue / calcVal * 100, 2);
                    }

                    int brushIndex = GetRangeIndex(graphicValue, thematicItem.RangeStarts);

                    SimpleFillSymbol symbol = new SimpleFillSymbol()
                    {
                        Fill = brushList[brushIndex],
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = 1
                    };

                    Graphic graphic = new Graphic();
                    graphic.Geometry = graphicFeature.Geometry;
                    graphic.Attributes.Add("Name", graphicFeature.Attributes["STATE_NAME"].ToString());
                    graphic.Attributes.Add("Description", thematicItem.Description);
                    graphic.Attributes.Add("Value", graphicValue.ToString());
                    graphic.Symbol = symbol;

                    graphicsLayer.Graphics.Add(graphic);
                }

                // Create new legend with ranges and swatches.
                LegendStackPanel.Children.Clear();

                ListBox legendList = new ListBox();
                LegendTitle.Text = thematicItem.Description;

                for (int c = 0; c < _classCount; c++)
                {
                    Rectangle swatchRect = new Rectangle()
                    {
                        Width = 20,
                        Height = 20,
                        Stroke = new SolidColorBrush(Colors.Black),
                        Fill = brushList[c]
                    };

                    TextBlock classTextBlock = new TextBlock() { Style = smallStyle };

                    // First classification
                    if (c == 0)
                        classTextBlock.Text = String.Format("  Less than {0}", Math.Round(thematicItem.RangeStarts[1], 2));
                    // Last classification
                    else if (c == _classCount - 1)
                        classTextBlock.Text = String.Format("  {0} and above", Math.Round(thematicItem.RangeStarts[c], 2));
                    // Middle classifications
                    else
                        classTextBlock.Text = String.Format("  {0} to {1}", Math.Round(thematicItem.RangeStarts[c], 2), Math.Round(thematicItem.RangeStarts[c + 1], 2));

                    StackPanel classStackPanel = new StackPanel();
                    classStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    classStackPanel.Children.Add(swatchRect);
                    classStackPanel.Children.Add(classTextBlock);

                    legendList.Items.Add(classStackPanel);
                }

                TextBlock minTextBlock = new TextBlock() { Style = smallStyle };
                StackPanel minStackPanel = new StackPanel();
                minStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                minTextBlock.Text = String.Format("Min: {0} ({1})", thematicItem.Min, thematicItem.MinName);
                minStackPanel.Children.Add(minTextBlock);
                legendList.Items.Add(minStackPanel);

                TextBlock maxTextBlock = new TextBlock() { Style = smallStyle };
                StackPanel maxStackPanel = new StackPanel();
                maxStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                maxTextBlock.Text = String.Format("Max: {0} ({1})", thematicItem.Max, thematicItem.MaxName);
                maxStackPanel.Children.Add(maxTextBlock);
                legendList.Items.Add(maxStackPanel);
                legendList.IsHitTestVisible = false;
                LegendStackPanel.Children.Add(legendList);
            }
        }

        private int GetRangeIndex(double val, List<double> ranges)
        {
            int index = _classCount - 1;
            for (int r = 0; r < _classCount - 1; r++)
            {
                if (val >= ranges[r] && val < ranges[r + 1]) index = r;
            }
            return index;
        }

        public struct Values
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        private void ClassTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassTypeListBox != null)
            {
                _classType = ClassTypeListBox.SelectedIndex;

                ClassTypeChoicesPage.IsOpen = false;
            }
        }

        private void ColorBlendListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorBlendListBox != null)
            {
                _colorShadeIndex = ColorBlendListBox.SelectedIndex;

                ColorBlendChoicesPage.IsOpen = false;
            }
        }

        private void ClassCountListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassCountListBox != null)
            {
                ListBoxItem item = ClassCountListBox.SelectedItem as ListBoxItem;
                _classCount = Convert.ToInt32(item.Content);

                ClassCountChoicesPage.IsOpen = false;
            }
        }

        private void FieldListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieldListBox != null)
            {
                _thematicListIndex = FieldListBox.SelectedIndex;

                FieldChoicesPage.IsOpen = false;
            }
        }

        private void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            SetRangeValues();
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            LegendDisplayBorder.Visibility = LegendDisplayBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Menu_Dialog_Click(object sender, EventArgs e)
        {
            ClassBorder.Visibility = ClassBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ColorBlendButton_Click(object sender, RoutedEventArgs e)
        {
            ColorBlendChoicesPage.IsOpen = true;
        }

        private void ClassCountButton_Click(object sender, RoutedEventArgs e)
        {
            ClassCountChoicesPage.IsOpen = true;
        }

        private void ClassTypeButton_Click(object sender, RoutedEventArgs e)
        {
            ClassTypeChoicesPage.IsOpen = true;
        }

        private void FieldButton_Click(object sender, RoutedEventArgs e)
        {
            FieldChoicesPage.IsOpen = true;
        }
    }
}