// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using JetBrains.Annotations;

    using OpenSky.Agent.Properties;
    using OpenSky.Agent.SimConnectMSFS;
    using OpenSky.Agent.Simulator;
    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;
#if DEBUG
    using OpenSky.Agent.Tools;
#endif

    using OpenSkyApi;

    using Syncfusion.SfSkinManager;

    using XDMessaging;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Main entry point class for WPF application.
    /// </content>
    ///// -------------------------------------------------------------------------------------------------
    public partial class App
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Named mutex that ensures single instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        private static readonly Mutex Mutex = new(false, "OpenSky.Agent.SingleInstance." + Environment.UserName);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The after shutdown clean up code delegate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        private Delegate afterShutdownCleanUpCodeDelegate;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this application is in design mode.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static bool IsDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("##SyncfusionLicense##");
            SfSkinManager.ApplyStylesOnApplication = false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Requests a graceful shutdown of the application.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// <param name="afterShutdownCleanUpCode">
        /// The after shutdown clean up code (only executed if the user hasn't aborted the shutdown).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void RequestShutdown([CanBeNull] Delegate afterShutdownCleanUpCode)
        {
            this.afterShutdownCleanUpCodeDelegate = afterShutdownCleanUpCode;
            new Thread(this.PerformShutdown) { Name = "ShutdownManager" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="e">
        /// A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.
        /// </param>
        /// <seealso cref="M:System.Windows.Application.OnStartup(StartupEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnStartup([NotNull] StartupEventArgs e)
        {
            // Check if we need to upgrade the user settings file
            if (Settings.Default.SettingsUpdateRequired)
            {
                try
                {
                    Debug.WriteLine("Updating user settings file to new version...");
                    Settings.Default.Upgrade();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to upgrade existing user settings file to this version: " + ex);
                }

                Settings.Default.SettingsUpdateRequired = false;
                Settings.Default.Save();
            }

            // Check for command line arguments
            var updatedToken = false;
            foreach (var arg in e.Args)
            {
                if (arg.StartsWith("opensky-agent://") || arg.StartsWith("opensky-agent-debug://"))
                {
                    var appTokenUri = arg.Replace("opensky-agent://", string.Empty).Replace("opensky-agent-debug://", string.Empty).TrimEnd('/');
                    var parameters = appTokenUri.Split('&');

                    string token = null;
                    DateTime? tokenExpiration = null;
                    string refresh = null;
                    DateTime? refreshExpiration = null;
                    string user = null;

                    foreach (var parameter in parameters)
                    {
                        if (parameter.StartsWith("token="))
                        {
                            token = parameter.Replace("token=", string.Empty);
                        }

                        if (parameter.StartsWith("tokenExpiration="))
                        {
                            if (long.TryParse(parameter.Replace("tokenExpiration=", string.Empty), out var ticks))
                            {
                                tokenExpiration = DateTime.FromFileTimeUtc(ticks);
                            }
                        }

                        if (parameter.StartsWith("refresh="))
                        {
                            refresh = parameter.Replace("refresh=", string.Empty);
                        }

                        if (parameter.StartsWith("refreshExpiration="))
                        {
                            if (long.TryParse(parameter.Replace("refreshExpiration=", string.Empty), out var ticks))
                            {
                                refreshExpiration = DateTime.FromFileTimeUtc(ticks);
                            }
                        }

                        if (parameter.StartsWith("user="))
                        {
                            user = parameter.Replace("user=", string.Empty);
                        }
                    }

                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refresh) && !string.IsNullOrEmpty(user) && tokenExpiration.HasValue && refreshExpiration.HasValue)
                    {
                        Settings.Default.OpenSkyApiToken = token;
                        Settings.Default.OpenSkyTokenExpiration = tokenExpiration.Value;
                        Settings.Default.OpenSkyRefreshToken = refresh;
                        Settings.Default.OpenSkyRefreshTokenExpiration = refreshExpiration.Value;
                        Settings.Default.OpenSkyUsername = user;
                        Settings.Default.Save();
                        updatedToken = true;
                    }
                    else
                    {
                        ModernWpf.MessageBox.Show("Invalid command line parameters, aborting.", "OpenSky", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Environment.Exit(1);
                    }
                }
            }

            // Ensure single instance
            try
            {
                if (!Mutex.WaitOne(TimeSpan.FromSeconds(3), false))
                {
                    if (!updatedToken)
                    {
                        ModernWpf.MessageBox.Show("The OpenSky flight tracking agent is already running.", "OpenSky", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        // We updated the token in the settings, before we exit let the running instance now about this
                        var client = new XDMessagingClient();
                        var broadcaster = client.Broadcasters.GetBroadcasterForMode(XDTransportMode.Compatibility);
                        broadcaster.SendToChannel("OPENSKY-AGENT", "TokensUpdated");
                    }

                    Environment.Exit(1);
                }
            }
            catch (AbandonedMutexException)
            {
                // Nothing to do here, lets continue starting the application
            }
            catch (Exception)
            {
                // Something went wrong, lets abort
                Views.Startup.StartupFailed = true;
                Environment.Exit(2);
            }

            // Add debug log handler
#if DEBUG
            var openSkyFolder = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky");
            if (!Directory.Exists(openSkyFolder))
            {
                Directory.CreateDirectory(openSkyFolder);
            }

            var logFilePath = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky\\agent_debug.log");
            var traceListener = new DateTimeTextWriterTraceListener(File.Open(logFilePath, FileMode.Append, FileAccess.Write));
            Debug.AutoFlush = true;
            Debug.Listeners.Add(traceListener);
#endif
            Debug.WriteLine("========================================================================================");
            Debug.WriteLine($"OPENSKY AGENT {Assembly.GetExecutingAssembly().GetName().Version.ToString(4)} STARTING UP");
            Debug.WriteLine("========================================================================================");

            // Unexpected error handler
            this.DispatcherUnhandledException += AppDispatcherUnhandledException;

            // Initialize speech sound pack manager
            SpeechSoundPacks.InitializeSpeechSoundPacks(Settings.Default.SoundPack, Settings.Default.TextToSpeechVoice);

            // Initialize selected simulator interface
            var simulatorInterface = Settings.Default.SimulatorInterface;
            if (SimConnect.SimulatorInterfaceName.Equals(simulatorInterface, StringComparison.InvariantCultureIgnoreCase))
            {
                Simulator.Simulator.SetSimulatorInstance(new SimConnect(Settings.Default.SimConnectHostName, Settings.Default.SimConnectPort, AgentOpenSkyService.Instance));
            }

            if (UdpXPlane11.UdpXPlane11.SimulatorInterfaceName.Equals(simulatorInterface, StringComparison.InvariantCultureIgnoreCase))
            {
                Simulator.Simulator.SetSimulatorInstance(new UdpXPlane11.UdpXPlane11(Settings.Default.XPlaneIPAddress, Settings.Default.XPlanePort, AgentOpenSkyService.Instance));
            }

            if (Simulator.Simulator.Instance != null)
            {
                Simulator.Simulator.Instance.LandingReported += (_, landingReportNotification) =>
                {
                    if (landingReportNotification.Equals(LandingReportNotification.Parse(Settings.Default.LandingReportNotification)))
                    {
                        UpdateGUIDelegate showNotification = () => new Views.LandingReport().Show();
                        Current.Dispatcher.BeginInvoke(showNotification);
                    }
                };
            }

            // Continue startup
            base.OnStartup(e);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Handles unhandled exceptions.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// The sender of the event.
        /// </param>
        /// <param name="e">
        /// The event arguments, containing the exception.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void AppDispatcherUnhandledException(
            [CanBeNull] object sender,
            [NotNull] DispatcherUnhandledExceptionEventArgs e)
        {
            var crashReport = "==============================================================================\r\n";
            crashReport += "  OPENSKY AGENT CRASH REPORT\r\n";
            crashReport += "  " + DateTime.Now + "\r\n";
            crashReport += "==============================================================================\r\n";
            crashReport += e.Exception + "\r\n";
            crashReport += "==============================================================================\r\n";
            crashReport += "\r\n\r\n";

            Debug.WriteLine(crashReport);
            var filePath = Environment.ExpandEnvironmentVariables("%localappdata%\\OpenSky\\agent_crash.log");

            try
            {
                File.AppendAllText(filePath, crashReport);
                ModernWpf.MessageBox.Show(
                    e.Exception.Message + "\r\n\r\nPlease check agent_crash.log for details!",
                    "Unexpected error!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception)
            {
                ModernWpf.MessageBox.Show(e.Exception.ToString(), "Unexpected error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Environment.Exit(3);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Performs the shutdown actions in a background thread.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PerformShutdown()
        {
            // Check if we are currently tracking a flight
            if (Simulator.Simulator.Instance.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
            {
                Debug.WriteLine("User requested shutdown, but flight tracking is still in progress...");
                MessageBoxResult? answer = MessageBoxResult.None;
                UpdateGUIDelegate askCancelTracking = () => answer = ModernWpf.MessageBox.Show("You have an active flight tracking session, are you sure you want to quit and loose all progress?", "Quit OpenSky?", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                this.Dispatcher.Invoke(askCancelTracking);
                if (answer != MessageBoxResult.Yes)
                {
                    Debug.WriteLine("Shutdown aborted");
                    return;
                }
            }

            // Run clean-up delegate code
            if (this.afterShutdownCleanUpCodeDelegate != null)
            {
                this.Dispatcher?.Invoke(this.afterShutdownCleanUpCodeDelegate);
            }

            // Do the actual WPF application shutdown (this can't be cancelled anymore)
            UpdateGUIDelegate shutdownApp = this.Shutdown;
            this.Dispatcher?.BeginInvoke(shutdownApp);
        }
    }
}
