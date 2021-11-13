// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Speech.Synthesis;
    using System.Windows;

    using JetBrains.Annotations;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Settings window view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 23/03/2021.
    /// </remarks>
    /// <seealso cref="ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class SettingsViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are there changes to the settings to be saved?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isDirty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected landing report notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LandingReportNotification selectedLandingReportNotification;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected text to speech voice.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string selectedTextToSpeechVoice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simulator host name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string simulatorHostName;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simulator port.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private uint simulatorPort;


        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "This is ok here as we want the voices for all cultures")]
        public SettingsViewModel()
        {
            // Create command first so that IsDirty can set the CanExecute property
            this.SaveSettingsCommand = new Command(this.SaveSettings, false);
            this.RestoreDefaultsCommand = new Command(this.RestoreDefaults);
            this.TestTextToSpeechVoiceCommand = new Command(this.TestTextToSpeechVoice);
            this.LoginOpenSkyUserCommand = new Command(this.LoginOpenSkyUser, !this.UserSession.IsUserLoggedIn);
            this.LogoutOpenSkyUserCommand = new AsynchronousCommand(this.LogoutOpenSkyUser, this.UserSession.IsUserLoggedIn);

            // Fetch available text to speech voices
            var speech = new SpeechSynthesizer();
            this.TextToSpeechVoices = new List<string>();
            foreach (var voice in speech.GetInstalledVoices())
            {
                this.TextToSpeechVoices.Add(voice.VoiceInfo.Name);
            }

            // Set the default voice in case the user hasn't selected one (yet)
            this.SelectedTextToSpeechVoice = speech.Voice.Name;

            // Load settings
            Properties.Settings.Default.Reload();
            this.SimulatorHostName = Properties.Settings.Default.SimulatorHostName;
            this.SimulatorPort = Properties.Settings.Default.SimulatorPort;
            this.SelectedLandingReportNotification = LandingReportNotification.Parse(Properties.Settings.Default.LandingReportNotification);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.TextToSpeechVoice))
            {
                this.SelectedTextToSpeechVoice = Properties.Settings.Default.TextToSpeechVoice;
            }

            // Make sure we are notified if the UserSession service changes user logged in status
            this.UserSession.PropertyChanged += this.UserSessionPropertyChanged;

            // No changes, just us loading
            this.IsDirty = false;
        }

        

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether there are changes to the settings to be saved.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsDirty
        {
            get => this.isDirty;

            set
            {
                if (Equals(this.isDirty, value))
                {
                    return;
                }

                this.isDirty = value;
                this.NotifyPropertyChanged();
                this.SaveSettingsCommand.CanExecute = value;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing report notifications.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<LandingReportNotification> LandingReportNotifications => LandingReportNotification.GetLandingReportNotifications();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the login OpenSky user command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command LoginOpenSkyUserCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the logout OpenSky user command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand LogoutOpenSkyUserCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the restore defaults command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command RestoreDefaultsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the save settings command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Command SaveSettingsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected landing report notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LandingReportNotification SelectedLandingReportNotification
        {
            get => this.selectedLandingReportNotification;

            set
            {
                if (Equals(this.selectedLandingReportNotification, value))
                {
                    return;
                }

                this.selectedLandingReportNotification = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected text to speech voice.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SelectedTextToSpeechVoice
        {
            get => this.selectedTextToSpeechVoice;

            set
            {
                if (Equals(this.selectedTextToSpeechVoice, value))
                {
                    return;
                }

                this.selectedTextToSpeechVoice = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the simulator host name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SimulatorHostName
        {
            get => this.simulatorHostName;

            set
            {
                if (Equals(this.simulatorHostName, value))
                {
                    return;
                }

                this.simulatorHostName = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the simulator port.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public uint SimulatorPort
        {
            get => this.simulatorPort;

            set
            {
                if (Equals(this.simulatorPort, value))
                {
                    return;
                }

                this.simulatorPort = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the test text to speech voice command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command TestTextToSpeechVoiceCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available text to speech voices.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<string> TextToSpeechVoices { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user session.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public UserSessionService UserSession => UserSessionService.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Login OpenSky user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LoginOpenSkyUser()
        {
            Process.Start(Properties.Settings.Default.OpenSkyTokenUrl);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Logout OpenSky user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LogoutOpenSkyUser()
        {
            try
            {
                var result = OpenSkyService.Instance.RevokeTokenAsync(new RevokeToken { Token = this.UserSession.RefreshToken }).Result;
                if (result.IsError)
                {
                    this.LogoutOpenSkyUserCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error revoking application token: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error revoking application token", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.LogoutOpenSkyUserCommand, "Error revoking application token", false);
            }

            this.UserSession.Logout();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Restore default settings (except keys and users).
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RestoreDefaults()
        {
            var answer = ModernWpf.MessageBox.Show("Are you sure you want to restore all default settings except for keys and users?", "Restore settings?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes)
            {
                Debug.WriteLine("Resetting settings to defaults...");
                this.SimulatorHostName = "localhost";
                this.SimulatorPort = 500;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SaveSettings()
        {
            Debug.WriteLine("Saving user settings...");
            try
            {
                Properties.Settings.Default.SimulatorHostName = this.SimulatorHostName;
                Properties.Settings.Default.SimulatorPort = this.SimulatorPort;
                Properties.Settings.Default.LandingReportNotification = this.SelectedLandingReportNotification?.NotificationID ?? 1;
                Properties.Settings.Default.TextToSpeechVoice = this.SelectedTextToSpeechVoice;
                if (!string.IsNullOrEmpty(this.SelectedTextToSpeechVoice))
                {
                    SimConnect.SimConnect.Instance.SetSpeechVoice(this.SelectedTextToSpeechVoice);
                }

                Properties.Settings.Default.Save();
                this.IsDirty = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving settings: " + ex);
                ModernWpf.MessageBox.Show(ex.Message, "Error saving settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tests text to speech voice.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/04/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void TestTextToSpeechVoice()
        {
            if (!string.IsNullOrEmpty(this.SelectedTextToSpeechVoice))
            {
                try
                {
                    var speech = new SpeechSynthesizer();
                    speech.SelectVoice(this.SelectedTextToSpeechVoice);
                    speech.SpeakAsync("OpenSky flight number 81 is now ready for boarding.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error testing text to speech voice: " + ex);
                    ModernWpf.MessageBox.Show(ex.Message, "Error testing voice", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// User session property changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Property changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void UserSessionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.UserSession.IsUserLoggedIn))
            {
                UpdateGUIDelegate updateCommands = () =>
                {
                    this.LoginOpenSkyUserCommand.CanExecute = !this.UserSession.IsUserLoggedIn;
                    this.LogoutOpenSkyUserCommand.CanExecute = this.UserSession.IsUserLoggedIn;
                };
                Application.Current.Dispatcher.BeginInvoke(updateCommands);

                if (this.UserSession.IsUserLoggedIn)
                {
                    try
                    {
                        _ = UserSessionService.Instance.RefreshLinkedAccounts().Result;

                        this.NotifyPropertyChanged(nameof(this.UserSession));
                    }
                    catch
                    {
                        // Ignore
                    }
                    
                }
            }
        }
    }
}