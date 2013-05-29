using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;

namespace ArcGISWindowsPhoneSDK
{
    public partial class MainPage : PhoneApplicationPage
    {
        object _selectedItem;
        string _xaml;
        int _listIndex = -1;
        int _lastListIndex = -1;
        List<ListBox> _listBoxes;

        //string _xmlUrl = "Samples.xml";

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox controls to the defined samples . . . maximum number for a category should be nine to keep within screen display
            _listBoxes = new List<ListBox>();

            // Mapping category
            MainViewModel mvm_map = new MainViewModel();
            mvm_map.Items.Add(new ItemViewModel() { Title = "ArcGIS Tiled Layer", XAML = "/Samples/Mapping/TiledLayer.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "ArcGIS Dynamic Layer", XAML = "/Samples/Mapping/DynamicLayer.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Dynamic and Tiled Layers", XAML = "/Samples/Mapping/DynamicAndTiledLayers.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Layer List", XAML = "/Samples/Mapping/LayerList.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Sublayer List", XAML = "/Samples/Mapping/SubLayerList.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Define Map Projection", XAML = "/Samples/Mapping/MapProjection.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Plane Projection", XAML = "/Samples/Mapping/PlaneProjection.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Show Map Extent", XAML = "/Samples/Mapping/ShowMapExtent.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Show Map Properties", XAML = "/Samples/Mapping/ShowMapProperties.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Show Coordinates", XAML = "/Samples/Mapping/Coordinates.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Switching Map Layers", XAML = "/Samples/Mapping/SwitchMap.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Pan Buttons", XAML = "/Samples/Mapping/PanButtons.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Adjust Map Animation", XAML = "/Samples/Mapping/MapAnimation.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Add Layer Dynamically", XAML = "/Samples/Mapping/AddLayerDynamically.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Element Layer", XAML = "/Samples/Mapping/ElementLayer.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Element Layer - Media", XAML = "/Samples/Mapping/ElementLayer_Media.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Group Layers", XAML = "/Samples/Mapping/GroupLayers.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Geodesic Operations", XAML = "/Samples/Mapping/GeodesicOperations.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Map Scale", XAML = "/Samples/Mapping/MapScale.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Hosted Feature Service", XAML = "/Samples/Mapping/HostedFeatureServiceSimple.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Hosted Tiled Service", XAML = "/Samples/Mapping/HostedTiledServiceSimple.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Dynamic Layers in Code", XAML = "/Samples/Mapping/DynamicLayerCode.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Dynamic Layers in XAML", XAML = "/Samples/Mapping/DynamicLayerXAML.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Generate Renderer", XAML = "/Samples/Mapping/GenerateRenderer.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Interactive Thematic Mapping", XAML = "/Samples/Mapping/DynamicLayerThematic.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Printing", XAML = "/Samples/Mapping/ExportWebMap.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Resample No Data Tiles", XAML = "/Samples/Mapping/ResampleNoDataTiles.xaml" });
            mvm_map.Items.Add(new ItemViewModel() { Title = "Swipe Map", XAML = "/Samples/Mapping/SwipeMap.xaml" });
            ListBox_Mapping.DataContext = mvm_map;
            _listBoxes.Add(ListBox_Mapping);

            // Toolkit category
            MainViewModel mvm_toolkit = new MainViewModel();
            mvm_toolkit.Items.Add(new ItemViewModel() { Title = "InfoWindow", XAML = "/Samples/Toolkit/InfoWindowSimple.xaml" });
            mvm_toolkit.Items.Add(new ItemViewModel() { Title = "InfoWindow Dynamic", XAML = "/Samples/Toolkit/InfoWindowDynamic.xaml" });
            mvm_toolkit.Items.Add(new ItemViewModel() { Title = "InfoWindow with Child Page", XAML = "/Samples/Toolkit/InfoWindowChildPage.xaml" });
            mvm_toolkit.Items.Add(new ItemViewModel() { Title = "Legend", XAML = "/Samples/Toolkit/LegendSimple.xaml" });
            mvm_toolkit.Items.Add(new ItemViewModel() { Title = "Legend with KML and WMS", XAML = "/Samples/Toolkit/LegendKmlWms.xaml" });
            mvm_toolkit.Items.Add(new ItemViewModel() { Title = "Legend With Templates", XAML = "/Samples/Toolkit/LegendWithTemplates.xaml" });
            mvm_toolkit.Items.Add(new ItemViewModel() { Title = "MapProgressBar", XAML = "/Samples/Toolkit/MapProgressBarSimple.xaml" });
            mvm_toolkit.Items.Add(new ItemViewModel() { Title = "ScaleLine", XAML = "/Samples/Toolkit/ScaleLine.xaml" });
           
            ListBox_Toolkit.DataContext = mvm_toolkit;
            _listBoxes.Add(ListBox_Toolkit);

            // Toolkit Data Sources category
            MainViewModel mvm_tds = new MainViewModel();
            mvm_tds.Items.Add(new ItemViewModel() { Title = "OpenStreetMap", XAML = "/Samples/DataSources/OpenStreetMapSimple.xaml" });
            mvm_tds.Items.Add(new ItemViewModel() { Title = "OSM Tile Servers", XAML = "/Samples/DataSources/OSMTileServers.xaml" });
            mvm_tds.Items.Add(new ItemViewModel() { Title = "GPS", XAML = "/Samples/DataSources/GpsSimple.xaml" });
            mvm_tds.Items.Add(new ItemViewModel() { Title = "KML", XAML = "/Samples/DataSources/KmlSimple.xaml" });
            mvm_tds.Items.Add(new ItemViewModel() { Title = "WMS", XAML = "/Samples/DataSources/WMSSimple.xaml" });
            mvm_tds.Items.Add(new ItemViewModel() { Title = "WMTS Layer", XAML = "/Samples/DataSources/WmtsLayerSimple.xaml" });
            mvm_tds.Items.Add(new ItemViewModel() { Title = "CSV Layer", XAML = "/Samples/DataSources/CSVLayer.xaml" });
            ListBox_ToolkitDataSources.DataContext = mvm_tds;
            _listBoxes.Add(ListBox_ToolkitDataSources);

            // Graphics category
            MainViewModel mvm_graphics = new MainViewModel();
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Add using XAML", XAML = "/Samples/Graphics/AddGraphicsXAML.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Add using Code", XAML = "/Samples/Graphics/AddGraphics.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Add interactively", XAML = "/Samples/Graphics/DrawGraphics.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Custom Symbols", XAML = "/Samples/Graphics/CustomSymbols.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Selections", XAML = "/Samples/Graphics/SelectGraphics.xaml" });
            //mvm_graphics.Items.Add(new ItemViewModel() { Title = "Video Fills", XAML = "/Samples/Graphics/VideoFills.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Simple Clusterer", XAML = "/Samples/Graphics/SimpleClusterer.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Custom Clusterer", XAML = "/Samples/Graphics/CustomClusterer.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Rendering with XAML", XAML = "/Samples/Graphics/RenderersXAML.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Rendering with Code", XAML = "/Samples/Graphics/Thematic_Interactive.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Show GeoRSS Feed", XAML = "/Samples/Graphics/GeoRSS.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Using GraphicsSource", XAML = "/Samples/Graphics/UsingGraphicsSource.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Auto-project Graphics", XAML = "/Samples/Graphics/AutoProjectGraphics.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Using PointDataSource", XAML = "/Samples/Graphics/UsingPointDataSource.xaml" });
            mvm_graphics.Items.Add(new ItemViewModel() { Title = "Symbol Rotation", XAML = "/Samples/Graphics/SymbolRotation.xaml" });
            ListBox_Graphics.DataContext = mvm_graphics;
            _listBoxes.Add(ListBox_Graphics);

            // Query category
            MainViewModel mvm_query = new MainViewModel();
            mvm_query.Items.Add(new ItemViewModel() { Title = "Query Only", XAML = "/Samples/Query/QueryWithoutMap.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Attribute Query", XAML = "/Samples/Query/AttributeQuery.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Spatial Query", XAML = "/Samples/Query/SpatialQuery.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Identify", XAML = "/Samples/Query/Identify.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Find", XAML = "/Samples/Query/Find.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Query with a Buffer", XAML = "/Samples/Query/BufferQuery.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Query with Order By Field", XAML = "/Samples/Query/OrderByFieldQuery.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Statistics", XAML = "/Samples/Query/Statistics.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Statistics with Map", XAML = "/Samples/Query/StatisticsRenderOnMap.xaml" });
            mvm_query.Items.Add(new ItemViewModel() { Title = "Query Related Records", XAML = "/Samples/Query/QueryRelatedRecords.xaml" });      
            ListBox_Query.DataContext = mvm_query;
            _listBoxes.Add(ListBox_Query);

            // FeatureLayer category
            MainViewModel mvm_featurelayer = new MainViewModel();
            mvm_featurelayer.Items.Add(new ItemViewModel() { Title = "Simple", XAML = "/Samples/FeatureLayers/FeatureLayerSimple.xaml" });
            mvm_featurelayer.Items.Add(new ItemViewModel() { Title = "Filtering", XAML = "/Samples/FeatureLayers/FeatureLayerFiltering.xaml" });
            mvm_featurelayer.Items.Add(new ItemViewModel() { Title = "Rendering", XAML = "/Samples/FeatureLayers/FeatureLayerRendering.xaml" });
            mvm_featurelayer.Items.Add(new ItemViewModel() { Title = "Selection", XAML = "/Samples/FeatureLayers/FeatureLayerSelection.xaml" });
            mvm_featurelayer.Items.Add(new ItemViewModel() { Title = "On Demand", XAML = "/Samples/FeatureLayers/FeatureLayerOnDemand.xaml" });
            mvm_featurelayer.Items.Add(new ItemViewModel() { Title = "Versions", XAML = "/Samples/FeatureLayers/FeatureLayerChangeVersion.xaml" });
            ListBox_FeatureLayers.DataContext = mvm_featurelayer;
            _listBoxes.Add(ListBox_FeatureLayers);

            // Editing category
            MainViewModel mvm_edit = new MainViewModel();
            mvm_edit.Items.Add(new ItemViewModel() { Title = "Simple", XAML = "/Samples/Editing/SimpleEditing.xaml" });
            mvm_edit.Items.Add(new ItemViewModel() { Title = "With Coded Value Domain", XAML = "/Samples/Editing/EditingCodedValueDomains.xaml" });
            mvm_edit.Items.Add(new ItemViewModel() { Title = "Editor Tracking", XAML = "/Samples/Editing/EditorTracking.xaml" });
            mvm_edit.Items.Add(new ItemViewModel() { Title = "Attribute Only", XAML = "/Samples/Editing/AttributeOnlyEditing.xaml" });
            mvm_edit.Items.Add(new ItemViewModel() { Title = "Ownership Based", XAML = "/Samples/Editing/OwnershipBasedEditing.xaml" });
            mvm_edit.Items.Add(new ItemViewModel() { Title = "Edit Tools - Selection Only", XAML = "/Samples/Editing/EditToolsSelectionOnly.xaml" });      
            ListBox_Editing.DataContext = mvm_edit;
            _listBoxes.Add(ListBox_Editing);

            // Address category
            MainViewModel mvm_address = new MainViewModel();
            mvm_address.Items.Add(new ItemViewModel() { Title = "Find an Address", XAML = "/Samples/Address/AddressToLocation.xaml" });
            mvm_address.Items.Add(new ItemViewModel() { Title = "Address of a Location", XAML = "/Samples/Address/LocationToAddress.xaml" });
            mvm_address.Items.Add(new ItemViewModel() { Title = "World Geocoding", XAML = "/Samples/Address/WorldGeocoding.xaml" });
            ListBox_Address.DataContext = mvm_address;
            _listBoxes.Add(ListBox_Address);

            // Network category
            MainViewModel mvm_network = new MainViewModel();
            mvm_network.Items.Add(new ItemViewModel() { Title = "Routing", XAML = "/Samples/Network/Routing.xaml" });
            mvm_network.Items.Add(new ItemViewModel() { Title = "Routing with Barriers", XAML = "/Samples/Network/RoutingBarriers.xaml" });
            mvm_network.Items.Add(new ItemViewModel() { Title = "Driving Directions", XAML = "/Samples/Network/RoutingDirections.xaml" });
            mvm_network.Items.Add(new ItemViewModel() { Title = "Closest Facility", XAML = "/Samples/Network/ClosestFacility.xaml" });
            mvm_network.Items.Add(new ItemViewModel() { Title = "Service Areas", XAML = "/Samples/Network/ServiceAreas.xaml" });
            ListBox_Network.DataContext = mvm_network;
            _listBoxes.Add(ListBox_Network);

            // Geoprocessing category
            MainViewModel mvm_gp = new MainViewModel();
            mvm_gp.Items.Add(new ItemViewModel() { Title = "Message in a Bottle", XAML = "/Samples/Geoprocessing/MessageInABottle.xaml" });
            mvm_gp.Items.Add(new ItemViewModel() { Title = "Drive Times", XAML = "/Samples/Geoprocessing/DriveTimes.xaml" });
            mvm_gp.Items.Add(new ItemViewModel() { Title = "Calculate Viewshed", XAML = "/Samples/Geoprocessing/ViewShed.xaml" });
            mvm_gp.Items.Add(new ItemViewModel() { Title = "Clip Features", XAML = "/Samples/Geoprocessing/ClipFeatures.xaml" });
            ListBox_Geoprocessing.DataContext = mvm_gp;
            _listBoxes.Add(ListBox_Geoprocessing);

            // Utilities category
            MainViewModel mvm_utilities = new MainViewModel();
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Area and Perimeter", XAML = "/Samples/Utilities/AreaAndLengths.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "AutoComplete", XAML = "/Samples/Utilities/AutoComplete.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Buffer a Point", XAML = "/Samples/Utilities/BufferPoint.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Convex Hull", XAML = "/Samples/Utilities/ConvexHull.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Cut", XAML = "/Samples/Utilities/Cut.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Densify", XAML = "/Samples/Utilities/Densify.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Difference", XAML = "/Samples/Utilities/Difference.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Distance", XAML = "/Samples/Utilities/Distance.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Generalize", XAML = "/Samples/Utilities/Generalize.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Intersect", XAML = "/Samples/Utilities/Intersect.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Label Points", XAML = "/Samples/Utilities/LabelPoints.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Line Length", XAML = "/Samples/Utilities/Lengths.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Offset", XAML = "/Samples/Utilities/Offset.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Project", XAML = "/Samples/Utilities/Project.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Relation", XAML = "/Samples/Utilities/Relation.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Reshape", XAML = "/Samples/Utilities/Reshape.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Simplify", XAML = "/Samples/Utilities/Simplify.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Trim or Extend", XAML = "/Samples/Utilities/TrimExtend.xaml" });
            mvm_utilities.Items.Add(new ItemViewModel() { Title = "Union", XAML = "/Samples/Utilities/Union.xaml" });
            ListBox_Utilities.DataContext = mvm_utilities;
            _listBoxes.Add(ListBox_Utilities);

