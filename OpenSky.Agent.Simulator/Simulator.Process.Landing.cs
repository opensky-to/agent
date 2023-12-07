// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Process.Landing.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.FlightLogXML;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - landing processing.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing reports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TouchDown> LandingReports { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a landing should be reported (event parameter specifies triggering time).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<LandingReportNotification> LandingReported;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check for and analyse landings.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// <param name="pla">
        /// The pla.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void CheckForAndAnalyseLanding(ProcessLandingAnalysis pla)
        {
            if (this.TrackingStatus == TrackingStatus.Tracking && !pla.Old.OnGround && pla.New.OnGround)
            {
                // We have touchdown
                Debug.WriteLine("Adding new landing report");
                var landingReport = new TouchDown(
                    DateTime.UtcNow,
                    pla.New.Location.Latitude,
                    pla.New.Location.Longitude,
                    (int)pla.New.Location.Altitude,
                    pla.New.LandingRate * -1.0,
                    Math.Max(pla.Old.Gforce, pla.New.Gforce),
                    pla.New.SpeedLong,
                    pla.New.SpeedLat,
                    pla.New.WindLong,
                    pla.New.WindLat,
                    pla.New.BankAngle,
                    pla.New.GroundSpeed,
                    pla.New.AirspeedTrue
                );

                this.LandingReports.Add(landingReport);

                if (this.LandingReports.Count == 1)
                {
                    // First landing for this flight
                    this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, FlightTrackingEventType.Touchdown, OpenSkyColors.OpenSkyTeal, "Touchdown");
                    this.LandingReported?.Invoke(this, LandingReportNotification.AsSoonAsPossible);
                }
            }
        }
    }
}