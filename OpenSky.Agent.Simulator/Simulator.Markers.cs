// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Markers.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    using TrackingEventLogEntry = OpenSky.Agent.Simulator.Models.TrackingEventLogEntry;
    using TrackingEventMarker = OpenSky.Agent.Simulator.Models.TrackingEventMarker;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - map markers.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tracking event markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<TrackingEventMarker> trackingEventMarkers = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last distance comparison used for a position report.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string lastDistancePositionReport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last non position report tracking event marker (for grouping events that happened at the same location).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        private TrackingEventMarker lastNonPositionReportMarker;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last added tracking location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        private GeoCoordinate lastPositionReport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the plane's location changed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<Location> LocationChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when SimConnect adds a new tracking event marker.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<TrackingEventMarker> TrackingEventMarkerAdded;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft trail locations collection to draw a poly line on the map.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection AircraftTrailLocations { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the last distance comparison used for a position report.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LastDistancePositionReport
        {
            get => this.lastDistancePositionReport;

            private set
            {
                if (Equals(this.lastDistancePositionReport, value))
                {
                    return;
                }

                this.lastDistancePositionReport = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking event log entries.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TrackingEventLogEntry> TrackingEventLogEntries { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Replay simbrief waypoint and tracking event markers (new tracking view was opened).
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void ReplayMapMarkers()
        {
            Debug.WriteLine("SimConnect is replaying map markers to listeners...");

            UpdateGUIDelegate restoreMarkers = () =>
            {
                foreach (var waypointMarker in this.simbriefWaypointMarkers)
                {
                    this.SimbriefWaypointMarkerAdded?.Invoke(this, waypointMarker);
                }

                lock (this.trackingEventMarkers)
                {
                    foreach (var trackingEventMarker in this.trackingEventMarkers)
                    {
                        this.TrackingEventMarkerAdded?.Invoke(this, trackingEventMarker);
                    }
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restoreMarkers);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Add a tracking event to the map and log.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="primary">
        /// The primary Simconnect tracking data.
        /// </param>
        /// <param name="secondary">
        /// The secondary Simconnect tracking data.
        /// </param>
        /// <param name="type">
        /// The flight tracking event type.
        /// </param>
        /// <param name="color">
        /// The color to use for the marker.
        /// </param>
        /// <param name="text">
        /// The event text (what happened?).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void AddTrackingEvent(PrimaryTracking primary, SecondaryTracking secondary, FlightTrackingEventType type, Color color, string text)
        {
            if (this.TrackingStatus != TrackingStatus.Tracking && this.TrackingStatus != TrackingStatus.GroundOperations)
            {
                return;
            }

            UpdateGUIDelegate addTrackingEvent = () =>
            {
                Debug.WriteLine($"Adding tracking event: {text}");
                if (this.lastNonPositionReportMarker == null || this.lastNonPositionReportMarker.GetDistanceTo(primary) >= 20)
                {
                    lock (this.trackingEventMarkers)
                    {
                        var newMarker = new TrackingEventMarker(primary, secondary, this.WeightAndBalance.FuelTotalQuantity, 16, color, text);
                        this.lastNonPositionReportMarker = newMarker;
                        this.trackingEventMarkers.Add(newMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, newMarker);
                    }
                }
                else
                {
                    this.lastNonPositionReportMarker.AddEventToMarker(DateTime.UtcNow, text);
                }

                this.TrackingEventLogEntries.Add(new TrackingEventLogEntry(type, DateTime.UtcNow, color, text, primary.MapLocation));
            };
            Application.Current.Dispatcher.BeginInvoke(addTrackingEvent);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds a position report to the map if needed (poly line point and position report point if requested).
        /// </summary>
        /// <remarks>
        /// sushi.at, 16/03/2021.
        /// </remarks>
        /// <param name="primary">
        /// The primary Simconnect tracking data.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AddPositionReport(PrimaryTracking primary)
        {
            if (this.TrackingStatus != TrackingStatus.Tracking)
            {
                this.lastPositionReport = null;
                return;
            }

            // Is this the first ever position?
            if (this.lastPositionReport == null)
            {
                this.lastPositionReport = primary.GeoCoordinate;
                UpdateGUIDelegate addPositionReport = () =>
                {
                    lock (this.trackingEventMarkers)
                    {
                        this.AircraftTrailLocations.Add(new AircraftTrailLocation(DateTime.UtcNow, primary, this.SecondaryTracking, this.WeightAndBalance.FuelTotalQuantity));
                        var newMarker = new TrackingEventMarker(primary, this.SecondaryTracking, this.WeightAndBalance.FuelTotalQuantity, 8, OpenSkyColors.OpenSkyTeal, "Position report");
                        this.trackingEventMarkers.Add(newMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, newMarker);
                    }
                };
                Application.Current.Dispatcher.BeginInvoke(addPositionReport);
            }

            // Check if we should even add a new position report (has the plane moved far enough?)
            var newPosition = primary.GeoCoordinate;
            var distance = this.lastPositionReport.GetDistanceTo(newPosition);

            int minDistance;
            if (primary.OnGround)
            {
                if (this.IsTurning)
                {
                    minDistance = (int)primary.GroundSpeed;
                    minDistance = Math.Max(minDistance, 15);
                    minDistance = Math.Min(minDistance, 30);
                }
                else
                {
                    minDistance = (int)primary.GroundSpeed * 6;
                    minDistance = Math.Max(minDistance, 50);
                    minDistance = Math.Min(minDistance, 500);
                }
            }
            else
            {
                var radioHeight = this.AircraftIdentity.EngineType is EngineType.Jet or EngineType.Turboprop ? 2500 : 1000;
                if (primary.RadioHeight < radioHeight)
                {
                    if (this.IsTurning)
                    {
                        minDistance = (int)primary.GroundSpeed * 2;
                        minDistance = Math.Max(minDistance, 100);
                        minDistance = Math.Min(minDistance, 300);
                    }
                    else
                    {
                        minDistance = (int)primary.GroundSpeed * 10;
                        minDistance = Math.Max(minDistance, 200);
                        minDistance = Math.Min(minDistance, 1000);
                    }
                }
                else
                {
                    if (this.IsTurning)
                    {
                        minDistance = (int)primary.GroundSpeed * 2;
                        minDistance = Math.Max(minDistance, 300);
                        minDistance = Math.Min(minDistance, 1000);
                    }
                    else
                    {
                        minDistance = (int)primary.GroundSpeed * 22;
                        minDistance = Math.Max(minDistance, 2000);
                        minDistance = Math.Min(minDistance, 10000);
                    }
                }
            }

            this.LastDistancePositionReport = $"{(int)distance} > {minDistance}";
            if (distance > minDistance)
            {
                this.lastPositionReport = newPosition;
                UpdateGUIDelegate addPositionReport = () =>
                {
                    lock (this.trackingEventMarkers)
                    {
                        this.AircraftTrailLocations.Add(new AircraftTrailLocation(DateTime.UtcNow, primary, this.SecondaryTracking, this.WeightAndBalance.FuelTotalQuantity));
                        var newMarker = new TrackingEventMarker(primary, this.SecondaryTracking, this.WeightAndBalance.FuelTotalQuantity, 8, OpenSkyColors.OpenSkyTeal, "Position report");
                        this.trackingEventMarkers.Add(newMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, newMarker);
                    }
                };
                Application.Current.Dispatcher.BeginInvoke(addPositionReport);
            }
        }
    }
}