            // BingMaps category
            MainViewModel mvm_bing = new MainViewModel();
            mvm_bing.Items.Add(new ItemViewModel() { Title = "Imagery", XAML = "/Samples/BingMaps/BingImagery.xaml" });
            mvm_bing.Items.Add(new ItemViewModel() { Title = "Routing", XAML = "/Samples/BingMaps/BingRouting.xaml" });
            ListBox_BingMaps.DataContext = mvm_bing;
            _listBoxes.Add(ListBox_BingMaps);

            // ImageServices category
            MainViewModel mvm_image = new MainViewModel();
            mvm_image.Items.Add(new ItemViewModel() { Title = "Simple", XAML = "/Samples/ImageServices/SimpleImageService.xaml" });
            mvm_image.Items.Add(new ItemViewModel() { Title = "Query", XAML = "/Samples/ImageServices/QueryImageService.xaml" });
            mvm_image.Items.Add(new ItemViewModel() { Title = "Shaded Relief", XAML = "/Samples/ImageServices/ShadedReliefImageService.xaml" });
            mvm_image.Items.Add(new ItemViewModel() { Title = "Raster Function", XAML = "/Samples/ImageServices/RasterFunctionImageService.xaml" });
            mvm_image.Items.Add(new ItemViewModel() { Title = "Stretch", XAML = "/Samples/ImageServices/StretchImageService.xaml" });
            mvm_image.Items.Add(new ItemViewModel() { Title = "Mensuration", XAML = "/Samples/ImageServices/Mensuration.xaml" });
            ListBox_ImageServices.DataContext = mvm_image;
            _listBoxes.Add(ListBox_ImageServices);

