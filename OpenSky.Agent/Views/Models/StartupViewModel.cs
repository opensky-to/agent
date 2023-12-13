// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartupViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
#if DEBUG
    using DiscordRPC.Logging;
#endif
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using DiscordRPC;

    using JetBrains.Annotations;

    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator;
    using OpenSky.Agent.Simulator.Controls;
    using OpenSky.Agent.Simulator.Controls.Models;
    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Tools;

    using OpenSkyApi;

    using Simulator = OpenSky.Agent.Simulator.Simulator;

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
                    @"pack://application:,,,/OpenSky.Agent;component/Resources/opensky_grey16.ico",
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
                    @"pack://application:,,,/OpenSky.Agent;component/Resources/opensky.ico",
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
                    @"pack://application:,,,/OpenSky.Agent;component/Resources/opensky_pause16.ico",
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
                    @"pack://application:,,,/OpenSky.Agent;component/Resources/opensky_red16.ico",
                    UriKind.RelativeOrAbsolute));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The discord RPC client.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DiscordRpcClient discordRpcClient;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The MOD commands visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility modCommandVisibility = Visibility.Collapsed;

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
        private string notificationStatusString = "OpenSky is trying to connect to the simulator...";

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

                // ReSharper disable HeuristicUnreachableCode
                Instance = this;
                Simulator.Instance.PropertyChanged += this.SimConnectPropertyChanged;
                Simulator.Instance.FlightChanged += this.SimConnectFlightChanged;
                Simulator.Instance.MessageBoxCreated += this.SimMessageBoxCreated;
                this.notificationIcon = this.greyIcon;

                if (!UserSessionService.Instance.IsUserLoggedIn)
                {
                    LoginNotification.Open();
                }
            }

            // Initialize data structures
            this.PausedFlights = new ObservableCollection<Flight>();

            // Initialize commands
            this.FlightTrackingCommand = new Command(this.OpenFlightTracking);
            this.TrackingDebugCommand = new Command(this.OpenTrackingDebug);
            this.SoundPackTesterCommand = new Command(this.OpenSoundPackTester);
            this.AddAircraftCommand = new Command(this.OpenAddAircraft);
            this.AircraftTypesCommand = new Command(this.OpenAircraftTypes);
            this.SettingsCommand = new Command(this.OpenSettings);
            this.QuitCommand = new Command(this.Quit);
            this.CheckForNextFlightNowCommand = new Command(this.CheckForNextFlightNow);
            this.ResumeFlightCommand = new AsynchronousCommand(this.ResumeFlight);

            // Initialize sound packs
            var soundPacks = SpeechSoundPacks.Instance.SoundPacks;
            foreach (var soundPack in soundPacks)
            {
                Debug.WriteLine($"Found sound pack: {soundPack.Key}, containing {soundPack.Value.Count} audio events");
            }

            // Check for update
            UpdateGUIDelegate autoUpdate = () => new AutoUpdate().Show();
            Application.Current.Dispatcher.BeginInvoke(autoUpdate);

            // Initialize discord RPC
            this.DiscordRpcClient = new DiscordRpcClient("918167200492314675");
#if DEBUG
            this.DiscordRpcClient.Logger = new ConsoleLogger() { Level = LogLevel.Info };
            this.DiscordRpcClient.OnReady += (_, e) => { Debug.WriteLine("Received Ready from user {0}", e.User.Username); };

            this.DiscordRpcClient.OnPresenceUpdate += (_, e) => { Debug.WriteLine("Received Update! {0}", e.Presence); };
