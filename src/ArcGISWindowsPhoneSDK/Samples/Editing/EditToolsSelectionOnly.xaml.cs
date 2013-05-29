using System;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using Microsoft.Phone.Controls;
using System.Linq;
using ESRI.ArcGIS.Client.FeatureService;
using System.Collections.Generic;

namespace ArcGISWindowsPhoneSDK
{
    public partial class EditToolsSelectionOnly : PhoneApplicationPage
    {
        bool _featureDataFormOpen = false;

        public EditToolsSelectionOnly()
        {
            InitializeComponent();
        }

        private void FeatureLayer_Initialized(object sender, EventArgs e)
        {
            var editor = LayoutRoot.Resources["MyEditor"] as Editor;
            if (editor.Select.CanExecute("new"))
                editor.Select.Execute("new");

            FeatureLayer l = ((FeatureLayer) sender);
            if (l == null)return;

            var uvr = l.Renderer as UniqueValueRenderer;
            if (uvr == null) return;
            if (uvr.Field != "symbolid") return;
            symbolIDLB.ItemsSource = uvr.Infos;
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
                        var layer = edit.Layer as FeatureLayer;
                        if (layer != null && layer.IsGeometryUpdateAllowed(edit.Graphic))
                        {
                            if (editor.EditVertices.CanExecute(edit.Graphic))
                                editor.EditVertices.Execute(edit.Graphic);

                            FeatureInfoPage.Visibility = System.Windows.Visibility.Visible;
                            symbolIDLB.SelectedIndex = (Int16)edit.Graphic.Attributes["symbolid"];
                            _featureDataFormOpen = true;


                            LayerDefinition layerDefinition = new LayerDefinition()
                            {
                                LayerID = 2,
                                Definition = string.Format("{0} <> {1}", layer.LayerInfo.ObjectIdField,
                                edit.Graphic.Attributes[layer.LayerInfo.ObjectIdField].ToString())
                            };

                            (MyMap.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer).LayerDefinitions =
                               new System.Collections.ObjectModel.ObservableCollection<LayerDefinition>() { layerDefinition };

                            (MyMap.Layers["WildFireDynamic"] as
                                    ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh();
                        }

                        FeatureInfoPage.DataContext = edit.Graphic;
                        break;
                    }
                }
            }
            else if (e.Action == Editor.EditAction.ClearSelection)
            {
                FeatureInfoPage.Visibility = System.Windows.Visibility.Collapsed;
                FeatureInfoPage.DataContext = null;
                (MyMap.Layers["WildFirePolygons"] as FeatureLayer).ClearSelection();
                (MyMap.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer).LayerDefinitions = null;
                (MyMap.Layers["WildFireDynamic"] as ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh();
            }
        }

        void ResetEditableSelection()
        {
            if (!_featureDataFormOpen)
            {
                FeatureInfoPage.DataContext = null;
                (MyMap.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer).LayerDefinitions = null;
                (MyMap.Layers["WildFirePolygons"] as FeatureLayer).ClearSelection();
            }

            (MyMap.Layers["WildFireDynamic"] as
                ESRI.ArcGIS.Client.ArcGISDynamicMapServiceLayer).Refresh();
        }

        private void FeatureLayer_EndSaveEdits(object sender, ESRI.ArcGIS.Client.Tasks.EndEditEventArgs e)
        {
            ResetEditableSelection();
        }

        private void FeatureLayer_SaveEditsFailed(object sender, ESRI.ArcGIS.Client.Tasks.TaskFailedEventArgs e)
        {
            ResetEditableSelection();
        }

        private void MyFeatureDataForm_EditEnded(object sender, EventArgs e)
        {
            FeatureInfoPage.Visibility = System.Windows.Visibility.Collapsed;
            _featureDataFormOpen = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            FeatureInfoPage.Visibility = Visibility.Collapsed;
        }

        private void SymbolIDLB_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UniqueValueInfo uvi = e.AddedItems[0] as UniqueValueInfo;

            if (uvi == null) return;


            Graphic g = FeatureInfoPage.DataContext as Graphic;
            if (g != null)
                g.Attributes["symbolid"] = (Int16)uvi.Value;
        }
    }
}