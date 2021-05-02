// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Process.Lights.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.AgentMSFS.SimConnect.Helpers;

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
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, OpenSkyColors.OpenSkyLightYellow, pst.New.LightBeacon ? "Beacon on" : "Beacon off");

                // Engine running?
                if (!pst.New.LightBeacon && pst.New.EngineRunning && (this.TrackingStatus == TrackingStatus.GroundOperations || this.TrackingStatus == TrackingStatus.Tracking))
                {
                    //todo add some kind of xp reduction
                    this.AddTrackingEvent(this.PrimaryTracking, pst.New, OpenSkyColors.OpenSkyRed, "Beacon turned off while engine was running");
                    this.fsConnect.SetText("OpenSky Warning: Beacon turned off while engine was running", 5);
                }
            }

            if (pst.Old.LightNav != pst.New.LightNav)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, OpenSkyColors.OpenSkyLightYellow, pst.New.LightNav ? "Nav lights on" : "Nav lights off");
            }

            if (pst.Old.LightStrobe != pst.New.LightStrobe)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, OpenSkyColors.OpenSkyLightYellow, pst.New.LightStrobe ? "Strobe lights on" : "Strobe lights off");
            }

            if (pst.Old.LightTaxi != pst.New.LightTaxi)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, OpenSkyColors.OpenSkyLightYellow, pst.New.LightTaxi ? "Taxi lights on" : "Taxi lights off");
            }

            if (pst.Old.LightLanding != pst.New.LightLanding)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, OpenSkyColors.OpenSkyLightYellow, pst.New.LightLanding ? "Landing lights on" : "Landing lights off");
            }

            if (this.TrackingStatus == TrackingStatus.Tracking)
            {
                if (this.PlaneIdentity.EngineType == EngineType.Jet || this.PlaneIdentity.EngineType == EngineType.Turboprop)
                {
                    // 10000 feet landing lights (give 500 feet spare)
                    if (this.PrimaryTracking.Altitude < 9500 && !this.PrimaryTracking.OnGround && !pst.New.LightLanding)
                    {
                        if (!this.landingLightWarningActive)
                        {
                            this.landingLightWarningActive = true;
                            this.AddTrackingEvent(this.PrimaryTracking, pst.New, OpenSkyColors.OpenSkyRed, "Landing lights off below 10k feet");
                            this.fsConnect.SetText("OpenSky Warning: Landing lights off below 10k feet", 5);
                            //todo add some kind of xp reduction
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
                        this.landingLightWarningActive = true;
                        this.AddTrackingEvent(this.PrimaryTracking, pst.New, OpenSkyColors.OpenSkyRed, "Landing lights off below 300 feet AGL");
                        this.fsConnect.SetText("OpenSky Warning: Landing lights off below 300 feet AGL", 5);
                        //todo add some kind of xp reduction
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