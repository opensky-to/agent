﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.SaveLoadXML.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml.Linq;

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
    /// Simulator interface - save/load.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Identifier for the agent.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const string AgentIdentifier = "OpenSky.Agent";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a XML save file for the current flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private XElement GenerateSaveFile()
        {
            if ((this.TrackingStatus != TrackingStatus.Tracking && this.TrackingStatus != TrackingStatus.GroundOperations) || this.Flight == null)
            {
                Debug.WriteLine("Not generating save file cause we are either not tracking or there is no active flight");
                return null;
            }

            try
            {
                Debug.WriteLine("Generating flight save XML file...");
                var log = new FlightLogXML.FlightLog
                {
                    Agent = AgentIdentifier,
                    AgentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    OpenSkyUser = this.OpenSkyUserName ?? "Unknown",
                    LocalTimeZone = TimeZoneInfo.Local.BaseUtcOffset.TotalHours,
                    TrackingStarted = this.trackingStarted ?? DateTime.MinValue,
                    TrackingStopped = DateTime.UtcNow,
                    WasAirborne = this.WasAirborne,
                    TimeSavedBecauseOfSimRate = this.timeSavedBecauseOfSimRate,
                    TotalPaused = this.totalPaused,
                    TimeConenctedToOnlineNetwork = TimeSpan.Zero,

                    FlightID = this.Flight.Id,
                    AircraftRegistry = this.Flight.Aircraft.Registry,

                    Origin = new FlightLogAirport
                    {
                        Icao = this.Flight.Origin.Icao,
                        Name = this.Flight.Origin.Name,
                        Latitude = this.Flight.Origin.Latitude,
                        Longitude = this.Flight.Origin.Longitude
                    },
                    Destination = new FlightLogAirport
                    {
                        Icao = this.Flight.Destination.Icao,
                        Name = this.Flight.Destination.Name,
                        Latitude = this.Flight.Destination.Latitude,
                        Longitude = this.Flight.Destination.Longitude
                    },
                    Alternate = new FlightLogAirport
                    {
                        Icao = this.Flight.Alternate.Icao,
                        Name = this.Flight.Alternate.Name,
                        Latitude = this.Flight.Alternate.Latitude,
                        Longitude = this.Flight.Alternate.Longitude
                    },

                    FuelGallons = this.Flight.FuelGallons ?? 0,
                    Payload = this.Flight.PayloadSummary,
                    PayloadPounds = this.Flight.PayloadPounds
                };

                if (this.Flight.OnlineNetwork != OnlineNetwork.Offline)
                {
                    log.TimeConenctedToOnlineNetwork = this.OnlineNetworkConnectionDuration;
                    if (this.OnlineNetworkConnectionStarted.HasValue)
                    {
                        log.TimeConenctedToOnlineNetwork += (DateTime.UtcNow - this.OnlineNetworkConnectionStarted.Value);
                    }
                }

                log.TrackingEventLogEntries.AddRange(this.TrackingEventLogEntries);
                lock (this.trackingEventMarkers)
                {
                    log.TrackingEventMarkers.AddRange(this.trackingEventMarkers.Select(m => m.Marker));
                }

                log.PositionReports.AddRange(this.AircraftTrailLocations.Cast<AircraftTrailLocation>().Select(loc => loc.Position));
                log.TouchDowns.AddRange(this.LandingReports);
                log.NavLogWaypoints.AddRange(this.simbriefWaypointMarkers.Select(w => w.WayPoint));

                return log.GenerateFlightLog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Flight save file creation failed: " + ex);
                throw;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Restores a save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="saveFile">
        /// The save file xml.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void RestoreSaveFile(string saveFile)
        {
            if (this.TrackingStatus != TrackingStatus.Resuming || this.Flight == null)
            {
                Debug.WriteLine("Not loading save file cause we are either not in resuming state or there is no active flight");
                return;
            }

            // Parse xml into memory and the flight log xml library parse it
            var save = XElement.Parse(saveFile);
            var log = new FlightLogXML.FlightLog();
            log.RestoreFlightLog(save);

            // ========================================================================================================================
            // CHECK THE SAVE FILE FOR INCOMPATIBILITY OR TAMPERING
            // ========================================================================================================================

            // Do some basic checks
            if (!AgentIdentifier.Equals(save.Element("Agent")?.Value))
            {
                throw new Exception("This save file was generated by a different agent!");
            }

            // Check if flight basics still match
            this.Flight.CheckSaveMatchesFlight(log);

            // ========================================================================================================================
            // ALL CHECKS PASSED, RESTORE THE FLIGHT
            // ========================================================================================================================

            // Restore basic tracking stats
            this.trackingStarted = log.TrackingStarted;
            this.WasAirborne = log.WasAirborne;
            this.timeSavedBecauseOfSimRate = log.TimeSavedBecauseOfSimRate;
            this.WarpInfo = this.timeSavedBecauseOfSimRate.TotalSeconds >= 1 ? $"Yes, saved {this.timeSavedBecauseOfSimRate:hh\\:mm\\:ss} [*]" : "No [*]";
            this.totalPaused = log.TotalPaused;
            if (this.Flight.OnlineNetwork != OnlineNetwork.Offline)
            {
                this.OnlineNetworkConnectionDuration = log.TimeConenctedToOnlineNetwork;
            }

            // Restore aircraft trail locations
            UpdateGUIDelegate restorePositionReports = () =>
            {
                this.AircraftTrailLocations.Clear();
                foreach (var location in log.PositionReports)
                {
                    this.AircraftTrailLocations.Add(new AircraftTrailLocation(location));
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restorePositionReports);

            // Restore map event markers
            UpdateGUIDelegate restoreMapEventMarkers = () =>
            {
                lock (this.trackingEventMarkers)
                {
                    this.trackingEventMarkers.Clear();

                    // Add airport markers, we don't save them
                    if (this.Flight != null)
                    {
                        var alternateMarker = new TrackingEventMarker(new GeoCoordinate(this.Flight.Alternate.Latitude, this.Flight.Alternate.Longitude), this.Flight.Alternate.Icao, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                        this.trackingEventMarkers.Add(alternateMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, alternateMarker);
                        var originMarker = new TrackingEventMarker(new GeoCoordinate(this.Flight.Origin.Latitude, this.Flight.Origin.Longitude), this.Flight.Origin.Icao, OpenSkyColors.OpenSkyTeal, Colors.White);
                        this.trackingEventMarkers.Add(originMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, originMarker);
                        var destinationMarker = new TrackingEventMarker(new GeoCoordinate(this.Flight.Destination.Latitude, this.Flight.Destination.Longitude), this.Flight.Destination.Icao, OpenSkyColors.OpenSkyTeal, Colors.White);
                        this.trackingEventMarkers.Add(destinationMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, destinationMarker);

                        var alternateDetailMarker = new TrackingEventMarker(this.Flight.Alternate, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                        this.trackingEventMarkers.Add(alternateDetailMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, alternateDetailMarker);
                        var originDetailMarker = new TrackingEventMarker(this.Flight.Origin, OpenSkyColors.OpenSkyTeal, Colors.White);
                        this.trackingEventMarkers.Add(originDetailMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, originDetailMarker);
                        var destinationDetailMarker = new TrackingEventMarker(this.Flight.Destination, OpenSkyColors.OpenSkyTeal, Colors.White);
                        this.trackingEventMarkers.Add(destinationDetailMarker);
                        this.TrackingEventMarkerAdded?.Invoke(this, destinationDetailMarker);

                        foreach (var runway in this.Flight.Alternate.Runways)
                        {
                            var runwayMarker = new TrackingEventMarker(runway);
                            this.trackingEventMarkers.Add(runwayMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, runwayMarker);
                        }

                        foreach (var runway in this.Flight.Origin.Runways)
                        {
                            var runwayMarker = new TrackingEventMarker(runway);
                            this.trackingEventMarkers.Add(runwayMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, runwayMarker);
                        }

                        foreach (var runway in this.Flight.Destination.Runways)
                        {
                            var runwayMarker = new TrackingEventMarker(runway);
                            this.trackingEventMarkers.Add(runwayMarker);
                        }
                    }

                    foreach (var marker in log.TrackingEventMarkers)
                    {
                        this.trackingEventMarkers.Add(new TrackingEventMarker(marker));
                    }
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restoreMapEventMarkers);

            // Restore event log entries
            UpdateGUIDelegate restoreEventLog = () =>
            {
                this.TrackingEventLogEntries.Clear();
                foreach (var logEntry in log.TrackingEventLogEntries)
                {
                    this.TrackingEventLogEntries.Add(new TrackingEventLogEntry(logEntry));
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restoreEventLog);

            // Restore landing reports
            this.LandingReports.Clear();
            foreach (var touchdown in log.TouchDowns)
            {
                this.LandingReports.Add(touchdown);
            }

            // Restore simbrief waypoint markers
            UpdateGUIDelegate restoreSimbrief = () =>
            {
                this.SimbriefRouteLocations.Clear();
                this.simbriefWaypointMarkers.Clear();
                if (log.NavLogWaypoints.Count > 0)
                {
                    this.SimbriefRouteLocations.Add(new Location(this.Flight.Origin.Latitude, this.Flight.Origin.Longitude));
                    foreach (var waypoint in log.NavLogWaypoints)
                    {
                        var waypointMarker = new SimbriefWaypointMarker(waypoint);
                        this.simbriefWaypointMarkers.Add(waypointMarker);
                        this.SimbriefRouteLocations.Add(new Location(waypoint.Latitude, waypoint.Longitude));
                    }

                    this.SimbriefRouteLocations.Add(new Location(this.Flight.Destination.Latitude, this.Flight.Destination.Longitude));
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restoreSimbrief);

            // Now that everything has been loaded, replay any and all map markers
            this.ReplayMapMarkers();
        }
    }
}