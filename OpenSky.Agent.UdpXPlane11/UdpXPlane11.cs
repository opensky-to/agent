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

            var primaryTracking = new PrimaryTrackingDataRef();
            primaryTracking.RegisterWithConnector(this.connector, this.SampleRates[Requests.Primary]);
            this.PrimaryTracking = primaryTracking;

            // Start our worker thread
            new Thread(this.ReadFromXPlane) { Name = "UdpXPlane11.ReadFromXPlane" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the simulator interface.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static string SimulatorInterfaceName => "UdpXPlane11";

        public override bool IsPaused { get; }

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
                this.connector.Start();
                while (!this.close)
                {
                    try
                    {
                        if ((DateTime.Now - this.connector.LastReceive) > TimeSpan.FromSeconds(30))
                        {
                            this.Connected = false;
                        }
                        else if ((DateTime.Now - this.connector.LastReceive) < TimeSpan.FromSeconds(5))
                        {
                            this.Connected = true;
                        }

                        // todo trigger "struct" received events like simconnect on configured intervals
                        // Nothing else to do here, just wait 5 seconds and monitor the last receive timestamp, since
                        // the connector doesn't report connection status.
                        Thread.Sleep(5000);
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
                Debug.WriteLine("Error starting/stopping XPlane connector: " + ex);
            }
        }
    }
}
