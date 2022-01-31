// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using CTrue.FsConnect;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Agent.SimConnectMSFS.Helpers;
    using OpenSky.Agent.SimConnectMSFS.Structs;
    using OpenSky.Agent.Simulator;
    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.AgentMSFS.SimConnect.Helpers;
    using OpenSky.AgentMSFS.SimConnect.Structs;
    using OpenSky.FlightLogXML;

    using AircraftIdentity = Structs.AircraftIdentity;
    using FuelTanks = Structs.FuelTanks;
    using LandingAnalysis = Structs.LandingAnalysis;
    using PayloadStations = Structs.PayloadStations;
    using PrimaryTracking = Structs.PrimaryTracking;
    using SecondaryTracking = Structs.SecondaryTracking;
    using WeightAndBalance = Structs.WeightAndBalance;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect : Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight simulator simconnect wrapper.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly FsConnect fsConnect;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Set to true to close the client.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool close;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if we are connected via SimConnect.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool connected;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last fuel tanks info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FuelTanks fuelTanksStruct;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last landing analysis info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LandingAnalysis landingAnalysisStruct;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The time the last pause started.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime? pauseStarted;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last payload stations info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private PayloadStations payloadStationsStruct;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last plane identity info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftIdentity aircraftIdentityStruct;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last primary tracking info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private PrimaryTracking primaryTrackingStruct;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last plane systems info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private SecondaryTracking secondaryTrackingStruct;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last slew plane into position info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private SlewPlaneIntoPosition slewPlaneIntoPosition;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The total paused timespan.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private TimeSpan totalPaused = TimeSpan.Zero;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last weight and balance info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private WeightAndBalance weightAndBalanceStruct;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnect"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// <param name="simulatorHostName">
        /// Name of the simulator host.
        /// </param>
        /// <param name="simulatorPort">
        /// The simulator port.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public SimConnect(string simulatorHostName, uint simulatorPort)
        {
            this.simulatorHostName = simulatorHostName;
            this.simulatorPort = simulatorPort;

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

            this.primaryTrackingProcessingQueue = new ConcurrentQueue<ProcessPrimaryTracking>();
            this.secondaryTrackingProcessingQueue = new ConcurrentQueue<ProcessSecondaryTracking>();
            this.landingAnalysisProcessingQueue = new ConcurrentQueue<ProcessLandingAnalysis>();
            this.AircraftTrailLocations = new LocationCollection();
            this.SimbriefRouteLocations = new LocationCollection();
            this.TrackingEventLogEntries = new ObservableCollection<Agent.Simulator.Models.TrackingEventLogEntry>();
            this.LandingReports = new ObservableCollection<TouchDown>();
            this.TrackingConditions = new Dictionary<int, TrackingCondition>
            {
                { (int)Agent.Simulator.Models.TrackingConditions.DateTime, new TrackingCondition { AutoSet = true } },
                { (int)Agent.Simulator.Models.TrackingConditions.Fuel, new TrackingCondition { AutoSet = true } },
                { (int)Agent.Simulator.Models.TrackingConditions.Payload, new TrackingCondition { AutoSet = true } },
                { (int)Agent.Simulator.Models.TrackingConditions.PlaneModel, new TrackingCondition() },
                { (int)Agent.Simulator.Models.TrackingConditions.RealismSettings, new TrackingCondition { Expected = "No slew, No unlimited fuel,\r\nCrash detection, SimRate=1" } },
                { (int)Agent.Simulator.Models.TrackingConditions.Location, new TrackingCondition() }
            };

            // Set up fsConnect client
            this.fsConnect = new FsConnect { SimConnectFileLocation = SimConnectFileLocation.Local };
            this.fsConnect.ConnectionChanged += this.FsConnectionChanged;
            this.fsConnect.FsDataReceived += this.FsDataReceived;
            this.fsConnect.PauseStateChanged += this.FsConnectPauseStateChanged;

            // Start our worker threads
            new Thread(this.ReadFromSimconnect) { Name = "SimConnect.ReadFromSim" }.Start();
            new Thread(this.ProcessPrimaryTracking) { Name = "SimConnect.ProcessPrimaryTracking" }.Start();
            new Thread(this.ProcessSecondaryTracking) { Name = "SimConnect.ProcessSecondaryTracking" }.Start();
            new Thread(this.ProcessLandingAnalysis) { Name = "SimConnect.ProcessLandingAnalysis" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the simulator is connected.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.Connected"/>
        /// -------------------------------------------------------------------------------------------------
        public override bool Connected
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
        /// Gets the latest fuel tanks info struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FuelTanks FuelTanksStruct
        {
            get => this.fuelTanksStruct;

            set
            {
                if (Equals(this.fuelTanksStruct, value))
                {
                    return;
                }

                this.fuelTanksStruct = value;
                this.OnPropertyChanged(nameof(this.FuelTanks));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel tanks data.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.FuelTanks"/>
        /// -------------------------------------------------------------------------------------------------
        public override Agent.Simulator.Models.FuelTanks FuelTanks => this.FuelTanksStruct.Convert();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the sim is paused (proper pause, not ESC menu and definitely
        /// not active pause).
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.IsPaused"/>
        /// -------------------------------------------------------------------------------------------------
        public override bool IsPaused => this.fsConnect.Paused;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest landing analysis struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LandingAnalysis LandingAnalysisStruct
        {
            get => this.landingAnalysisStruct;

            set
            {
                if (Equals(this.landingAnalysisStruct, value))
                {
                    return;
                }

                this.landingAnalysisStruct = value;
                this.OnPropertyChanged(nameof(this.LandingAnalysis));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing analysis data.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.LandingAnalysis"/>
        /// -------------------------------------------------------------------------------------------------
        public override Agent.Simulator.Models.LandingAnalysis LandingAnalysis => this.LandingAnalysisStruct.Convert();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last received/request date times dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableConcurrentDictionary<Requests, DateTime?> LastReceivedTimes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest payload stations info struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private PayloadStations PayloadStationsStruct
        {
            get => this.payloadStationsStruct;

            set
            {
                if (Equals(this.payloadStationsStruct, value))
                {
                    return;
                }

                this.payloadStationsStruct = value;
                this.OnPropertyChanged(nameof(this.PayloadStations));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload stations data.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.PayloadStations"/>
        /// -------------------------------------------------------------------------------------------------
        public override Agent.Simulator.Models.PayloadStations PayloadStations => this.PayloadStationsStruct.Convert();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest aircraft identity info struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftIdentity AircraftIdentityStruct
        {
            get => this.aircraftIdentityStruct;

            set
            {
                if (Equals(this.aircraftIdentityStruct, value))
                {
                    return;
                }

                this.aircraftIdentityStruct = value;
                this.OnPropertyChanged(nameof(this.AircraftIdentity));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft identity data.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.AircraftIdentity"/>
        /// -------------------------------------------------------------------------------------------------
        public override Agent.Simulator.Models.AircraftIdentity AircraftIdentity => this.AircraftIdentityStruct.Convert();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest primary tracking info struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private PrimaryTracking PrimaryTrackingStruct
        {
            get => this.primaryTrackingStruct;

            set
            {
                if (Equals(this.primaryTrackingStruct, value))
                {
                    return;
                }

                this.primaryTrackingStruct = value;
                this.OnPropertyChanged(nameof(this.PrimaryTracking));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the primary tracking data.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.PrimaryTracking"/>
        /// -------------------------------------------------------------------------------------------------
        public override Agent.Simulator.Models.PrimaryTracking PrimaryTracking => this.PrimaryTrackingStruct.Convert();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The sample rates/request dictionary.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.SampleRates"/>
        /// -------------------------------------------------------------------------------------------------
        public override ObservableConcurrentDictionary<Requests, int> SampleRates { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest secondary tracking info struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private SecondaryTracking SecondaryTrackingStruct
        {
            get => this.secondaryTrackingStruct;

            set
            {
                if (Equals(this.secondaryTrackingStruct, value))
                {
                    return;
                }

                this.secondaryTrackingStruct = value;
                this.OnPropertyChanged(nameof(this.SecondaryTracking));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the secondary tracking data.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.SecondaryTracking"/>
        /// -------------------------------------------------------------------------------------------------
        public override Agent.Simulator.Models.SecondaryTracking SecondaryTracking => this.SecondaryTrackingStruct.Convert();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the latest slew plane into position.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SlewPlaneIntoPosition SlewPlaneIntoPosition
        {
            get => this.slewPlaneIntoPosition;

            set
            {
                if (Equals(this.slewPlaneIntoPosition, value))
                {
                    return;
                }

                this.slewPlaneIntoPosition = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest weight and balance info struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private WeightAndBalance WeightAndBalanceStruct
        {
            get => this.weightAndBalanceStruct;

            set
            {
                if (Equals(this.weightAndBalanceStruct, value))
                {
                    return;
                }

                this.weightAndBalanceStruct = value;
                this.OnPropertyChanged(nameof(this.WeightAndBalance));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the weight and balance data.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.WeightAndBalance"/>
        /// -------------------------------------------------------------------------------------------------
        public override Agent.Simulator.Models.WeightAndBalance WeightAndBalance => this.WeightAndBalanceStruct.Convert();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Close all connections and dispose the simulator client.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.Close()"/>
        /// -------------------------------------------------------------------------------------------------
        public override void Close()
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
        /// sushi.at, 21/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="pause">
        /// True to pause, false to un-pause.
        /// </param>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.Pause(bool)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void Pause(bool pause)
        {
            if (this.fsConnect.Connected)
            {
                Debug.WriteLine($"SimConnect pausing sim...{pause}");
                this.fsConnect.Pause(pause);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

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
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.RefreshModelNow(Requests)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void RefreshModelNow(Requests request)
        {
            this.LastReceivedTimes[request] = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Replay simbrief waypoint and tracking event markers (new tracking view was opened).
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void ReplayMapMarkers()
        {
            // todo RESTORE THIS
            //Debug.WriteLine("SimConnect is replaying map markers to listeners...");

            //UpdateGUIDelegate restoreMarkers = () =>
            //{
            //    foreach (var waypointMarker in this.simbriefWaypointMarkers)
            //    {
            //        this.SimbriefWaypointMarkerAdded?.Invoke(this, waypointMarker);
            //    }

            //    lock (this.trackingEventMarkers)
            //    {
            //        foreach (var trackingEventMarker in this.trackingEventMarkers)
            //        {
            //            this.TrackingEventMarkerAdded?.Invoke(this, trackingEventMarker);
            //        }
            //    }
            //};
            //Application.Current.Dispatcher.BeginInvoke(restoreMarkers);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets fuel and payload to values restored from a save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/04/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.SetFuelAndPayloadFromSave()"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetFuelAndPayloadFromSave()
        {
            if (this.fsConnect.Connected)
            {
                if (this.flightLoadingTempStructs == null)
                {
                    throw new Exception("No restored fuel and payload station values found.");
                }

                Debug.WriteLine("SimConnect setting fuel and payload stations from temp structs restored from save");
                this.fsConnect.UpdateData<Agent.Simulator.Models.FuelTanks>(Requests.FuelTanks, this.flightLoadingTempStructs.FuelTanks);
                this.fsConnect.UpdateData<Agent.Simulator.Models.PayloadStations>(Requests.PayloadStations, this.flightLoadingTempStructs.PayloadStations);
                this.RefreshModelNow(Requests.FuelTanks);
                this.RefreshModelNow(Requests.PayloadStations);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the fuel tanks quantities in the sim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="newFuelTanks">
        /// The new fuel tank quantities to set.
        /// </param>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.SetFuelTanks(Agent.Simulator.Models.FuelTanks)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetFuelTanks(Agent.Simulator.Models.FuelTanks newFuelTanks)
        {
            if (this.fsConnect.Connected)
            {
                Debug.WriteLine("SimConnect setting fuel tanks");
                this.fsConnect.UpdateData(Requests.FuelTanks, newFuelTanks.ConvertBack());
                this.RefreshModelNow(Requests.FuelTanks);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the payload station weights in the sim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="newPayloadStations">
        /// The new payload station weights to set.
        /// </param>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.SetPayloadStations(Agent.Simulator.Models.PayloadStations)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetPayloadStations(Agent.Simulator.Models.PayloadStations newPayloadStations)
        {
            if (this.fsConnect.Connected)
            {
                Debug.WriteLine("SimConnect setting payload stations");
                this.fsConnect.UpdateData(Requests.PayloadStations, newPayloadStations.ConvertBack());
                this.RefreshModelNow(Requests.PayloadStations);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the aircraft registration in the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="registry">
        /// The registry to set.
        /// </param>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.SetAircraftRegistry(string)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetAircraftRegistry(string registry)
        {
            if (string.IsNullOrEmpty(registry))
            {
                return;
            }

            if (this.fsConnect.Connected)
            {
                var planeRegistry = new PlaneRegistry { AtcID = registry };
                this.fsConnect.UpdateData(Requests.PlaneRegistry, planeRegistry);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets slew to on or off.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="enable">
        /// True to enable, false to disable.
        /// </param>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.SetSlew(bool)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetSlew(bool enable)
        {
            if (this.fsConnect.Connected)
            {
                this.fsConnect.TransmitClientEvent(enable ? ClientEvents.SlewOn : ClientEvents.SlewOff, 0, ClientEvents.Slew);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the UTC time in the sim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="time">
        /// The new UTC time.
        /// </param>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.SetTime(DateTime)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetTime(DateTime time)
        {
            if (this.fsConnect.Connected)
            {
                this.fsConnect.TransmitClientEvent(ClientEvents.SetZuluYears, (uint)time.Year, ClientEvents.SetTime);
                this.fsConnect.TransmitClientEvent(ClientEvents.SetZuluDays, (uint)time.DayOfYear, ClientEvents.SetTime);
                this.fsConnect.TransmitClientEvent(ClientEvents.SetZuluHours, (uint)time.Hour, ClientEvents.SetTime);
                this.fsConnect.TransmitClientEvent(ClientEvents.SetZuluMinute, (uint)time.Minute, ClientEvents.SetTime);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Slew plane to flight position - flight object determines where, either moves plane to
        /// starting position or to last reported flight position.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.SlewPlaneToFlightPosition()"/>
        /// -------------------------------------------------------------------------------------------------
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void SlewPlaneToFlightPosition()
        {
            if (this.fsConnect.Connected)
            {
                if (this.Flight == null)
                {
                    throw new Exception("Not currently tracking a flight!");
                }

                // Slew into starting position at origin airport?
                if (!this.Flight.Resume)
                {
                    Debug.WriteLine("Requesting current SlewPlaneIntoPosition struct");
                    this.LastReceivedTimes.Remove(Requests.SlewPlaneIntoPosition);
                    this.fsConnect.RequestData(Requests.SlewPlaneIntoPosition, Requests.SlewPlaneIntoPosition);
                    var waited = 0;
                    while (!this.LastReceivedTimes.ContainsKey(Requests.SlewPlaneIntoPosition) && waited < 3000)
                    {
                        Thread.Sleep(50);
                        waited += 50;
                    }

                    if (!this.LastReceivedTimes.ContainsKey(Requests.SlewPlaneIntoPosition))
                    {
                        throw new Exception("Timeout waiting for sim data response!");
                    }

                    var slewTo = this.SlewPlaneIntoPosition;
                    if (!this.PrimaryTrackingStruct.OnGround || this.PrimaryTrackingStruct.GroundSpeed > 0)
                    {
                        throw new Exception("Plane needs to be stationary on the ground for this!");
                    }

                    if (!this.PrimaryTrackingStruct.SlewActive)
                    {
                        this.SetSlew(true);
                    }

                    slewTo.Latitude = this.Flight.Origin.Latitude;
                    slewTo.Longitude = this.Flight.Origin.Longitude;
                    this.fsConnect.UpdateData(Requests.SlewPlaneIntoPosition, slewTo);
                }
                else
                {
                    if (this.flightLoadingTempStructs == null)
                    {
                        throw new Exception("No resume position available.");
                    }

                    if (!this.PrimaryTrackingStruct.SlewActive)
                    {
                        this.SetSlew(true);
                    }

                    this.fsConnect.UpdateData(Requests.SlewPlaneIntoPosition, this.flightLoadingTempStructs.SlewPlaneIntoPosition);
                }
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// File system connection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FsConnectionChanged([CanBeNull] object sender, bool e)
        {
            Debug.WriteLine($"SimConnect fsConnect connection status changed: {e}");
            this.Connected = this.fsConnect.Connected;

            if (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
            {
                Debug.WriteLine("Lost connection to sim, saving flight and stopping tracking session...");
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.LostSimSavingStopTracking);
                this.StopTracking(true);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// SimConnect pause state changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Pause state changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FsConnectPauseStateChanged(object sender, PauseStateChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(this.IsPaused));

            Debug.WriteLine($"SimConnect fsConnect pause state changed: {e.Paused}");
            if (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
            {
                if (e.Paused)
                {
                    this.pauseStarted = DateTime.UtcNow;
                }
                else
                {
                    if (this.pauseStarted.HasValue)
                    {
                        this.totalPaused += DateTime.UtcNow - this.pauseStarted.Value;
                        this.pauseStarted = null;
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// FsConnect client returning data we requested.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// SimConnect data received event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void FsDataReceived(object sender, FsDataReceivedEventArgs e)
        {
            foreach (var simConnectObject in e.Data)
            {
                if (simConnectObject is PrimaryTracking isPrimaryTracking)
                {
                    this.primaryTrackingProcessingQueue.Enqueue(new ProcessPrimaryTracking { Old = this.PrimaryTrackingStruct, New = isPrimaryTracking });
                    this.OnPropertyChanged(nameof(SimConnect.PrimaryTrackingProcessingQueueLength));

                    this.PrimaryTrackingStruct = isPrimaryTracking;
                    this.LastReceivedTimes[Requests.Primary] = DateTime.UtcNow;
                }

                if (simConnectObject is SecondaryTracking isSecondaryTracking)
                {
                    this.secondaryTrackingProcessingQueue.Enqueue(new ProcessSecondaryTracking { Old = this.SecondaryTrackingStruct, New = isSecondaryTracking });
                    this.OnPropertyChanged(nameof(SimConnect.SecondaryTrackingProcessingQueueLength));

                    this.SecondaryTrackingStruct = isSecondaryTracking;
                    this.LastReceivedTimes[Requests.Secondary] = DateTime.UtcNow;
                }

                if (simConnectObject is FuelTanks isFuelTanks)
                {
                    this.FuelTanksStruct = isFuelTanks;
                    this.LastReceivedTimes[Requests.FuelTanks] = DateTime.UtcNow;
                }

                if (simConnectObject is PayloadStations isPayloadStations)
                {
                    new Thread(
                        () =>
                        {
                            this.ProcessPayloadStations(this.PayloadStationsStruct, isPayloadStations);
                            this.PayloadStationsStruct = isPayloadStations;
                        })
                    { Name = "OpenSky.ProcessPayloadStations" }.Start();
                    this.LastReceivedTimes[Requests.PayloadStations] = DateTime.UtcNow;
                }

                if (simConnectObject is AircraftIdentity isPlaneIdentity)
                {
                    this.AircraftIdentityStruct = isPlaneIdentity;
                    this.LastReceivedTimes[Requests.PlaneIdentity] = DateTime.UtcNow;
                    new Thread(this.ProcessPlaneIdentity) { Name = "OpenSky.ProcessPlaneIdentity" }.Start();
                }

                if (simConnectObject is WeightAndBalance isWeightAndBalance)
                {
                    new Thread(
                            () =>
                            {
                                this.ProcessWeightAndBalance(this.WeightAndBalanceStruct, isWeightAndBalance);
                                this.WeightAndBalanceStruct = isWeightAndBalance;
                            })
                    { Name = "OpenSky.ProcessWeightAndBalance" }.Start();
                    this.LastReceivedTimes[Requests.WeightAndBalance] = DateTime.UtcNow;
                }

                if (simConnectObject is LandingAnalysis isLandingAnalysis)
                {
                    this.landingAnalysisProcessingQueue.Enqueue(new ProcessLandingAnalysis { Old = this.LandingAnalysisStruct, New = isLandingAnalysis });
                    this.OnPropertyChanged(nameof(SimConnect.LandingAnalysisProcessingQueueLength));

                    this.LandingAnalysisStruct = isLandingAnalysis;
                    this.LastReceivedTimes[Requests.LandingAnalysis] = DateTime.UtcNow;
                }

                if (simConnectObject is SlewPlaneIntoPosition isSlewPlaneIntoPosition)
                {
                    this.SlewPlaneIntoPosition = isSlewPlaneIntoPosition;
                    this.LastReceivedTimes[Requests.SlewPlaneIntoPosition] = DateTime.UtcNow;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Name of the simulator host.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string simulatorHostName;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simulator port.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private uint simulatorPort;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Read from SimConnect on background thread.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ReadFromSimconnect()
        {
            var veryFirstConnectError = true;
            while (!this.close)
            {
                try
                {
                    if (!this.fsConnect.Connected)
                    {
                        try
                        {
                            this.fsConnect.Connect("OpenSky.Agent.SimConnectMSFS.Primary", this.simulatorHostName, this.simulatorPort, SimConnectProtocol.Ipv4);

                            // Register struct data definitions
                            this.fsConnect.RegisterDataDefinition<PrimaryTracking>(Requests.Primary, PrimaryTrackingDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<SecondaryTracking>(Requests.Secondary, SecondaryTrackingDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<FuelTanks>(Requests.FuelTanks, FuelTanksDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<PayloadStations>(Requests.PayloadStations, PayloadStationsDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<AircraftIdentity>(Requests.PlaneIdentity, AircraftIdentityDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<WeightAndBalance>(Requests.WeightAndBalance, WeightAndBalanceDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<LandingAnalysis>(Requests.LandingAnalysis, LandingAnalysisDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<SlewPlaneIntoPosition>(Requests.SlewPlaneIntoPosition, SlewPlaneIntoPositionDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<PlaneRegistry>(Requests.PlaneRegistry, PlaneRegistryDefinition.Definition);

                            // Register client events
                            this.fsConnect.MapClientEventToSimEvent(ClientEvents.SetTime, ClientEvents.SetZuluYears, "ZULU_YEAR_SET");
                            this.fsConnect.MapClientEventToSimEvent(ClientEvents.SetTime, ClientEvents.SetZuluDays, "ZULU_DAY_SET");
                            this.fsConnect.MapClientEventToSimEvent(ClientEvents.SetTime, ClientEvents.SetZuluHours, "ZULU_HOURS_SET");
                            this.fsConnect.MapClientEventToSimEvent(ClientEvents.SetTime, ClientEvents.SetZuluMinute, "ZULU_MINUTES_SET");
                            this.fsConnect.MapClientEventToSimEvent(ClientEvents.Slew, ClientEvents.SlewOn, "SLEW_ON");
                            this.fsConnect.MapClientEventToSimEvent(ClientEvents.Slew, ClientEvents.SlewOff, "SLEW_OFF");

                            // Wait a bit before starting to pull data
                            Thread.Sleep(2000);
                        }
                        catch (Exception ex)
                        {
                            // After the first one ignore, and don't log connection errors, they only fill up the logs
                            if (veryFirstConnectError)
                            {
                                veryFirstConnectError = false;
                                Debug.WriteLine("Error connecting to sim: " + ex);
                            }

                        }
                    }

                    if (this.fsConnect.Connected)
                    {
                        veryFirstConnectError = true;
                        this.fsConnect.RequestData(Requests.Primary, Requests.Primary);

                        foreach (Requests request in Enum.GetValues(typeof(Requests)))
                        {
                            if (this.SampleRates.ContainsKey(request))
                            {
                                var lastTime = this.LastReceivedTimes[request];
                                if (request != Requests.Primary && (!lastTime.HasValue || (DateTime.UtcNow - lastTime.Value).TotalMilliseconds > this.SampleRates[request]))
                                {
                                    this.fsConnect.RequestData(request, request);
                                }
                            }
                        }

                        Thread.Sleep(Math.Min(this.SampleRates[Requests.Primary], this.SampleRates[Requests.LandingAnalysis]));
                    }
                    else
                    {
                        SleepScheduler.SleepFor(TimeSpan.FromSeconds(this.Flight == null ? 30 : 5));
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    SleepScheduler.SleepFor(TimeSpan.FromSeconds(this.Flight == null ? 30 : 5));
                }
            }
        }
    }
}