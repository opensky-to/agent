// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartupViewModel.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using JetBrains.Annotations;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.Models.API;
    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.SimConnect;
    using OpenSky.AgentMSFS.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The startup view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class StartupViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight IDs for which we already displayed notifications.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<int> flightNotificationsDisplayed = new List<int>();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky icon (tracking).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly BitmapImage openSkyPlaneIcon =
            new BitmapImage(
                new Uri(
                    @"pack://application:,,,/OpenSky.AgentMSFS;component/Resources/opensky.ico",
                    UriKind.RelativeOrAbsolute));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The green OpenSky icon (connected).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly BitmapImage greenPlaneIcon =
            new BitmapImage(
                new Uri(
                    @"pack://application:,,,/OpenSky.AgentMSFS;component/Resources/opensky_green16.ico",
                    UriKind.RelativeOrAbsolute));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The red OpenSky icon (not connected).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly BitmapImage redPlaneIcon =
            new BitmapImage(
                new Uri(
                    @"pack://application:,,,/OpenSky.AgentMSFS;component/Resources/opensky_red16.ico",
                    UriKind.RelativeOrAbsolute));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The notification icon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private ImageSource notificationIcon;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The notification status string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private string notificationStatusString = "OpenSky is trying to connect to MSFS 2020...";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The notification icon visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility notificationVisibility = Visibility.Visible;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        // ReSharper disable NotNullMemberIsNotInitialized
        public StartupViewModel()
        {
            if (!Startup.StartupFailed)
            {
                if (Instance != null)
                {
                    throw new Exception("Only one instance of the startup view model may be created!");
                }

                Instance = this;
                SimConnect.Instance.PropertyChanged += this.SimConnectPropertyChanged;
                SimConnect.Instance.FlightChanged += this.SimConnectFlightChanged;
                this.notificationIcon = this.redPlaneIcon;
            }

            // Initialize commands
            this.FlightTrackingCommand = new Command(this.OpenFlightTracking);
            this.TrackingDebugCommand = new Command(this.OpenTrackingDebug);
            this.TestFlightCommand = new Command(this.OpenTestFlight);
            this.PlaneIdentityCollectorCommand = new Command(this.OpenPlaneIdentityCollector);
            this.SettingsCommand = new Command(this.OpenSettings);
            this.QuitCommand = new Command(this.Quit);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the plane identity collector command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PlaneIdentityCollectorCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// SimConnect flight changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// A Flight to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimConnectFlightChanged(object sender, Flight e)
        {
            UpdateGUIDelegate flightChangedUI = () =>
            {
                // Show notification mini-view
                if (e != null && FlightTracking.Instance == null)
                {
                    if (!this.flightNotificationsDisplayed.Contains(e.FlightID))
                    {
                        Debug.WriteLine("Showing new flight notification to user (standard timeout)");
                        new NewFlightNotification().Show();
                        this.flightNotificationsDisplayed.Add(e.FlightID);
                    }
                }

                // Bring an existing flight tracking view forward, but don't create a new one
                if (e != null && FlightTracking.Instance != null)
                {
                    if (!this.flightNotificationsDisplayed.Contains(e.FlightID))
                    {
                        Debug.WriteLine("Showing new flight notification to user (short timeout)");
                        new NewFlightNotification(10 * 1000).Show();
                        this.flightNotificationsDisplayed.Add(e.FlightID);
                    }

                    Debug.WriteLine("New flight, bringing existing flight tracking view into focus");
                    FlightTracking.Open();
                }
            };

            Application.Current.Dispatcher.BeginInvoke(flightChangedUI);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// SimConnect property changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Property changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimConnectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SimConnect.Connected) || e.PropertyName == nameof(SimConnect.Instance.TrackingStatus))
            {
                if (!SimConnect.Instance.Connected)
                {
                    this.NotificationIcon = this.redPlaneIcon;
                    this.NotificationStatusString = "OpenSky is trying to connect to MSFS 2020...";
                }
                else
                {
                    if (SimConnect.Instance.TrackingStatus == TrackingStatus.NotTracking || SimConnect.Instance.TrackingStatus == TrackingStatus.Preparing || SimConnect.Instance.TrackingStatus == TrackingStatus.Resuming)
                    {
                        this.NotificationIcon = this.greenPlaneIcon;
                        this.NotificationStatusString = "OpenSky is connected to the sim but not tracking a flight";
                    }
                    else
                    {
                        this.NotificationIcon = this.openSkyPlaneIcon;
                        this.NotificationStatusString = "OpenSky is tracking your flight";
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the single instance of the startup view model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public static StartupViewModel Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the notification icon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public ImageSource NotificationIcon
        {
            get => this.notificationIcon;

            set
            {
                if (Equals(this.notificationIcon, value))
                {
                    return;
                }

                this.notificationIcon = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the notification status string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public string NotificationStatusString
        {
            get => this.notificationStatusString;

            set
            {
                if (Equals(this.notificationStatusString, value))
                {
                    return;
                }

                this.notificationStatusString = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the notification icon visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility NotificationVisibility
        {
            get => this.notificationVisibility;

            set
            {
                if (Equals(this.notificationVisibility, value))
                {
                    return;
                }

                this.notificationVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the quit command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command QuitCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking status command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command TrackingDebugCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight tracking command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command FlightTrackingCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the settings command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command SettingsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the test flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command TestFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens the create test flight view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenTestFlight()
        {
#if DEBUG
            Debug.WriteLine("Opening test flight view");
            TestFlight.Open();
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens the plane identity collector view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenPlaneIdentityCollector()
        {
#if DEBUG
            Debug.WriteLine("Opening plane identity collector view");
            PlaneIdentityCollector.Open();
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens the tracking debug view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenTrackingDebug()
        {
#if DEBUG
            Debug.WriteLine("Opening tracking debug view");
            TrackingDebug.Open();
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens the flight tracking view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenFlightTracking()
        {
            Debug.WriteLine("Opening flight tracking view");
            FlightTracking.Open();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens the settings view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenSettings()
        {
            Debug.WriteLine("Opening settings view");
            Settings.Open();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Quits the application.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void Quit()
        {
            UpdateGUIDelegate cleanUp = () =>
            {
                SimConnect.Instance.Close();
                SleepScheduler.Shutdown();
                this.NotificationVisibility = Visibility.Collapsed;
            };
            ((App)Application.Current).RequestShutdown(cleanUp);
        }
    }
}