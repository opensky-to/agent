// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Speech.Synthesis;
    using System.Threading;
    using System.Windows;

    using CTrue.FsConnect;

    using OpenSky.AgentMSFS.Properties;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.AgentMSFS.SimConnect.Helpers;
    using OpenSky.AgentMSFS.SimConnect.Structs;
    using OpenSky.AgentMSFS.Tools;
    using OpenSky.FlightLogXML;

    using TrackingEventLogEntry = OpenSky.AgentMSFS.Models.TrackingEventLogEntry;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect : INotifyPropertyChanged
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
        private FuelTanks fuelTanks;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last landing analysis info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LandingAnalysis landingAnalysis;

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
        private PayloadStations payloadStations;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last plane identity info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private PlaneIdentity planeIdentity;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last primary tracking info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private PrimaryTracking primaryTracking;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last plane systems info received from the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private SecondaryTracking secondaryTracking;

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
        private WeightAndBalance weightAndBalance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="SimConnect"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static SimConnect()
        {
            Instance = new SimConnect();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnect"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private SimConnect()
        {
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
            this.Speech = new SpeechSynthesizer();
            if (!string.IsNullOrEmpty(Settings.Default.TextToSpeechVoice))
            {
                try
                {
                    this.Speech.SelectVoice(Settings.Default.TextToSpeechVoice);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error setting text-to-speech voice from settings: " + ex);
                }
            }

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
            this.TrackingEventLogEntries = new ObservableCollection<TrackingEventLogEntry>();
            this.LandingReports = new ObservableCollection<TouchDown>();
            this.TrackingConditions = new Dictionary<int, TrackingCondition>
            {
                { (int)Models.TrackingConditions.DateTime, new TrackingCondition { AutoSet = true } },
                { (int)Models.TrackingConditions.Fuel, new TrackingCondition { AutoSet = true } },
                { (int)Models.TrackingConditions.Payload, new TrackingCondition { AutoSet = true } },
                { (int)Models.TrackingConditions.PlaneModel, new TrackingCondition() },
                { (int)Models.TrackingConditions.RealismSettings, new TrackingCondition { Expected = "No slew, No unlimited fuel,\r\nCrash detection, SimRate=1" } },
                { (int)Models.TrackingConditions.Location, new TrackingCondition() }
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
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of the SimConnect class.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public static SimConnect Instance { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether we are connected via SimConnect.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool Connected
        {
            get => this.connected;

            private set
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
        /// Gets the latest fuel tanks info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FuelTanks FuelTanks
        {
            get => this.fuelTanks;

            private set
            {
                if (Equals(this.fuelTanks, value))
                {
                    return;
                }

                this.fuelTanks = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the sim is paused (proper pause, not ESC menu and definitely not active pause).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsPaused => this.fsConnect.Paused;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest landing analysis.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LandingAnalysis LandingAnalysis
        {
            get => this.landingAnalysis;

            private set
            {
                if (Equals(this.landingAnalysis, value))
                {
                    return;
                }

                this.landingAnalysis = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last received/request date times dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableConcurrentDictionary<Requests, DateTime?> LastReceivedTimes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest payload stations info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PayloadStations PayloadStations
        {
            get => this.payloadStations;

            private set
            {
                if (Equals(this.payloadStations, value))
                {
                    return;
                }

                this.payloadStations = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest plane identity info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PlaneIdentity PlaneIdentity
        {
            get => this.planeIdentity;

            private set
            {
                if (Equals(this.planeIdentity, value))
                {
                    return;
                }

                this.planeIdentity = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest primary tracking info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PrimaryTracking PrimaryTracking
        {
            get => this.primaryTracking;

            private set
            {
                if (Equals(this.primaryTracking, value))
                {
                    return;
                }

                this.primaryTracking = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The sample rates/request dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableConcurrentDictionary<Requests, int> SampleRates { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest secondary tracking info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SecondaryTracking SecondaryTracking
        {
            get => this.secondaryTracking;

            private set
            {
                if (Equals(this.secondaryTracking, value))
                {
                    return;
                }

                this.secondaryTracking = value;
                this.OnPropertyChanged();
            }
        }

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
        /// Gets the speech synthesizer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SpeechSynthesizer Speech { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the latest weight and balance info.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public WeightAndBalance WeightAndBalance
        {
            get => this.weightAndBalance;

            private set
            {
                if (Equals(this.weightAndBalance, value))
                {
                    return;
                }

                this.weightAndBalance = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Close all connections and dispose the client.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Close()
        {
            Debug.WriteLine("SimConnect closing down...");
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
        /// -------------------------------------------------------------------------------------------------
        public void Pause(bool pause)
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
        /// Refresh SimConnect with the specified request ID now.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/03/2021.
        /// </remarks>
        /// <param name="request">
        /// The request ID.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void RefreshStructNow(Requests request)
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
            Debug.WriteLine("SimConnect is replaying map markers to listeners...");

            UpdateGUIDelegate restoreMarkers = () =>
            {
                foreach (var waypointMarker in this.simbriefWaypointMarkers)
                {
                    this.SimbriefWaypointMarkerAdded?.Invoke(this, waypointMarker);
                }

                lock (this.trackingEventMarkers)
                {
                    foreach (var trackingEventMarker in this.trackingEventMarkers)
                    {
                        this.TrackingEventMarkerAdded?.Invoke(this, trackingEventMarker);
                    }
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restoreMarkers);
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
        /// -------------------------------------------------------------------------------------------------
        public void SetFuelAndPayloadFromSave()
        {
            if (this.fsConnect.Connected)
            {
                if (this.flightLoadingTempStructs == null)
                {
                    throw new Exception("No restored fuel and payload station values found.");
                }

                Debug.WriteLine("SimConnect setting fuel and payload stations from temp structs restored from save");
                this.fsConnect.UpdateData(Requests.FuelTanks, this.flightLoadingTempStructs.FuelTanks);
                this.fsConnect.UpdateData(Requests.PayloadStations, this.flightLoadingTempStructs.PayloadStations);
                this.RefreshStructNow(Requests.FuelTanks);
                this.RefreshStructNow(Requests.PayloadStations);
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
        /// <param name="newFuelTanks">
        /// The new fuel tank quantities to set.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetFuelTanks(FuelTanks newFuelTanks)
        {
            if (this.fsConnect.Connected)
            {
                Debug.WriteLine("SimConnect setting fuel tanks");
                this.fsConnect.UpdateData(Requests.FuelTanks, newFuelTanks);
                this.RefreshStructNow(Requests.FuelTanks);
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
        /// <param name="newPayloadStations">
        /// The new payload station weights to set.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetPayloadStations(PayloadStations newPayloadStations)
        {
            if (this.fsConnect.Connected)
            {
                Debug.WriteLine("SimConnect setting payload stations");
                this.fsConnect.UpdateData(Requests.PayloadStations, newPayloadStations);
                this.RefreshStructNow(Requests.PayloadStations);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets plane registry in the sim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/04/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="registry">
        /// The registry to set.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetPlaneRegistry(string registry)
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
        /// <param name="enable">
        /// True to enable, false to disable.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetSlew(bool enable)
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
        /// Sets the text-to-speech voice.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/04/2021.
        /// </remarks>
        /// <param name="voiceName">
        /// Name of the voice.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetSpeechVoice(string voiceName)
        {
            try
            {
                this.Speech.SelectVoice(voiceName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error setting text-to-speech voice: " + ex);
                UpdateGUIDelegate showError = () => ModernWpf.MessageBox.Show(ex.Message, "Error setting voice", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.BeginInvoke(showError);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the UTC time in the sim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/03/2021.
        /// </remarks>
        /// <param name="time">
        /// The new UTC time.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetTime(DateTime time)
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
        /// -------------------------------------------------------------------------------------------------
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void SlewPlaneToFlightPosition()
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
                    if (!this.PrimaryTracking.OnGround || this.PrimaryTracking.GroundSpeed > 0)
                    {
                        throw new Exception("Plane needs to be stationary on the ground for this!");
                    }

                    if (!this.PrimaryTracking.SlewActive)
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

                    if (!this.PrimaryTracking.SlewActive)
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
        protected virtual void OnPropertyChanged([CallerMemberName][CanBeNull] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                this.Speech.SpeakAsync("Lost connection to sim, saving flight and stopping tracking session.");
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
                    this.primaryTrackingProcessingQueue.Enqueue(new ProcessPrimaryTracking { Old = this.PrimaryTracking, New = isPrimaryTracking });
                    this.OnPropertyChanged(nameof(this.PrimaryTrackingProcessingQueueLength));

                    this.PrimaryTracking = isPrimaryTracking;
                    this.LastReceivedTimes[Requests.Primary] = DateTime.UtcNow;
                }

                if (simConnectObject is SecondaryTracking isSecondaryTracking)
                {
                    this.secondaryTrackingProcessingQueue.Enqueue(new ProcessSecondaryTracking { Old = this.SecondaryTracking, New = isSecondaryTracking });
                    this.OnPropertyChanged(nameof(this.SecondaryTrackingProcessingQueueLength));

                    this.SecondaryTracking = isSecondaryTracking;
                    this.LastReceivedTimes[Requests.Secondary] = DateTime.UtcNow;
                }

                if (simConnectObject is FuelTanks isFuelTanks)
                {
                    this.FuelTanks = isFuelTanks;
                    this.LastReceivedTimes[Requests.FuelTanks] = DateTime.UtcNow;
                }

                if (simConnectObject is PayloadStations isPayloadStations)
                {
                    this.PayloadStations = isPayloadStations;
                    this.LastReceivedTimes[Requests.PayloadStations] = DateTime.UtcNow;
                }

                if (simConnectObject is PlaneIdentity isPlaneIdentity)
                {
                    this.PlaneIdentity = isPlaneIdentity;
                    this.LastReceivedTimes[Requests.PlaneIdentity] = DateTime.UtcNow;
                    new Thread(this.ProcessPlaneIdentity) { Name = "OpenSky.ProcessPlaneIdentity" }.Start();
                }

                if (simConnectObject is WeightAndBalance isWeightAndBalance)
                {
                    new Thread(
                            () => { this.ProcessWeightAndBalance(this.WeightAndBalance, isWeightAndBalance); })
                    { Name = "OpenSky.ProcessWeightAndBalance" }.Start();
                    this.WeightAndBalance = isWeightAndBalance;
                    this.LastReceivedTimes[Requests.WeightAndBalance] = DateTime.UtcNow;
                }

                if (simConnectObject is LandingAnalysis isLandingAnalysis)
                {
                    this.landingAnalysisProcessingQueue.Enqueue(new ProcessLandingAnalysis { Old = this.LandingAnalysis, New = isLandingAnalysis });
                    this.OnPropertyChanged(nameof(this.LandingAnalysisProcessingQueueLength));

                    this.LandingAnalysis = isLandingAnalysis;
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
                            this.fsConnect.Connect("OpenSky.AgentMSFS.Primary", Settings.Default.SimulatorHostName, Settings.Default.SimulatorPort, SimConnectProtocol.Ipv4);

                            // Register struct data definitions
                            this.fsConnect.RegisterDataDefinition<PrimaryTracking>(Requests.Primary, PrimaryTrackingDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<SecondaryTracking>(Requests.Secondary, SecondaryTrackingDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<FuelTanks>(Requests.FuelTanks, FuelTanksDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<PayloadStations>(Requests.PayloadStations, PayloadStationsDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<PlaneIdentity>(Requests.PlaneIdentity, PlaneIdentityDefinition.Definition);
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