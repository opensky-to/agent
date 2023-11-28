// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Process.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Collections.Concurrent;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Media;
    using System.Reflection;
    using System.Threading;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - data processing.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The landing analysis processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        protected readonly ConcurrentQueue<ProcessLandingAnalysis> landingAnalysisProcessingQueue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The primary tracking processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        protected readonly ConcurrentQueue<ProcessPrimaryTracking> primaryTrackingProcessingQueue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The secondary tracking processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        protected readonly ConcurrentQueue<ProcessSecondaryTracking> secondaryTrackingProcessingQueue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the landing analysis processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int LandingAnalysisProcessingQueueLength => this.landingAnalysisProcessingQueue.Count;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last received/request date times dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableConcurrentDictionary<Requests, DateTime?> LastReceivedTimes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the primary tracking processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int PrimaryTrackingProcessingQueueLength => this.primaryTrackingProcessingQueue.Count;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The sample rates/request dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableConcurrentDictionary<Requests, int> SampleRates { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the secondary tracking processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int SecondaryTrackingProcessingQueueLength => this.secondaryTrackingProcessingQueue.Count;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh the request-associated data model object NOW, don't wait for normal refresh interval.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="request">
        /// The request ID type to refresh.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void RefreshModelNow(Requests request)
        {
            this.LastReceivedTimes[request] = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Monitor start/resume tracking conditions.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="secondary">
        /// The secondary tracking data.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void MonitorTrackingStartConditions(SecondaryTracking secondary)
        {
            if (this.Flight != null && this.TrackingStatus == TrackingStatus.Preparing || this.TrackingStatus == TrackingStatus.Resuming)
            {
                this.TrackingConditions[(int)Models.TrackingConditions.DateTime].Expected = $"{DateTime.UtcNow:HH:mm dd.MM.yyyy}";
                this.TrackingConditions[(int)Models.TrackingConditions.DateTime].Current = $"{secondary.UtcDateTime:HH:mm dd.MM.yyyy}";
                this.TrackingConditions[(int)Models.TrackingConditions.DateTime].ConditionMet =
                    this.TrackingConditions[(int)Models.TrackingConditions.DateTime].AutoSet || Math.Abs((DateTime.UtcNow - secondary.UtcDateTime).TotalMinutes) < 5;

                this.TrackingConditions[(int)Models.TrackingConditions.PlaneModel].Current = this.AircraftIdentity.Type;
                this.TrackingConditions[(int)Models.TrackingConditions.PlaneModel].ConditionMet = this.Flight?.Aircraft.Type.MatchesAircraftInSimulator() ?? false;

                if (this.TrackingStatus == TrackingStatus.Preparing)
                {
                    this.TrackingConditions[(int)Models.TrackingConditions.Fuel].Enabled = true;
                    this.TrackingConditions[(int)Models.TrackingConditions.Payload].Enabled = true;

                    this.TrackingConditions[(int)Models.TrackingConditions.Fuel].Current =
                        $"{this.WeightAndBalance.FuelTotalQuantity:F1} gal, {this.WeightAndBalance.FuelTotalQuantity * 3.78541:F1} liters ▶ {this.WeightAndBalance.FuelTotalQuantity * this.Flight?.Aircraft.Type.FuelWeightPerGallon:F1} lbs, {this.WeightAndBalance.FuelTotalQuantity * this.Flight?.Aircraft.Type.FuelWeightPerGallon * 0.453592:F1} kg";
                    this.TrackingConditions[(int)Models.TrackingConditions.Payload].Current = $"{this.PayloadStations.TotalWeight:F1} lbs, {this.PayloadStations.TotalWeight * 0.453592:F1} kg";

                    this.TrackingConditions[(int)Models.TrackingConditions.Fuel].ConditionMet =
                        this.TrackingConditions[(int)Models.TrackingConditions.Fuel].AutoSet || (this.WeightAndBalance.FuelTotalQuantity >= (this.Flight?.FuelGallons ?? 0));

                    this.TrackingConditions[(int)Models.TrackingConditions.Payload].ConditionMet =
                        this.TrackingConditions[(int)Models.TrackingConditions.Payload].AutoSet || (this.PayloadStations.TotalWeight >= (this.Flight?.PayloadPounds ?? 0));

                    this.TrackingConditions[(int)Models.TrackingConditions.RealismSettings].Expected = "No slew, No unlimited fuel,\r\nCrash detection, SimRate=0 or 1";
                    this.TrackingConditions[(int)Models.TrackingConditions.RealismSettings].ConditionMet =
                        !this.PrimaryTracking.SlewActive && !secondary.UnlimitedFuel && secondary.CrashDetection && ((int)this.PrimaryTracking.SimulationRate == 0 || (int)this.PrimaryTracking.SimulationRate == 1);

                    this.TrackingConditions[(int)Models.TrackingConditions.Location].Current =
                        $"{this.PrimaryTracking.GeoCoordinate.GetDistanceTo(new GeoCoordinate(this.Flight?.Origin.Latitude ?? 0, this.Flight?.Origin.Longitude ?? 0)) / 1000:F2} km from starting location - {(this.PrimaryTracking.OnGround ? "On ground" : "Airborne")}";
                    this.TrackingConditions[(int)Models.TrackingConditions.Location].ConditionMet =
                        this.PrimaryTracking.GeoCoordinate.GetDistanceTo(new GeoCoordinate(this.Flight?.Origin.Latitude ?? 0, this.Flight?.Origin.Longitude ?? 0)) < 5000;
                }

                if (this.TrackingStatus == TrackingStatus.Resuming)
                {
                    if (this.Flight?.Aircraft.Type.RequiresManualFuelling != true)
                    {
                        this.TrackingConditions[(int)Models.TrackingConditions.Fuel].Enabled = false;
                    }
                    else
                    {
                        this.TrackingConditions[(int)Models.TrackingConditions.Fuel].Enabled = true;
                        this.TrackingConditions[(int)Models.TrackingConditions.Fuel].Current =
                            $"{this.WeightAndBalance.FuelTotalQuantity:F1} gal, {this.WeightAndBalance.FuelTotalQuantity * 3.78541:F1} liters ▶ {this.WeightAndBalance.FuelTotalQuantity * this.Flight?.Aircraft.Type.FuelWeightPerGallon:F1} lbs, {this.WeightAndBalance.FuelTotalQuantity * this.Flight?.Aircraft.Type.FuelWeightPerGallon * 0.453592:F1} kg";

                        // Allow 5% of margin, as it can be very hard to get this right otherwise
                        this.TrackingConditions[(int)Models.TrackingConditions.Fuel].ConditionMet =
                            this.TrackingConditions[(int)Models.TrackingConditions.Fuel].AutoSet ||
                            Math.Abs(this.WeightAndBalance.FuelTotalQuantity - this.flightLoadingTempModels.FuelTanks.TotalQuantity) < this.flightLoadingTempModels.FuelTanks.TotalQuantity * 0.05; 
                    }

                    if (this.Flight?.Aircraft.Type.RequiresManualLoading != true)
                    {
                        this.TrackingConditions[(int)Models.TrackingConditions.Payload].Enabled = false;
                    }
                    else
                    {
                        this.TrackingConditions[(int)Models.TrackingConditions.Payload].Enabled = true;
                        this.TrackingConditions[(int)Models.TrackingConditions.Payload].Current = $"{this.PayloadStations.TotalWeight:F1} lbs, {this.PayloadStations.TotalWeight * 0.453592:F1} kg";
                        this.TrackingConditions[(int)Models.TrackingConditions.Payload].ConditionMet =
                            this.TrackingConditions[(int)Models.TrackingConditions.Payload].AutoSet || (this.PayloadStations.TotalWeight >= (this.Flight?.PayloadPounds ?? 0));
                    }

                    var currentLocation = $"{this.PrimaryTracking.GeoCoordinate.GetDistanceTo(this.flightLoadingTempModels?.SlewAircraftIntoPosition.GeoCoordinate ?? new GeoCoordinate(0, 0, 0)) / 1000:F2} km from resume location";
                    currentLocation += $"\r\nLatitude: {this.flightLoadingTempModels?.SlewAircraftIntoPosition.Latitude:F4}";
                    currentLocation += $"\r\nLongitude: {this.flightLoadingTempModels?.SlewAircraftIntoPosition.Longitude:F4}";
                    currentLocation += $"\r\nAltitude (AGL): {this.flightLoadingTempModels?.SlewAircraftIntoPosition.RadioHeight:F0}";

                    this.TrackingConditions[(int)Models.TrackingConditions.RealismSettings].Expected = "No unlimited fuel,\r\nCrash detection, SimRate=0 or 1";
                    this.TrackingConditions[(int)Models.TrackingConditions.RealismSettings].ConditionMet = !secondary.UnlimitedFuel && secondary.CrashDetection && ((int)this.PrimaryTracking.SimulationRate == 0 || (int)this.PrimaryTracking.SimulationRate == 1);

                    this.TrackingConditions[(int)Models.TrackingConditions.Location].Current = currentLocation;
                    this.TrackingConditions[(int)Models.TrackingConditions.Location].ConditionMet =
                        (this.PrimaryTracking.GeoCoordinate.GetDistanceTo(this.flightLoadingTempModels?.SlewAircraftIntoPosition.GeoCoordinate ?? new GeoCoordinate(0, 0, 0)) < 2000) &&
                        Math.Abs((int)(this.PrimaryTracking.RadioHeight - this.flightLoadingTempModels?.SlewAircraftIntoPosition.RadioHeight ?? -1000)) < 1000;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Process aircraft identity.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        protected void ProcessAircraftIdentity()
        {
            try
            {
                // Make sure the player didn't use the dev mode to switch the plane
                if (this.Flight != null && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking) && !this.Flight.Aircraft.Type.MatchesAircraftInSimulator())
                {
                    Debug.WriteLine("OpenSky Warning: Tracking aborted, aircraft type was changed.");
                    var assembly = Assembly.GetExecutingAssembly();
                    var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                    player.PlaySync();
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedAircraftType);
                    this.StopTracking(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error processing plane identity: " + ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Process the payload stations.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="older">
        /// The older data.
        /// </param>
        /// <param name="newer">
        /// The newer data.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void ProcessPayloadStations(PayloadStations older, PayloadStations newer)
        {
            try
            {
                if (this.Flight != null && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
                {
                    if (Math.Abs(older.TotalWeight - newer.TotalWeight) > 0.1)
                    {
                        if (Math.Abs(newer.TotalWeight - this.Flight.PayloadPounds) > this.Flight.Aircraft.Type.MaxPayloadDeltaAllowed)
                        {
                            Debug.WriteLine("OpenSky Warning: Tracking aborted, payload changed below required load.");
                            var assembly = Assembly.GetExecutingAssembly();
                            var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                            player.PlaySync();
                            SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedPayloadChange);
                            this.StopTracking(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error processing payload stations: " + ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Process the weight and balance.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// <param name="oldWB">
        /// The old weight and balance.
        /// </param>
        /// <param name="newWB">
        /// The new weight and balance.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected void ProcessWeightAndBalance(WeightAndBalance oldWB, WeightAndBalance newWB)
        {
            try
            {
                if (this.Flight != null && (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking))
                {
                    // Did the fuel go up?
                    if (newWB.FuelTotalQuantity > oldWB.FuelTotalQuantity)
                    {
                        if (newWB.FuelTotalQuantity - oldWB.FuelTotalQuantity > 0.5)
                        {
                            Debug.WriteLine("OpenSky Warning: Tracking aborted, fuel increased.");
                            var assembly = Assembly.GetExecutingAssembly();
                            var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSnegative.wav"));
                            player.PlaySync();
                            SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedFuelIncreased);
                            this.StopTracking(false);
                        }
                        else
                        {
                            Debug.WriteLine($"Small fuel jump detected: {newWB.FuelTotalQuantity - oldWB.FuelTotalQuantity} gallons");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error processing weight and balance: " + ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Process the landing analysis data (old vs new)
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ProcessLandingAnalysis()
        {
            while (!this.close)
            {
                while (this.landingAnalysisProcessingQueue.TryDequeue(out var pla))
                {
                    try
                    {
                        this.CheckForAndAnalyseLanding(pla);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error processing landing analysis: " + ex);
                    }
                }

                Thread.Sleep(500);
            }
        }

        private DateTime lastLocationUpdate=DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Process the primary tracking data (old vs new).
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ProcessPrimaryTracking()
        {
            while (!this.close)
            {
                Location newLocation = null;
                while (this.primaryTrackingProcessingQueue.TryDequeue(out var ppt))
                {
                    try
                    {
                        if (ppt.Old != null && ppt.New != null)
                        {
                            this.CheckChangesOverTime(ppt.New);
                            this.MonitorPrimarySystems(ppt);
                            this.AddPositionReport(ppt.New);
                            this.TrackFlight(ppt);

                            // Fire the location changed event?
                            if (!ppt.Old.MapLocation.Equals(ppt.New.MapLocation) || (DateTime.Now-this.lastLocationUpdate).TotalSeconds>5)
                            {
                                this.lastLocationUpdate = DateTime.Now;
                                newLocation = ppt.New.MapLocation;
                            }

                            // Are we close to landing?
                            this.SampleRates[Requests.LandingAnalysis] = this.WasAirborne && ppt.New.RadioHeight < 500 ? 25 : 500;
                            this.OnPropertyChanged(nameof(this.SampleRates));

                            // Was the sim paused/un-paused?
                            if (Math.Abs(ppt.Old.SimulationRate - ppt.New.SimulationRate) > 0.1)
                            {
                                this.OnPropertyChanged(nameof(this.IsPaused));
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Dequeued primary tracking containing at least one NULL value.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error processing primary tracking: " + ex);
                    }
                }

                if (newLocation != null)
                {
                    this.LocationChanged?.Invoke(this, newLocation);
                }

                Thread.Sleep(500);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Process the secondary tracking data (old vs new).
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ProcessSecondaryTracking()
        {
            while (!this.close)
            {
                while (this.secondaryTrackingProcessingQueue.TryDequeue(out var pst))
                {
                    try
                    {
                        if (pst.Old != null && pst.New != null)
                        {
                            this.MonitorLights(pst);
                            this.TransitionFlightPhase();
                            this.MonitorSecondarySystems(pst);
                            this.MonitorTrackingStartConditions(pst.New);
                        }
                        else
                        {
                            Debug.WriteLine("Dequeued secondary tracking containing at least one NULL value.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error processing secondary tracking: " + ex);
                    }
                }

                Thread.Sleep(500);
            }
        }
    }
}