            // JSON category
            MainViewModel mvm_json = new MainViewModel();
            mvm_json.Items.Add(new ItemViewModel() { Title = "Feature Set", XAML = "/Samples/JSON/FeatureSetJson.xaml" });
            mvm_json.Items.Add(new ItemViewModel() { Title = "Feature Layer", XAML = "/Samples/JSON/FeatureLayerFromJson.xaml" });
            mvm_json.Items.Add(new ItemViewModel() { Title = "Renderers", XAML = "/Samples/JSON/RendererJson.xaml" });
            mvm_json.Items.Add(new ItemViewModel() { Title = "Symbols", XAML = "/Samples/JSON/SymbolJson.xaml" });
            mvm_json.Items.Add(new ItemViewModel() { Title = "Create WebMap From JSON", XAML = "/Samples/JSON/CreateWebMapFromJson.xaml" });
            mvm_json.Items.Add(new ItemViewModel() { Title = "Geometry To/From JSON", XAML = "/Samples/JSON/GeometryJSON.xaml" });
            ListBox_Json.DataContext = mvm_json;
            _listBoxes.Add(ListBox_Json);

            // WebMaps category
            MainViewModel mvm_webmaps = new MainViewModel();
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Load WebMap", XAML = "/Samples/WebMaps/LoadWebMap.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Load WebMap Dynamically", XAML = "/Samples/WebMaps/LoadWebMapDynamically.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Load Secure WebMap", XAML = "/Samples/WebMaps/LoadSecureWebMap.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Load WebMap with Bing", XAML = "/Samples/WebMaps/LoadWebMapWithBing.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Map Notes with Pop-ups", XAML = "/Samples/WebMaps/WebMapNotesPopups.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Feature Service with Pop-ups", XAML = "/Samples/WebMaps/WebMapFeatureServicePopups.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Dynamic Service with Pop-ups", XAML = "/Samples/WebMaps/WebMapDynamicServicePopups.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Tiled Service with Pop-ups", XAML = "/Samples/WebMaps/WebMapTiledServicePopups.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "WebMap with Charts", XAML = "/Samples/WebMaps/WebMapCharts.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "WebMap with CSV", XAML = "/Samples/WebMaps/WebMapCSV.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "WebMap with WMS", XAML = "/Samples/WebMaps/WebMapWMS.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "WebMap with KML", XAML = "/Samples/WebMaps/WebMapKML.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Mobile Content Server", XAML = "/Samples/WebMaps/WebMapMobileContentServer.xaml" });
            mvm_webmaps.Items.Add(new ItemViewModel() { Title = "Create WebMap Object", XAML = "/Samples/WebMaps/CreateWebMapObject.xaml" });
            ListBox_WebMaps.DataContext = mvm_webmaps;
            _listBoxes.Add(ListBox_WebMaps);

