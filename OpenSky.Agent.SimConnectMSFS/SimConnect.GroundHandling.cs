// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.GroundHandling.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using OpenSky.Agent.Simulator;
    using OpenSky.AgentMSFS.Models;
    using OpenSky.FlightLogXML;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simconnect client - ground handling code.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The is ground handling thread running mutex.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex isGroundHandlingThreadRunning = new(false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if fuel loading is complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool fuelLoadingComplete;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if fuel loading complete sound played.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool fuelLoadingCompleteSoundPlayed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel loading estimate in minutes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string fuelLoadingEstimateMinutes = "??";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if ground handling is complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool groundHandlingComplete;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if payload loading is complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool payloadLoadingComplete;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if payload loading complete sound played.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool payloadLoadingCompleteSoundPlayed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload loading estimate in minutes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string payloadLoadingEstimateMinutes = "??";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the fuel loading is complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool FuelLoadingComplete
        {
            get => this.fuelLoadingComplete;

            private set
            {
                if (Equals(this.fuelLoadingComplete, value))
                {
                    return;
                }

                this.fuelLoadingComplete = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel loading estimate minutes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FuelLoadingEstimateMinutes
        {
            get => this.fuelLoadingEstimateMinutes;

            private set
            {
                if (Equals(this.fuelLoadingEstimateMinutes, value))
                {
                    return;
                }

                this.fuelLoadingEstimateMinutes = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the ground handling is complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool GroundHandlingComplete
        {
            get => this.groundHandlingComplete;

            private set
            {
                if (Equals(this.groundHandlingComplete, value))
                {
                    return;
                }

                this.groundHandlingComplete = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the payload loading is complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool PayloadLoadingComplete
        {
            get => this.payloadLoadingComplete;

            private set
            {
                if (Equals(this.payloadLoadingComplete, value))
                {
                    return;
                }

                this.payloadLoadingComplete = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload loading estimate minutes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PayloadLoadingEstimateMinutes
        {
            get => this.payloadLoadingEstimateMinutes;

            private set
            {
                if (Equals(this.payloadLoadingEstimateMinutes, value))
                {
                    return;
                }

                this.payloadLoadingEstimateMinutes = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Skip ground handling (add time saved to time warp).
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// <returns>
        /// True if it succeeds, false if it fails.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public bool SkipGroundHandling(bool startTracking)
        {
            if (this.Flight != null && !this.GroundHandlingComplete && (this.TrackingStatus == TrackingStatus.GroundOperations || startTracking))
            {
                var fuelSecondsLeft = ((this.Flight?.FuelLoadingComplete ?? DateTime.UtcNow) - DateTime.UtcNow).TotalSeconds;
                var payloadSecondsLeft = ((this.Flight?.PayloadLoadingComplete ?? DateTime.UtcNow) - DateTime.UtcNow).TotalSeconds;
                var secondsToSkip = Math.Max(fuelSecondsLeft, payloadSecondsLeft);
                this.Flight.FuelLoadingComplete = DateTimeOffset.UtcNow;
                this.Flight.PayloadLoadingComplete = DateTimeOffset.UtcNow;

                if (startTracking)
                {
                    this.GroundHandlingComplete = true;
                    this.TrackingStatus = TrackingStatus.Preparing;
                    this.StartTracking();
                }

                this.timeSavedBecauseOfSimRate = this.timeSavedBecauseOfSimRate.AddSeconds(secondsToSkip);
                this.WarpInfo = this.timeSavedBecauseOfSimRate.TotalSeconds >= 1 ? $"Yes, saved {this.timeSavedBecauseOfSimRate:hh\\:mm\\:ss} [*]" : "No [*]";
                this.AddTrackingEvent(this.PrimaryTrackingStruct, this.SecondaryTrackingStruct, FlightTrackingEventType.SkippedGroundHandling, OpenSkyColors.OpenSkyWarningOrange, "Skipped ground handling");
                return true;
            }

            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Speed up ground handling by 2X (add time saved to time warp).
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// <returns>
        /// True if it succeeds, false if it fails.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public bool SpeedUpGroundHandling()
        {
            if (this.Flight != null && !this.GroundHandlingComplete && this.TrackingStatus == TrackingStatus.GroundOperations)
            {
                var fuelSecondsLeft = ((this.Flight?.FuelLoadingComplete ?? DateTime.UtcNow) - DateTime.UtcNow).TotalSeconds;
                var payloadSecondsLeft = ((this.Flight?.PayloadLoadingComplete ?? DateTime.UtcNow) - DateTime.UtcNow).TotalSeconds;
                var secondsToSkip = Math.Max(fuelSecondsLeft, payloadSecondsLeft) / 2;

                this.timeSavedBecauseOfSimRate = this.timeSavedBecauseOfSimRate.AddSeconds(secondsToSkip);
                this.WarpInfo = this.timeSavedBecauseOfSimRate.TotalSeconds >= 1 ? $"Yes, saved {this.timeSavedBecauseOfSimRate:hh\\:mm\\:ss} [*]" : "No [*]";
                this.Flight.FuelLoadingComplete = DateTimeOffset.UtcNow.AddSeconds(fuelSecondsLeft / 2);
                this.Flight.PayloadLoadingComplete = DateTimeOffset.UtcNow.AddSeconds(payloadSecondsLeft / 2);
                this.AddTrackingEvent(this.PrimaryTrackingStruct, this.SecondaryTrackingStruct, FlightTrackingEventType.SkippedHalfGroundHandling, OpenSkyColors.OpenSkyWarningOrange, "Skipped half ground handling");
                return true;
            }

            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update ground handling estimates from API and update UI/play sounds.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/04/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DoGroundHandling()
        {
            if (!this.isGroundHandlingThreadRunning.WaitOne(1000))
            {
                Debug.WriteLine("Duplicate ground handling thread denied.");
                return;
            }

            Debug.WriteLine("Ground handling thread started.");

            try
            {
                // No flight? No go! (should not happen but just in case)
                if (this.Flight == null)
                {
                    this.GroundHandlingComplete = false;
                    this.FuelLoadingComplete = false;
                    this.PayloadLoadingComplete = false;
                    this.FuelLoadingEstimateMinutes = "??";
                    this.PayloadLoadingEstimateMinutes = "??";
                    this.fuelLoadingCompleteSoundPlayed = false;
                    this.payloadLoadingCompleteSoundPlayed = false;
                }

                var fuelMinutes = ((this.Flight?.FuelLoadingComplete ?? DateTime.UtcNow) - DateTime.UtcNow).TotalMinutes;
                var payloadMinutes = ((this.Flight?.PayloadLoadingComplete ?? DateTime.UtcNow) - DateTime.UtcNow).TotalMinutes;
                this.GroundHandlingComplete = false;
                while ((fuelMinutes > 0 || payloadMinutes > 0) && !SleepScheduler.IsShutdownInProgress)
                {
                    // Fuel checks
                    if (fuelMinutes > 1)
                    {
                        this.FuelLoadingEstimateMinutes = $"{(int)fuelMinutes} min{((int)fuelMinutes == 1 ? string.Empty : "s")}";
                        this.FuelLoadingComplete = false;
                    }
                    else if (fuelMinutes > 0)
                    {
                        this.FuelLoadingEstimateMinutes = "<1 min";
                        this.FuelLoadingComplete = false;
                    }
                    else
                    {
                        this.FuelLoadingComplete = true;
                        if (!this.fuelLoadingCompleteSoundPlayed)
                        {
                            this.fuelLoadingCompleteSoundPlayed = true;
                            if (this.TrackingStatus == TrackingStatus.GroundOperations)
                            {
                                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.FuelLoadingComplete, false);
                            }
                        }
                    }

                    // Payload checks
                    if (payloadMinutes > 1)
                    {
                        this.PayloadLoadingEstimateMinutes = $"{(int)payloadMinutes} min{((int)payloadMinutes == 1 ? string.Empty : "s")}";
                        this.PayloadLoadingComplete = false;
                    }
                    else if (payloadMinutes > 0)
                    {
                        this.PayloadLoadingEstimateMinutes = "<1 min";
                        this.PayloadLoadingComplete = false;
                    }
                    else
                    {
                        this.PayloadLoadingComplete = true;
                        if (!this.payloadLoadingCompleteSoundPlayed)
                        {
                            this.payloadLoadingCompleteSoundPlayed = true;
                            if (this.TrackingStatus == TrackingStatus.GroundOperations)
                            {
                                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.PayloadSetLoadsheet, false);
                            }
                        }
                    }

                    Thread.Sleep(5000);
                    fuelMinutes = ((this.Flight?.FuelLoadingComplete ?? DateTime.UtcNow) - DateTime.UtcNow).TotalMinutes;
                    payloadMinutes = ((this.Flight?.PayloadLoadingComplete ?? DateTime.UtcNow) - DateTime.UtcNow).TotalMinutes;
                }

                if (this.Flight == null)
                {
                    this.GroundHandlingComplete = false;
                    this.FuelLoadingComplete = false;
                    this.PayloadLoadingComplete = false;
                    this.FuelLoadingEstimateMinutes = "??";
                    this.PayloadLoadingEstimateMinutes = "??";
                }
                else
                {
                    this.FuelLoadingComplete = true;
                    this.PayloadLoadingComplete = true;
                    this.AddTrackingEvent(this.PrimaryTrackingStruct, this.SecondaryTrackingStruct, FlightTrackingEventType.GroundHandlingComplete, OpenSkyColors.OpenSkyTealLight, "Ground handling complete");

                    if (this.TrackingStatus == TrackingStatus.GroundOperations)
                    {
                        if (!this.fuelLoadingCompleteSoundPlayed)
                        {
                            SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.FuelLoadingComplete, false);
                            Thread.Sleep(1000);
                        }

                        if (!this.payloadLoadingCompleteSoundPlayed)
                        {
                            SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.PayloadSetLoadsheet, false);
                            Thread.Sleep(1000);
                        }

                        // Report all done and move tracking status to tracking
                        this.GroundHandlingComplete = true;
                        this.TrackingStatus = TrackingStatus.Tracking;
                        SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.ReadyPushStart);
                    }
                    else
                    {
                        Thread.Sleep(2000);
                        this.GroundHandlingComplete = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (this.Flight == null)
                {
                    this.GroundHandlingComplete = false;
                    this.FuelLoadingComplete = false;
                    this.PayloadLoadingComplete = false;
                    this.FuelLoadingEstimateMinutes = "??";
                    this.PayloadLoadingEstimateMinutes = "??";
                }
            }
            finally
            {
                Debug.WriteLine("Ground handling thread finished.");
                this.isGroundHandlingThreadRunning.ReleaseMutex();
            }
        }
    }
}