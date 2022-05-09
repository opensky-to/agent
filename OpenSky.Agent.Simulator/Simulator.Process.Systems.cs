// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Process.Systems.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Diagnostics;
    using System.Media;
    using System.Reflection;
    using System.Windows;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.FlightLogXML;

    using TrackingEventMarker = OpenSky.Agent.Simulator.Models.TrackingEventMarker;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - systems processing.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last overspeed Date/Time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastOverspeed = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last stall Date/Time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastStall = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last time change Date/Time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastTimeChange = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the 250 below 10000 speed limit warning currently active?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool speedLimitWarningActive;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Monitor primary systems.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="ppt">
        /// The primary Simconnect tracking data (old and new).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MonitorPrimarySystems(ProcessPrimaryTracking ppt)
        {
            // Start of overspeed?
            if (!ppt.Old.OverspeedWarning && ppt.New.OverspeedWarning)
            {
                if ((DateTime.UtcNow - this.lastOverspeed).TotalSeconds > 10)
                {
                    this.lastOverspeed = DateTime.UtcNow;
                    this.AddTrackingEvent(ppt.New, this.SecondaryTracking, FlightTrackingEventType.Overspeed, OpenSkyColors.OpenSkyRed, "Overspeed");
                }
            }

            // Start of stall?
            if (!ppt.Old.StallWarning && ppt.New.StallWarning)
            {
                if ((DateTime.UtcNow - this.lastStall).TotalSeconds > 10)
                {
                    this.lastStall = DateTime.UtcNow;
                    this.AddTrackingEvent(ppt.New, this.SecondaryTracking, FlightTrackingEventType.Stall, OpenSkyColors.OpenSkyRed, "Stall");
                }
            }

            // 10000 feet speed limit 250 knots (give 500 feet and 10 knots spare)
            if (ppt.New.IndicatedAltitude < 9500 && !ppt.New.OnGround && ppt.New.AirspeedIndicated > 260)
            {
                if (!this.speedLimitWarningActive)
                {
                    Debug.WriteLine($"Speed limit 250 below 10K: indicated {ppt.New.IndicatedAltitude}, alt {ppt.New.Altitude} => {ppt.New.AirspeedIndicated} knots");
                    this.speedLimitWarningActive = true;
                    this.AddTrackingEvent(ppt.New, this.SecondaryTracking, FlightTrackingEventType.SpeedLimit250Below10K, OpenSkyColors.OpenSkyRed, "Speed over 250 below 10k feet");
                }
            }
            else
            {
                this.speedLimitWarningActive = false;
            }

            // Slew activated?
            if (ppt.New.SlewActive && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
            {
                Debug.WriteLine("OpenSky Warning: Tracking aborted, slew detected.");
                var assembly = Assembly.GetExecutingAssembly();
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                player.Play();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedSlew);
                this.StopTracking(true);
            }

            // Teleport to another position?
            // todo define max knots ground speed allowed per plane?
            if (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
            {
                var groundSpeedMetersPerSecond = 600 / 1.944 * ppt.New.SimulationRate;
                var distanceTraveledInMeters = ppt.Old.GeoCoordinate.GetDistanceTo(ppt.New.GeoCoordinate) * (this.SampleRates[Requests.Primary] / 1000.0);
                if (distanceTraveledInMeters > groundSpeedMetersPerSecond)
                {
                    Debug.WriteLine("OpenSky Warning: Tracking aborted, teleport detected.");
                    var assembly = Assembly.GetExecutingAssembly();
                    var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                    player.Play();
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedTeleport);
                    this.StopTracking(true);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Monitor secondary systems.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="pst">
        /// The secondary Simconnect tracking data (old and new).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void MonitorSecondarySystems(ProcessSecondaryTracking pst)
        {
            // Was the engine turned off/on?
            if (pst.Old.EngineRunning != pst.New.EngineRunning)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.Engine, OpenSkyColors.OpenSkyTealLight, pst.New.EngineRunning ? "Engine started" : "Engine shut down");
                if (pst.New.EngineRunning && this.TrackingStatus == TrackingStatus.Tracking)
                {
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.WelcomeOpenSky);
                }

                // Was the engine turned on while we are in ground handling tracking mode?
                if (pst.New.EngineRunning && this.TrackingStatus == TrackingStatus.GroundOperations)
                {
                    Debug.WriteLine("OpenSky Warning: Tracking aborted, you cannot start your engines while ground handling isn't complete!");
                    var assembly = Assembly.GetExecutingAssembly();
                    var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                    player.PlaySync();
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedEnginesGroundHandling);
                    this.StopTracking(true);
                }

                // Was the beacon light off when the engine was started?
                if (pst.New.EngineRunning && !pst.New.LightBeacon && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
                {
                    this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.BeaconOffEnginesOn, OpenSkyColors.OpenSkyRed, "Beacon turned off when engine was started");
                }

                // Was the taxi or landing light on when turning on/off the engine?
                if ((pst.New.LightTaxi || pst.New.LightLanding) && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
                {
                    this.AddTrackingEvent(
                        this.PrimaryTracking,
                        pst.New,
                        FlightTrackingEventType.TaxiLandingLightsEngine,
                        OpenSkyColors.OpenSkyRed,
                        $"OpenSky Warning: Taxi and/or Landing light on when engine was turned {(pst.New.EngineRunning ? "on" : "off")}");
                }

                // Was the engine turned off on the ground, not moving, while tracking? -> End tracking session, save reports and report to API
                if (!pst.New.EngineRunning && this.PrimaryTracking.OnGround && this.PrimaryTracking.GroundSpeed < 1 && this.TrackingStatus == TrackingStatus.Tracking)
                {
                    // todo expand this to battery off and proper shutdown/secure flow for extra xp (enable/disable in the settings)

                    if (!this.WasAirborne)
                    {
                        Debug.WriteLine("Engine was turned off, but the plane was never airborne, aborting...");
                        var assembly = Assembly.GetExecutingAssembly();
                        var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                        player.PlaySync();
                        SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.EngineOffNeverAirborne);
                        this.StopTracking(false);
                    }
                    else
                    {
                        Debug.WriteLine("Engine was turned off, wrapping up flight");

                        // Did the user shut the engines off on the runway or was there some taxi to parking?
                        if (this.taxiInStarted)
                        {
                            if (!this.taxiInTurned)
                            {
                                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.EngineOffRunway, OpenSkyColors.OpenSkyWarningOrange, "Engine turned off on the runway?");
                            }

                            this.taxiInStarted = false;
                        }

                        // Add one last position report
                        UpdateGUIDelegate addPositionReport = () =>
                        {
                            this.AircraftTrailLocations.Add(new AircraftTrailLocation(DateTime.UtcNow, this.PrimaryTracking, pst.New, this.WeightAndBalance.FuelTotalQuantity));
                            this.TrackingEventMarkerAdded?.Invoke(this, new TrackingEventMarker(this.PrimaryTracking, pst.New, this.WeightAndBalance.FuelTotalQuantity, 8, OpenSkyColors.OpenSkyTeal, "Position report"));
                        };
                        Application.Current.Dispatcher.Invoke(addPositionReport);

                        this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.TrackingStopped, OpenSkyColors.OpenSkyTealLight, "Flight tracking stopped");

                        // Show landing report notification now?
                        this.LandingReported?.Invoke(this, LandingReportNotification.AfterTurningEnginesOff);

                        // Actually finish up the tracking session now
                        SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.FlightCompleteSubmitting);
                        this.FinishUpFlightTracking();
                    }
                }
            }

            // Is there a pushback while we are in ground handling tracking mode?
            if (pst.New.Pushback != Pushback.NoPushback && this.TrackingStatus == TrackingStatus.GroundOperations)
            {
                Debug.WriteLine("OpenSky Warning: Tracking aborted, you cannot start your pushback while ground handling isn't complete!");
                var assembly = Assembly.GetExecutingAssembly();
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                player.PlaySync();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedPushbackGroundHandling);
                this.StopTracking(true);
            }

            // Pushback start?
            if (pst.Old.Pushback == Pushback.NoPushback && pst.New.Pushback != Pushback.NoPushback)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.PushbackStarted, OpenSkyColors.OpenSkyTealLight, "Pushback started");
            }

            // Pushback finished?
            if (pst.Old.Pushback != Pushback.NoPushback && pst.New.Pushback == Pushback.NoPushback)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.PushbackFinished, OpenSkyColors.OpenSkyTealLight, "Pushback finished");
            }

            // Was the battery master turned off/on?
            if (pst.Old.ElectricalMasterBattery != pst.New.ElectricalMasterBattery && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.BatteryMaster, OpenSkyColors.OpenSkyTealLight, pst.New.ElectricalMasterBattery ? "Battery master on" : "Battery master off");
            }

            // Was the landing gear lowered/raised?
            if (pst.Old.GearHandle != pst.New.GearHandle)
            {
                if (!this.PrimaryTracking.OnGround)
                {
                    this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.LandingGear, OpenSkyColors.OpenSkyTealLight, pst.New.GearHandle ? "Landing gear lowered" : "Landing gear raised");

                    // todo add check that removes xp if gear is still lowered above a certain agl depending on engine type (if gear is retractable)?
                }
                else
                {
                    if (!pst.New.GearHandle)
                    {
                        this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.GearUpOnGround, OpenSkyColors.OpenSkyRed, "Tried to raise landing gear on the ground");
                    }
                }
            }

            // Was the flaps position changed?
            if (Math.Abs(pst.Old.FlapsHandle - pst.New.FlapsHandle) > 1)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.Flaps, OpenSkyColors.OpenSkyTealLight, $"Flaps set to {(int)pst.New.FlapsHandle} %");
            }

            // Was the autopilot engaged/disengaged?
            if (pst.Old.AutoPilot != pst.New.AutoPilot)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.AutoPilot, OpenSkyColors.OpenSkyTealLight, pst.New.AutoPilot ? "AP engaged" : "AP disengaged");
            }

            // Was the parking brake set/released?
            if (pst.Old.ParkingBrake != pst.New.ParkingBrake)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.ParkingBrake, OpenSkyColors.OpenSkyTealLight, pst.New.ParkingBrake ? "Parking brake set" : "Parking brake released");
            }

            // Were the spoilers armed/disarmed?
            if (pst.Old.SpoilersArmed != pst.New.SpoilersArmed)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.Spoilers, OpenSkyColors.OpenSkyTealLight, pst.New.SpoilersArmed ? "Spoilers armed" : "Spoilers disarmed");
            }

            // Was the APU turned on/off?
            if (pst.Old.ApuRunning != pst.New.ApuRunning)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.APU, OpenSkyColors.OpenSkyTealLight, pst.New.ApuRunning ? "APU started" : "APU shut down");
            }

            // Were the seatbelt signs turned on/off
            if (pst.Old.SeatBeltSign != pst.New.SeatBeltSign)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.SeatbeltSigns, OpenSkyColors.OpenSkyTealLight, pst.New.SeatBeltSign ? "Seat belt signs on" : "Seat belt signs off");
            }

            // Were the no smoking signs turned on/off
            if (pst.Old.NoSmokingSign != pst.New.NoSmokingSign)
            {
                this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.NoSmokingSigns, OpenSkyColors.OpenSkyTealLight, pst.New.NoSmokingSign ? "No smoking signs on" : "No smoking signs off");
            }

            // Was the crash detection turned off?
            if (!pst.New.CrashDetection && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
            {
                Debug.WriteLine("OpenSky Warning: Tracking aborted, crash detection turned off!");
                var assembly = Assembly.GetExecutingAssembly();
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                player.PlaySync();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedCrashDetectOff);
                this.StopTracking(false);
            }

            // Was unlimited fuel turned on?
            if (pst.New.UnlimitedFuel && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
            {
                Debug.WriteLine("OpenSky Warning: Tracking aborted, unlimited fuel turned on!");
                var assembly = Assembly.GetExecutingAssembly();
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                player.PlaySync();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedUnlimitedFuel);
                this.StopTracking(false);
            }

            // Was the time in the sim changed?
            if (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
            {
                var timeDelta = pst.New.UtcDateTime - pst.Old.UtcDateTime;
                if (timeDelta.TotalSeconds is < -60 or > 60)
                {
                    if ((DateTime.UtcNow - this.lastTimeChange).TotalSeconds > 10)
                    {
                        this.lastTimeChange = DateTime.UtcNow;
                        this.AddTrackingEvent(this.PrimaryTracking, pst.New, FlightTrackingEventType.TimeInSimChanged, OpenSkyColors.OpenSkyTealLight, "Time in simulator changed");
                    }
                }
            }
        }
    }
}