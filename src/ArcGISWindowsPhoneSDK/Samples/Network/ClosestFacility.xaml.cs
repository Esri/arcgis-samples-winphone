﻿using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISWindowsPhoneSDK
{
    public partial class ClosestFacility : PhoneApplicationPage
    {
        private RouteTask myRouteTask;
        private GraphicsLayer facilitiesGraphicsLayer;
        private GraphicsLayer IncidentsGraphicsLayer;
        private GraphicsLayer barriersGraphicsLayer;
        private GraphicsLayer routeGraphicsLayer;
        List<Graphic> pointBarriers;
        List<Graphic> polylineBarriers;
        List<Graphic> polygonBarriers;
        Random random;
        Editor facilitiesEditor;
        Editor incidentsEditor;
        Editor barriersEditor;

        List<string> travelDirectionChoices = new List<string>(new string[] { "From Facility", "To Facility" });
        List<string> attributeParameterChoices = new List<string>(new string[] { "None", "15 MPH", "20 MPH", "25 MPH", "35 MPH", "45 MPH", "50 MPH", "55 MPH", "65 MPH", "Other Roads" });
        List<string> restrictUTurnChoices = new List<string>(new string[] { "Allow Backtrack", "At Dead Ends Only", "No Backtrack" });
        List<string> outputLineChoices = new List<string>(new string[] {"None", "Straight", "True Shape" });
        List<string> outputGeomPrecisionChoices = new List<string>(new string[] { "Unknown", "Decimal Degrees", "Kilometers", "Meters", "Miles", "Nautical Miles", "Inches", "Points", "Feet", "Yards", "Millimeters", "Centimeters", "Decimeters" });
        List<string> directionsLengthUnitChoices = new List<string>(new string[] { "Unkown", "Kilometers", "Meters", "Miles", "Nautical Miles" });
                                    
        public ClosestFacility()
        {
            InitializeComponent();

            facilitiesGraphicsLayer = MyMap.Layers["MyFacilitiesGraphicsLayer"] as GraphicsLayer;
            IncidentsGraphicsLayer = MyMap.Layers["MyIncidentsGraphicsLayer"] as GraphicsLayer;
            barriersGraphicsLayer = MyMap.Layers["MyBarriersGraphicsLayer"] as GraphicsLayer;
            routeGraphicsLayer = MyMap.Layers["MyRoutesGraphicsLayer"] as GraphicsLayer;

            myRouteTask = new RouteTask("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Network/USA/NAServer/Closest%20Facility");
            myRouteTask.SolveClosestFacilityCompleted += SolveClosestFacility_Completed;
            myRouteTask.Failed += SolveClosestFacility_Failed;

            pointBarriers = new List<Graphic>();
            polylineBarriers = new List<Graphic>();
            polygonBarriers = new List<Graphic>();

            facilitiesEditor = LayoutRoot.Resources["MyFacilitiesEditor"] as Editor;
            incidentsEditor = LayoutRoot.Resources["MyIncidentsEditor"] as Editor;
            barriersEditor = LayoutRoot.Resources["MyBarriersEditor"] as Editor;

            random = new Random();
        }

        private void SolveButton_Click(object sender, EventArgs e)
        {
            MyRouteInfoWindow.IsOpen = false;

            List<AttributeParameter> aps = new List<AttributeParameter>();
            AttributeParameter ap = GetAttributeParameterValue(AttributeParameter2.Content.ToString().Trim());
            if (ap != null)
                aps.Add(ap);

            GenerateBarriers();

            RouteClosestFacilityParameters routeParams = new RouteClosestFacilityParameters()
            {
                Incidents = IncidentsGraphicsLayer.Graphics,
                Barriers = pointBarriers.Count > 0 ? pointBarriers : null,
                PolylineBarriers = polylineBarriers.Count > 0 ? polylineBarriers : null,
                PolygonBarriers = polygonBarriers.Count > 0 ? polygonBarriers : null,
                Facilities = facilitiesGraphicsLayer.Graphics,

                AttributeParameterValues = aps,
                ReturnDirections = ReturnDirections2.IsChecked.HasValue ? ReturnDirections2.IsChecked.Value : false,
                DirectionsLanguage = String.IsNullOrEmpty(DirectionsLanguage2.Text) ? new System.Globalization.CultureInfo("en-US") : new System.Globalization.CultureInfo(DirectionsLanguage2.Text),
                DirectionsLengthUnits = GetDirectionsLengthUnits(DirectionsLengthUnits2.Content.ToString().Trim()),
                DirectionsTimeAttribute = DirectionsTimeAttributeName2.Text,

                ReturnRoutes = ReturnRoutes2.IsChecked.HasValue ? ReturnRoutes2.IsChecked.Value : false,
                ReturnFacilities = ReturnFacilities2.IsChecked.HasValue ? ReturnFacilities2.IsChecked.Value : false,
                ReturnIncidents = ReturnIncidents2.IsChecked.HasValue ? ReturnIncidents2.IsChecked.Value : false,
                ReturnBarriers = ReturnBarriers2.IsChecked.HasValue ? ReturnBarriers2.IsChecked.Value : false,
                ReturnPolylineBarriers = ReturnPolylineBarriers2.IsChecked.HasValue ? ReturnPolylineBarriers2.IsChecked.Value : false,
                ReturnPolygonBarriers = ReturnPolygonBarriers2.IsChecked.HasValue ? ReturnPolygonBarriers2.IsChecked.Value : false,

                FacilityReturnType = FacilityReturnType.ServerFacilityReturnAll,
                OutputLines = GetOutputLines(OutputLines2.Content.ToString().Trim()),
                DefaultCutoff = string.IsNullOrEmpty(DefaultCutoff2.Text) ? 100 : double.Parse(DefaultCutoff2.Text),
                DefaultTargetFacilityCount = string.IsNullOrEmpty(DefaultTargetFacilityCount2.Text) ? 1 : int.Parse(DefaultTargetFacilityCount2.Text),
                TravelDirection = GetFacilityTravelDirections(TravelDirection2.Content.ToString().Trim()),
                OutSpatialReference = string.IsNullOrEmpty(OutputSpatialReference2.Text) ? MyMap.SpatialReference : new SpatialReference(int.Parse(OutputSpatialReference2.Text)),

                AccumulateAttributes = AccumulateAttributeNames2.Text.Split(','),
                ImpedanceAttribute = ImpedanceAttributeName2.Text,
                RestrictionAttributes = RestrictionAttributeNames2.Text.Split(','),
                RestrictUTurns = GetRestrictUTurns(RestrictUTurns2.Content.ToString().Trim()),
                UseHierarchy = UseHierarchy2.IsChecked.HasValue ? UseHierarchy2.IsChecked.Value : false,
                OutputGeometryPrecision = string.IsNullOrEmpty(OutputGeometryPrecision2.Text) ? 0 : double.Parse(OutputGeometryPrecision2.Text),
                OutputGeometryPrecisionUnits = GetGeometryPrecisionUnits(OutputGeometryPrecisionUnits2.Content.ToString().Trim())
            };

            if (myRouteTask.IsBusy)
                myRouteTask.CancelAsync();

            myRouteTask.SolveClosestFacilityAsync(routeParams);
        }

        public void GenerateBarriers()
        {
            foreach (Graphic g in barriersGraphicsLayer)
            {
                Type gType = g.Geometry.GetType();

                if (gType == typeof(MapPoint))
                    pointBarriers.Add(g);
                else if (gType == typeof(Polyline))
                    polylineBarriers.Add(g);
                else if (gType == typeof(Polygon) || gType == typeof(Envelope))
                    polygonBarriers.Add(g);
            }
        }

        void SolveClosestFacility_Completed(object sender, RouteEventArgs e)
        {
            routeGraphicsLayer.Graphics.Clear();

            if (e.RouteResults != null)
            {
                foreach (RouteResult route in e.RouteResults)
                {
                    Graphic g = route.Route;
                    routeGraphicsLayer.Graphics.Add(g);
                }
            }
        }

        void SolveClosestFacility_Failed(object sender, TaskFailedEventArgs e)
        {
            MessageBox.Show("Network Analysis error: " + e.Error.Message);
        }

        private void Editor_EditCompleted(object sender, Editor.EditEventArgs e)
        {
            Editor editor = sender as Editor;

            if (e.Action == Editor.EditAction.Add)
            {
                if (editor.LayerIDs.ElementAt(0) == "MyFacilitiesGraphicsLayer")
                {
                    e.Edits.ElementAt(0).Graphic.Attributes.Add("FacilityNumber",
                        (e.Edits.ElementAt(0).Layer as GraphicsLayer).Graphics.Count);
                }
                else if (editor.LayerIDs.ElementAt(0) == "MyIncidentsGraphicsLayer")
                {
                    e.Edits.ElementAt(0).Graphic.Attributes.Add("IncidentNumber",
                        (e.Edits.ElementAt(0).Layer as GraphicsLayer).Graphics.Count);
                }

                if (facilitiesGraphicsLayer.Graphics.Count > 0 && IncidentsGraphicsLayer.Graphics.Count > 0)
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            foreach (Layer layer in MyMap.Layers)
                if (layer is GraphicsLayer)
                    (layer as GraphicsLayer).ClearGraphics();

            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
        }

        private AttributeParameter GetAttributeParameterValue(string attributeParamSelection)
        {
            // See Attribute Parameter Values list at 
            // http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Network/USA/NAServer/Closest%20Facility
            if (attributeParamSelection.Equals("None"))
                return null;

            if (attributeParamSelection.Equals("Other Roads"))
                return new AttributeParameter
                {
                    attributeName = "Time",
                    parameterName = "OtherRoads",
                    value = "5"
                };

            return new AttributeParameter 
			{ 
                attributeName = "Time",
                parameterName = attributeParamSelection,
                value = attributeParamSelection.Replace(" MPH", "") 
            };
        }

        private esriUnits GetDirectionsLengthUnits(string directionsLengthUnits)
        {
            esriUnits units = esriUnits.esriUnknownUnits;
            if (directionsLengthUnits.Equals(string.Empty))
                return units;

            switch (directionsLengthUnits.ToLower())
            {
                case "kilometers":
                    units = esriUnits.esriKilometers;
                    break;
                case "meters":
                    units = esriUnits.esriMeters;
                    break;
                case "miles":
                    units = esriUnits.esriMiles;
                    break;
                case "nautical miles":
                    units = esriUnits.esriNauticalMiles;
                    break;
                case "unknown":
                    units = esriUnits.esriUnknownUnits;
                    break;
                default:
                    break;
            }

            return units;
        }

        private string GetOutputLines(string outputLines)
        {
            string result = "";
            if (outputLines.Equals(string.Empty))
                return result;

            switch (outputLines.ToLower())
            {
                case "none":
                    result = "esriNAOutputLineNone";
                    break;
                case "straight":
                    result = "esriNAOutputLineStraight";
                    break;
                case "true shape":
                    result = "esriNAOutputLineTrueShape";
                    break;
                default:
                    break;
            }
            return result;
        }

        private FacilityTravelDirection GetFacilityTravelDirections(string direction)
        {
            FacilityTravelDirection ftd = FacilityTravelDirection.TravelDirectionToFacility;
            switch (direction.ToLower())
            {
                case "to facility":
                    ftd = FacilityTravelDirection.TravelDirectionToFacility;
                    break;
                case "from facility":
                    ftd = FacilityTravelDirection.TravelDirectionFromFacility;
                    break;
                default:
                    break;
            }
            return ftd;
        }

        private string GetRestrictUTurns(string restrictUTurns)
        {
            string result = "esriNFSBAllowBacktrack";
            switch (restrictUTurns.ToLower())
            {
                case "allow backtrack":
                    result = "esriNFSBAllowBacktrack";
                    break;
                case "at dead ends only":
                    result = "esriNFSBAtDeadEndsOnly";
                    break;
                case "no backtrack":
                    result = "esriNFSBNoBacktrack";
                    break;
                default:
                    break;
            }
            return result;
        }

        private esriUnits GetGeometryPrecisionUnits(string outputGeometryPrecisionUnits)
        {
            esriUnits units = esriUnits.esriUnknownUnits;
            switch (outputGeometryPrecisionUnits.ToLower())
            {
                case "unknown":
                    units = esriUnits.esriUnknownUnits;
                    break;
                case "decimal degrees":
                    units = esriUnits.esriDecimalDegrees;
                    break;
                case "kilometers":
                    units = esriUnits.esriKilometers;
                    break;
                case "meters":
                    units = esriUnits.esriMeters;
                    break;
                case "miles":
                    units = esriUnits.esriMiles;
                    break;
                case "nautical miles":
                    units = esriUnits.esriNauticalMiles;
                    break;
                case "inches":
                    units = esriUnits.esriInches;
                    break;
                case "points":
                    units = esriUnits.esriPoints;
                    break;
                case "feet":
                    units = esriUnits.esriFeet;
                    break;
                case "yards":
                    units = esriUnits.esriYards;
                    break;
                case "millimeters":
                    units = esriUnits.esriMillimeters;
                    break;
                case "centimeters":
                    units = esriUnits.esriCentimeters;
                    break;
                case "decimeters":
                    units = esriUnits.esriDecimeters;
                    break;
                default:
                    break;
            }
            return units;
        }

        private void MyMap_MapGesture(object sender, Map.MapGestureEventArgs e)
        {
            if (e.Gesture == GestureType.Tap &&
                !facilitiesEditor.CancelActive.CanExecute(null) &&
                !incidentsEditor.CancelActive.CanExecute(null) &&
                !barriersEditor.CancelActive.CanExecute(null))
            {
                MyRouteInfoWindow.IsOpen = false;

                GraphicsLayer graphicsLayer = MyMap.Layers["MyRoutesGraphicsLayer"] as GraphicsLayer;
                IEnumerable<Graphic> selected = e.DirectlyOver(10, new GraphicsLayer[] { graphicsLayer });
                foreach (Graphic g in selected)
                {
                    MyRouteInfoWindow.Anchor = e.MapPoint;
                    MyRouteInfoWindow.IsOpen = true;
                    MyRouteInfoWindow.DataContext = g;
                    return;
                }
            }
        }

        private void AddFacility_Click(object sender, EventArgs e)
        {
            facilitiesEditor.Add.Execute(LayoutRoot.Resources["MyFacilitiesSymbol"]);
        }

        private void AddIncident_Click(object sender, EventArgs e)
        {
            incidentsEditor.Add.Execute(LayoutRoot.Resources["MyIncidentsSymbol"]);
        }

        private void AddPointButton_Click(object sender, RoutedEventArgs e)
        {
            barriersEditor.Add.Execute(LayoutRoot.Resources["BarrierMarkerSymbol"]);
        }

        private void AddLineButton_Click(object sender, RoutedEventArgs e)
        {
            barriersEditor.Add.Execute(LayoutRoot.Resources["BarrierLineSymbol"]);
        }

        private void AddPolygonButton_Click(object sender, RoutedEventArgs e)
        {
            barriersEditor.Add.Execute(LayoutRoot.Resources["BarrierFillSymbol"]);
        }

        private void ShowInfo_Click(object sender, EventArgs e)
        {
            Info.Visibility = Info.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void AddBarriersInfoVisibility_Click(object sender, EventArgs e)
        {
            if (AddBarriersInfo.Visibility == Visibility.Visible)
            {
                AddBarriersInfo.Visibility = Visibility.Collapsed;
                // set the context menu label
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = "show add barriers";
            }
            else // make it visible
            {
                AddBarriersInfo.Visibility = Visibility.Visible;
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = "hide add barriers";

                // hide the parameter info & update its context menu item
                ParameterInfo.Visibility = Visibility.Collapsed;
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = "show parameters";
            }
        }

        private void ParameterInfoVisibility_Click(object sender, EventArgs e)
        {
            if (ParameterInfo.Visibility == Visibility.Visible)
            {
                ParameterInfo.Visibility = Visibility.Collapsed;
                // set the context menu label
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = "show parameters";
            }
            else // make it visible
            {
                ParameterInfo.Visibility = Visibility.Visible;
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = "hide parameters";

                // hide the barrier info & update its context menu item
                AddBarriersInfo.Visibility = Visibility.Collapsed;
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = "show add barriers";
            }
        }

        private void ChoicesButton_Click(object sender, RoutedEventArgs e)
        {
            ChoicesListBox.Tag = (sender as Button).Name;
            List<string> choices;

            switch ((sender as Button).Name)
            {
                case "TravelDirection2":
                    choices = travelDirectionChoices;
                    break;
                case "AttributeParameter2":
                    choices = attributeParameterChoices;
                    break;
                case "RestrictUTurns2":
                    choices = restrictUTurnChoices;
                    break;
                case "OutputLines2":
                    choices = outputLineChoices;
                    break;
                case "OutputGeometryPrecisionUnits2":
                    choices = outputGeomPrecisionChoices;
                    break;
                case "DirectionsLengthUnits2":
                    choices = directionsLengthUnitChoices;
                    break;
                default:
                    return;
            }

            ChoicesListBox.SelectionChanged -= ChoicesListBox_SelectionChanged;
            ChoicesListBox.ItemsSource = choices;
            int selectedIndex = choices.IndexOf((sender as Button).Content.ToString());
            ChoicesListBox.SelectedIndex = selectedIndex;
            ChoicesListBox.SelectionChanged += ChoicesListBox_SelectionChanged;
            ChoicesPage.IsOpen = true;

        }

        private void ChoicesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selected = e.AddedItems[0].ToString();

            switch (ChoicesListBox.Tag.ToString())
            {
                case "TravelDirection2":
                    TravelDirection2.Content = selected;
                    break;
                case "AttributeParameter2":
                    AttributeParameter2.Content = selected;
                    break;
                case "RestrictUTurns2":
                    RestrictUTurns2.Content = selected;
                    break;
                case "OutputLines2":
                    OutputLines2.Content = selected;
                    break;
                case "OutputGeometryPrecisionUnits2":
                    OutputGeometryPrecisionUnits2.Content = selected;
                    break;
                case "DirectionsLengthUnits2":
                    DirectionsLengthUnits2.Content = selected;
                    break;
                default:
                    break;
            }

            ChoicesPage.IsOpen = false;
        }

    }
}