// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestFlightViewModel.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Globalization;

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Net;
    using System.Windows;

    using Newtonsoft.Json.Linq;

    using OpenSky.AgentMSFS.Models.API;
    using OpenSky.AgentMSFS.MVVM;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Test flight view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 26/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class TestFlightViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Flight flight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel loading time in seconds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int fuelSeconds;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload in seconds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int payloadSeconds;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFlightViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public TestFlightViewModel()
        {
            this.Flight = SimConnect.SimConnect.Instance.Flight ?? new Flight();

            this.CreateFlightCommand = new Command(this.CreateFlight);
            this.ResetFormCommand = new Command(this.ResetForm);
            this.CreateDemoFlightCommand = new Command(this.CreateDemoFlight);

            this.LookupOriginCommand = new AsynchronousCommand(this.LookupOrigin);
            this.LookupDestinationCommand = new AsynchronousCommand(this.LookupDestination);
            this.LookupAlternateCommand = new AsynchronousCommand(this.LookupAlternate);

            this.PlaneFromSimCommand = new Command(this.PlaneFromSim);
            this.PayloadFromSimCommand = new Command(this.PayloadFromSim);
            this.FuelFromSimCommand = new Command(this.FuelFromSim);

            this.ResetForm();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler CloseWindow;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the create demo flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CreateDemoFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the create flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CreateFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Flight Flight
        {
            get => this.flight;

            private set
            {
                if (Equals(this.flight, value))
                {
                    return;
                }

                this.flight = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel from simulation command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command FuelFromSimCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel loading time in seconds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int FuelSeconds
        {
            get => this.fuelSeconds;

            set
            {
                if (Equals(this.fuelSeconds, value))
                {
                    return;
                }

                this.fuelSeconds = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lookup alternate command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LookupAlternateCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lookup destination command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LookupDestinationCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lookup origin command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LookupOriginCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload from simulation command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PayloadFromSimCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload loading time in seconds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int PayloadSeconds
        {
            get => this.payloadSeconds;

            set
            {
                if (Equals(this.payloadSeconds, value))
                {
                    return;
                }

                this.payloadSeconds = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the plane from simulation command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PlaneFromSimCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the reset form command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ResetFormCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates demo flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CreateDemoFlight()
        {
            this.Flight = Flight.CreateDebugJob();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates the test flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CreateFlight()
        {
            try
            {
                if (string.IsNullOrEmpty(this.Flight.PlaneIdentifier) || string.IsNullOrEmpty(this.Flight.PlaneName))
                {
                    throw new Exception("Plane not selected!");
                }

                if (string.IsNullOrEmpty(this.Flight.OriginICAO) || string.IsNullOrEmpty(this.Flight.Origin) || this.Flight.OriginCoordinates.IsUnknown)
                {
                    throw new Exception("Origin airport not selected!");
                }

                if (string.IsNullOrEmpty(this.Flight.DestinationICAO) || string.IsNullOrEmpty(this.Flight.Destination) || this.Flight.DestinationCoordinates.IsUnknown)
                {
                    throw new Exception("Destination airport not selected!");
                }

                if (string.IsNullOrEmpty(this.Flight.AlternateICAO) || string.IsNullOrEmpty(this.Flight.Alternate) || this.Flight.AlternateCoordinates.IsUnknown)
                {
                    throw new Exception("Alternate airport not selected!");
                }

                if (this.Flight.FuelGallons == 0)
                {
                    throw new Exception("Can't fly with zero fuel!");
                }

                this.Flight.FuelLoadingComplete = DateTime.UtcNow.AddSeconds(this.FuelSeconds);
                this.Flight.PayloadLoadingComplete = DateTime.UtcNow.AddSeconds(this.PayloadSeconds);
                SimConnect.SimConnect.Instance.Flight = this.Flight;
                this.CloseWindow?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating test flight: " + ex);
                ModernWpf.MessageBox.Show(ex.Message, "Error creating test flight", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the target fuel amount to the current fuel in the simulation.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void FuelFromSim()
        {
            if (!SimConnect.SimConnect.Instance.Connected)
            {
                ModernWpf.MessageBox.Show("Not connected to sim!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.Flight.FuelGallons = SimConnect.SimConnect.Instance.WeightAndBalance.FuelTotalQuantity;
            this.NotifyPropertyChanged(nameof(this.Flight));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Look up the alternate airport using the specified ICAO code.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LookupAlternate()
        {
            if (string.IsNullOrEmpty(this.Flight.AlternateICAO))
            {
                return;
            }

            using (var client = new WebClient())
            {
                try
                {
                    if (!Equals(this.Flight.AlternateICAO, this.Flight.AlternateICAO.ToUpper(CultureInfo.InvariantCulture)))
                    {
                        this.Flight.AlternateICAO = this.Flight.AlternateICAO.ToUpper(CultureInfo.InvariantCulture);
                    }

                    var json = client.DownloadString($"http://iatageo.com/getICAOLatLng/{this.Flight.AlternateICAO}");
                    Debug.WriteLine(json);

                    var geoLocation = JObject.Parse(json);
                    var error = (string)geoLocation["error"];
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    this.Flight.Alternate = (string)geoLocation["name"];
                    this.Flight.AlternateCoordinates.Latitude = double.Parse((string)geoLocation["latitude"] ?? "0");
                    this.Flight.AlternateCoordinates.Longitude = double.Parse((string)geoLocation["longitude"] ?? "0");
                    this.NotifyPropertyChanged(nameof(this.Flight));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error looking alternate ICAO: " + ex);
                    this.LookupAlternateCommand.ReportProgress(() => { ModernWpf.MessageBox.Show(ex.Message, "Error fetching geo location for ICAO code", MessageBoxButton.OK, MessageBoxImage.Error); });
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Look up the destination airport using the specified ICAO code.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LookupDestination()
        {
            if (string.IsNullOrEmpty(this.Flight.DestinationICAO))
            {
                return;
            }

            using (var client = new WebClient())
            {
                try
                {
                    if (!Equals(this.Flight.DestinationICAO, this.Flight.DestinationICAO.ToUpper(CultureInfo.InvariantCulture)))
                    {
                        this.Flight.DestinationICAO = this.Flight.DestinationICAO.ToUpper(CultureInfo.InvariantCulture);
                    }

                    var json = client.DownloadString($"http://iatageo.com/getICAOLatLng/{this.Flight.DestinationICAO}");
                    Debug.WriteLine(json);

                    var geoLocation = JObject.Parse(json);
                    var error = (string)geoLocation["error"];
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    this.Flight.Destination = (string)geoLocation["name"];
                    this.Flight.DestinationCoordinates.Latitude = double.Parse((string)geoLocation["latitude"] ?? "0");
                    this.Flight.DestinationCoordinates.Longitude = double.Parse((string)geoLocation["longitude"] ?? "0");
                    this.NotifyPropertyChanged(nameof(this.Flight));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error looking destination ICAO: " + ex);
                    this.LookupDestinationCommand.ReportProgress(() => { ModernWpf.MessageBox.Show(ex.Message, "Error fetching geo location for ICAO code", MessageBoxButton.OK, MessageBoxImage.Error); });
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Look up the origin airport using the specified ICAO code.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LookupOrigin()
        {
            if (string.IsNullOrEmpty(this.Flight.OriginICAO))
            {
                return;
            }

            using var client = new WebClient();
            try
            {
                if (!Equals(this.Flight.OriginICAO, this.Flight.OriginICAO.ToUpper(CultureInfo.InvariantCulture)))
                {
                    this.Flight.OriginICAO = this.Flight.OriginICAO.ToUpper(CultureInfo.InvariantCulture);
                }

                var json = client.DownloadString($"http://iatageo.com/getICAOLatLng/{this.Flight.OriginICAO}");
                Debug.WriteLine(json);

                var geoLocation = JObject.Parse(json);
                var error = (string)geoLocation["error"];
                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                this.Flight.Origin = (string)geoLocation["name"];
                this.Flight.OriginCoordinates.Latitude = double.Parse((string)geoLocation["latitude"] ?? "0");
                this.Flight.OriginCoordinates.Longitude = double.Parse((string)geoLocation["longitude"] ?? "0");
                this.NotifyPropertyChanged(nameof(this.Flight));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error looking origin ICAO: " + ex);
                this.LookupOriginCommand.ReportProgress(() => { ModernWpf.MessageBox.Show(ex.Message, "Error fetching geo location for ICAO code", MessageBoxButton.OK, MessageBoxImage.Error); });
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the target payload weight to the current payload in the simulation.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PayloadFromSim()
        {
            if (!SimConnect.SimConnect.Instance.Connected)
            {
                ModernWpf.MessageBox.Show("Not connected to sim!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.Flight.PayloadPounds = SimConnect.SimConnect.Instance.WeightAndBalance.PayloadWeight;
            this.NotifyPropertyChanged(nameof(this.Flight));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the plane identifier and model to the current one loaded in the simulation.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PlaneFromSim()
        {
            if (!SimConnect.SimConnect.Instance.Connected)
            {
                ModernWpf.MessageBox.Show("Not connected to sim!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //this.Flight.PlaneIdentifier = SimConnect.SimConnect.Instance.PlaneIdentifierHash;
            this.Flight.AtcType = SimConnect.SimConnect.Instance.PlaneIdentity.AtcType;
            this.Flight.AtcModel = SimConnect.SimConnect.Instance.PlaneIdentity.AtcModel;
            this.Flight.PlaneName = SimConnect.SimConnect.Instance.PlaneIdentity.Type;
            this.NotifyPropertyChanged(nameof(this.Flight));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resets the form.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ResetForm()
        {
            this.Flight.FlightID = 1234;
            this.Flight.PlaneRegistry = "OE-LBQ";
            this.Flight.UtcOffset = 0;
            this.Flight.Payload = "3 passengers and their luggage";
            this.FuelSeconds = 60;
            this.PayloadSeconds = 150;

            this.Flight.OriginCoordinates = new GeoCoordinate();
            this.Flight.DestinationCoordinates = new GeoCoordinate();
            this.Flight.AlternateCoordinates = new GeoCoordinate();

            this.NotifyPropertyChanged(nameof(this.Flight));
        }
    }
}