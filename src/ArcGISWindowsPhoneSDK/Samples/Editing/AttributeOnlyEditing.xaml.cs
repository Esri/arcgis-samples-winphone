using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.FeatureService;
using Microsoft.Phone.Controls;
using System.Windows.Data;

namespace ArcGISWindowsPhoneSDK
{
    public partial class AttributeOnlyEditing : PhoneApplicationPage
    {
        public AttributeOnlyEditing()
        {
            InitializeComponent();
        }

        private void Editor_EditCompleted(object sender, Editor.EditEventArgs e)
        {
            var editor = sender as Editor;
            if (e.Action == Editor.EditAction.Select)
            {
                foreach (var edit in e.Edits)
                {
                    if (edit.Graphic != null && edit.Graphic.Selected)
                    {
                        // edit attribute
                        FeatureInfoPage.DataContext = edit.Graphic;
                        FeatureInfoPage.Visibility = Visibility.Visible;
                        break;
                    }
                }
             }
           
        }
        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
           var has_PoolField = ((FeatureLayer)sender).LayerInfo.Fields.Where(f => f.Name == "has_pool").First();
            var has_PoolFieldDomain = has_PoolField.Domain as CodedValueDomain;
            Has_PoolChoicesListBox.ItemsSource = has_PoolFieldDomain.CodedValues;
            Has_PoolChoicesListBox.SetBinding(ListBox.SelectedItemProperty, new Binding(string.Format("Attributes[{0}]", has_PoolField.Name))
            {
                Mode = BindingMode.TwoWay,
                Converter = new CodeToValueConverter(),
                ConverterParameter = has_PoolFieldDomain
            });

            var editor = LayoutRoot.Resources["MyEditor"] as Editor;
            if (editor.Select.CanExecute("New"))
                editor.Select.Execute("New");
        }
               

        private void FeatureLayer_EndSaveEdits(object sender, ESRI.ArcGIS.Client.Tasks.EndEditEventArgs e)
        {
            if (e.Success)
                (MyMap.Layers["PoolPermitDynamicLayer"] as ArcGISDynamicMapServiceLayer).Refresh();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            FeatureInfoPage.Visibility = Visibility.Collapsed;
        }

        
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {

            if (FeatureInfoPage.Visibility == Visibility.Visible)
            {
                FeatureInfoPage.Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }
        }
    }

    public class CodeToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is CodedValueDomain)
            {
                var cvd = parameter as CodedValueDomain;
                value = (from c in cvd.CodedValues
                        where (int)c.Key == (int)value
                        select c).FirstOrDefault();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var kvp = (KeyValuePair<object, string>)value;
            return kvp.Key;
        }
    }
}
