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

        GeoCoordinateWatcher gcw;
        bool isRunning = false;
        int count = 0;

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

            stkWorkoutDetails.DataContext = this;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            gcw.PositionChanged += gcw_PositionChanged;
            gcw.StatusChanged += gcw_StatusChanged;

            map.ColorMode = Microsoft.Phone.Maps.Controls.MapColorMode.Light;
            map.CartographicMode = Microsoft.Phone.Maps.Controls.MapCartographicMode.Road;
            map.LandmarksEnabled = true;
            map.PedestrianFeaturesEnabled = true;
            map.ZoomLevel = 17;

            if (!App.IsInBackground)
            {
                txtCurrentSpeed.Foreground = new SolidColorBrush(Colors.Black);
                overlay.Content = txtCurrentSpeed;

                routeQuery.TravelMode = Microsoft.Phone.Maps.Services.TravelMode.Walking;
                routeQuery.QueryCompleted += rq_QueryCompleted;

                MapLayer currentSpeedLayer = new MapLayer();
                currentSpeedLayer.Add(overlay);
                map.Layers.Add(currentSpeedLayer);
                map.Layers.Add(historicalReadingsLayer);
                map.MapElements.Add(polyline);
            }

            base.OnNavigatedTo(e);
        }

        void gcw_StatusChanged(object sender, System.Device.Location.GeoPositionStatusChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("New status: " + e.Status);

            if (e.Status == GeoPositionStatus.Ready)
            {
                map.Center = gcw.Position.Location;
                polyline.Path.Clear();

                btnStartStop.Content = "Stop";
                map.Visibility = System.Windows.Visibility.Visible;

                Mileage = 0;
                Runtime = TimeSpan.Zero;
            }
        }

        void gcw_PositionChanged(object sender, System.Device.Location.GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate> e)
        {
            System.Diagnostics.Debug.WriteLine("New position:" + e.Position.Location.Latitude + "," + e.Position.Location.Longitude);

            map.Center = e.Position.Location;
            map.Heading = e.Position.Location.Course;

            count++;

            polyline.Path.Add(e.Position.Location);

            if (count > 1)
            {
                txtCurrentSpeed.Text = e.Position.Location.Speed + "m/s";
                overlay.GeoCoordinate = e.Position.Location;
            }

            if (positions.Count > 0)
            {
                MapOverlay ovrl = new MapOverlay();
                TextBlock txtTimestamp = new TextBlock();
                txtTimestamp.Foreground = new SolidColorBrush(Colors.Black);
                ovrl.Content = txtTimestamp;
                ovrl.GeoCoordinate = positions.Last().Location;
                txtTimestamp.Text = string.Format("{0:hh\\:mm\\:ss}", positions.Last().Timestamp.TimeOfDay);
                historicalReadingsLayer.Add(ovrl);
            }

            positions.Add(e.Position);
            waypoints.Add(e.Position.Location);

            if (!routeQuery.IsBusy && count > 1)
            {

                routeQuery.InitialHeadingInDegrees = e.Position.Location.Course;
                routeQuery.Waypoints = waypoints;
                routeQuery.QueryAsync();
            }
        }

        void rq_QueryCompleted(object sender, Microsoft.Phone.Maps.Services.QueryCompletedEventArgs<Microsoft.Phone.Maps.Services.Route> e)
        {
            TimeSpan timeDiff = positions.Last().Timestamp.TimeOfDay - positions.First().Timestamp.TimeOfDay;

            Mileage = float.Parse(e.Result.LengthInMeters.ToString()) * KM_MILE;
            Runtime = timeDiff;

            System.Diagnostics.Debug.WriteLine("Passed: " + Mileage + " miles. in " + string.Format("{0:hh\\:mm\\:ss}", Runtime));
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                map.Visibility = System.Windows.Visibility.Collapsed;
                txtMessage.Text = "Getting ready...";
                gcw.Start();
                polyline.Path.Clear();

                isRunning = true;
            }
            else
            {
                gcw.Stop();

                btnStartStop.Content = "Start";
                isRunning = false;
            }
        }

        public float Mileage
        {
            get { return (float)GetValue(MileageProperty); }
            set { SetValue(MileageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Mileage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MileageProperty =
            DependencyProperty.Register("Mileage", typeof(float), typeof(MainPage), new PropertyMetadata(0.0f));


        public TimeSpan Runtime
        {
            get { return (TimeSpan)GetValue(RuntimeProperty); }
            set { SetValue(RuntimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Runtime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RuntimeProperty =
            DependencyProperty.Register("Runtime", typeof(TimeSpan), typeof(MainPage), new PropertyMetadata(TimeSpan.Zero));

    }
}