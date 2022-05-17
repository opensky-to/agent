// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using CTrue.FsConnect;

    using JetBrains.Annotations;

    using OpenSky.Agent.SimConnectMSFS.Enums;
    using OpenSky.Agent.SimConnectMSFS.Structs;
    using OpenSky.Agent.Simulator;
    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;

    using OpenSkyApi;

    using AircraftIdentity = OpenSky.Agent.SimConnectMSFS.Structs.AircraftIdentity;
    using FuelTanks = OpenSky.Agent.SimConnectMSFS.Structs.FuelTanks;
    using LandingAnalysis = OpenSky.Agent.SimConnectMSFS.Structs.LandingAnalysis;
    using PayloadStations = OpenSky.Agent.SimConnectMSFS.Structs.PayloadStations;
    using PrimaryTracking = OpenSky.Agent.SimConnectMSFS.Structs.PrimaryTracking;
    using SecondaryTracking = OpenSky.Agent.SimConnectMSFS.Structs.SecondaryTracking;
    using Simulator = OpenSky.Agent.Simulator.Simulator;
    using SlewAircraftIntoPosition = OpenSky.Agent.SimConnectMSFS.Structs.SlewAircraftIntoPosition;
    using WeightAndBalance = OpenSky.Agent.SimConnectMSFS.Structs.WeightAndBalance;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client for Microsoft Flight Simulator 2020.
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
        /// Name of the simulator host.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly string simulatorHostName;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simulator port.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly uint simulatorPort;

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
        /// <param name="openSkyServiceInstance">
        /// The OpenSky service instance.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public SimConnect(string simulatorHostName, uint simulatorPort, OpenSkyService openSkyServiceInstance) : base(openSkyServiceInstance)
        {
            this.simulatorHostName = simulatorHostName;
            this.simulatorPort = simulatorPort;

            // Set up fsConnect client
            this.fsConnect = new FsConnect { SimConnectFileLocation = SimConnectFileLocation.Local };
            this.fsConnect.ConnectionChanged += this.FsConnectionChanged;
            this.fsConnect.FsDataReceived += this.FsDataReceived;
            this.fsConnect.PauseStateChanged += this.FsConnectPauseStateChanged;

            // Start our worker thread
            new Thread(this.ReadFromSimconnect) { Name = "SimConnect.ReadFromSim" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the simulator interface.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static string SimulatorInterfaceName => "SimConnectMSFS";

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
        /// Gets the type of the simulator.
        /// </summary>
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SimulatorType"/>
        /// -------------------------------------------------------------------------------------------------
        public override OpenSkyApi.Simulator SimulatorType => OpenSkyApi.Simulator.MSFS;

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
                this.fsConnect.UpdateData(Requests.AircraftRegistry, planeRegistry);
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
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
                if (this.flightLoadingTempModels == null)
                {
                    throw new Exception("No restored fuel and payload station values found.");
                }

                Debug.WriteLine("SimConnect setting fuel and payload stations from temp structs restored from save");
                if (this.Flight?.Aircraft.Type.RequiresManualFuelling == false)
                {
                    this.fsConnect.UpdateData(Requests.FuelTanks, this.flightLoadingTempModels.FuelTanks.ConvertBack());
                    this.RefreshModelNow(Requests.FuelTanks);
                }
                if (this.Flight?.Aircraft.Type.RequiresManualLoading == false)
                {
                    this.fsConnect.UpdateData(Requests.PayloadStations, this.flightLoadingTempModels.PayloadStations.ConvertBack());
                    this.RefreshModelNow(Requests.PayloadStations);
                }
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

                    var slewTo = this.SlewAircraftIntoPosition;
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
                    this.fsConnect.UpdateData(Requests.SlewPlaneIntoPosition, slewTo.ConvertBack());
                }
                else
                {
                    if (this.flightLoadingTempModels == null)
                    {
                        throw new Exception("No resume position available.");
                    }

                    if (!this.PrimaryTracking.SlewActive)
                    {
                        this.SetSlew(true);
                    }

                    this.fsConnect.UpdateData(Requests.SlewPlaneIntoPosition, this.flightLoadingTempModels.SlewAircraftIntoPosition.ConvertBack());
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
                    var converted = isPrimaryTracking.Convert();
                    this.primaryTrackingProcessingQueue.Enqueue(new ProcessPrimaryTracking { Old = this.PrimaryTracking, New = converted });
                    this.OnPropertyChanged(nameof(this.PrimaryTrackingProcessingQueueLength));

                    this.PrimaryTracking = converted;
                    this.LastReceivedTimes[Requests.Primary] = DateTime.UtcNow;
                }

                if (simConnectObject is SecondaryTracking isSecondaryTracking)
                {
                    var converted = isSecondaryTracking.Convert();
                    this.secondaryTrackingProcessingQueue.Enqueue(new ProcessSecondaryTracking { Old = this.SecondaryTracking, New = converted });
                    this.OnPropertyChanged(nameof(this.SecondaryTrackingProcessingQueueLength));

                    this.SecondaryTracking = converted;
                    this.LastReceivedTimes[Requests.Secondary] = DateTime.UtcNow;
                }

                if (simConnectObject is FuelTanks isFuelTanks)
                {
                    this.FuelTanks = isFuelTanks.Convert();
                    this.LastReceivedTimes[Requests.FuelTanks] = DateTime.UtcNow;
                }

                if (simConnectObject is PayloadStations isPayloadStations)
                {
                    new Thread(
                            () =>
                            {
                                var converted = isPayloadStations.Convert();
                                this.ProcessPayloadStations(this.PayloadStations, converted);
                                this.PayloadStations = converted;
                            })
                        { Name = "OpenSky.ProcessPayloadStations" }.Start();
                    this.LastReceivedTimes[Requests.PayloadStations] = DateTime.UtcNow;
                }

                if (simConnectObject is AircraftIdentity isAircraftIdentity)
                {
                    this.AircraftIdentity = isAircraftIdentity.Convert();
                    this.LastReceivedTimes[Requests.AircraftIdentity] = DateTime.UtcNow;
                    new Thread(this.ProcessAircraftIdentity) { Name = "OpenSky.ProcessAircraftIdentity" }.Start();
                }

                if (simConnectObject is WeightAndBalance isWeightAndBalance)
                {
                    new Thread(
                            () =>
                            {
                                var converted = isWeightAndBalance.Convert();
                                this.ProcessWeightAndBalance(this.WeightAndBalance, converted);
                                this.WeightAndBalance = converted;
                            })
                        { Name = "OpenSky.ProcessWeightAndBalance" }.Start();
                    this.LastReceivedTimes[Requests.WeightAndBalance] = DateTime.UtcNow;
                }

                if (simConnectObject is LandingAnalysis isLandingAnalysis)
                {
                    var converted = isLandingAnalysis.Convert();
                    this.landingAnalysisProcessingQueue.Enqueue(new ProcessLandingAnalysis { Old = this.LandingAnalysis, New = converted });
                    this.OnPropertyChanged(nameof(this.LandingAnalysisProcessingQueueLength));

                    this.LandingAnalysis = converted;
                    this.LastReceivedTimes[Requests.LandingAnalysis] = DateTime.UtcNow;
                }

                if (simConnectObject is SlewAircraftIntoPosition isSlewPlaneIntoPosition)
                {
                    this.SlewAircraftIntoPosition = isSlewPlaneIntoPosition.Convert();
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
                            this.fsConnect.Connect("OpenSky.Agent.SimConnectMSFS", this.simulatorHostName, this.simulatorPort, SimConnectProtocol.Ipv4);

                            // Register struct data definitions
                            this.fsConnect.RegisterDataDefinition<PrimaryTracking>(Requests.Primary, PrimaryTrackingDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<SecondaryTracking>(Requests.Secondary, SecondaryTrackingDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<FuelTanks>(Requests.FuelTanks, FuelTanksDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<PayloadStations>(Requests.PayloadStations, PayloadStationsDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<AircraftIdentity>(Requests.AircraftIdentity, AircraftIdentityDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<WeightAndBalance>(Requests.WeightAndBalance, WeightAndBalanceDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<LandingAnalysis>(Requests.LandingAnalysis, LandingAnalysisDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<SlewAircraftIntoPosition>(Requests.SlewPlaneIntoPosition, SlewAircraftIntoPositionDefinition.Definition);
                            this.fsConnect.RegisterDataDefinition<PlaneRegistry>(Requests.AircraftRegistry, PlaneRegistryDefinition.Definition);

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