using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISWindowsPhoneSDK
{
    public partial class VideoFills : PhoneApplicationPage
    {
        List<Graphic> _lastActiveGraphics;

        public VideoFills()
        {
            InitializeComponent();
            _lastActiveGraphics = new List<Graphic>();

            MyMap.Layers.LayersInitialized += Layers_LayersInitialized;
        }

        void Layers_LayersInitialized(object sender, EventArgs args)
        {
            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query()
            {
                OutSpatialReference = MyMap.SpatialReference,
                ReturnGeometry = true
            };
            query.OutFields.Add("STATE_NAME");
            query.Where = "STATE_NAME IN ('Alaska', 'Hawaii', 'Washington', 'Oregon', 'Arizona', 'Nevada', 'Idaho', 'Montana', 'Utah', 'Wyoming', 'Colorado', 'New Mexico')";

            QueryTask myQueryTask = new QueryTask("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5");
            myQueryTask.ExecuteCompleted += myQueryTask_ExecuteCompleted;
            myQueryTask.ExecuteAsync(query);
        }

        void myQueryTask_ExecuteCompleted(object sender, QueryEventArgs queryArgs)
        {
            if (queryArgs.FeatureSet == null)
                return;

            FeatureSet resultFeatureSet = queryArgs.FeatureSet;
            ESRI.ArcGIS.Client.GraphicsLayer graphicsLayer =
                MyMap.Layers["MyGraphicsLayer"] as ESRI.ArcGIS.Client.GraphicsLayer;

            if (resultFeatureSet != null && resultFeatureSet.Features.Count > 0)
            {
                foreach (ESRI.ArcGIS.Client.Graphic graphicFeature in resultFeatureSet.Features)
                {
                    graphicFeature.Symbol = LayoutRoot.Resources["TransparentFillSymbol"] as Symbol;
                    graphicsLayer.Graphics.Add(graphicFeature);
                }
            }
        }

        private void GraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            string stateName = Convert.ToString(e.Graphic.Attributes["STATE_NAME"]);

            foreach (Graphic g in _lastActiveGraphics)
                g.Symbol = LayoutRoot.Resources["TransparentFillSymbol"] as Symbol;
            _lastActiveGraphics.Clear();

            if (_lastActiveGraphics.Count > 0)
            {
                for (int i = 0; i < _lastActiveGraphics.Count; i++)
                {
                    if (Convert.ToString(_lastActiveGraphics[i].Attributes["STATE_NAME"]) != stateName)
                        ClearVideoSymbol(_lastActiveGraphics[i]);
                    else
                        return;
                }
            }

            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;

            Grid videoGrid = FindName("MediaGrid") as Grid;
            videoGrid.Children.Clear();

            MediaElement stateMediaElement = new MediaElement()
            {
                Source = new Uri(String.Format("http://serverapps.esri.com/media/{0}_small.wmv", stateName), UriKind.Absolute),
                Stretch = Stretch.None,
                AutoPlay = true,
                IsMuted = true,
                Opacity = 0.0,
                IsHitTestVisible = false
            };
            stateMediaElement.MediaEnded += State_Media_MediaEnded;
            videoGrid.Children.Add(stateMediaElement);
            //FillSymbol stateVideoFillSymbol = LayoutRoot.Resources["StateVideoFillSymbol"] as FillSymbol;
            FillSymbol stateVideoFillSymbol = LayoutRoot.Resources["RedFillSymbol"] as FillSymbol;
            //(stateVideoFillSymbol.Fill as VideoBrush).SetSource(stateMediaElement);
            e.Graphic.Symbol = stateVideoFillSymbol;

            _lastActiveGraphics.Add(e.Graphic);
        }

        private void GraphicsLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            _lastActiveGraphics.Remove(e.Graphic);
        }

        private void ClearVideoSymbol(Graphic graphic)
        {
            Grid videoGrid = FindName("MediaGrid") as Grid;
            if (videoGrid.Children != null && videoGrid.Children.Count > 0)
            {
                MediaElement m = videoGrid.Children.ElementAt(0) as MediaElement;
                if (m != null)
                {
                    m.MediaEnded -= State_Media_MediaEnded;
                    m.Stop();
                }
                videoGrid.Children.Clear();
            }

            graphic.Symbol = LayoutRoot.Resources["TransparentFillSymbol"] as Symbol;

            _lastActiveGraphics.Remove(graphic);
        }

        private void State_Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Repeat play of the video
            MediaElement media = sender as MediaElement;
            media.Position = TimeSpan.FromSeconds(0);
            media.Play();
        }
    }
}