            // Portal category
            MainViewModel mvm_portal = new MainViewModel();
            mvm_portal.Items.Add(new ItemViewModel() { Title = "Portal Properties", XAML = "/Samples/Portal/PortalMetadata.xaml" });
            mvm_portal.Items.Add(new ItemViewModel() { Title = "Portal Search", XAML = "/Samples/Portal/PortalSearch.xaml" });
            ListBox_Portal.DataContext = mvm_portal;
            _listBoxes.Add(ListBox_Portal);

            // TimeAware category
            MainViewModel mvm_timeaware = new MainViewModel();
            mvm_timeaware.Items.Add(new ItemViewModel() { Title = "Map Service Over Time", XAML = "/Samples/TimeAware/TimeMapService.xaml" });
            mvm_timeaware.Items.Add(new ItemViewModel() { Title = "Feature Layer Over Time", XAML = "/Samples/TimeAware/TimeFeatureLayer.xaml" });
            mvm_timeaware.Items.Add(new ItemViewModel() { Title = "Image Service Over Time", XAML = "/Samples/TimeAware/TimeImageService.xaml" });
            // these don't work well on win phone 7
            // a) the LatestObservationRenderer doesn't work reliably (never in the emulator and only sometimes on the device)
            // b) tracks don't show up in the hurricane example
            //mvm_timeaware.Items.Add(new ItemViewModel() { Title = "Temporal Renderer - Points", XAML = "/Samples/TimeAware/TemporalRendererPoints.xaml" });
            //mvm_timeaware.Items.Add(new ItemViewModel() { Title = "Temporal Renderer - Tracks", XAML = "/Samples/TimeAware/TemporalRendererTracks.xaml" });
            ListBox_TimeAware.DataContext = mvm_timeaware;
            _listBoxes.Add(ListBox_TimeAware);

