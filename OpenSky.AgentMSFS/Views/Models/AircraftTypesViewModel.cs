// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypesViewModel.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows;

    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.SimConnect;
    using OpenSky.AgentMSFS.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft types view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTypesViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The add aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility addAircraftVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type details visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility aircraftTypeDetailsVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The category.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftTypeCategory category;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The optional comments (what mod was loaded, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string comments = string.Empty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if is aircraft type is vanilla, false if not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isVanilla;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type this one is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType isVariantOf;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The maximum price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int maximumPrice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The minimum price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int minimumPrice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The name of the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string name = string.Empty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if this type needs a co-pilot.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool needsCoPilot;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType selectedAircraftType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The version number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int versionNumber = 1;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTypesViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypesViewModel()
        {
            this.ExistingAircraftTypes = new ObservableCollection<AircraftType>();
            this.SelectedAircraftTypes = new ObservableCollection<AircraftType>();

            this.GetUserRolesCommand = new AsynchronousCommand(this.GetUserRoles);
            this.RefreshAircraftTypesCommand = new AsynchronousCommand(this.RefreshAircraftTypes);
            this.AddAircraftTypeCommand = new AsynchronousCommand(this.AddAircraftType);
            this.StartAddAircraftCommand = new Command(this.StartAddAircraft);
            this.CancelAddAircraftCommand = new Command(this.CancelAddAircraft);
            this.ClearVariantOfCommand = new Command(this.ClearVariantOf);
            this.ClearTypeSelectionCommand = new Command(this.ClearTypeSelection);
            this.EnableTypeCommand = new AsynchronousCommand(this.EnableType, false);
            this.DisableTypeCommand = new AsynchronousCommand(this.DisableType, false);

            this.GetUserRolesCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the add aircraft type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AddAircraftTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the add aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AddAircraftVisibility
        {
            get => this.addAircraftVisibility;

            set
            {
                if (Equals(this.addAircraftVisibility, value))
                {
                    return;
                }

                this.addAircraftVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type details visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AircraftTypeDetailsVisibility
        {
            get => this.aircraftTypeDetailsVisibility;

            set
            {
                if (Equals(this.aircraftTypeDetailsVisibility, value))
                {
                    return;
                }

                this.aircraftTypeDetailsVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the abort add aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CancelAddAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypeCategory Category
        {
            get => this.category;

            set
            {
                if (Equals(this.category, value))
                {
                    return;
                }

                this.category = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear type selection command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearTypeSelectionCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear VariantOf command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearVariantOfCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the optional comments (what mod was loaded, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Comments
        {
            get => this.comments;

            set
            {
                if (Equals(this.comments, value))
                {
                    return;
                }

                this.comments = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the disable type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DisableTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the enable type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand EnableTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of of the existing aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> ExistingAircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get user roles command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetUserRolesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft type is vanilla.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsVanilla
        {
            get => this.isVanilla;

            set
            {
                if (Equals(this.isVanilla, value))
                {
                    return;
                }

                this.isVanilla = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type this one is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType IsVariantOf
        {
            get => this.isVariantOf;

            set
            {
                if (Equals(this.isVariantOf, value))
                {
                    return;
                }

                this.isVariantOf = value;
                this.NotifyPropertyChanged();
            }
        }

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
        /// Gets or sets the maximum price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MaximumPrice
        {
            get => this.maximumPrice;

            set
            {
                if (Equals(this.maximumPrice, value))
                {
                    return;
                }

                this.maximumPrice = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the minimum price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MinimumPrice
        {
            get => this.minimumPrice;

            set
            {
                if (Equals(this.minimumPrice, value))
                {
                    return;
                }

                this.minimumPrice = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name
        {
            get => this.name;

            set
            {
                if (Equals(this.name, value))
                {
                    return;
                }

                this.name = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the type needs a co-pilot.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool NeedsCoPilot
        {
            get => this.needsCoPilot;

            set
            {
                if (Equals(this.needsCoPilot, value))
                {
                    return;
                }

                this.needsCoPilot = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh aircraft types command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAircraftTypesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the the selected aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType SelectedAircraftType
        {
            get => this.selectedAircraftType;

            set
            {
                if (Equals(this.selectedAircraftType, value))
                {
                    return;
                }

                this.selectedAircraftType = value;
                this.NotifyPropertyChanged();
                this.AircraftTypeDetailsVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;

                if (UserSessionService.Instance.IsModerator)
                {
                    this.EnableTypeCommand.CanExecute = value != null;
                    this.DisableTypeCommand.CanExecute = value != null;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> SelectedAircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the SimConnect object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SimConnect SimConnect => SimConnect.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start add aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command StartAddAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int VersionNumber
        {
            get => this.versionNumber;

            set
            {
                if (Equals(this.versionNumber, value))
                {
                    return;
                }

                this.versionNumber = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the current plane identity to the CSV.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AddAircraftType()
        {
            if (!this.SimConnect.Connected)
            {
                this.AddAircraftTypeCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Not connected to sim!", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                return;
            }

            if (string.IsNullOrEmpty(this.SimConnect.PlaneIdentity.AtcModel) || string.IsNullOrEmpty(this.SimConnect.PlaneIdentity.AtcType))
            {
                this.AddAircraftTypeCommand.ReportProgress(
                    () => ModernWpf.MessageBox.Show("ATC plane model or type not set, are you connected to the simulator and is the plane loaded correctly?", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                return;
            }

            if (string.IsNullOrEmpty(this.Name) || this.Name.Length < 5)
            {
                this.AddAircraftTypeCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Name not specified or less than 5 characters!", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                return;
            }

            var newAircraftType = new AircraftType
            {
                Id = Guid.Empty, // API will assign this
                UploaderID = "OpenSkyUser", // API will assign this
                AtcType = this.SimConnect.PlaneIdentity.AtcType,
                AtcModel = this.SimConnect.PlaneIdentity.AtcModel,
                EngineType = this.SimConnect.PlaneIdentity.EngineType,
                EngineCount = this.SimConnect.PlaneIdentity.EngineCount,
                EmptyWeight = this.SimConnect.WeightAndBalance.EmptyWeight,
                FuelTotalCapacity = this.SimConnect.WeightAndBalance.FuelTotalCapacity,
                MaxGrossWeight = this.SimConnect.WeightAndBalance.MaxGrossWeight,
                FlapsAvailable = this.SimConnect.PlaneIdentity.FlapsAvailable,
                IsGearRetractable = this.SimConnect.PlaneIdentity.GearRetractable,
                Name = this.Name,
                VersionNumber = this.VersionNumber,
                Category = this.Category,
                IsVanilla = this.IsVanilla,
                NeedsCoPilot = this.NeedsCoPilot,
                IsVariantOf = this.IsVariantOf?.Id,
                MinPrice = this.MinimumPrice,
                MaxPrice = this.MaximumPrice,
                Comments = this.Comments
            };

            try
            {
                var result = OpenSkyService.Instance.AddAircraftTypeAsync(newAircraftType).Result;
                if (!result.IsError)
                {
                    this.AddAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            ModernWpf.MessageBox.Show(result.Message, "New aircraft type", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.CancelAddAircraft(); // This resets the input form and hides the groupbox
                        });
                }
                else
                {
                    this.AddAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error adding new aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error adding new aircraft type", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.AddAircraftTypeCommand, "Error adding new aircraft type");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cancel add aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CancelAddAircraft()
        {
            this.AddAircraftVisibility = Visibility.Collapsed;

            this.Name = null;
            this.VersionNumber = 1;
            this.Category = AircraftTypeCategory.SEP;
            this.IsVanilla = false;
            this.NeedsCoPilot = false;
            this.IsVariantOf = null;
            this.MinimumPrice = 0;
            this.MaximumPrice = 0;
            this.Comments = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the type selection.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearTypeSelection()
        {
            this.SelectedAircraftType = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the IsVariantOf property.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearVariantOf()
        {
            this.IsVariantOf = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Disables the select aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DisableType()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.DisableTypeCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Please select exactly one aircraft type!", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                return;
            }

            this.LoadingText = "Disabling aircraft type";
            try
            {
                var result = OpenSkyService.Instance.DisableAircraftTypeAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.DisableTypeCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.DisableTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error disabling aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error disabling aircraft type", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.DisableTypeCommand, "Error disabling aircraft type");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Enables the select aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void EnableType()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.EnableTypeCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Please select exactly one aircraft type!", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                return;
            }

            this.LoadingText = "Enabling aircraft type";
            try
            {
                var result = OpenSkyService.Instance.EnableAircraftTypeAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.EnableTypeCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.EnableTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error enabling aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error enabling aircraft type", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.EnableTypeCommand, "Error enabling aircraft type");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user's OpenSky roles.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetUserRoles()
        {
            this.LoadingText = "Fetching your roles";
            var result = UserSessionService.Instance.UpdateUserRoles().Result;
            if (result)
            {
                this.GetUserRolesCommand.ReportProgress(() => this.RefreshAircraftTypesCommand.DoExecute(null));
            }
            else
            {
                this.GetUserRolesCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Error fetching your user roles.", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }

            this.LoadingText = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the list of existing aircraft types.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAircraftTypes()
        {
            this.LoadingText = "Refreshing aircraft types";
            try
            {
                var result = OpenSkyService.Instance.GetAllAircraftTypesAsync().Result;
                if (!result.IsError)
                {
                    this.RefreshAircraftTypesCommand.ReportProgress(
                        () =>
                        {
                            this.ExistingAircraftTypes.Clear();
                            foreach (var type in result.Data)
                            {
                                this.ExistingAircraftTypes.Add(type);
                            }
                        });
                }
                else
                {
                    this.RefreshAircraftTypesCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing aircraft types: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing aircraft types", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshAircraftTypesCommand, "Error refreshing aircraft types");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts add aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartAddAircraft()
        {
            this.AddAircraftVisibility = Visibility.Visible;
        }
    }
}