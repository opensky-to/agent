// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System.Collections.Concurrent;

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
    public abstract class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulator instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Simulator Instance { get; private set; }

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

        public abstract PrimaryTracking PrimaryTracking { get; }

        public abstract SecondaryTracking SecondaryTracking { get; }

        public abstract WeightAndBalance WeightAndBalance { get; }

        public abstract AircraftIdentity AircraftIdentity { get; }

        public abstract FuelTanks FuelTanks { get; }

        public abstract PayloadStations PayloadStations { get; }

        public abstract LandingAnalysis LandingAnalysis { get; }

        public abstract bool Connected { get; }

        public abstract FlightPhase FlightPhase { get; }

        public abstract TrackingStatus TrackingStatus { get; }

        public abstract VerticalProfile VerticalProfile { get; }

        public abstract string WarpInfo { get; }

        public abstract string PauseInfo { get; }

        public abstract bool WasAirborne { get; }

        public abstract string TrackingDuration { get; }

        public abstract bool IsTurning { get; }

        public abstract int PrimaryTrackingProcessingQueueLength { get; }

        public abstract int SecondaryTrackingProcessingQueueLength { get; }

        public abstract int LandingAnalysisProcessingQueueLength { get; }

        public abstract ObservableConcurrentDictionary<Requests, int> SampleRates { get; }

        public abstract string LastDistancePositionReport { get; }

    }
}