            // IdentityManager category
            MainViewModel mvm_security = new MainViewModel();
            mvm_security.Items.Add(new ItemViewModel() { Title = "Identity Manager - Services", XAML = "/Samples/Security/IdentityManagerServices.xaml" });
            ListBox_Security.DataContext = mvm_security;
            _listBoxes.Add(ListBox_Security);

            // Device category
            MainViewModel mvm_extras = new MainViewModel();
            mvm_extras.Items.Add(new ItemViewModel() { Title = "ArcGISWebClient", XAML = "/Samples/Extras/ArcGISWebClientSimple.xaml" });
            mvm_extras.Items.Add(new ItemViewModel() { Title = "Elevation Profile", XAML = "/Samples/Extras/ElevationProfile/ElevationProfile.xaml" });
            mvm_extras.Items.Add(new ItemViewModel() { Title = "SOE Elevation - Data Contract", XAML = "/Samples/Extras/SOEElevationDataContract.xaml" });
            mvm_extras.Items.Add(new ItemViewModel() { Title = "SOE Elevation - JSON Object", XAML = "/Samples/Extras/SOEElevationLatLonJsonObject.xaml" });
            mvm_extras.Items.Add(new ItemViewModel() { Title = "SOE Elevation Data", XAML = "/Samples/Extras/SOEElevationData.xaml" });
            ListBox_Extras.DataContext = mvm_extras;
            _listBoxes.Add(ListBox_Extras);

