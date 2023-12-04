// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Vatsim.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    using Newtonsoft.Json;

    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - Vatsim.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Vatsim user ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string VatsimUserID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current vatsim client connection, or NULL if there is no current connection.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public VatsimClientConnection VatsimClientConnection { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the duration of the vatsim connection(s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public TimeSpan VatsimConnectionDuration { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Monitor the flight on Vatsim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void MonitorVatsimFlight()
        {
            using var client = new WebClient();
            while (!this.close)
            {
                if (this.Flight?.OnlineNetwork == OnlineNetwork.Vatsim && !string.IsNullOrEmpty(this.VatsimUserID))
                {
                    try
                    {
                        var jsonString = client.DownloadString($"https://data.vatsim.net/v3/vatsim-data.json");
                        dynamic json = JsonConvert.DeserializeObject(jsonString);

                        var updateTime = (DateTime?)json?.general.update_timestamp;
                        if (updateTime != null)
                        {
                            if ((DateTime.UtcNow - updateTime.Value).TotalMinutes < 1)
                            {
                                if (json.pilots.Count > 0)
                                {
                                    for (var i = 0; i < json.pilots.Count; i++)
                                    {
                                        if (this.VatsimUserID.Equals((string)json.pilots[i].cid))
                                        {
                                            Debug.WriteLine($"Found active vatsim client connection for user {this.VatsimUserID}");

                                            this.VatsimClientConnection ??= new VatsimClientConnection
                                            {
                                                CID = (string)json.pilots[i].cid,
                                                Callsign = (string)json.pilots[i].callsign,
                                                LogonTime = (DateTime)json.pilots[i].logon_time
                                            };

                                            this.VatsimClientConnection.Departure = (string)json.pilots[i].flight_plan.departure;
                                            this.VatsimClientConnection.Arrival = (string)json.pilots[i].flight_plan.arrival;
                                            this.VatsimClientConnection.LastUpdated = (DateTime)json.pilots[i].last_updated;
                                            this.VatsimClientConnection.Latitude = (double)json.pilots[i].latitude;
                                            this.VatsimClientConnection.Longitude = (double)json.pilots[i].longitude;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("Data is too old.");
                            }
                        }

                        // Update tracking condition
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].Current = $"Connected: {this.VatsimClientConnection != null}, Callsign: {this.VatsimClientConnection?.Callsign ?? "none"}, Flight plan: {this.VatsimClientConnection?.Departure ?? "??"}-{this.VatsimClientConnection?.Arrival ?? "??"}";
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].ConditionMet = this.VatsimClientConnection != null &&
                                                                                                      this.VatsimClientConnection.Callsign.Equals(this.Flight.AtcCallsign, StringComparison.InvariantCultureIgnoreCase) &&
                                                                                                      this.VatsimClientConnection.Departure.Equals(this.Flight.Origin.Icao, StringComparison.InvariantCultureIgnoreCase) &&
                                                                                                      this.VatsimClientConnection.Arrival.Equals(this.Flight.Destination.Icao, StringComparison.InvariantCultureIgnoreCase);

                        SleepScheduler.SleepFor(TimeSpan.FromSeconds(this.VatsimClientConnection == null ? 15 : 60));

                        // Track how long the connection was established during a flight
                        // TODO when flight is active and connection is good, record start time ... then when flight ends or connection drops, calculate duration and add to timespan
                        // TODO init the duration to 0 when starting a new flight and check for connection when tracking start/end
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error monitoring Vatsim: " + ex);
                        SleepScheduler.SleepFor(TimeSpan.FromSeconds(30));

                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].Current = "Error";
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].ConditionMet = false;
                    }
                }
                else
                {
                    this.VatsimClientConnection = null;
                    Thread.Sleep(5000);
                }
            }
        }
    }
}