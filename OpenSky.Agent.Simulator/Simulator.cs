// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    using TrackingEventLogEntry = OpenSky.Agent.Simulator.Models.TrackingEventLogEntry;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simulator interface.
    /// </summary>
    /// <remarks>
    /// sushi.at, 30/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public abstract partial class Simulator : INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Set to true to close the simulator client.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        protected bool close;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight loading temporary models.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        protected FlightLoadingTempModels flightLoadingTempModels;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The time the last pause started.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        protected DateTime? pauseStarted;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The total paused timespan.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        protected TimeSpan totalPaused = TimeSpan.Zero;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky service instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly OpenSkyService openSkyServiceInstance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if we are connected to the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool connected;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pause info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string pauseInfo;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Simulator"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="openSkyServiceInstance">
        /// The OpenSky service instance.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected Simulator(OpenSkyService openSkyServiceInstance)
        {
            this.openSkyServiceInstance = openSkyServiceInstance;
            this.primaryTrackingProcessingQueue = new ConcurrentQueue<ProcessPrimaryTracking>();
            this.secondaryTrackingProcessingQueue = new ConcurrentQueue<ProcessSecondaryTracking>();
            this.landingAnalysisProcessingQueue = new ConcurrentQueue<ProcessLandingAnalysis>();
            this.TrackingEventLogEntries = new ObservableCollection<TrackingEventLogEntry>();
            this.AircraftTrailLocations = new LocationCollection();
            this.SimbriefRouteLocations = new LocationCollection();
            this.LandingReports = new ObservableCollection<TouchDown>();

            // Default values and init data structures
            this.SampleRates = new ObservableConcurrentDictionary<Requests, int>
            {
                { Requests.Primary, 50 },
                { Requests.Secondary, 500 },
                { Requests.FuelTanks, 15000 },
                { Requests.PayloadStations, 15000 },
                { Requests.PlaneIdentity, 15000 },
                { Requests.WeightAndBalance, 15000 },
                { Requests.LandingAnalysis, 500 }
            };

            this.LastReceivedTimes = new ObservableConcurrentDictionary<Requests, DateTime?>();
            foreach (Requests request in Enum.GetValues(typeof(Requests)))
            {
                this.LastReceivedTimes.Add(request, null);
            }

            this.TrackingConditions = new Dictionary<int, TrackingCondition>
            {
                { (int)Agent.Simulator.Models.TrackingConditions.DateTime, new TrackingCondition { AutoSet = true } },
                { (int)Agent.Simulator.Models.TrackingConditions.Fuel, new TrackingCondition { AutoSet = true } },
                { (int)Agent.Simulator.Models.TrackingConditions.Payload, new TrackingCondition { AutoSet = true } },
                { (int)Agent.Simulator.Models.TrackingConditions.PlaneModel, new TrackingCondition() },
                { (int)Agent.Simulator.Models.TrackingConditions.RealismSettings, new TrackingCondition { Expected = "No slew, No unlimited fuel,\r\nCrash detection, SimRate=1" } },
                { (int)Agent.Simulator.Models.TrackingConditions.Location, new TrackingCondition() }
            };

            // Start our worker threads
            new Thread(this.ProcessPrimaryTracking) { Name = "Simulator.ProcessPrimaryTracking" }.Start();
            new Thread(this.ProcessSecondaryTracking) { Name = "Simulator.ProcessSecondaryTracking" }.Start();
            new Thread(this.ProcessLandingAnalysis) { Name = "Simulator.ProcessLandingAnalysis" }.Start();
        }

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
        /// Gets a value indicating whether the simulator is connected.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool Connected
        {
            get => this.connected;

            protected set
            {
                if (Equals(this.connected, value))
                {
                    return;
                }

                this.connected = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the sim is paused (proper pause, not ESC menu and definitely
        /// not active pause).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract bool IsPaused { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the pause info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PauseInfo
        {
            get => this.pauseInfo;

            protected set
            {
                if (Equals(this.pauseInfo, value))
                {
                    return;
                }

                this.pauseInfo = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the type of the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public abstract OpenSkyApi.Simulator SimulatorType { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking conditions.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<int, TrackingCondition> TrackingConditions { get; }

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
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Close()
        {
            Debug.WriteLine("SimConnect simulator interface closing down...");
            if (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
            {
                this.StopTracking(false);
            }

            this.close = true;
        }

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