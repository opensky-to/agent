// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Process.PositionReports.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Device.Location;
    using System.Windows;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Structs;
    using OpenSky.AgentMSFS.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client - position reports code.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
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
                var radioHeight = this.PlaneIdentity.EngineType is EngineType.Jet or EngineType.Turboprop ? 2500 : 1000;
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