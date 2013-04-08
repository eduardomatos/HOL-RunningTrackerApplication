using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;

namespace RunningTracker_WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const float KM_MILE = 0.000621371192f;
        private const double SourceLatitude = 48.85815;
        private const double SourceLongtitude = 2.29452;
        private const double DestinationLatitude = 48.860395;
        private const double DestinationLongtitude = 2.337599;

        MapLayer historicalReadingsLayer = new MapLayer();
        MapOverlay overlay = new MapOverlay();
        MapPolyline polyline = new MapPolyline();
        TextBlock txtCurrentSpeed = new TextBlock();

        Microsoft.Phone.Maps.Services.RouteQuery routeQuery = new Microsoft.Phone.Maps.Services.RouteQuery();
        List<GeoCoordinate> waypoints = new List<GeoCoordinate>();

        List<GeoPosition<GeoCoordinate>> positions = new List<GeoPosition<GeoCoordinate>>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // setup GPS
            map.ColorMode = Microsoft.Phone.Maps.Controls.MapColorMode.Light;
            map.CartographicMode = Microsoft.Phone.Maps.Controls.MapCartographicMode.Road;
            map.LandmarksEnabled = true;
            map.PedestrianFeaturesEnabled = true;
            map.ZoomLevel = 17;

            txtCurrentSpeed.Foreground = new SolidColorBrush(Colors.Black);
            overlay.Content = txtCurrentSpeed;

            routeQuery.TravelMode = Microsoft.Phone.Maps.Services.TravelMode.Walking;
            routeQuery.QueryCompleted += rq_QueryCompleted;

            MapLayer currentSpeedLayer = new MapLayer();
            currentSpeedLayer.Add(overlay);
            map.Layers.Add(currentSpeedLayer);
            map.Layers.Add(historicalReadingsLayer);
            map.MapElements.Add(polyline);

            base.OnNavigatedTo(e);
        }

        void rq_QueryCompleted(object sender, Microsoft.Phone.Maps.Services.QueryCompletedEventArgs<Microsoft.Phone.Maps.Services.Route> e)
        {
            if (null == e.Error)
            {
                polyline.Path.Clear();
                foreach (var point in e.Result.Legs.First().Geometry)
                    polyline.Path.Add(point);
            }
            else
                MessageBox.Show("Error occured:\n" + e.Error.Message);
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            map.Center = new GeoCoordinate(SourceLatitude, SourceLongtitude);
            map.Visibility = System.Windows.Visibility.Visible;

            List<GeoCoordinate> wayPoints = new List<GeoCoordinate>
                {
                    new GeoCoordinate(SourceLatitude, SourceLongtitude),
                    new GeoCoordinate(DestinationLatitude, DestinationLongtitude) 
                };

            if (!routeQuery.IsBusy)
            {
                routeQuery.Waypoints = wayPoints;
                routeQuery.QueryAsync();
            }
        }

    }
}