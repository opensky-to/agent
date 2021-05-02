// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTracking.xaml.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.Controls;
    using OpenSky.AgentMSFS.Converters;
    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.AgentMSFS.Tools;
    using OpenSky.AgentMSFS.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight tracking view.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightTracking
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightTracking"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightTracking()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static FlightTracking Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open the flight tracking view or bring the existing instance into view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static void Open()
        {
            if (Instance == null)
            {
                Instance = new FlightTracking();
                Instance.Closed += (sender, e) => Instance = null;
                Instance.Show();
            }
            else
            {
                Instance.BringIntoView();
                Instance.Activate();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds aircraft and trails to the map view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AddAircraftAndTrailsToMap()
        {
            Debug.WriteLine("Adding aircraft and trails to the map view");

            // Add Simbrief route to map
            var simbriefRoute = new MapPolyline { Stroke = new SolidColorBrush(OpenSkyColors.OpenSkySimBrief), StrokeThickness = 4, Locations = ((FlightTrackingViewModel)this.DataContext).SimConnect.SimbriefRouteLocations };
            this.MapView.Children.Add(simbriefRoute);

            // Add aircraft trail to map
            var aircraftTrail = new MapPolyline { Stroke = new SolidColorBrush(OpenSkyColors.OpenSkyTeal), StrokeThickness = 4, Locations = ((FlightTrackingViewModel)this.DataContext).SimConnect.AircraftTrailLocations };
            this.MapView.Children.Add(aircraftTrail);

            // Add aircraft position to map
            var aircraftPosition = new Image { Width = 40, Height = 40 };
            aircraftPosition.SetValue(Panel.ZIndexProperty, 999);
            aircraftPosition.SetValue(MapLayer.PositionOriginProperty, PositionOrigin.Center);
            var rotateTransform = new RotateTransform { CenterX = 20, CenterY = 20 };
            var headingBinding = new Binding { Source = this.DataContext, Path = new PropertyPath("SimConnect.PrimaryTracking.Heading"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(rotateTransform, RotateTransform.AngleProperty, headingBinding);
            aircraftPosition.RenderTransform = rotateTransform;
            var aircraftDrawingImage = this.FindResource("OpenSkyLogoPointingUp") as DrawingImage;
            aircraftPosition.Source = aircraftDrawingImage;
            var positionBinding = new Binding { Source = this.DataContext, Path = new PropertyPath("SimConnect.PrimaryTracking.MapLocation"), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(aircraftPosition, MapLayer.PositionProperty, positionBinding);
            this.MapView.Children.Add(aircraftPosition);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Event log mouse double click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse button event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void EventLogMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.EventLog.SelectedItem is TrackingEventLogEntry selectedEventLog)
            {
                Debug.WriteLine($"User double clicked event log entry: {selectedEventLog.LogMessage}");
                this.MapView.Center = selectedEventLog.Location;
                this.UserMapInteraction(this, EventArgs.Empty);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight tracking loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightTrackingLoaded(object sender, RoutedEventArgs e)
        {
            // Configure map view
            this.MapView.CredentialsProvider = new ApplicationIdCredentialsProvider(Properties.Settings.Default.BingMapsKey);
            this.AddAircraftAndTrailsToMap();

            // Add fuel tank controls
            foreach (FuelTank tank in Enum.GetValues(typeof(FuelTank)))
            {
                var control = new FuelTankControl { FuelTankName = tank.GetStringValue(), Margin = new Thickness(5, 2, 5, 2) };
                var capacityBinding = new Binding { Source = this.DataContext, Path = new PropertyPath($"SimConnect.FuelTanks.Capacities[{(int)tank}]"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(control, FuelTankControl.FuelTankCapacityProperty, capacityBinding);
                var quantityBinding = new Binding { Source = this.DataContext, Path = new PropertyPath($"FuelTankQuantities[{(int)tank}]"), Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                BindingOperations.SetBinding(control, FuelTankControl.FuelTankQuantityProperty, quantityBinding);
                var visibilityBinding = new Binding { Source = this.DataContext, Path = new PropertyPath($"SimConnect.FuelTanks.Capacities[{(int)tank}]"), Mode = BindingMode.OneWay, Converter = new FuelTankCapacityVisibilityConverter() };
                BindingOperations.SetBinding(control, VisibilityProperty, visibilityBinding);
                var infoStringBinding = new Binding { Source = this.DataContext, Path = new PropertyPath($"FuelTankInfos[{(int)tank}]"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(control, FuelTankControl.FuelTankInfoProperty, infoStringBinding);
                this.FuelTanksPanel.Children.Add(control);
            }

            // Add payload station controls
            for (var i = 0; i < 15; i++)
            {
                var control = new PayloadStationControl { Margin = new Thickness(5, 2, 5, 2) };
                var nameBinding = new Binding { Source = this.DataContext, Path = new PropertyPath($"SimConnect.PayloadStations.Names[{i}]"), Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(control, PayloadStationControl.PayloadStationNameProperty, nameBinding);
                var weightBinding = new Binding { Source = this.DataContext, Path = new PropertyPath($"PayloadStationWeights[{i}]"), Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                BindingOperations.SetBinding(control, PayloadStationControl.PayloadStationWeightProperty, weightBinding);
                var visibilityBinding = new Binding
                    { Source = this.DataContext, Path = new PropertyPath("SimConnect.PayloadStations.Count"), Mode = BindingMode.OneWay, Converter = new PayloadStationsVisibilityConverter(), ConverterParameter = i + 1 };
                BindingOperations.SetBinding(control, VisibilityProperty, visibilityBinding);
                this.PayloadStationsPanel.Children.Add(control);
            }

            // Replay past tracking markers to add to this view
            var viewModel = (FlightTrackingViewModel)this.DataContext;
            viewModel.SimConnect.ReplayMapMarkers();
            viewModel.SimConnect.NextStepFlashingChanged += this.SimConnectNextStepFlashingChanged;
            this.SimConnectNextStepFlashingChanged(this, viewModel.SimConnect.NextStepFlashing);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight tracking window on unloaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightTrackingOnUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (UIElement child in this.MapView.Children)
            {
                if (child is SimbriefWaypointMarker simbrief)
                {
                    BindingOperations.ClearAllBindings(simbrief);
                }
            }

            this.MapView.Children.Clear();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight tracking view model on map position updated.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// A Location to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightTrackingViewModelOnMapPositionUpdated(object sender, MapPositionUpdate e)
        {
            if (e.IsUserAction)
            {
                this.MapView.Center = e.Location;
            }
            else
            {
                // Check if the plane is out of bounds before we move the map
                var point = this.MapView.LocationToViewportPoint(e.Location);
                var horizontalBorder = Math.Min(this.MapView.ActualWidth * 0.1, 40);
                var verticalBorder = Math.Min(this.MapView.ActualHeight * 0.1, 40);
                if (point.X <= horizontalBorder || point.X >= (this.MapView.ActualWidth - horizontalBorder) || point.Y <= verticalBorder || point.Y >= (this.MapView.ActualHeight - verticalBorder))
                {
                    Debug.WriteLine($"Plane icon out-of-bounds on mapview, re-centering to {e.Location.Latitude} {e.Location.Longitude}");
                    this.MapView.Center = e.Location;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight tracking view model wants to reset the tracking map.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightTrackingViewModelOnResetTrackingMap(object sender, EventArgs e)
        {
            this.MapView.Children.Clear();
            this.AddAircraftAndTrailsToMap();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight tracking view model on simbrief waypoint marker added.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// A SimbriefWaypointMarker to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightTrackingViewModelOnSimbriefWaypointMarkerAdded(object sender, SimbriefWaypointMarker e)
        {
            try
            {
                // Make sure we remove it from any previous map layers
                var existingMapLayer = e.Parent as MapLayer;
                existingMapLayer?.Children.Remove(e);
                if (BindingOperations.IsDataBound(e, SimbriefWaypointMarker.TextLabelVisibleProperty))
                {
                    BindingOperations.ClearBinding(e, SimbriefWaypointMarker.TextLabelVisibleProperty);
                }

                this.MapView.Children.Add(e);

                // Add zoom level -> visibility binding with custom converter
                var zoomLevelBinding = new Binding { Source = this.MapView, Path = new PropertyPath("ZoomLevel"), Converter = new MapZoomLevelVisibilityConverter() };
                BindingOperations.SetBinding(e, SimbriefWaypointMarker.TextLabelVisibleProperty, zoomLevelBinding);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight tracking view model tracking event marker added.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// A TrackingEventMarker to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FlightTrackingViewModelTrackingEventMarkerAdded(object sender, TrackingEventMarker e)
        {
            try
            {
                // Make sure we remove it from any previous map layers
                var existingMapLayer = e.Parent as MapLayer;
                existingMapLayer?.Children.Remove(e);

                this.MapView.Children.Add(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Ground handling warning on is visible changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Dependency property changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void GroundHandlingWarningOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var storyboard = this.Resources["WarningFade"] as Storyboard;
            if (this.GroundHandlingWarning.IsVisible)
            {
                storyboard?.Begin();
            }
            else
            {
                storyboard?.Stop();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Map type selection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Selection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MapTypeOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MapType.SelectedItem is ComboBoxItem item)
            {
                switch (item.Content as string)
                {
                    case "Road":
                        this.MapView.Mode = new RoadMode();
                        break;
                    case "Aerial":
                        this.MapView.Mode = new AerialMode();
                        break;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// SimConnect next step flashing changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="nextStepFlashing">
        /// True to make the next step flashing.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimConnectNextStepFlashingChanged(object sender, bool nextStepFlashing)
        {
            if (nextStepFlashing)
            {
                UpdateGUIDelegate toggleStoryboard = () =>
                {
                    var storyboard = this.Resources["NextStepFade"] as Storyboard;
                    storyboard?.Begin();
                };
                this.Dispatcher.BeginInvoke(toggleStoryboard);
            }
            else
            {
                UpdateGUIDelegate toggleStoryboard = () =>
                {
                    var storyboard = this.Resources["NextStepFade"] as Storyboard;
                    storyboard?.Stop();
                };
                this.Dispatcher.BeginInvoke(toggleStoryboard);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user interacted with the map.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse button event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void UserMapInteraction(object sender, EventArgs e)
        {
            var viewModel = (FlightTrackingViewModel)this.DataContext;
            viewModel.LastUserMapInteraction = DateTime.UtcNow;
        }
    }
}