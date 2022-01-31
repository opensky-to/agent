﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Process.Systems.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Diagnostics;
    using System.Media;
    using System.Reflection;
    using System.Windows;

    using OpenSky.Agent.Simulator;
    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.AgentMSFS.Properties;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.AgentMSFS.SimConnect.Helpers;
    using OpenSky.FlightLogXML;

    using TrackingEventMarker = Models.TrackingEventMarker;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client - systems monitoring code.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
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
                    this.AddTrackingEvent(ppt.New, this.SecondaryTrackingStruct, FlightTrackingEventType.Overspeed, OpenSkyColors.OpenSkyRed, "Overspeed");
                }
            }

            // Start of stall?
            if (!ppt.Old.StallWarning && ppt.New.StallWarning)
            {
                if ((DateTime.UtcNow - this.lastStall).TotalSeconds > 10)
                {
                    this.lastStall = DateTime.UtcNow;
                    this.AddTrackingEvent(ppt.New, this.SecondaryTrackingStruct, FlightTrackingEventType.Stall, OpenSkyColors.OpenSkyRed, "Stall");
                }
            }

            // Slew activated?
            if (ppt.New.SlewActive && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
            {
                Debug.WriteLine("OpenSky Warning: Tracking aborted, slew detected.");
                var assembly = Assembly.GetExecutingAssembly();
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                player.Play();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedSlew);
                this.StopTracking(false);
                this.fsConnect.SetText("OpenSky Warning: Tracking aborted, slew detected.", 5);
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
                    var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                    player.Play();
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedTeleport);
                    this.StopTracking(false);
                    this.fsConnect.SetText("OpenSky Warning: Tracking aborted, teleport detected.", 5);
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
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.Engine, OpenSkyColors.OpenSkyTealLight, pst.New.EngineRunning ? "Engine started" : "Engine shut down");
                if (pst.New.EngineRunning && this.TrackingStatus == TrackingStatus.Tracking)
                {
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.WelcomeOpenSky);
                }

                // Was the engine turned on while we are in ground handling tracking mode?
                if (pst.New.EngineRunning && this.TrackingStatus == TrackingStatus.GroundOperations)
                {
                    Debug.WriteLine("OpenSky Warning: Tracking aborted, you cannot start your engines while ground handling isn't complete!");
                    var assembly = Assembly.GetExecutingAssembly();
                    var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                    player.PlaySync();
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedEnginesGroundHandling);
                    this.StopTracking(false);
                    this.fsConnect.SetText("OpenSky Warning: Tracking aborted, you cannot start your engines while ground handling isn't complete!", 5);
                }

                // Was the beacon light off when the engine was started?
                if (pst.New.EngineRunning && !pst.New.LightBeacon && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
                {
                    this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.BeaconOffEnginesOn, OpenSkyColors.OpenSkyRed, "Beacon turned off when engine was started");
                    this.fsConnect.SetText("OpenSky Warning: Beacon turned off when engine was started", 5);
                }

                // Was the taxi or landing light on when turning on/off the engine?
                if ((pst.New.LightTaxi || pst.New.LightLanding) && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
                {
                    this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.TaxiLandingLightsEngine, OpenSkyColors.OpenSkyRed, $"OpenSky Warning: Taxi and/or Landing light on when engine was turned {(pst.New.EngineRunning ? "on" : "off")}");
                    this.fsConnect.SetText($"OpenSky Warning: Taxi and/or Landing light on when engine was turned {(pst.New.EngineRunning ? "on" : "off")}", 5);
                }

                // Was the engine turned off on the ground, not moving, while tracking? -> End tracking session, save reports and report to API
                if (!pst.New.EngineRunning && this.PrimaryTrackingStruct.OnGround && this.PrimaryTrackingStruct.GroundSpeed < 1 && this.TrackingStatus == TrackingStatus.Tracking)
                {
                    // todo expand this to battery off and proper shutdown/secure flow for extra xp (enable/disable in the settings)

                    if (!this.WasAirborne)
                    {
                        Debug.WriteLine("Engine was turned off, but the plane was never airborne, aborting...");
                        var assembly = Assembly.GetExecutingAssembly();
                        var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                        player.PlaySync();
                        SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.EngineOffNeverAirborne);
                        this.fsConnect.SetText("OpenSky Warning: Engine was turned off, but the plane was never airborne, aborting...", 5);
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
                                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.EngineOffRunway, OpenSkyColors.OpenSkyWarningOrange, "Engine turned off on the runway?");
                                this.fsConnect.SetText("OpenSky Warning: Engine turned off on the runway?", 5);
                            }

                            this.taxiInStarted = false;
                        }

                        // Add one last position report
                        UpdateGUIDelegate addPositionReport = () =>
                        {
                            this.AircraftTrailLocations.Add(new AircraftTrailLocation(DateTime.UtcNow, this.PrimaryTrackingStruct, pst.New, this.WeightAndBalanceStruct.FuelTotalQuantity));
                            this.TrackingEventMarkerAdded?.Invoke(this, new TrackingEventMarker(this.PrimaryTrackingStruct, pst.New, this.WeightAndBalanceStruct.FuelTotalQuantity, 8, OpenSkyColors.OpenSkyTeal, "Position report"));
                        };
                        Application.Current.Dispatcher.Invoke(addPositionReport);

                        this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.TrackingStopped, OpenSkyColors.OpenSkyTealLight, "Flight tracking stopped");

                        // Show landing report notification now?
                        if (LandingReportNotification.AfterTurningEnginesOff.Equals(LandingReportNotification.Parse(Settings.Default.LandingReportNotification)))
                        {
                            UpdateGUIDelegate showNotification = () => new Views.LandingReport().Show();
                            Application.Current.Dispatcher.BeginInvoke(showNotification);
                        }

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
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                player.PlaySync();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedPushbackGroundHandling);
                this.StopTracking(false);
                this.fsConnect.SetText("OpenSky Warning: Tracking aborted, you cannot start your pushback while ground handling isn't complete!", 5);
            }

            // Pushback start?
            if (pst.Old.Pushback == Pushback.NoPushback && pst.New.Pushback != Pushback.NoPushback)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.PushbackStarted, OpenSkyColors.OpenSkyTealLight, "Pushback started");
            }

            // Pushback finished?
            if (pst.Old.Pushback != Pushback.NoPushback && pst.New.Pushback == Pushback.NoPushback)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.PushbackFinished, OpenSkyColors.OpenSkyTealLight, "Pushback finished");
            }

            // Was the battery master turned off/on?
            if (pst.Old.ElectricalMasterBattery != pst.New.ElectricalMasterBattery && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.BatteryMaster, OpenSkyColors.OpenSkyTealLight, pst.New.ElectricalMasterBattery ? "Battery master on" : "Battery master off");
            }

            // Was the landing gear lowered/raised?
            if (pst.Old.GearHandle != pst.New.GearHandle)
            {
                if (!this.PrimaryTrackingStruct.OnGround)
                {
                    this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.LandingGear, OpenSkyColors.OpenSkyTealLight, pst.New.GearHandle ? "Landing gear lowered" : "Landing gear raised");

                    // todo add check that removes xp if gear is still lowered above a certain agl depending on engine type (if gear is retractable)?
                }
                else
                {
                    if (!pst.New.GearHandle)
                    {
                        this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.GearUpOnGround, OpenSkyColors.OpenSkyRed, "Tried to raise landing gear on the ground");
                    }
                }
            }

            // Was the flaps position changed?
            if (Math.Abs(pst.Old.FlapsHandle - pst.New.FlapsHandle) > 1)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.Flaps, OpenSkyColors.OpenSkyTealLight, $"Flaps set to {(int)pst.New.FlapsHandle} %");
            }

            // Was the autopilot engaged/disengaged?
            if (pst.Old.AutoPilot != pst.New.AutoPilot)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.AutoPilot, OpenSkyColors.OpenSkyTealLight, pst.New.AutoPilot ? "AP engaged" : "AP disengaged");
            }

            // Was the parking brake set/released?
            if (pst.Old.ParkingBrake != pst.New.ParkingBrake)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.ParkingBrake, OpenSkyColors.OpenSkyTealLight, pst.New.ParkingBrake ? "Parking brake set" : "Parking brake released");
            }

            // Were the spoilers armed/disarmed?
            if (pst.Old.SpoilersArmed != pst.New.SpoilersArmed)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.Spoilers, OpenSkyColors.OpenSkyTealLight, pst.New.SpoilersArmed ? "Spoilers armed" : "Spoilers disarmed");
            }

            // Was the APU turned on/off?
            if (pst.Old.ApuGenerator != pst.New.ApuGenerator)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.APU, OpenSkyColors.OpenSkyTealLight, pst.New.ApuGenerator ? "APU started" : "APU shut down");
            }

            // Were the seatbelt signs turned on/off
            if (pst.Old.SeatBeltSign != pst.New.SeatBeltSign)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.SeatbeltSigns, OpenSkyColors.OpenSkyTealLight, pst.New.SeatBeltSign ? "Seat belt signs on" : "Seat belt signs off");
            }

            // Were the no smoking signs turned on/off
            if (pst.Old.NoSmokingSign != pst.New.NoSmokingSign)
            {
                this.AddTrackingEvent(this.PrimaryTrackingStruct, pst.New, FlightTrackingEventType.NoSmokingSigns, OpenSkyColors.OpenSkyTealLight, pst.New.NoSmokingSign ? "No smoking signs on" : "No smoking signs off");
            }

            // Was the crash detection turned off?
            if (!pst.New.CrashDetection && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
            {
                Debug.WriteLine("OpenSky Warning: Tracking aborted, crash detection turned off!");
                var assembly = Assembly.GetExecutingAssembly();
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                player.PlaySync();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedCrashDetectOff);
                this.StopTracking(false);
                this.fsConnect.SetText("OpenSky Warning: Tracking aborted, crash detection turned off!", 5);
            }

            // Was unlimited fuel turned on?
            if (pst.New.UnlimitedFuel && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
            {
                Debug.WriteLine("OpenSky Warning: Tracking aborted, unlimited fuel turned on!");
                var assembly = Assembly.GetExecutingAssembly();
                var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                player.PlaySync();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedUnlimitedFuel);
                this.StopTracking(false);
                this.fsConnect.SetText("OpenSky Warning: Tracking aborted, unlimited fuel turned on!", 5);
            }

            // Was the time in the sim changed?
            if (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
            {
                var timeDelta = pst.New.UtcDateTime - pst.Old.UtcDateTime;
                if (timeDelta.TotalSeconds < -5)
                {
                    Debug.WriteLine("OpenSky Warning: Tracking aborted, time moved backwards!");
                    var assembly = Assembly.GetExecutingAssembly();
                    var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                    player.PlaySync();
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedTimeBackwards);
                    this.StopTracking(false);
                    this.fsConnect.SetText("OpenSky Warning: Tracking aborted, time moved backwards!", 5);
                }

                if (timeDelta.TotalSeconds > 30)
                {
                    Debug.WriteLine("OpenSky Warning: Tracking aborted, time changed in sim!");
                    var assembly = Assembly.GetExecutingAssembly();
                    var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                    player.PlaySync();
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedTimeChanged);
                    this.StopTracking(false);
                    this.fsConnect.SetText("OpenSky Warning: Tracking aborted, time changed in sim!", 5);
                }
            }
        }
    }
}