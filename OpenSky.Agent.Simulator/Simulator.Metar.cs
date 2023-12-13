// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Metar.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - Online Metar.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The alternate metar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string alternateMetar = "???\n";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Destination metar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string destinationMetar = "???\n";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The origin metar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string originMetar = "???\n";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate metar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateMetar
        {
            get => this.alternateMetar;

            set
            {
                if (Equals(this.alternateMetar, value))
                {
                    return;
                }

                this.alternateMetar = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination metar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DestinationMetar
        {
            get => this.destinationMetar;

            set
            {
                if (Equals(this.destinationMetar, value))
                {
                    return;
                }

                this.destinationMetar = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin metar.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OriginMetar
        {
            get => this.originMetar;

            set
            {
                if (Equals(this.originMetar, value))
                {
                    return;
                }

                this.originMetar = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh metar for flight airports.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void RefreshMetar()
        {
            new Thread(
                    () =>
                    {
                        using var client = new WebClient();
                        try
                        {
                            if (!string.IsNullOrEmpty(this.Flight?.Origin.Icao))
                            {
                                var url = $"https://aviationweather.gov/api/data/metar?ids={this.Flight.Origin.Icao}";
                                this.OriginMetar = client.DownloadString(url);
                                if (string.IsNullOrEmpty(this.OriginMetar))
                                {
                                    this.OriginMetar = "Unavailable\n";
                                }
                            }
                            else
                            {
                                this.OriginMetar = "???\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error refreshing origin metar: {ex}");
                            this.OriginMetar = "???";
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(this.Flight?.Destination.Icao))
                            {
                                var url = $"https://aviationweather.gov/api/data/metar?ids={this.Flight.Destination.Icao}";
                                this.DestinationMetar = client.DownloadString(url);
                                if (string.IsNullOrEmpty(this.DestinationMetar))
                                {
                                    this.DestinationMetar = "Unavailable\n";
                                }
                            }
                            else
                            {
                                this.DestinationMetar = "???\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error refreshing destination metar: {ex}");
                            this.DestinationMetar = "???";
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(this.Flight?.Alternate.Icao))
                            {
                                var url = $"https://aviationweather.gov/api/data/metar?ids={this.Flight.Alternate.Icao}";
                                this.AlternateMetar = client.DownloadString(url);
                                if (string.IsNullOrEmpty(this.AlternateMetar))
                                {
                                    this.AlternateMetar = "Unavailable\n";
                                }
                            }
                            else
                            {
                                this.AlternateMetar = "???\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error refreshing destination metar: {ex}");
                            this.AlternateMetar = "???";
                        }
                    })
                { Name = "OpenSky.Simulator.Metar" }.Start();
        }
    }
}