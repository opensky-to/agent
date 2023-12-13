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
        /// (Immutable) The process landing analysis queue (to monitor values like g-force over a period
        /// of reports)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LandingAnalysis[] plaQueue = new LandingAnalysis[100];

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last touchdown even recorded (to group bounces etc., before creating new touchdown event).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastTouchdown = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pla queue countdown after initial touchdown (to monitor values like g-force for a short
        /// period after touchdown)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int plaQueueCountdown = -1;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current pla queue index.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int plaQueueIndex;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a landing should be reported (event parameter specifies triggering time).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<LandingReportNotification> LandingReported;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the zero-based index of the final touch down landing report (in case there are multiple
        /// like a bounce on takeoff).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int FinalTouchDownIndex { get; private set; } = -1;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing reports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<TouchDown> LandingReports { get; }

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
                // We have a touchdown
                Debug.WriteLine("Adding new landing report");

                // Check for max g-force before touchdown
                var maxGeforce = Math.Max(pla.Old.Gforce, pla.New.Gforce);
                for (var i = 0; i < 100; i++)
                {
                    if (this.plaQueue[i] != null)
                    {
                        maxGeforce = Math.Max(maxGeforce, this.plaQueue[i].Gforce);
                    }
                }

                var landingReport = new TouchDown(
                    DateTime.UtcNow,
                    pla.New.Location.Latitude,
                    pla.New.Location.Longitude,
                    (int)pla.New.Location.Altitude,
                    pla.New.LandingRate * -1.0,
                    maxGeforce,
                    pla.New.SpeedLong,
                    pla.New.SpeedLat,
                    pla.New.WindLong,
                    pla.New.WindLat,
                    pla.New.BankAngle,
                    pla.New.GroundSpeed,
                    pla.New.AirspeedTrue
                );

                // Add to the list and start the countdown
                this.LandingReports.Add(landingReport);
                if ((DateTime.UtcNow - this.lastTouchdown).TotalSeconds > 30)
                {
                    this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, FlightTrackingEventType.Touchdown, OpenSkyColors.OpenSkyTeal, "Touchdown");
                    this.FinalTouchDownIndex = this.LandingReports.Count - 1;
                    this.plaQueueCountdown = 100;
                }

                this.lastTouchdown = DateTime.UtcNow;
            }

            this.plaQueue[this.plaQueueIndex++] = pla.New;
            if (this.plaQueueIndex == 100)
            {
                this.plaQueueIndex = 0;
            }

            if (this.plaQueueCountdown > 0)
            {
                this.plaQueueCountdown--;
            }

            if (this.plaQueueCountdown == 0 && this.FinalTouchDownIndex >= 0)
            {
                this.plaQueueCountdown = -1;

                // Check for max g-force after touchdown
                var maxGeforce = this.LandingReports[this.FinalTouchDownIndex].GForce;
                for (var i = 0; i < 100; i++)
                {
                    if (this.plaQueue[i] != null)
                    {
                        maxGeforce = Math.Max(maxGeforce, this.plaQueue[i].Gforce);
                    }
                }

                this.LandingReports[this.FinalTouchDownIndex].GForce = maxGeforce;

                // First landing for this flight
                this.LandingReported?.Invoke(this, LandingReportNotification.AsSoonAsPossible);
            }
        }
    }
}