            // What's New category
            MainViewModel mvm_whatsnew = new MainViewModel();
            ListBox_WhatsNew.DataContext = mvm_whatsnew;
            mvm_whatsnew.Items.Add(new ItemViewModel() { Title = "CSV Layer", XAML = "/Samples/DataSources/CSVLayer.xaml" });
            mvm_whatsnew.Items.Add(new ItemViewModel() { Title = "OSM Tile Servers", XAML = "/Samples/DataSources/OSMTileServers.xaml" });
            mvm_whatsnew.Items.Add(new ItemViewModel() { Title = "World Geocoding", XAML = "/Samples/Address/WorldGeocoding.xaml" });
            mvm_whatsnew.Items.Add(new ItemViewModel() { Title = "Geometry To/From JSON", XAML = "/Samples/JSON/GeometryJSON.xaml" });
            mvm_whatsnew.Items.Add(new ItemViewModel() { Title = "ArcGISWebClient", XAML = "/Samples/Extras/ArcGISWebClientSimple.xaml" });
            mvm_whatsnew.Items.Add(new ItemViewModel() { Title = "Elevation Profile", XAML = "/Samples/Extras/ElevationProfile/ElevationProfile.xaml" });
            _listBoxes.Add(ListBox_WhatsNew);
        }

        // When page is navigated to, set data context 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set the data context of the listbox control to the sample data
            if (DataContext == null)
                DataContext = App.ViewModel;
        }

        // When back key is pressed and samples from one category are displayed, go to top menu list for all samples 
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (CategoryListGrid.Visibility == Visibility.Collapsed)
            {
                ESRI.ArcGIS.Client.IdentityManager.Current.ChallengeMethod = null;
                foreach (IdentityManager.Credential crd in ESRI.ArcGIS.Client.IdentityManager.Current.Credentials)
                    ESRI.ArcGIS.Client.IdentityManager.Current.RemoveCredential(crd);
                ShowMainMenu();
                e.Cancel = true;
            }
        }

        private void ListBoxCategory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ESRI.ArcGIS.Client.IdentityManager.Current.ChallengeMethod = null;
            foreach (IdentityManager.Credential crd in ESRI.ArcGIS.Client.IdentityManager.Current.Credentials)
                ESRI.ArcGIS.Client.IdentityManager.Current.RemoveCredential(crd);

            (ApplicationBar.Buttons[2] as IApplicationBarIconButton).IsEnabled = true;

            // Capture selected item data
            _selectedItem = (sender as ListBox).SelectedItem;
            if (_selectedItem != null)
            {
                ItemViewModel ivm = _selectedItem as ItemViewModel;
                _xaml = ivm.XAML;
                // Start page transition animation
                NavigationService.Navigate(new Uri(_xaml, UriKind.Relative));
                FrameworkElement root = Application.Current.RootVisual as FrameworkElement;
                root.DataContext = _selectedItem;
            }
            (sender as ListBox).SelectedItem = null;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (_listIndex > 0)
                _listIndex--;
            else
                _listIndex = _listBoxes.Count - 1;
            SwitchLists();
            //CategoryListBox.SelectedIndex = _listIndex;
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            if (_listIndex < _listBoxes.Count - 1)
                _listIndex++;
            else
                _listIndex = 0;
            SwitchLists();
            //CategoryListBox.SelectedIndex = _listIndex;
        }

        private void SwitchLists()
        {
            ContentPanel.Visibility = Visibility.Visible;
            ListTitle.Visibility = Visibility.Visible;

            // Reset the IdentityManager in order not to impact other samples
            ESRI.ArcGIS.Client.IdentityManager.Current.ChallengeMethod = null;
            foreach (IdentityManager.Credential crd in ESRI.ArcGIS.Client.IdentityManager.Current.Credentials)
                ESRI.ArcGIS.Client.IdentityManager.Current.RemoveCredential(crd);

            //if (_lastListIndex != _listIndex)
            //{
            if (_lastListIndex > -1)
                _listBoxes[_lastListIndex].Visibility = Visibility.Collapsed;
            _listBoxes[_listIndex].Visibility = Visibility.Visible;
            ListTitle.Text = (string)_listBoxes[_listIndex].Tag;
            //}

            _lastListIndex = _listIndex;
            CategoryListGrid.Visibility = Visibility.Collapsed;
            (ApplicationBar.Buttons[2] as IApplicationBarIconButton).IsEnabled = true;
        }

        private void Menu_List_Click(object sender, EventArgs e)
        {
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            (ApplicationBar.Buttons[2] as IApplicationBarIconButton).IsEnabled = false;
            CategoryListGrid.Visibility = Visibility.Visible;
            ContentPanel.Visibility = Visibility.Collapsed;
            ListTitle.Visibility = Visibility.Collapsed;
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            _listIndex = Convert.ToInt32(button.Tag);
            CategoryListGrid.Visibility = Visibility.Collapsed;
            if (_listIndex > -1)
                SwitchLists();
        }

        private void fillColor_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            (sender as Rectangle).Opacity = 0.0;
        }

        private void fillColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as Rectangle).Opacity = 0.45;
        }
    }
}