#endif
            this.DiscordRpcClient.Initialize();
            this.DiscordRpcClient.SetPresence(
                new RichPresence
                {
                    State = "Not Connected",
                    Details = "Trying to connect to simulator",
                    Assets = new Assets
                    {
                        LargeImageKey = "openskylogogrey512",
                        LargeImageText = "OpenSky Agent"
                    }
                });

            // Check for new flight from API
            new Thread(
                    () =>
                    {
                        if (UserSessionService.Instance.IsUserLoggedIn)
                        {
                            _ = UserSessionService.Instance.UpdateUserRoles().Result;
                            _ = UserSessionService.Instance.RefreshLinkedAccounts().Result;
                            _ = UserSessionService.Instance.RefreshUserAccountOverview().Result;
                            Simulator.Instance.OpenSkyUserName = UserSessionService.Instance.Username;
                            UpdateGUIDelegate updateMenu = this.UpdateMainMenu;
                            Application.Current.Dispatcher.BeginInvoke(updateMenu);
                        }
                        else
                        {
                            Simulator.Instance.OpenSkyUserName = null;
                        }

                        UserSessionService.Instance.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(UserSessionService.Instance.IsUserLoggedIn))
                            {
                                if (UserSessionService.Instance.IsUserLoggedIn)
                                {
                                    _ = UserSessionService.Instance.UpdateUserRoles().Result;
                                    _ = UserSessionService.Instance.RefreshLinkedAccounts().Result;
                                    _ = UserSessionService.Instance.RefreshUserAccountOverview().Result;
                                    Simulator.Instance.OpenSkyUserName = UserSessionService.Instance.Username;
                                    UpdateGUIDelegate updateMenu = this.UpdateMainMenu;
                                    Application.Current.Dispatcher.BeginInvoke(updateMenu);
                                }
                                else
                                {
                                    Simulator.Instance.OpenSkyUserName = null;
                                }
                            }
                        };

                        while (!SleepScheduler.IsShutdownInProgress)
                        {
                            if (UserSessionService.Instance.IsUserLoggedIn)
                            {
                                try
                                {
                                    // Check for active flight
                                    var result = AgentOpenSkyService.Instance.GetFlightAsync().Result;
                                    if (!result.IsError)
                                    {
                                        if (result.Data.Id != Guid.Empty)
                                        {
                                            UpdateGUIDelegate resetPausedFlights = () => this.PausedFlights.Clear();
                                            Application.Current.Dispatcher.BeginInvoke(resetPausedFlights);

                                            if (Simulator.Instance.Flight == null)
                                            {
                                                Simulator.Instance.Flight = result.Data;
                                                Simulator.Instance.VatsimUserID = UserSessionService.Instance.LinkedAccounts?.VatsimID;
                                            }
                                            else
                                            {
                                                if (Simulator.Instance.Flight.Id != result.Data.Id)
                                                {
                                                    // Different flight from current one?
                                                    Simulator.Instance.StopTracking(true);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Simulator.Instance.Flight != null)
                                            {
                                                Simulator.Instance.Flight = null;
                                            }

                                            // Check for paused flights
                                            var pausedResult = AgentOpenSkyService.Instance.GetMyFlightsAsync().Result;
                                            if (!result.IsError)
                                            {
                                                UpdateGUIDelegate addPausedFlights = () =>
                                                {
                                                    this.PausedFlights.Clear();
                                                    foreach (var flight in pausedResult.Data.Where(f => f.Paused.HasValue))
                                                    {
                                                        this.PausedFlights.Add(flight);
                                                    }
                                                };
                                                Application.Current.Dispatcher.BeginInvoke(addPausedFlights);
                                            }
                                            else
                                            {
                                                Debug.WriteLine("Error checking for paused flights: " + result.Message + "\r\n" + result.ErrorDetails);
                                            }
                                        }
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

                            this.NextFlightUpdateCheckSeconds = Simulator.Instance.Flight == null ? 30 : 120;
                            while (this.NextFlightUpdateCheckSeconds > 0 && !SleepScheduler.IsShutdownInProgress)
                            {
                                Thread.Sleep(1000);
                                if (this.NextFlightUpdateCheckSeconds > 0)
                                {
                                    this.NextFlightUpdateCheckSeconds--;
                                }
                            }
                        }
                    })
                { Name = "OpenSky.StartupViewModel.CheckForFlights" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simulator interface was changed in the settings.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void SimulatorChanged()
        {
            Simulator.Instance.PropertyChanged += this.SimConnectPropertyChanged;
            Simulator.Instance.FlightChanged += this.SimConnectFlightChanged;
            Simulator.Instance.MessageBoxCreated += this.SimMessageBoxCreated;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the discord RPC client.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DiscordRpcClient DiscordRpcClient
        {
            get => this.discordRpcClient;

            private set
            {
                if (Equals(this.discordRpcClient, value))
                {
                    return;
                }

                this.discordRpcClient = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resume the specified paused flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/11/2023.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ResumeFlight(object parameter)
        {
            if (parameter is Guid flightId)
            {
                try
                {
                    var result = AgentOpenSkyService.Instance.ResumeFlightAsync(flightId).Result;
                    if (result.IsError)
                    {
                        this.ResumeFlightCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox("Flight resume error", $"Error resuming flight: {result.Message}", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                messageBox.SetErrorColorStyle();
                                FlightTracking.Instance.ShowMessageBox(messageBox);
                            });
                    }
                    else
                    {
                        // Refresh now
                        this.NextFlightUpdateCheckSeconds = 0;
                    }
                }
                catch (Exception ex)
                {
                    this.ResumeFlightCommand.ReportProgress(
                        () =>
                        {
                            var messageBox = new OpenSkyMessageBox(ex, "Flight resume error", "Error resuming flight.", ExtendedMessageBoxImage.Error, 30);
                            FlightTracking.Instance.ShowMessageBox(messageBox);
                        });
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the paused flights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Flight> PausedFlights { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The seconds until the next flight update check.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int nextFlightUpdateCheckSeconds = 30;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the seconds until the next flight update check.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int NextFlightUpdateCheckSeconds
        {
            get => this.nextFlightUpdateCheckSeconds;

            private set
            {
                if (Equals(this.nextFlightUpdateCheckSeconds, value))
                {
                    return;
                }

                this.nextFlightUpdateCheckSeconds = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the MOD commands visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility ModCommandVisibility
        {
            get => this.modCommandVisibility;

            private set
            {
                if (Equals(this.modCommandVisibility, value))
                {
                    return;
                }

                this.modCommandVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user logged in command visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility userLoggedInCommandVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user logged in command visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility UserLoggedInCommandVisibility
        {
            get => this.userLoggedInCommandVisibility;

            private set
            {
                if (Equals(this.userLoggedInCommandVisibility, value))
                {
                    return;
                }

                this.userLoggedInCommandVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the check for next flight now command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CheckForNextFlightNowCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check for next flight now.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CheckForNextFlightNow()
        {
            this.NextFlightUpdateCheckSeconds = 0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the resume flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand ResumeFlightCommand { get; }

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
            if (e.PropertyName is nameof(Simulator.Connected) or nameof(Simulator.Instance.TrackingStatus) or nameof(Simulator.Instance.IsPaused) or nameof(Simulator.Instance.Flight) or nameof(Simulator.Instance.FlightPhase))
            {
                if (!Simulator.Instance.Connected)
                {
                    this.redFlashing = false;
                    this.NotificationIcon = this.greyIcon;
                    this.NotificationStatusString = "OpenSky is trying to connect to the simulator...";

                    this.DiscordRpcClient?.SetPresence(
                        new RichPresence
                        {
                            State = "Not Connected",
                            Details = "Trying to connect to simulator",
                            Assets = new Assets
                            {
                                LargeImageKey = "openskylogogrey512",
                                LargeImageText = "OpenSky Agent"
                            }
                        });
                }
                else
                {
                    if (Simulator.Instance.TrackingStatus is TrackingStatus.NotTracking or TrackingStatus.Preparing or TrackingStatus.Resuming)
                    {
                        if (Simulator.Instance.Flight == null)
                        {
                            this.redFlashing = false;
                            this.NotificationIcon = this.openSkyIcon;
                            this.NotificationStatusString = "OpenSky is connected to the sim but not tracking a flight";

                            this.DiscordRpcClient?.SetPresence(
                                new RichPresence
                                {
                                    State = "Idle",
                                    Details = "Waiting for a flight",
                                    Assets = new Assets
                                    {
                                        LargeImageKey = "openskylogo512",
                                        LargeImageText = "OpenSky Agent"
                                    }
                                });
                        }
                        else
                        {
                            this.redFlashing = false;
                            this.NotificationIcon = this.openSkyIcon;
                            this.NotificationStatusString = $"OpenSky is preparing to track flight {Simulator.Instance.Flight?.FullFlightNumber}";

                            this.DiscordRpcClient?.SetPresence(
                                new RichPresence
                                {
                                    State = Simulator.Instance.TrackingStatus.ToString(),
                                    Details = $"Preparing flight {Simulator.Instance.Flight?.FullFlightNumber}",
                                    Assets = new Assets
                                    {
                                        LargeImageKey = "openskylogo512",
                                        LargeImageText = "OpenSky Agent"
                                    }
                                });
                        }
                    }
                    else if (Simulator.Instance.IsPaused)
                    {
                        this.redFlashing = false;
                        this.NotificationIcon = this.pauseIcon;
                        this.NotificationStatusString = $"OpenSky tracking and your flight {Simulator.Instance.Flight?.FullFlightNumber} are paused";

                        this.DiscordRpcClient?.SetPresence(
                            new RichPresence
                            {
                                State = $"Paused, {Simulator.Instance.FlightPhase}",
                                Details = $"Tracking flight {Simulator.Instance.Flight?.FullFlightNumber}",
                                Assets = new Assets
                                {
                                    LargeImageKey = "openskylogo512",
                                    LargeImageText = "OpenSky Agent",
                                    SmallImageKey = "pause512",
                                    SmallImageText = "Paused"
                                }
                            });
                    }
                    else
                    {
                        this.NotificationIcon = this.redIcon;
                        this.redFlashing = true;
                        this.NotificationStatusString = $"OpenSky is tracking your flight {Simulator.Instance.Flight?.FullFlightNumber}";

                        this.DiscordRpcClient?.SetPresence(
                            new RichPresence
                            {
                                State = $"Recording, {Simulator.Instance.FlightPhase}",
                                Details = $"Tracking flight {Simulator.Instance.Flight?.FullFlightNumber}",
                                Assets = new Assets
                                {
                                    LargeImageKey = "openskylogo512",
                                    LargeImageText = "OpenSky Agent",
                                    SmallImageKey = "record512",
                                    SmallImageText = "Recording"
                                }
                            });

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
        [NotNull]
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
        /// Gets the add aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command AddAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the sound pack tester command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command SoundPackTesterCommand { get; }

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
            Debug.WriteLine("Opening tracking debug view");
            TrackingDebug.Open();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens sound pack tester view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenSoundPackTester()
        {
            Debug.WriteLine("Opening sound pack tester view");
            SoundPackTester.Open();
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
        /// The simulator interface created a message box and asked as to show it to the user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="openSkyMessageBox">
        /// The OpenSky message box.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimMessageBoxCreated(object sender, OpenSkyMessageBox openSkyMessageBox)
        {
            FlightTracking.Open();
            FlightTracking.Instance.ShowMessageBox(openSkyMessageBox);
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
        /// Opens the add aircraft view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void OpenAddAircraft()
        {
            Debug.WriteLine("Opening add aircraft view");
            AddAircraft.Open();
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
                Simulator.Instance.Close();
                SleepScheduler.Shutdown();
                this.NotificationVisibility = Visibility.Collapsed;
                this.DiscordRpcClient.Dispose();
            };
            ((App)Application.Current).RequestShutdown(cleanUp);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the main menu (typically after a user login/logout and view model construction).
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateMainMenu()
        {
            this.UserLoggedInCommandVisibility = UserSessionService.Instance.IsUserLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            this.ModCommandVisibility = UserSessionService.Instance.IsModerator ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}