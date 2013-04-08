<a name="title" />
#Running Tracker Application#

---
<a name="Overview" />
## Overview ##
The running tracker application is a demo application that allows the user to get statistics of his running track, the application shows the user the track of his running over a map.

This application uses three new capabilities: Nokia map control, Route Query and the run in background capability.

The map control is a new control that allows the developer an easy interaction with the map. It uses the Bing map services to get map as geographic data. In order to get the current location of the device, the application will use the GPS.

The QueryRoute class is used to get information regarding the route between few geo locations.

The run in background is highly important for this kind of application, since in previous versions of Windows Phone, if an application was tombstoned the application was stop and wasn't running any more. The run in background feature allows the application to continue running even while not being available to the user.

When an application is active it may be interrupted by a phone call or when the user decides to press the start button, in such cases the application will be deactivated.

This lab will take you through the steps required to create an application that uses all three new capabilities.

### Objectives ###

This lab provides instructions to help you achieve the following:

- Learn how to work with map control

- Learn how to Integrate the map with GPS

- Learn how to use the route service

- Learn how to run an application in the background

<a name="technologies" />
### Prerequisites ###

The following is required to complete this hands-on lab:

- Previous Windows Phone development experience.

- Visual Studio 2012 Express for Windows Phone or Microsoft Visual Studio 2012

- Windows Phone Developer Tools

