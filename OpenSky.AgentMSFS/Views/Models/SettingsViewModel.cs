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
    using System.IO;
    using System.Linq;
    using System.Speech.Synthesis;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using JetBrains.Annotations;

    using Microsoft.Win32;

    using OpenSky.AgentMSFS.Controls;
    using OpenSky.AgentMSFS.Controls.Models;
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
        /// The Bing maps key.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string bingMapsKey;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are there changes to the settings to be saved?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isDirty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The profile image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private BitmapImage profileImage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected landing report notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LandingReportNotification selectedLandingReportNotification;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected sound pack.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string selectedSoundPack;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected text to speech voice.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string selectedTextToSpeechVoice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simBrief username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string simBriefUsername;

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
            this.SaveSettingsCommand = new AsynchronousCommand(this.SaveSettings, false);
            this.RestoreDefaultsCommand = new Command(this.RestoreDefaults);
            this.TestTextToSpeechVoiceCommand = new Command(this.TestTextToSpeechVoice);
            this.LoginOpenSkyUserCommand = new Command(this.LoginOpenSkyUser, !this.UserSession.IsUserLoggedIn);
            this.LogoutOpenSkyUserCommand = new AsynchronousCommand(this.LogoutOpenSkyUser, this.UserSession.IsUserLoggedIn);
            this.ChangePasswordCommand = new Command(this.ChangePassword, this.UserSession.IsUserLoggedIn);
            this.UpdateProfileImageCommand = new AsynchronousCommand(this.UpdateProfileImage, this.UserSession.IsUserLoggedIn);
            this.ClearSoundPackCommand = new Command(this.ClearSoundPack);

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
            this.BingMapsKey = UserSessionService.Instance.LinkedAccounts?.BingMapsKey;
            this.SimBriefUsername = UserSessionService.Instance.LinkedAccounts?.SimbriefUsername;
            this.SelectedLandingReportNotification = LandingReportNotification.Parse(Properties.Settings.Default.LandingReportNotification);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SoundPack))
            {
                this.SelectedSoundPack = Properties.Settings.Default.SoundPack;
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.TextToSpeechVoice))
            {
                this.SelectedTextToSpeechVoice = Properties.Settings.Default.TextToSpeechVoice;
            }

            // Load profile image
            if (UserSessionService.Instance.AccountOverview?.ProfileImage?.Length > 0)
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(UserSessionService.Instance.AccountOverview?.ProfileImage))
                {
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }

                image.Freeze();
                this.ProfileImage = image;
            }
            else
            {
                this.ProfileImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.AgentMSFS;component/Resources/profile200.png"));
            }

            // Make sure we are notified if the UserSession service changes user logged in status
            this.UserSession.PropertyChanged += this.UserSessionPropertyChanged;

            // No changes, just us loading
            this.IsDirty = false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Bing maps key.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string BingMapsKey
        {
            get => this.bingMapsKey;

            set
            {
                if (Equals(this.bingMapsKey, value))
                {
                    return;
                }

                this.bingMapsKey = value;
                this.NotifyPropertyChanged();
                this.IsDirty = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the change password command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ChangePasswordCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear sound pack command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearSoundPackCommand { get; }

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
        /// Gets or sets the loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LoadingText
        {
            get => this.loadingText;

            set
            {
                if (Equals(this.loadingText, value))
                {
                    return;
                }

                this.loadingText = value;
                this.NotifyPropertyChanged();
            }
        }

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
        /// Gets or sets the profile image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public BitmapImage ProfileImage
        {
            get => this.profileImage;

            set
            {
                if (Equals(this.profileImage, value))
                {
                    return;
                }

                this.profileImage = value;
                this.NotifyPropertyChanged();
            }
        }

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
        public AsynchronousCommand SaveSettingsCommand { get; }

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
        /// Gets or sets the selected sound pack.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SelectedSoundPack
        {
            get => this.selectedSoundPack;

            set
            {
                if (Equals(this.selectedSoundPack, value))
                {
                    return;
                }

                this.selectedSoundPack = value;
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
        /// Gets or sets the simBrief username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SimBriefUsername
        {
            get => this.simBriefUsername;

            set
            {
                if (Equals(this.simBriefUsername, value))
                {
                    return;
                }

                this.simBriefUsername = value;
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
        /// Gets the available sound packs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<string> SoundPacks => SpeechSoundPacks.Instance.SoundPacks.Keys.ToList();

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
        /// Gets the update profile image command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand UpdateProfileImageCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user session.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public UserSessionService UserSession => UserSessionService.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Change password (opens page in browser).
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ChangePassword()
        {
            Process.Start(Properties.Settings.Default.OpenSkyChangePasswordUrl);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the selected sound pack.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearSoundPack()
        {
            this.SelectedSoundPack = null;
        }

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

                            var notification = new OpenSkyNotification("Error revoking application token", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.LogoutOpenSkyUserCommand, "Error revoking application token", false);
            }

            this.UserSession.Logout();
            this.NotifyPropertyChanged(nameof(this.UserSession));

            this.LogoutOpenSkyUserCommand.ReportProgress(
                () =>
                {
                    var wasDirty = this.IsDirty;
                    this.BingMapsKey = null;
                    this.SimBriefUsername = null;
                    this.ProfileImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.AgentMSFS;component/Resources/profile200.png"));
                    this.IsDirty = wasDirty;
                });
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
            var messageBox = new OpenSkyMessageBox(
                "Restore settings?",
                "Are you sure you want to restore all default settings except for keys and users?",
                MessageBoxButton.YesNo,
                ExtendedMessageBoxImage.Question);
            messageBox.Closed += (_, _) =>
            {
                if (messageBox.Result == ExtendedMessageBoxResult.Yes)
                {
                    Debug.WriteLine("Resetting settings to defaults...");
                    this.SimulatorHostName = "localhost";
                    this.SimulatorPort = 500;
                }
            };
            this.ViewReference.ShowMessageBox(messageBox);
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
            this.LoadingText = "Saving settings...";

            // Save local settings
            try
            {
                Properties.Settings.Default.SimulatorHostName = this.SimulatorHostName;
                Properties.Settings.Default.SimulatorPort = this.SimulatorPort;
                Properties.Settings.Default.LandingReportNotification = this.SelectedLandingReportNotification?.NotificationID ?? 1;
                Properties.Settings.Default.SoundPack = this.SelectedSoundPack;
                SpeechSoundPacks.Instance.SelectedSoundPack = this.SelectedSoundPack;
                Properties.Settings.Default.TextToSpeechVoice = this.SelectedTextToSpeechVoice;
                if (!string.IsNullOrEmpty(this.SelectedTextToSpeechVoice))
                {
                    SpeechSoundPacks.Instance.SetSpeechVoice(this.SelectedTextToSpeechVoice);
                }

                Properties.Settings.Default.Save();
                this.SaveSettingsCommand.ReportProgress(() => this.IsDirty = false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving settings: " + ex);
                this.SaveSettingsCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error saving settings", ex.Message, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        this.ViewReference.ShowNotification(notification);
                    });
            }

            // Save server-side settings
            try
            {
                var linkedAccounts = new LinkedAccounts
                {
                    BingMapsKey = this.BingMapsKey,
                    SimbriefUsername = this.SimBriefUsername
                };

                var result = OpenSkyService.Instance.UpdateLinkedAccountsAsync(linkedAccounts).Result;
                if (!result.IsError)
                {
                    _ = UserSessionService.Instance.RefreshLinkedAccounts();
                    this.SaveSettingsCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Saving settings", "Successfully saved settings.", MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            this.ViewReference.ShowNotification(notification);
                        });
                }
                else
                {
                    this.SaveSettingsCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error saving settings: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error saving settings", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SaveSettingsCommand, "Error saving settings.");
            }

            this.LoadingText = null;
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
                    var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error testing voice", ex.Message, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the profile image.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateProfileImage()
        {
            bool? answer = null;
            string fileName = null;
            this.UpdateProfileImageCommand.ReportProgress(
                () =>
                {
                    var openDialog = new OpenFileDialog
                    {
                        Title = "Select new profile image",
                        CheckFileExists = true,
                        Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
                    };

                    answer = openDialog.ShowDialog();
                    if (answer == true)
                    {
                        fileName = openDialog.FileName;
                    }
                },
                true);

            if (answer != true || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            try
            {
                var result = OpenSkyService.Instance.UploadProfileImageAsync(new FileParameter(File.OpenRead(fileName), fileName, fileName.ToLowerInvariant().EndsWith(".png") ? "image/png" : "image/jpeg")).Result;
                if (result.IsError)
                {
                    this.UpdateProfileImageCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error updating profile image: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error updating profile image", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
                else
                {
                    _ = UserSessionService.Instance.RefreshUserAccountOverview().Result;

                    // Load profile image
                    if (UserSessionService.Instance.AccountOverview?.ProfileImage?.Length > 0)
                    {
                        var image = new BitmapImage();
                        using (var mem = new MemoryStream(UserSessionService.Instance.AccountOverview?.ProfileImage))
                        {
                            image.BeginInit();
                            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.UriSource = null;
                            image.StreamSource = mem;
                            image.EndInit();
                        }

                        image.Freeze();
                        this.ProfileImage = image;
                    }
                    else
                    {
                        this.ProfileImage = new BitmapImage(new Uri("pack://application:,,,/OpenSky.AgentMSFS;component/Resources/profile200.png"));
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.UpdateProfileImageCommand, "Error updating profile image.");
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
                // Update login/logout buttons
                UpdateGUIDelegate updateCommands = () =>
                {
                    this.LoginOpenSkyUserCommand.CanExecute = !this.UserSession.IsUserLoggedIn;
                    this.LogoutOpenSkyUserCommand.CanExecute = this.UserSession.IsUserLoggedIn;
                    this.ChangePasswordCommand.CanExecute = this.UserSession.IsUserLoggedIn;
                    this.UpdateProfileImageCommand.CanExecute = this.UserSession.IsUserLoggedIn;
                };
                Application.Current.Dispatcher.BeginInvoke(updateCommands);

                if (this.UserSession.IsUserLoggedIn)
                {
                    try
                    {
                        UpdateGUIDelegate updateUserSettings = () =>
                        {
                            try
                            {
                                // Load profile image
                                if (UserSessionService.Instance.AccountOverview?.ProfileImage?.Length > 0)
                                {
                                    var image = new BitmapImage();
                                    using (var mem = new MemoryStream(UserSessionService.Instance.AccountOverview?.ProfileImage))
                                    {
                                        image.BeginInit();
                                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                        image.CacheOption = BitmapCacheOption.OnLoad;
                                        image.UriSource = null;
                                        image.StreamSource = mem;
                                        image.EndInit();
                                    }

                                    image.Freeze();
                                    this.ProfileImage = image;
                                }

                                var wasDirty = this.IsDirty;
                                this.BingMapsKey = UserSessionService.Instance.LinkedAccounts?.BingMapsKey;
                                this.SimBriefUsername = UserSessionService.Instance.LinkedAccounts?.SimbriefUsername;
                                this.IsDirty = wasDirty;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error updating user settings after login: " + ex);
                            }
                        };
                        Application.Current.Dispatcher.BeginInvoke(updateUserSettings);

                        this.NotifyPropertyChanged(nameof(this.UserSession));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error processing user after-login: " + ex);
                    }
                }
            }
        }
    }
}