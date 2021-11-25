// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartupViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using JetBrains.Annotations;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.SimConnect;
    using OpenSky.AgentMSFS.Tools;

    using OpenSkyApi;

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
        private readonly List<Guid> flightNotificationsDisplayed = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The grey OpenSky icon (idling, not connected).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly BitmapImage greyIcon =
            new(
                new Uri(
                    @"pack://application:,,,/OpenSky.AgentMSFS;component/Resources/opensky_grey16.ico",
                    UriKind.RelativeOrAbsolute));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky icon (idling, connected, but also between red recording to get blinking effect).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly BitmapImage openSkyIcon =
            new(
                new Uri(
                    @"pack://application:,,,/OpenSky.AgentMSFS;component/Resources/opensky.ico",
                    UriKind.RelativeOrAbsolute));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pause OpenSky icon (recording).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly BitmapImage pauseIcon =
            new(
                new Uri(
                    @"pack://application:,,,/OpenSky.AgentMSFS;component/Resources/opensky_pause16.ico",
                    UriKind.RelativeOrAbsolute));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The red OpenSky icon (recording).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private readonly BitmapImage redIcon =
            new(
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
        private string notificationStatusString = "OpenSky is trying to connect to Flight Simulator 2020...";

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
                this.notificationIcon = this.greyIcon;

                if (!UserSessionService.Instance.IsUserLoggedIn)
                {
                    LoginNotification.Open();
                }
            }

            // Initialize commands
            this.FlightTrackingCommand = new Command(this.OpenFlightTracking);
            this.TrackingDebugCommand = new Command(this.OpenTrackingDebug);
            this.AircraftTypesCommand = new Command(this.OpenAircraftTypes);
            this.SettingsCommand = new Command(this.OpenSettings);
            this.QuitCommand = new Command(this.Quit);

            // Check for update
            UpdateGUIDelegate autoUpdate = () => new AutoUpdate().Show();
            Application.Current.Dispatcher.BeginInvoke(autoUpdate);

            // Check for new flight from API
            new Thread(
                () =>
                {
                    if (UserSessionService.Instance.IsUserLoggedIn)
                    {
                        _ = UserSessionService.Instance.RefreshLinkedAccounts().Result;
                        _ = UserSessionService.Instance.RefreshUserAccountOverview().Result;
                    }

                    while (!SleepScheduler.IsShutdownInProgress)
                    {
                        if (UserSessionService.Instance.IsUserLoggedIn && SimConnect.Instance.Flight == null)
                        {
                            try
                            {
                                var result = OpenSkyService.Instance.GetFlightAsync().Result;
                                if (!result.IsError)
                                {
                                    if (result.Data.Id != Guid.Empty)
                                    {
                                        SimConnect.Instance.Flight = result.Data;
                                    }

                                    // todo test what happens if we set the flight to NULL in simconnect?
                                }
                                else
                                {
                                    Debug.WriteLine("Error checking for new flight: " + result.Message + "\r\n" + result.ErrorDetails);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error checking for new flight: " + ex);
                            }
                        }


                        SleepScheduler.SleepFor(TimeSpan.FromSeconds(30));
                    }
                })
            { Name = "OpenSky.StartupViewModel.CheckForFlights" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft types command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command AircraftTypesCommand { get; }

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
                    if (!this.flightNotificationsDisplayed.Contains(e.Id))
                    {
                        Debug.WriteLine("Showing new flight notification to user (standard timeout)");
                        new NewFlightNotification().Show();
                        this.flightNotificationsDisplayed.Add(e.Id);
                    }
                }

                // Bring an existing flight tracking view forward, but don't create a new one
                if (e != null && FlightTracking.Instance != null)
                {
                    if (!this.flightNotificationsDisplayed.Contains(e.Id))
                    {
                        Debug.WriteLine("Showing new flight notification to user (short timeout)");
                        new NewFlightNotification(10 * 1000).Show();
                        this.flightNotificationsDisplayed.Add(e.Id);
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
            if (e.PropertyName is nameof(SimConnect.Connected) or nameof(SimConnect.Instance.TrackingStatus) or nameof(SimConnect.Instance.IsPaused))
            {
                if (!SimConnect.Instance.Connected)
                {
                    this.redFlashing = false;
                    this.NotificationIcon = this.greyIcon;
                    this.NotificationStatusString = "OpenSky is trying to connect to Flight Simulator 2020...";
                }
                else
                {
                    if (SimConnect.Instance.TrackingStatus is TrackingStatus.NotTracking or TrackingStatus.Preparing or TrackingStatus.Resuming)
                    {
                        this.redFlashing = false;
                        this.NotificationIcon = this.openSkyIcon;
                        this.NotificationStatusString = "OpenSky is connected to the sim but not tracking a flight";
                    }
                    else if (SimConnect.Instance.IsPaused)
                    {
                        this.redFlashing = false;
                        this.NotificationIcon = this.pauseIcon;
                        this.NotificationStatusString = "OpenSky tracking and your flight are paused";
                    }
                    else
                    {
                        this.NotificationIcon = this.redIcon;
                        this.redFlashing = true;
                        this.NotificationStatusString = "OpenSky is tracking your flight";


                        new Thread(
                                () =>
                                {
                                    if (Monitor.TryEnter(this.openSkyIcon, new TimeSpan(0, 0, 1)))
                                    {
                                        try
                                        {
                                            while (this.redFlashing)
                                            {
                                                Thread.Sleep(1500);
                                                if (this.NotificationIcon == this.redIcon && this.redFlashing)
                                                {
                                                    UpdateGUIDelegate updateIcon = () => this.NotificationIcon = this.openSkyIcon;
                                                    Application.Current.Dispatcher.BeginInvoke(updateIcon);
                                                }

                                                if (this.NotificationIcon == this.openSkyIcon && this.redFlashing)
                                                {
                                                    UpdateGUIDelegate updateIcon = () => this.NotificationIcon = this.redIcon;
                                                    Application.Current.Dispatcher.BeginInvoke(updateIcon);
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            Monitor.Exit(this.openSkyIcon);
                                        }
                                    }
                                })
                        { Name = "OpenSky.StartupViewModel.RedFlashing" }.Start();
                    }
                }
            }
        }

        // Should the background worker flash the red icon?
        private bool redFlashing;

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
        /// Gets the version string of the application assembly.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string VersionString => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

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
        /// Opens the aircraft types view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenAircraftTypes()
        {
            Debug.WriteLine("Opening aircraft types view");
            AircraftTypes.Open();
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