> **Note:** All of these Tools can be downloaded together in a single package from [http://developer.windowsphone.com](http://developer.windowsphone.com/)

<a name="Exercises" />
## Exercises ##
This hands-on lab includes the following exercises:

1. [Working with Maps](#Exercise1)

1. [Using Location Services](#Exercise2)

Estimated time to complete this lab: **at least 60 minutes**.

<a name="Exercise1" />
## Exercise 1 - Working with Maps ##

In this exercise we will use the Map control and the Route Query to create the basics use of the application. In this exercise we will create the single view of this application which contains the map control and a button to start tracking the run.
Since in the exercise we won't use a real GPS we will simulate some geo coordinate.

<a name="Ex1Task1" />
#### Task 1 - WMAppManifest changes ####

The Running Tracker application uses the Nokia map control. In order to use the map control the application must define it as a capability. In this task we will add this capability to the configuration.

This task also creates the application from the beginning.

Lets' create the application:

1. Open Visual Studio 2012

1. Create new Windows Phone Application and name it **RunningTracker**

1. Locate the WMAppManifest

1. Locate the Capabilities section

1. Add the following:

	````XML
	<Capability Name="ID_CAP_MAP"/>  
 	````
 
<a name="Ex1Task2" />
#### Task 2 - UI ####

In this task we will add the application with some UI.

The entire view is located in the MainPage.xaml. Its default will be divided into two areas, while the first being the header part and the second used for the content.
We will modify mainly the content part which will be divided into three parts, as follows-
First, there's the map, which will take most of the view, while on top of it there will be a textblock that will show the state of the map.
Below the map there will be a few textblocks that will show the statistics of the run and at the bottom we will add a button to start and stop the run.

_**Modify the MainPage.xaml:**_

1. Open the MainPage.xaml.

1. Locate the textblock with text **MY APPLICATION**, change its text to "Running Tracker"

1. Locate the textblock with text **page name**, changes its text to "Let's run!"

1. Now the header is set we are ready to move to the content part.
In order to divide the content into three areas we will create to the **ContentPanel**'s three rows:

	````XML
	<Grid.RowDefinitions>
		<RowDefinition Height="*"/>
		<RowDefinition Height="Auto"/>
	</Grid.RowDefinitions>
	````

1. In this code fragment we have declared that the first row will take as much space available for the map control, to be defined next, after defining the "Start/Stop" button and textblocks.

1. Now we will add the "Start"/"Stop" button. The next xaml code should be added right after the previous code:

	````XML
	<StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Center">
		<Button Content="Start" x:Name="btnStartStop" Click="btnStartStop_Click" MinWidth="200"/>
	</StackPanel>
	````

1. This code added a new button to the view. This button will be used to start and stop the run and will change its' face from within the code behind from "Start" to "Stop" accordingly.

1. The next stage is to add is the map control itself. Before we can do that we first have to add the xml namespace for the map control. 
Add the following xml namespace to the root of the page:

	````XML
	xmlns:maps="clr-namespace:Microsoft.Phone.Maps.Controls;assembly=Microsoft.Phone.Maps"
	````

1. Now we can use the map control by adding the following xaml code:

	````XML
	<maps:Map x:Name="map" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Visibility="Collapsed"/>
	<TextBlock Text="Tap 'Start' button when ready" HorizontalAlignment="Center" VerticalAlignment="Center" x:Name="txtMessage"/>
	````

1. At the current stage, the view is ready, but the project will not pass a Build process since there are events declarations in the XAML file that were not yet implemented in the code behind.
This will be done in the next task.

<a name="Ex1Task3" />
#### Task 3 - Implementation ####

 The Running Tracker application allows the user to use the GPS to track his/hers jogging route. In this early task we will not yet use the GPS but rather use static coordinates to simulate a route. 
This will allow us to focus on two main issues:  Map Control and the RouteQuery.

 We begin by adding the code behind to the **MainPage.xaml.cs**:

1. First we will add required references:

	````C#
	using Microsoft.Phone.Maps.Controls;
	using System.Device.Location;
	using System.Windows.Media;    
	````

1. Next we will add the data members:

	````C#
	private const float KM_MILE = 0.000621371192f;
	private const double SourceLatitude = 48.85815;
	private const double SourceLongtitude = 2.29452;
	private const double DestinationLatitude = 48.860395;
	private const double DestinationLongtitude = 2.337599;
	
	MapLayer historicalReadingsLayer = new MapLayer();
	MapOverlay overlay = new MapOverlay();
	MapPolyline polyline = new MapPolyline();
	TextBlock txtCurrentSpeed = new TextBlock();

	Microsoft.Phone.Maps.Services.RouteQuery routeQuery = 
		new Microsoft.Phone.Maps.Services.RouteQuery();
	List<GeoCoordinate> waypoints = new List<GeoCoordinate>();

	List<GeoPosition<GeoCoordinate>> positions = 
		new List<GeoPosition<GeoCoordinate>>();
	````

	We can see here many data members in the above code, let's review some of them:
One of the application goals is to display the route of the run. Therefore we need to collect all of the points the user goes through, along with the time in which the user passed through each point.

	The data member we use for that purpose is the "historicalReadingsLayer". Any time the user's GPS changes position, another point is added to that object. For the current exercise purposes we only add two hardcoded points as we do not use the GPS hardware yet.

	The "overlay" member is used to display the current movement speed over the map. It will be set at the position of the current geo point and will show the calculated speed.

	The "polyline" member is used to draw the blue line between the points.

	"routeQuery" is the object that provides the route between any two or more points on the map.

	"waypoints" saves all of the geo points that the user has gone through. The difference between this object and the "historicalReadingsLayer" is that the "waypoints" member is data only and not map-UI oriented. 

	"waypoints" is being provided to the "routeQuery" in order to calculate the route between the points.

	The last member is the "positions" member that saves all of the GeoPositions<GeoCoordinate> the user runs through. A GeoPosition<GeoCoordinate> consists of a GeoCoordinate and timestamp.

1. The next step is to override the OnNavigatedTo method, as follows:

	````C#
	protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
	{
		// setup GPS

		map.ColorMode = Microsoft.Phone.Maps.Controls.MapColorMode.Light;
		map.CartographicMode = 
			Microsoft.Phone.Maps.Controls.MapCartographicMode.Road;
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
	````

	Let's review the code above:

	The first line is a comment place-holder for our next exercise where we will add the GPS capabilities.
Next we have some interaction with the map object where we set it to operate as required,
set the overlay with the textbox and set the controls' colors.
The RouteQuery member is being set to "Walking" mode as the application is meant to do jog recording.
It is important to note that RouteQuery works in an asynchronous pattern, so we need to subscribe to the callback.

	The map has a few layers and maps elements which have to be set, and then at the end of the method we invoke two methods, which will only be used for the current exercise and will be replaced later on.

1. Since we executed an async call to the RouteQuery method at the end of the previous method, it is time to add the callback method:

	````C#
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
	````

	The callback provides us with a set of GeoCoordinates, which we use in order to re-create the "polyline" member, bound to the map, in order to draw the line between the various points.

1. The last method is the one that sets the next point, which occurs when the start button is clicked.

	Let's add the following method:

	````C#
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
	````

	This method occurs when the "Start"/"Stop" button is being clicked. For the current exercise, we use the button only as a "Start" button.

	The first thing to do upon starting the recording is to center the map control to the temporary static simulated location we defined earlier using the static members- "SourceLatitude" and "SourceLongtitude".
We then create a new wayPoints list and set it to simulate movement between "Source.." and "Destination.." locations.

	The last step is to set the routeQuery's "WayPoints" member to the new information and call upon "QueryAsync", depending that the routeQuery is not busy doing previous calculations.

	In the next exercise we will alter the above code in order to connect to the GPS and use real data.

1. You can now run the application. Once run, the first point will be displayed. Clicking the "Start" button will reveal the next point, draw the line between them.

<a name="Exercise2" />
### Exercise 2 - Using Location Services ###

 Up until now the application that we created wasn't really working with the GPS hardware but was rather navigating from one pre-known location to another.

 This exercise will demonstrate how to add GPS information to the application.

 For that, we will have to add the GPS capabilities to the Running tracker application, and therefore make three main changes to the application: WMAppManifest alteration, GPS API interaction and GPS location movements simulations.

<a name="Ex2Task1" />
#### Task 1 - WMAppManifest changes ####

 In this task we rely on the code that was created in the previous exercise and apply the changes required in the WMAppManifest.

 As the application main goal is to track the current locations of the user, it must stay active the for the complete user run session. But, during the run the user will probably not be using the phone at all, thus the application will become deactivated after a while, therefore ceasing to record the track, count time and calculating the distance. 
 We can overcome that by changing the WMAppManifest file in order to set the recording portion of the application to be run in the background, keeping the location tracking ON and not losing any important data.

Let's begin:

1. Open the Begin solution from the **Source/Ex2-LocationServices\Begin** folder for this exercise

1. Locate the WMAppManifest

1. Locate the Task section, under it the DefaultTask

1. Expand the DefaultTask and add the following within it:

	````XML
	<BackgroundExecution>
		<ExecutionType Name="LocationTracking" />
	</BackgroundExecution> 
	````

1. In the MainPage.xaml replace the Content Panel section with the following XAML code:

	````XML
	<Grid x:Name="ContentPanel" Grid.Row="1" Margin="12,0,12,0">
		<Grid.RowDefinitions>
			<RowDefinition Height="*"/>
			<RowDefinition Height="Auto"/>
			<RowDefinition Height="Auto"/>
		</Grid.RowDefinitions>
		<StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Center">
			<Button Content="Start" x:Name="btnStartStop" Click="btnStartStop_Click" MinWidth="200"/>
		</StackPanel>
		<StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Stretch" x:Name="stkWorkoutDetails">
			<TextBlock Text="Workout details: "/>
			<TextBlock Text="{Binding Mileage, StringFormat=\{0:F\} mi.}"/>
			<TextBlock Text=" in "/>
			<TextBlock Text="{Binding Runtime, StringFormat=\{0:hh\\:mm\\:ss\} minutes}"/>
		</StackPanel>
		<TextBlock Text="Tap 'Start' button when ready" HorizontalAlignment="Center" VerticalAlignment="Center" x:Name="txtMessage"/>
		<maps:Map x:Name="map" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Visibility="Collapsed">
		</maps:Map>
	</Grid>
	````

<a name="Ex2Task2" />
#### Task 2 - GPS ####

 As we now want to use real GPS information, instead of the fake static points we created on the previous exercise, we are going to alter the various methods accordingly.

 Let's get started:

1. Locate the MainPage.xaml.cs

1. Add the following variables

	<!-- mark:1,3 -->
	````C#
	GeoCoordinateWatcher gcw;
	bool isRunning = false;
	int count = 0;
	````

	**gcw** is the object that interacts with the GPS sensor.

1. Add the following code to the constructor:

	````C#
	stkWorkoutDetails.DataContext = this;
	````

	Later on this module we will see that the MainPage has few Dependency Properties, used to display data on the UI. The UI is therefore bound to these properties while the DataContext has to be the MainPage.

1. We now have to initialize the "gcw" object from within the OnNavigateTo method:

	````C#
	gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
	gcw.PositionChanged += gcw_PositionChanged;
	gcw.StatusChanged += gcw_StatusChanged;
	````

	The code above initializes the GeoCoordinateWathcer with the Enum value defining accuracy to "High", meaning highest possible GPS available.

	We also register two event handlers:

	StatusChanged - occurs once the GPS is either initialized or stops working for some reason

	PositionChanged - occurs when the GPS detects movement

1. Let's add the event handlers themselves. We begin with gcw_StatusChanged:

	````C#
	void gcw_StatusChanged(object sender, System.Device.Location.GeoPositionStatusChangedEventArgs e)
	{
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
	````

	This method occurs is being called to initialize and prepare the map control by centering to the current location, and to reset the various members used for the measurement process.

1. The other event handler is gcw_PositionChanged:

	````C#
	void gcw_PositionChanged(object sender, System.Device.Location.GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate> e)
	{
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
	````

	This method, called whenever new readings arrive from the GPS, is the heart of our application's logic. It is responsible for the following tasks:

	Centering the map control to the current position of user, calculating and displaying the current time and speed and drawing the lines representing the running track.

	Based on the previous exercise, the last segment calls upon the asynchronous "QueryAsync" method on the "routeQuery" member, again- only if not already busy on a previous call.

1. Before we continue altering the code we have to address an important issue: if you recall, at the beginning of this exercise we added the capability to keep running while deactivated to the application.

	This means that our code may run in two different scenarios- when "First time run" or when "Re-Activated".

	For that, we have to add code to the App.xaml.cs that will allow us to detect the current scenario.

	Let's add the following variable to the App.xaml.cs:

	````C#
	public static bool IsInBackground = false;
	````

	And register for the "RunningInBackground" event by altering the generated code in "App.xaml":

	````C#
	<shell:PhoneApplicationService 
		Launching="Application_Launching" Closing="Application_Closing" RunningInBackground="Application_RunningInBackground"
		Activated="Application_Activated" Deactivated="Application_Deactivated"/>
	````

1. We should now implement the event handler in "App.xaml.cs":

	````C#
	private void Application_RunningInBackground(object sender, RunningInBackgroundEventArgs e)
	{
		IsInBackground = true;
	}
	````

	This method will be executed only when the application has been reactivated, therefore allowing us to alter our custom Boolean member.

1. Now that we have the indication that the application has been reactivated we can add a few changes to the OnNavigatedTo method, performing the UI update only when not in background.
The resulting code should look as follows:

	````C#
		gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
		gcw.PositionChanged += gcw_PositionChanged;
		gcw.StatusChanged += gcw_StatusChanged;

		map.ColorMode = Microsoft.Phone.Maps.Controls.MapColorMode.Light;
		map.CartographicMode = 
			Microsoft.Phone.Maps.Controls.MapCartographicMode.Road;
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
	````

	Note the condition (if (!App.IsInBackground)) around the code block that is in-charge of updating and displaying the map control.

1. We would also like to alter the gcw_StatusChanged event handler, in order to add some debug output: 

	````C#
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
	````

1. In the previous exercise, the button was only used as a "Start" button. The button should now perform the complete "Start"/"Stop" mechanism, as designed.

	In order to do so, we will alter the "btnStartStop_Click" event handler, as follows:

	````C#
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
	````

	Now we can start and stop the location collection process.

1. There's only one last thing to do and that is to add the Dependency Properties:

	````C#
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
	````

1. Now you can build and run your application!
 
	We would also like to test our application. Let's see how in the next task

<a name="Ex2Task3" />
#### Task 3 - Testing the application ####

In order to test the application we need to simulate the change of positions, the emulator provides us with the capability.

In this task we will see how we can test the GPS using the emulator:

1. Run the application over the emulator.

1. On the emulator main screen you will find a menu bar on the right upper corner of the emulator. Press on the ">>" button to reveal an additional tool panel.

1. Select the "Location" tab

	![Working App](./Images/WorkingApp.png?raw=true "Working App")

	As you can see in the above picture, the panel provides us with a map that is used to simulate the device movement. 
You can simulate movement either by clicking on the map, thus the emulator gets a GPS signal simulating the movement to that location, or by recording data simulating a movement and playing it back to the emulator.

	Using the second method allows the developer to test known routes.

	You can also control how frequent would the events fire: 

1. Let's test our application in manual mode: Press on the "Start" button on your application, start clicking the emulator map to simulate the run route and see on the blue line representing the route on your application.

1. Let's test the background feature: Click the "Home" button of the device emulator in order to deactivate the application.
Click the emulator map a few more times to signal additional movements to the emulator GPS.

	Go to the output windows of Visual Studio and see the debug lines that keep coming out even though the application is already deactivated:  "New position:......".

	This means that even when the application is not active it is still running, getting events from the GPS and processing them correctly.

1. Click the "Back" button on the device emulator to reactivate the application and see the complete route.

<a name="Summary" />
## Summary ##

 This lab discussed and demonstrated few important issues:
 Working with map control, integrating the map with GPS, using the route service and running an application in the background.
These capabilities are a major part of many applications and can help you widen your application's features.

 This lab, the "Running Tracker" is a demo application implementing yet a few aspects of these features, allowing you to acquire the basic skills required to develop Windows Phone 8 application.
