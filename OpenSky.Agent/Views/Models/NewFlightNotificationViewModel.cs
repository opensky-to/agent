// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewFlightNotificationViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Media;
    using System.Reflection;
    using System.Threading;

    using OpenSky.Agent.Simulator;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// New flight notification view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class NewFlightNotificationViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string airports;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string flightNumber;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="NewFlightNotificationViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public NewFlightNotificationViewModel()
        {
            this.Timeout = 60 * 1000;
            this.FlightNumber = $"{SimConnect.SimConnect.Instance.Flight?.FullFlightNumber}";
            this.Airports = $"{SimConnect.SimConnect.Instance.Flight?.Origin.Icao} - {SimConnect.SimConnect.Instance.Flight?.Destination.Icao}";

            var assembly = Assembly.GetExecutingAssembly();
            var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSannouncement.wav"));
            player.Play();

            new Thread(
                () =>
                {
                    Thread.Sleep(2000);
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.ReadyForBoarding, true, SimConnect.SimConnect.Instance.Flight?.FlightNumber.ToString());
                })
            { Name = "OpenSky.NewFlightNotificationViewModel.SpeechFlightNumber" }.Start();

            this.StartTrackingCommand = new Command(this.StartTracking);
            new Thread(this.NotificationTimeout) { Name = "OpenSky.NewFlightNotificationTimeout" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler CloseWindow;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Airports
        {
            get => this.airports;

            private set
            {
                if (Equals(this.airports, value))
                {
                    return;
                }

                this.airports = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FlightNumber
        {
            get => this.flightNumber;

            private set
            {
                if (Equals(this.flightNumber, value))
                {
                    return;
                }

                this.flightNumber = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start tracking command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command StartTrackingCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the timeout for the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Timeout { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Notification timeout.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void NotificationTimeout()
        {
            var waited = 0;
            while (waited < this.Timeout && !SleepScheduler.IsShutdownInProgress)
            {
                Thread.Sleep(5000);
                waited += 5000;
            }

            this.CloseWindow?.Invoke(this, EventArgs.Empty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens the flight tracking view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartTracking()
        {
            FlightTracking.Open();
            this.CloseWindow?.Invoke(this, EventArgs.Empty);
        }
    }
}