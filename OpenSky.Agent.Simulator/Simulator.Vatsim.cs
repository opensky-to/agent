// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Vatsim.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Device.Location;
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
        /// Gets the duration of the online network connection(s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public TimeSpan OnlineNetworkConnectionDuration { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the date/time the current online network connection started.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? OnlineNetworkConnectionStarted { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Monitor the flight on Vatsim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/12/2023.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// -------------------------------------------------------------------------------------------------
        private void MonitorVatsimFlight()
        {
            using var client = new WebClient();
            var lastDataUpdate = DateTime.MinValue;
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
                                lastDataUpdate = DateTime.UtcNow;
                                var foundPilot = false;
                                if (json.pilots.Count > 0)
                                {
                                    for (var i = 0; i < json.pilots.Count; i++)
                                    {
                                        if (this.VatsimUserID.Equals((string)json.pilots[i].cid))
                                        {
                                            Debug.WriteLine($"Found active vatsim client connection for user {this.VatsimUserID}");
                                            foundPilot = true;

                                            this.VatsimClientConnection ??= new VatsimClientConnection
                                            {
                                                CID = (string)json.pilots[i].cid,
                                                Callsign = (string)json.pilots[i].callsign,
                                                LogonTime = (DateTime)json.pilots[i].logon_time
                                            };

                                            this.VatsimClientConnection.LastUpdated = (DateTime)json.pilots[i].last_updated;
                                            this.VatsimClientConnection.Latitude = (double)json.pilots[i].latitude;
                                            this.VatsimClientConnection.Longitude = (double)json.pilots[i].longitude;
                                            this.VatsimClientConnection.Altitude = (double)json.pilots[i].altitude;

                                            try
                                            {
                                                this.VatsimClientConnection.Departure = (string)json.pilots[i].flight_plan.departure;
                                                this.VatsimClientConnection.Arrival = (string)json.pilots[i].flight_plan.arrival;
                                            }
                                            catch
                                            {
                                                // When no flight plan is filed these two fail, so set to empty
                                                this.VatsimClientConnection.Departure = string.Empty;
                                                this.VatsimClientConnection.Arrival = string.Empty;
                                            }


                                            this.OnlineNetworkConnectionStarted ??= DateTime.UtcNow;
                                        }
                                    }
                                }

                                // CID not connected as pilot?
                                if (!foundPilot)
                                {
                                    this.VatsimClientConnection = null;
                                    if (this.OnlineNetworkConnectionStarted.HasValue)
                                    {
                                        this.OnlineNetworkConnectionDuration += (DateTime.UtcNow - this.OnlineNetworkConnectionStarted.Value);
                                        this.OnlineNetworkConnectionStarted = null;
                                    }
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Vatsim data received is stale.");
                            }
                        }

                        // Did our connection time-out?
                        if ((this.VatsimClientConnection != null || this.OnlineNetworkConnectionStarted.HasValue) && (DateTime.UtcNow - lastDataUpdate).TotalMinutes > 5)
                        {
                            this.VatsimClientConnection = null;
                            if (this.OnlineNetworkConnectionStarted.HasValue)
                            {
                                this.OnlineNetworkConnectionDuration += (DateTime.UtcNow - this.OnlineNetworkConnectionStarted.Value);
                                this.OnlineNetworkConnectionStarted = null;
                            }
                        }

                        // Update tracking condition
                        var locationDiffKm = this.PrimaryTracking.GeoCoordinate.GetDistanceTo(new GeoCoordinate(this.VatsimClientConnection?.Latitude ?? 0, this.VatsimClientConnection?.Longitude ?? 0, 0.3048 * this.VatsimClientConnection?.Altitude ?? 0)) / 1000;
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].Current = $"{this.VatsimClientConnection != null}, Callsign: {this.VatsimClientConnection?.Callsign ?? "none"}, Flight plan: {this.VatsimClientConnection?.Departure ?? "??"}-{this.VatsimClientConnection?.Arrival ?? "??"}, Location: {locationDiffKm:N1} km";
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].ConditionMet =
                            this.VatsimClientConnection != null &&
                            locationDiffKm < 50 &&
                            this.VatsimClientConnection.Callsign.Equals(this.Flight.AtcCallsign, StringComparison.InvariantCultureIgnoreCase) &&
                            this.VatsimClientConnection.Departure.Equals(this.Flight.Origin.Icao, StringComparison.InvariantCultureIgnoreCase) &&
                            this.VatsimClientConnection.Arrival.Equals(this.Flight.Destination.Icao, StringComparison.InvariantCultureIgnoreCase);

                        SleepScheduler.SleepFor(TimeSpan.FromSeconds(this.VatsimClientConnection == null ? 15 : 60));
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