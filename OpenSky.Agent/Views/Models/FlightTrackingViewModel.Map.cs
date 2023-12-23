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
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Windows;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator.Controls;
    using OpenSky.Agent.Simulator.Controls.Models;
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
        /// The import simbrief visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility importSimbriefVisibility = Visibility.Visible;

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
        /// Gets the import simbrief command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand ImportSimbriefCommand { get; }

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
        /// Import simbrief waypoints.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ImportSimbrief()
        {
            using var client = new WebClient();
            try
            {
                Debug.WriteLine("Importing sim brief flight plan waypoints");
                if (string.IsNullOrEmpty(UserSessionService.Instance.LinkedAccounts?.SimbriefUsername))
                {
                    throw new Exception("No Simbrief user name configured, please configure it using the OpenSky client!");
                }

                if (this.Simulator.Flight == null)
                {
                    throw new Exception("No flight loaded!");
                }

                var xml = client.DownloadString($"https://www.simbrief.com/api/xml.fetcher.php?username={UserSessionService.Instance.LinkedAccounts?.SimbriefUsername}");

                var ofp = XElement.Parse(xml);
                var originICAO = (string)ofp.Element("origin")?.Element("icao_code");
                var destinationICAO = (string)ofp.Element("destination")?.Element("icao_code");

                if (!this.Simulator.Flight.Origin.Icao.Trim().Equals(originICAO.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception("Departure airport doesn't match!");
                }

                if (!this.Simulator.Flight.Destination.Icao.Trim().Equals(destinationICAO.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception("Destination airport doesn't match!");
                }

                this.Simulator.ImportSimbrief(ofp);
                this.ImportSimbriefVisibility = Visibility.Collapsed;
            }
            catch (WebException ex)
            {
                Debug.WriteLine("Web error received from simBrief api: " + ex);

                var responseStream = ex.Response.GetResponseStream();
                if (responseStream != null)
                {
                    var responseString = string.Empty;
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var buffer = new char[1024];
                    var count = reader.Read(buffer, 0, buffer.Length);
                    while (count > 0)
                    {
                        responseString += new string(buffer, 0, count);
                        count = reader.Read(buffer, 0, buffer.Length);
                    }

                    Debug.WriteLine(responseString);
                    if (responseString.Contains("<OFP>"))
                    {
                        var ofp = XElement.Parse(responseString);
                        var status = ofp.Element("fetch")?.Element("status")?.Value;
                        if (!string.IsNullOrWhiteSpace(status))
                        {
                            Debug.WriteLine("Error fetching flight plan from simBrief: " + status);
                            this.ImportSimbriefCommand.ReportProgress(
                                () =>
                                {
                                    var notification = new OpenSkyNotification("Error fetching flight plan from simBrief", status, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                    notification.SetErrorColorStyle();
                                    this.ViewReference.ShowNotification(notification);
                                });
                            return;
                        }
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error fetching flight plan from simBrief: " + ex);
                this.ImportSimbriefCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error fetching flight plan from simBrief", ex.Message, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        this.ViewReference.ShowNotification(notification);
                    });
            }
        }

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