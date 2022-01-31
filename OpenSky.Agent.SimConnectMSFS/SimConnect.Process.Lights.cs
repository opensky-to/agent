// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Process.Lights.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System.Diagnostics;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Helpers;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client - lights monitoring code.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the landing light warning currently active?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool landingLightWarningActive;

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
            if (pst.Old.LightBeacon != pst.New.LightBeacon)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.Beacon, OpenSkyColors.OpenSkyLightYellow, pst.New.LightBeacon ? "Beacon on" : "Beacon off");

                // Engine running?
                if (!pst.New.LightBeacon && pst.New.EngineRunning && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
                {
                    this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.BeaconOffEnginesOn, OpenSkyColors.OpenSkyRed, "Beacon turned off while engine was running");
                    this.fsConnect.SetText("OpenSky Warning: Beacon turned off while engine was running", 5);
                }
            }

            if (pst.Old.LightNav != pst.New.LightNav)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.NavLights, OpenSkyColors.OpenSkyLightYellow, pst.New.LightNav ? "Nav lights on" : "Nav lights off");
            }

            if (pst.Old.LightStrobe != pst.New.LightStrobe)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.Strobe, OpenSkyColors.OpenSkyLightYellow, pst.New.LightStrobe ? "Strobe lights on" : "Strobe lights off");
            }

            if (pst.Old.LightTaxi != pst.New.LightTaxi)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.TaxiLights, OpenSkyColors.OpenSkyLightYellow, pst.New.LightTaxi ? "Taxi lights on" : "Taxi lights off");
            }

            if (pst.Old.LightLanding != pst.New.LightLanding)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.LandingLights, OpenSkyColors.OpenSkyLightYellow, pst.New.LightLanding ? "Landing lights on" : "Landing lights off");
            }

            if (this.TrackingStatus == TrackingStatus.Tracking)
            {
                if (this.AircraftIdentityStruct.EngineType is EngineType.Jet or EngineType.Turboprop)
                {
                    // 10000 feet landing lights (give 500 feet spare)
                    if (this.PrimaryTrackingStruct.IndicatedAltitude < 9500 && !this.PrimaryTrackingStruct.OnGround && !pst.New.LightLanding)
                    {
                        if (!this.landingLightWarningActive)
                        {
                            Debug.WriteLine($"Landing lights 10K: indicated {this.PrimaryTrackingStruct.IndicatedAltitude}, alt {this.PrimaryTrackingStruct.Altitude}");
                            this.landingLightWarningActive = true;
                            this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.LandingLightsOffBelow10K, OpenSkyColors.OpenSkyRed, "Landing lights off below 10k feet");
                            this.fsConnect.SetText("OpenSky Warning: Landing lights off below 10k feet", 5);
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
                    if (this.PrimaryTrackingStruct.RadioHeight < 300 && !this.PrimaryTrackingStruct.OnGround && !pst.New.LightLanding)
                    {
                        if (!this.landingLightWarningActive)
                        {
                            this.landingLightWarningActive = true;
                            this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.LandingLightsOffBelow300AGL, OpenSkyColors.OpenSkyRed, "Landing lights off below 300 feet AGL");
                            this.fsConnect.SetText("OpenSky Warning: Landing lights off below 300 feet AGL", 5);
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