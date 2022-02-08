// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UdpXP11.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.Agent.UdpXPlane11.Models;

    using OpenSkyApi;

    using XPlaneConnector;

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
        /// The UDP xplane connector.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly XPlaneConnector connector;

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
            this.connector = new XPlaneConnector(simulatorIPAddress, (int)simulatorPort);



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

        public override void Pause(bool pause)
        {

        }

        public override void SetAircraftRegistry(string registry)
        {

        }

        public override void SetFuelAndPayloadFromSave()
        {

        }

        public override void SetFuelTanks(FuelTanks newFuelTanks)
        {

        }

        public override void SetPayloadStations(PayloadStations newPayloadStations)
        {

        }

        public override void SetSlew(bool enable)
        {

        }

        public override void SetTime(DateTime time)
        {

        }

        public override void SlewPlaneToFlightPosition()
        {

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
                // Subscribe to datarefs
                var primaryTracking = new PrimaryTrackingDataRef();
                primaryTracking.RegisterWithConnector(this.connector, this.SampleRates[Requests.Primary]);

                var secondaryTracking = new SecondaryTrackingDataRef();
                secondaryTracking.RegisterWithConnector(this.connector, this.SampleRates[Requests.Secondary]);

                var aircraftIdentity = new AircraftIdentityDataRef();
                aircraftIdentity.RegisterWithConnector(this.connector, this.SampleRates[Requests.AircraftIdentity]);

                var fuelTanks = new FuelTanksDataRef();
                fuelTanks.RegisterWithConnector(this.connector, this.SampleRates[Requests.FuelTanks]);

                var payloadStations = new PayloadStationsDataRef();
                payloadStations.RegisterWithConnector(this.connector, this.SampleRates[Requests.PayloadStations]);

                var weightAndBalance = new WeightAndBalanceDataRef();
                weightAndBalance.RegisterWithConnector(this.connector, this.SampleRates[Requests.WeightAndBalance]);

                var landingAnalysis = new LandingAnalysisDataRef();
                landingAnalysis.RegisterWithConnector(this.connector, this.SampleRates[Requests.LandingAnalysis]);

                this.connector.Start();
                while (!this.close)
                {
                    try
                    {
                        if ((DateTime.Now - this.connector.LastReceive) > TimeSpan.FromSeconds(10))
                        {
                            this.Connected = false;
                        }
                        else if ((DateTime.Now - this.connector.LastReceive) < TimeSpan.FromSeconds(3))
                        {
                            this.Connected = true;
                        }

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
