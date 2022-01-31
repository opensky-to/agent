// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.AgentMSFS.Models;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simulator interface class.
    /// </summary>
    /// <remarks>
    /// sushi.at, 30/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public abstract class Simulator : INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulator instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Simulator Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft identity data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract AircraftIdentity AircraftIdentity { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the simulator is connected.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract bool Connected { get; protected set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight phase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract FlightPhase FlightPhase { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel tanks data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract FuelTanks FuelTanks { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the sim is paused (proper pause, not ESC menu and definitely
        /// not active pause).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract bool IsPaused { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the aircraft is currently turning.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract bool IsTurning { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing analysis data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract LandingAnalysis LandingAnalysis { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the landing analysis processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract int LandingAnalysisProcessingQueueLength { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the last distance position report.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract string LastDistancePositionReport { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets information describing the pause status.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract string PauseInfo { get; protected set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload stations data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract PayloadStations PayloadStations { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the primary tracking data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract PrimaryTracking PrimaryTracking { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the primary tracking processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract int PrimaryTrackingProcessingQueueLength { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the sample rates.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract ObservableConcurrentDictionary<Requests, int> SampleRates { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the secondary tracking data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract SecondaryTracking SecondaryTracking { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the secondary tracking processing queue.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract int SecondaryTrackingProcessingQueueLength { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets information about the tracking duration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract string TrackingDuration { get; protected set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking status.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract TrackingStatus TrackingStatus { get; protected set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the vertical profile.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract VerticalProfile VerticalProfile { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets information describing the warp status.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract string WarpInfo { get; protected set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the aircraft was airborne.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract bool WasAirborne { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the weight and balance data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract WeightAndBalance WeightAndBalance { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the simulator instance.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void SetSimulatorInstance(Simulator simulator)
        {
            Instance = simulator;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Close all connections and dispose the simulator client.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public abstract void Close();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Pauses the simulator (proper pause).
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="pause">
        /// True to pause, false to un-pause.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public abstract void Pause(bool pause);

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
        public abstract void RefreshModelNow(Requests request);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts flight tracking.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public abstract void StartTracking();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the aircraft registration in the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="registry">
        /// The registry to set.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public abstract void SetAircraftRegistry(string registry);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets fuel and payload to values restored from a save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public abstract void SetFuelAndPayloadFromSave();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the fuel tanks quantities in the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="newFuelTanks">
        /// The new fuel tank quantities to set.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public abstract void SetFuelTanks(FuelTanks newFuelTanks);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the payload station weights in the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="newPayloadStations">
        /// The new payload station weights to set.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public abstract void SetPayloadStations(PayloadStations newPayloadStations);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets slew to on or off.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="enable">
        /// True to enable, false to disable.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public abstract void SetSlew(bool enable);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the UTC time in the sim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="time">
        /// The new UTC time.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public abstract void SetTime(DateTime time);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Slew plane to flight position - flight object determines where, either moves plane to
        /// starting position or to last reported flight position.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public abstract void SlewPlaneToFlightPosition();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// (Optional) Name of the property.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] [CanBeNull] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}