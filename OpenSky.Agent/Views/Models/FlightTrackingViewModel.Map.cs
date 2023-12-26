// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingViewModel.Map.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using System;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Windows;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// The view model for the flight tracking view - map code.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightTrackingViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double aircraftHeading;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Location aircraftLocation;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to follow plane on the map.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool followPlane = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last aircraft position update.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastAircraftPositionUpdate = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last user map interaction date/time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastUserMapInteraction = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to update the map position.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<MapPositionUpdate> MapPositionUpdated;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when SimConnect adds a new simbrief waypoint marker (forwarded event).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<SimbriefWaypointMarker> SimbriefWaypointMarkerAdded;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when SimConnect adds a new tracking event marker (forwarded event).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<TrackingEventMarker> TrackingEventMarkerAdded;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double AircraftHeading
        {
            get => this.aircraftHeading;
            set
            {
                if (Equals(this.aircraftHeading, value))
                {
                    return;
                }

                this.aircraftHeading = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Location AircraftLocation
        {
            get => this.aircraftLocation;
            set
            {
                if (Equals(value, this.aircraftLocation))
                {
                    return;
                }

                this.aircraftLocation = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to follow the plane on the map.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool FollowPlane
        {
            get => this.followPlane;

            set
            {
                if (Equals(this.followPlane, value))
                {
                    return;
                }

                this.followPlane = value;
                this.NotifyPropertyChanged();
                Debug.WriteLine($"Follow plane toggled {value}");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the date/time of the last user map interaction.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime LastUserMapInteraction
        {
            get => this.lastUserMapInteraction;

            set
            {
                if (Equals(this.lastUserMapInteraction, value))
                {
                    return;
                }

                this.lastUserMapInteraction = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the move map to coordinate command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command MoveMapToCoordinateCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Move the map to coordinate specified in command parameter (either Location or GeoCoordinate).
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/03/2021.
        /// </remarks>
        /// <param name="commandParameter">
        /// The command parameter.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MoveMapToCoordinate(object commandParameter)
        {
            if (commandParameter is Airport airport)
            {
                this.MapPositionUpdated?.Invoke(this, new MapPositionUpdate(new Location(airport.Latitude, airport.Longitude, airport.Altitude), true));
            }

            if (commandParameter is GeoCoordinate geoCoordinate)
            {
                this.MapPositionUpdated?.Invoke(this, new MapPositionUpdate(new Location(geoCoordinate.Latitude, geoCoordinate.Longitude, geoCoordinate.Altitude), true));
            }

            if (commandParameter is Location location)
            {
                this.MapPositionUpdated?.Invoke(this, new MapPositionUpdate(location, true));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simulator aircraft location changed.
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
        private void SimulatorLocationChanged(object sender, Location e)
        {
            if ((DateTime.UtcNow - this.lastAircraftPositionUpdate).TotalMilliseconds > Properties.Settings.Default.AircraftPositionUpdateInterval)
            {
                this.lastAircraftPositionUpdate = DateTime.UtcNow;
                this.AircraftLocation = e;
                this.AircraftHeading = this.Simulator.PrimaryTracking?.Heading ?? 0;
            }

            if (this.FollowPlane && (DateTime.UtcNow - this.LastUserMapInteraction).TotalSeconds > 10)
            {
                UpdateGUIDelegate sendEvent = () => this.MapPositionUpdated?.Invoke(this, new MapPositionUpdate(e));
                Application.Current.Dispatcher.BeginInvoke(sendEvent);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simulator simbrief waypoint marker added (forward event from Simulator).
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// The newly added SimbriefWaypointMarker to add to the map.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimulatorSimbriefWaypointMarkerAdded(object sender, SimbriefWaypointMarker e)
        {
            this.SimbriefWaypointMarkerAdded?.Invoke(sender, e);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simulator tracking event marker added (forward event from Simulator).
        /// </summary>
        /// <remarks>
        /// sushi.at, 16/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// The newly added TrackingEventMarker to add to the map.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimulatorTrackingEventMarkerAdded(object sender, TrackingEventMarker e)
        {
            this.TrackingEventMarkerAdded?.Invoke(sender, e);
        }
    }
}