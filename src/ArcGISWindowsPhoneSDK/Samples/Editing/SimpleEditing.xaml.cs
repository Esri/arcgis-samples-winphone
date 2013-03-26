using System;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using Microsoft.Phone.Controls;

namespace ArcGISWindowsPhoneSDK
{
    public partial class SimpleEditing : PhoneApplicationPage
    {
        Editor editor;
        FeatureLayer editLayer;

        public SimpleEditing()
        {
            InitializeComponent();

            editor = LayoutRoot.Resources["MyEditor"] as Editor;
            editor.Map = MyMap;

            editLayer = MyMap.Layers["ThreatPoints"] as FeatureLayer;
        }

        private void DrawPointButton_Click(object sender, EventArgs e)
        {
            editor.Add.Execute(10301);
        }

        private void StopGraphicsMenuItem_Click(object sender, EventArgs e)
        {
            editor.CancelActive.Execute(null);
            AttributeGrid.Visibility = System.Windows.Visibility.Collapsed;
            CoverGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Editor_EditCompleted(object sender, Editor.EditEventArgs e)
        {
            if (e.Action != Editor.EditAction.Cancel)
            {
                AttributeGrid.Visibility = System.Windows.Visibility.Visible;
                CoverGrid.Visibility = System.Windows.Visibility.Visible;
            }            
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            editLayer.SaveEdits();
            AttributeGrid.Visibility = System.Windows.Visibility.Collapsed;
            CoverGrid.Visibility = System.Windows.Visibility.Collapsed;

            DescriptionTextBox.Text = "Description";
            DescriptionTextBox.Foreground = LayoutRoot.Resources["GrayBrush"] as Brush;
        }

        private void Description_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DescriptionTextBox.Text == "Description")
            {
                DescriptionTextBox.Foreground = LayoutRoot.Resources["BlackBrush"] as Brush;
                DescriptionTextBox.Text = "";
            }
        }
    }
}