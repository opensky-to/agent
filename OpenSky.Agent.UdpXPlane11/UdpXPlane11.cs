// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UdpXPlane11.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.Agent.UdpXPlane11.Models;

    using OpenSkyApi;

    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    using Simulator = Simulator.Simulator;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// UDP client for X-Plane 11.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/02/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class UdpXPlane11 : Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) the simulator IP address.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly string simulatorIPAddress;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) the simulator port.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly uint simulatorPort;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The UDP xplane connector.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private XPlaneConnector connector;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpXPlane11"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/02/2022.
        /// </remarks>
        /// <param name="simulatorIPAddress">
        /// Name of the simulator IP address.
        /// </param>
        /// <param name="simulatorPort">
        /// The simulator port.
        /// </param>
        /// <param name="openSkyServiceInstance">
        /// The OpenSky service instance.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public UdpXPlane11(string simulatorIPAddress, uint simulatorPort, OpenSkyService openSkyServiceInstance) : base(openSkyServiceInstance)
        {
            this.simulatorIPAddress = simulatorIPAddress;
            this.simulatorPort = simulatorPort;

            // Initialize empty data structures, to prevent NullReferenceExceptions
            this.PrimaryTracking = new PrimaryTracking();
            this.SecondaryTracking = new SecondaryTracking();
            this.FuelTanks = new FuelTanks();
            this.PayloadStations = new PayloadStations();
            this.AircraftIdentity = new AircraftIdentity();
            this.WeightAndBalance = new WeightAndBalance();

            // Start our worker thread
            new Thread(this.ReadFromXPlane) { Name = "UdpXPlane11.ReadFromXPlane" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the simulator interface.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static string SimulatorInterfaceName => "UdpXPlane11";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the sim is paused (proper pause, not ESC menu and definitely
        /// not active pause).
        /// </summary>
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.IsPaused"/>
        /// -------------------------------------------------------------------------------------------------
        public override bool IsPaused => this.PrimaryTracking?.SimulationRate == 0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the type of the simulator.
        /// </summary>
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SimulatorType"/>
        /// -------------------------------------------------------------------------------------------------
        public override OpenSkyApi.Simulator SimulatorType => OpenSkyApi.Simulator.XPlane11;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Pauses the simulator, in XPlane 11 that means setting the simrate to 0.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="pause">
        /// True to pause, false to un-pause.
        /// </param>
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.Pause(bool)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void Pause(bool pause)
        {
            this.connector?.SetDataRefValue(DataRefs.TimeSimSpeed, pause ? 0 : 1);
        }

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
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SetAircraftRegistry(string)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetAircraftRegistry(string registry)
        {
            // Doesn't work in XP11 (comes from livery, the dataref is write-able but has no effect)
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets fuel and payload to values restored from a save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SetFuelAndPayloadFromSave()"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetFuelAndPayloadFromSave()
        {
            if (this.Connected)
            {
                if (this.flightLoadingTempModels == null)
                {
                    throw new Exception("No restored fuel and payload station values found.");
                }

                Debug.WriteLine("UdpXplane11 setting fuel and payload stations from temp structs restored from save");
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[0]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankLeftMainQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[1]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankRightMainQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[2]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankLeftTipQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[3]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankRightTipQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[4]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankLeftAuxQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[5]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankRightAuxQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[6]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankCenterQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[7]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankCenter2Quantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[8]", (float)this.flightLoadingTempModels.FuelTanks.FuelTankCenter3Quantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
                this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFixed, (float)this.flightLoadingTempModels.PayloadStations.Weight1 / 2.205f);

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
        /// Sets the fuel tanks quantities in the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="newFuelTanks">
        /// The new fuel tank quantities to set.
        /// </param>
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SetFuelTanks(FuelTanks)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetFuelTanks(FuelTanks newFuelTanks)
        {
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[0]", (float)newFuelTanks.FuelTankLeftMainQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[1]", (float)newFuelTanks.FuelTankRightMainQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[2]", (float)newFuelTanks.FuelTankLeftTipQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[3]", (float)newFuelTanks.FuelTankRightTipQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[4]", (float)newFuelTanks.FuelTankLeftAuxQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[5]", (float)newFuelTanks.FuelTankRightAuxQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[6]", (float)newFuelTanks.FuelTankCenterQuantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[7]", (float)newFuelTanks.FuelTankCenter2Quantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFuel.DataRef + "[8]", (float)newFuelTanks.FuelTankCenter3Quantity / 2.205f * (float)this.WeightAndBalance.FuelWeightPerGallon);
        }

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
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SetPayloadStations(PayloadStations)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetPayloadStations(PayloadStations newPayloadStations)
        {
            this.connector?.SetDataRefValue(DataRefs.FlightmodelWeightMFixed, (float)newPayloadStations.Weight1 / 2.205f);
        }

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
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SetSlew(bool)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetSlew(bool enable)
        {
            // Slew does not exist in XPlane11, so we are going to use pause instead
            this.connector?.SetDataRefValue(DataRefs.TimeSimSpeed, enable ? 0 : 1);
        }

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
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SetTime(DateTime)"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SetTime(DateTime time)
        {
            this.connector?.SetDataRefValue(DataRefs.Cockpit2ClockTimerCurrentDay, time.Day);
            this.connector?.SetDataRefValue(DataRefs.Cockpit2ClockTimerCurrentMonth, time.Month);
            this.connector?.SetDataRefValue(DataRefs.TimeZuluTimeSec, time.Hour * 60 * 60 + time.Minute * 60 + time.Second);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Slew plane to flight position - flight object determines where, either moves plane to
        /// starting position or to last reported flight position.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <seealso cref="OpenSky.Agent.Simulator.Simulator.SlewPlaneToFlightPosition()"/>
        /// -------------------------------------------------------------------------------------------------
        public override void SlewPlaneToFlightPosition()
        {
            if (this.Connected)
            {
                if (this.Flight == null)
                {
                    throw new Exception("Not currently tracking a flight!");
                }

                // Slew into starting position at origin airport?
                if (!this.Flight.Resume)
                {
                    throw new Exception("Slew to Origin is currently not supported for X-Plane 11. Please use the map in the simulator.");

                    //if (!this.PrimaryTracking.OnGround || this.PrimaryTracking.GroundSpeed > 1)
                    //{
                    //    throw new Exception("Plane needs to be stationary on the ground for this!");
                    //}

                    //if (!this.PrimaryTracking.SlewActive)
                    //{
                    //    this.SetSlew(true);
                    //}

                    //var dg = new XPDatagram();
                    //dg.Add("POSI");
                    //dg.Bytes.Add(0); // aircraft index, 0 is the player's

                    //// Long, Lat and Alt are doubles
                    //{
                    //    var doubleBytes = BitConverter.GetBytes(this.Flight?.Origin.Latitude ?? 0);
                    //    dg.Bytes.AddRange(BitConverter.IsLittleEndian ? doubleBytes : doubleBytes.Reverse());
                    //}
                    //{
                    //    var doubleBytes = BitConverter.GetBytes(this.Flight?.Origin.Longitude ?? 0);
                    //    dg.Bytes.AddRange(BitConverter.IsLittleEndian ? doubleBytes : doubleBytes.Reverse());
                    //}
                    //{
                    //    var doubleBytes = BitConverter.GetBytes(-998d); // TODO what would you put there without knowing the ground elevation at the target coordinates?
                    //    dg.Bytes.AddRange(BitConverter.IsLittleEndian ? doubleBytes : doubleBytes.Reverse());
                    //}

                    ////dg.Add((float)(this.Flight?.Origin.Latitude ?? 0));
                    ////dg.Add((float)(this.Flight?.Origin.Longitude ?? 0));
                    ////dg.Add(-998f);

                    //dg.Add(-998f);
                    //dg.Add(-998f);
                    //dg.Add(-998f);
                    //dg.Add(-998f);
                    ////dg.FillTo(509);

                    //var client = new UdpClient();
                    //client.Connect(this.simulatorIPAddress, 49009);
                    //client.Send(dg.Get(), dg.Len);
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

                    var dg = new XPDatagram();
                    dg.Add("POSI");
                    dg.Bytes.Add(0); // aircraft index, 0 is the player's

                    // Long, Lat and Alt are doubles
                    {
                        var doubleBytes = BitConverter.GetBytes(this.flightLoadingTempModels.SlewAircraftIntoPosition.Latitude);
                        dg.Bytes.AddRange(BitConverter.IsLittleEndian ? doubleBytes : doubleBytes.Reverse());
                    }
                    {
                        var doubleBytes = BitConverter.GetBytes(this.flightLoadingTempModels.SlewAircraftIntoPosition.Longitude);
                        dg.Bytes.AddRange(BitConverter.IsLittleEndian ? doubleBytes : doubleBytes.Reverse());
                    }
                    {
                        var doubleBytes = BitConverter.GetBytes(this.flightLoadingTempModels.SlewAircraftIntoPosition.Altitude / 3.281);
                        dg.Bytes.AddRange(BitConverter.IsLittleEndian ? doubleBytes : doubleBytes.Reverse());
                    }

                    dg.Add((float)this.flightLoadingTempModels.SlewAircraftIntoPosition.PitchAngle);
                    dg.Add((float)this.flightLoadingTempModels.SlewAircraftIntoPosition.BankAngle);
                    dg.Add((float)this.flightLoadingTempModels.SlewAircraftIntoPosition.Heading);
                    dg.Add(-998f); // Don't change gear

                    var client = new UdpClient();
                    client.Connect(this.simulatorIPAddress, 49009);
                    client.Send(dg.Get(), dg.Len);
                }
            }
            else
            {
                throw new Exception("Not connected to sim!");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Read from X-Plane 11 UDP connector on background thread.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ReadFromXPlane()
        {
            try
            {
                // Create dataref handlers
                var primaryTracking = new PrimaryTrackingDataRef();
                var secondaryTracking = new SecondaryTrackingDataRef();
                var aircraftIdentity = new AircraftIdentityDataRef();
                var fuelTanks = new FuelTanksDataRef();
                var payloadStations = new PayloadStationsDataRef();
                var weightAndBalance = new WeightAndBalanceDataRef();
                var landingAnalysis = new LandingAnalysisDataRef();

                while (!this.close)
                {
                    try
                    {
                        if ((DateTime.Now - (this.connector?.LastReceive ?? DateTime.MinValue)) > TimeSpan.FromSeconds(10))
                        {
                            this.Connected = false;
                            try
                            {
                                this.connector?.Stop();
                            }
                            catch
                            {
                                // Ignore
                            }

                            this.connector = new XPlaneConnector(this.simulatorIPAddress, (int)this.simulatorPort);
                            primaryTracking.RegisterWithConnector(this.connector, this.SampleRates[Requests.Primary]);
                            secondaryTracking.RegisterWithConnector(this.connector, this.SampleRates[Requests.Secondary]);
                            aircraftIdentity.RegisterWithConnector(this.connector, this.SampleRates[Requests.AircraftIdentity]);
                            fuelTanks.RegisterWithConnector(this.connector, this.SampleRates[Requests.FuelTanks]);
                            payloadStations.RegisterWithConnector(this.connector, this.SampleRates[Requests.PayloadStations]);
                            weightAndBalance.RegisterWithConnector(this.connector, this.SampleRates[Requests.WeightAndBalance]);
                            landingAnalysis.RegisterWithConnector(this.connector, this.SampleRates[Requests.LandingAnalysis]);
                            this.connector.Start();
                        }
                        else if ((DateTime.Now - this.connector.LastReceive) < TimeSpan.FromSeconds(3))
                        {
                            this.Connected = true;
                        }

                        if (this.Connected)
                        {
                            // Primary tracking
                            {
                                var clone = primaryTracking.Clone();
                                this.primaryTrackingProcessingQueue.Enqueue(new ProcessPrimaryTracking { Old = this.PrimaryTracking, New = clone });
                                this.OnPropertyChanged(nameof(this.PrimaryTrackingProcessingQueueLength));

                                this.PrimaryTracking = clone;
                                this.LastReceivedTimes[Requests.Primary] = DateTime.UtcNow;
                            }
                            foreach (Requests request in Enum.GetValues(typeof(Requests)))
                            {
                                if (this.SampleRates.ContainsKey(request))
                                {
                                    var lastTime = this.LastReceivedTimes[request];
                                    if (request != Requests.Primary && (!lastTime.HasValue || (DateTime.UtcNow - lastTime.Value).TotalMilliseconds > this.SampleRates[request]))
                                    {
                                        if (request == Requests.Secondary)
                                        {
                                            var clone = secondaryTracking.Clone();
                                            this.secondaryTrackingProcessingQueue.Enqueue(new ProcessSecondaryTracking { Old = this.SecondaryTracking, New = clone });
                                            this.OnPropertyChanged(nameof(this.SecondaryTrackingProcessingQueueLength));

                                            this.SecondaryTracking = clone;
                                            this.LastReceivedTimes[Requests.Secondary] = DateTime.UtcNow;
                                        }

                                        if (request == Requests.AircraftIdentity)
                                        {
                                            this.AircraftIdentity = aircraftIdentity.Clone();
                                            this.LastReceivedTimes[Requests.AircraftIdentity] = DateTime.UtcNow;
                                            new Thread(this.ProcessAircraftIdentity) { Name = "OpenSky.ProcessAircraftIdentity" }.Start();
                                        }

                                        if (request == Requests.FuelTanks)
                                        {
                                            this.FuelTanks = fuelTanks.Clone();
                                            this.LastReceivedTimes[Requests.FuelTanks] = DateTime.UtcNow;
                                        }

                                        if (request == Requests.PayloadStations)
                                        {
                                            new Thread(
                                                    () =>
                                                    {
                                                        var clone = payloadStations.Clone();
                                                        this.ProcessPayloadStations(this.PayloadStations, clone);
                                                        this.PayloadStations = clone;
                                                    })
                                            { Name = "OpenSky.ProcessPayloadStations" }.Start();
                                            this.LastReceivedTimes[Requests.PayloadStations] = DateTime.UtcNow;
                                        }

                                        if (request == Requests.WeightAndBalance)
                                        {
                                            new Thread(
                                                    () =>
                                                    {
                                                        var clone = weightAndBalance.Clone();
                                                        this.ProcessWeightAndBalance(this.WeightAndBalance, clone);
                                                        this.WeightAndBalance = clone;
                                                    })
                                            { Name = "OpenSky.ProcessWeightAndBalance" }.Start();
                                            this.LastReceivedTimes[Requests.WeightAndBalance] = DateTime.UtcNow;
                                        }

                                        if (request == Requests.LandingAnalysis)
                                        {
                                            var clone = landingAnalysis.Clone();
                                            this.landingAnalysisProcessingQueue.Enqueue(new ProcessLandingAnalysis { Old = this.LandingAnalysis, New = clone });
                                            this.OnPropertyChanged(nameof(this.LandingAnalysisProcessingQueueLength));

                                            this.LandingAnalysis = clone;
                                            this.LastReceivedTimes[Requests.LandingAnalysis] = DateTime.UtcNow;
                                        }
                                    }
                                }
                            }

                            Thread.Sleep(Math.Min(this.SampleRates[Requests.Primary], this.SampleRates[Requests.LandingAnalysis]));
                        }
                        else
                        {
                            SleepScheduler.SleepFor(TimeSpan.FromSeconds(10));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        SleepScheduler.SleepFor(TimeSpan.FromSeconds(this.Flight == null ? 30 : 5));
                    }
                }

                this.connector.Stop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error managing XPlane connector: " + ex);
            }
        }
    }
}