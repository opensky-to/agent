// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Process.Landing.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows;

    using OpenSky.AgentMSFS.Properties;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Helpers;
    using OpenSky.AgentMSFS.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simconnect client - landing analysis code.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing reports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<LandingReport> LandingReports { get; }

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
                var landingReport = new LandingReport(
                    DateTime.UtcNow,
                    pla.New.Location,
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
                    this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, OpenSkyColors.OpenSkyTeal, "Touchdown");

                    // Show landing report notification now?
                    if (LandingReportNotification.AsSoonAsPossible.Equals(LandingReportNotification.Parse(Settings.Default.LandingReportNotification)))
                    {
                        UpdateGUIDelegate showNotification = () => new Views.LandingReport().Show();
                        Application.Current.Dispatcher.BeginInvoke(showNotification);
                    }
                }
            }
        }
    }
}