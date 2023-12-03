// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Process.Lights.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Diagnostics;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - lights.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the landing light warning currently active?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool landingLightWarningActive;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When did the status of the beacon light last change? Some aircraft seem to toggle this on and
        /// off with every red flash, omg.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastBeaconChange = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last beacon state, or NULL if are not currently tracking a change - timeout occurred
        /// while still different.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool? lastBeaconStatus;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Monitor lights.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="pst">
        /// The secondary Simconnect tracking data (old and new).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MonitorLights(ProcessSecondaryTracking pst)
        {
            // Beacon
            var wasBeaconChange = false;
            if (pst.Old.LightBeacon != pst.New.LightBeacon)
            {
                if ((DateTime.UtcNow - this.lastBeaconChange).TotalSeconds > 5)
                {
                    this.lastBeaconStatus = pst.New.LightBeacon;
                    wasBeaconChange = true;
                }

                this.lastBeaconChange = DateTime.UtcNow;
            }

            if (this.lastBeaconStatus.HasValue && this.lastBeaconStatus.Value != pst.New.LightBeacon && (DateTime.UtcNow - this.lastBeaconChange).TotalSeconds > 5)
            {
                this.lastBeaconChange = DateTime.UtcNow;
                this.lastBeaconStatus = null;
                wasBeaconChange = true;
            }

            if (wasBeaconChange)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.Beacon, OpenSkyColors.OpenSkyLightYellow, pst.New.LightBeacon ? "Beacon on" : "Beacon off");

                // Engine running?
                if (!pst.New.LightBeacon && pst.New.EngineRunning && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking) && this.Flight?.Aircraft.Type.UsesStrobeForBeacon != true)
                {
                    this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.BeaconOffEnginesOn, OpenSkyColors.OpenSkyRed, "Beacon turned off while engine was running");
                }
            }

            // Nav lights
            if (pst.Old.LightNav != pst.New.LightNav)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.NavLights, OpenSkyColors.OpenSkyLightYellow, pst.New.LightNav ? "Nav lights on" : "Nav lights off");
            }

            if (pst.Old.LightStrobe != pst.New.LightStrobe)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.Strobe, OpenSkyColors.OpenSkyLightYellow, pst.New.LightStrobe ? "Strobe lights on" : "Strobe lights off");

                // Engine running?
                if (!pst.New.LightStrobe && pst.New.EngineRunning && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking) && this.Flight?.Aircraft.Type.UsesStrobeForBeacon == true)
                {
                    this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.BeaconOffEnginesOn, OpenSkyColors.OpenSkyRed, "Strobe turned off while engine was running");
                }
            }

            if (pst.Old.LightTaxi != pst.New.LightTaxi)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.TaxiLights, OpenSkyColors.OpenSkyLightYellow, pst.New.LightTaxi ? "Taxi lights on" : "Taxi lights off");
            }

            if (pst.Old.LightLanding != pst.New.LightLanding)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.LandingLights, OpenSkyColors.OpenSkyLightYellow, pst.New.LightLanding ? "Landing lights on" : "Landing lights off");
            }

            if (this.TrackingStatus == TrackingStatus.Tracking)
            {
                if (this.AircraftIdentity.EngineType is EngineType.Jet or EngineType.Turboprop)
                {
                    // 10000 feet landing lights (give 1000 feet spare)
                    if (this.PrimaryTracking.IndicatedAltitude < 9000 && !this.PrimaryTracking.OnGround && !pst.New.LightLanding)
                    {
                        if (!this.landingLightWarningActive)
                        {
                            Debug.WriteLine($"Landing lights 10K: indicated {this.PrimaryTracking.IndicatedAltitude}, alt {this.PrimaryTracking.Altitude}");
                            this.landingLightWarningActive = true;
                            this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.LandingLightsOffBelow10K, OpenSkyColors.OpenSkyLightYellow, "Landing lights off below 10k feet");
                        }
                    }
                    else
                    {
                        this.landingLightWarningActive = false;
                    }
                }
                else
                {
                    // 300 feet AGL landing lights
                    if (this.PrimaryTracking.RadioHeight < 300 && !this.PrimaryTracking.OnGround && !pst.New.LightLanding)
                    {
                        if (!this.landingLightWarningActive)
                        {
                            this.landingLightWarningActive = true;
                            this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.LandingLightsOffBelow300AGL, OpenSkyColors.OpenSkyLightYellow, "Landing lights off below 300 feet AGL");
                        }
                    }
                    else
                    {
                        this.landingLightWarningActive = false;
                    }
                }
            }
        }
    }
}