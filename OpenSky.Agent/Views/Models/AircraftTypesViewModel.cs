// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypesViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using OpenSky.Agent.Controls;
    using OpenSky.Agent.Controls.Models;
    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.Agent.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft types view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Agent.MVVM.ViewModel"/>
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
        /// The edit aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility editAircraftVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type currently being edited.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType editedAircraftType = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The aircraft type the edited type is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType editedIsVariantOf;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The next version of the edited type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType editedNextVersion;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to include, false to exclude the type in the world population.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool includeInWorldPopulation;

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
        /// The manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string manufacturer;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The maximum price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int maximumPrice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The maximum payload delta allowed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int maxPayloadDeltaAllowed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The minimum price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int minimumPrice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The minimum length of the runway required by the type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int minimumRunwayLength;

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
        /// True if this type needs a flight engineer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool needsFlightEngineer;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if requires manual fuelling.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool requiresManualFuelling;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if requires manual loading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool requiresManualLoading;

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
            this.ClearVariantOfNewCommand = new Command(this.ClearVariantOfNew);
            this.ClearTypeSelectionCommand = new Command(this.ClearTypeSelection);
            this.EnableTypeCommand = new AsynchronousCommand(this.EnableType, false);
            this.DisableTypeCommand = new AsynchronousCommand(this.DisableType, false);
            this.EnableDetailedChecksCommand = new AsynchronousCommand(this.EnableDetailedChecks, false);
            this.DisableDetailedChecksCommand = new AsynchronousCommand(this.DisableDetailedChecks, false);
            this.DeleteTypeCommand = new AsynchronousCommand(this.DeleteType, false);
            this.StartEditAircraftCommand = new Command(this.StartEditAircraft, false);
            this.CancelEditAircraftCommand = new Command(this.CancelEditAircraft, false);
            this.ClearVariantOfEditedCommand = new Command(this.ClearVariantOfEdited);
            this.ClearNextVersionOfEditedCommand = new Command(this.ClearNextVersionOfEdited);
            this.SaveEditedAircraftTypeCommand = new AsynchronousCommand(this.SaveEditedAircraftType, false);
            this.IdentifyAircraftCommand = new Command(this.IdentifyAircraft);

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
        /// Gets the cancel edit aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CancelEditAircraftCommand { get; }

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
        /// Gets the clear edited next version command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearNextVersionOfEditedCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear type selection command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearTypeSelectionCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear VariantOf edited command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearVariantOfEditedCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear VariantOfNew command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearVariantOfNewCommand { get; }

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
        /// Gets the delete type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DeleteTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the enable detailed checks command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DisableDetailedChecksCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the disable type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand DisableTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the edit aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility EditAircraftVisibility
        {
            get => this.editAircraftVisibility;

            set
            {
                if (Equals(this.editAircraftVisibility, value))
                {
                    return;
                }

                this.editAircraftVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type currently being edited.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType EditedAircraftType
        {
            get => this.editedAircraftType;

            set
            {
                if (Equals(this.editedAircraftType, value))
                {
                    return;
                }

                this.editedAircraftType = value;
                this.NotifyPropertyChanged();

                this.CancelEditAircraftCommand.CanExecute = value != null;
                this.SaveEditedAircraftTypeCommand.CanExecute = value != null;
                this.EditAircraftVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft type the edited type is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType EditedIsVariantOf
        {
            get => this.editedIsVariantOf;

            set
            {
                if (Equals(this.editedIsVariantOf, value))
                {
                    return;
                }

                this.editedIsVariantOf = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the next version of the edited type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType EditedNextVersion
        {
            get => this.editedNextVersion;

            set
            {
                if (Equals(this.editedNextVersion, value))
                {
                    return;
                }

                this.editedNextVersion = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the enable detailed checks command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand EnableDetailedChecksCommand { get; }

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
        /// Gets the identify aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command IdentifyAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to include or exclude the type from the world
        /// population.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IncludeInWorldPopulation
        {
            get => this.includeInWorldPopulation;

            set
            {
                if (Equals(this.includeInWorldPopulation, value))
                {
                    return;
                }

                this.includeInWorldPopulation = value;
                this.NotifyPropertyChanged();
            }
        }

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
        /// Gets or sets the manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Manufacturer
        {
            get => this.manufacturer;

            set
            {
                if (Equals(this.manufacturer, value))
                {
                    return;
                }

                this.manufacturer = value;
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
        /// Gets or sets the maximum payload delta allowed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MaxPayloadDeltaAllowed
        {
            get => this.maxPayloadDeltaAllowed;

            set
            {
                if (Equals(this.maxPayloadDeltaAllowed, value))
                {
                    return;
                }

                this.maxPayloadDeltaAllowed = value;
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
        /// Gets or sets the minimum runway length required by the type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MinimumRunwayLength
        {
            get => this.minimumRunwayLength;

            set
            {
                if (Equals(this.minimumRunwayLength, value))
                {
                    return;
                }

                this.minimumRunwayLength = value;
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
        /// Gets or sets a value indicating whether the type needs a flight engineer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool NeedsFlightEngineer
        {
            get => this.needsFlightEngineer;

            set
            {
                if (Equals(this.needsFlightEngineer, value))
                {
                    return;
                }

                this.needsFlightEngineer = value;
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
        /// Gets or sets a value indicating whether the aircraft requires manual fuelling.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool RequiresManualFuelling
        {
            get => this.requiresManualFuelling;

            set
            {
                if (Equals(this.requiresManualFuelling, value))
                {
                    return;
                }

                this.requiresManualFuelling = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the aircraft requires manual loading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool RequiresManualLoading
        {
            get => this.requiresManualLoading;

            set
            {
                if (Equals(this.requiresManualLoading, value))
                {
                    return;
                }

                this.requiresManualLoading = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the save edited aircraft type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SaveEditedAircraftTypeCommand { get; }

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
                    this.EnableDetailedChecksCommand.CanExecute = value != null;
                    this.DisableDetailedChecksCommand.CanExecute = value != null;
                    this.StartEditAircraftCommand.CanExecute = value != null;
                }

                if (UserSessionService.Instance.IsAdmin)
                {
                    this.DeleteTypeCommand.CanExecute = value != null;
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
        /// Gets the simulator object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Agent.Simulator.Simulator Simulator => Agent.Simulator.Simulator.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start add aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command StartAddAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start edit aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command StartEditAircraftCommand { get; }

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
            if (!this.Simulator.Connected)
            {
                this.AddAircraftTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error ", "Not connected to sim!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
                return;
            }

            if (string.IsNullOrEmpty(this.Simulator.AircraftIdentity.AtcModel) || string.IsNullOrEmpty(this.Simulator.AircraftIdentity.AtcType))
            {
                ExtendedMessageBoxResult? answer = null;
                this.AddAircraftTypeCommand.ReportProgress(
                    () =>
                    {
                        var messageBox = new OpenSkyMessageBox(
                            "Missing ATC identification",
                            "ATC plane model or type not set, are you connected to the simulator and is the plane loaded correctly? Are you sure you want to continue?",
                            MessageBoxButton.YesNo,
                            ExtendedMessageBoxImage.Hand);
                        messageBox.SetWarningColorStyle();
                        messageBox.Closed += (_, _) =>
                        {
                            answer = messageBox.Result;
                        };
                        this.ViewReference.ShowMessageBox(messageBox);
                    });
                while (answer == null && !SleepScheduler.IsShutdownInProgress)
                {
                    Thread.Sleep(500);
                }

                if (answer != ExtendedMessageBoxResult.Yes)
                {
                    return;
                }
            }

            if (string.IsNullOrEmpty(this.Name) || this.Name.Length < 5)
            {
                this.AddAircraftTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Name not specified or less than 5 characters!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
                return;
            }

            var newAircraftType = new AircraftType
            {
                Id = Guid.Empty, // API will assign this
                UploaderID = "OpenSkyUser", // API will assign this
                AtcType = !string.IsNullOrEmpty(this.Simulator.AircraftIdentity.AtcType) ? this.Simulator.AircraftIdentity.AtcType : "MISSING",
                AtcModel = !string.IsNullOrEmpty(this.Simulator.AircraftIdentity.AtcModel) ? this.Simulator.AircraftIdentity.AtcModel : "MISSING",
                EngineType = this.Simulator.AircraftIdentity.EngineType,
                EngineCount = this.Simulator.AircraftIdentity.EngineCount,
                EmptyWeight = this.Simulator.WeightAndBalance.EmptyWeight,
                FuelTotalCapacity = this.Simulator.WeightAndBalance.FuelTotalCapacity,
                FuelWeightPerGallon = Math.Round(this.Simulator.WeightAndBalance.FuelWeightPerGallon, 2),
                MaxGrossWeight = this.Simulator.WeightAndBalance.MaxGrossWeight,
                FlapsAvailable = this.Simulator.AircraftIdentity.FlapsAvailable,
                IsGearRetractable = this.Simulator.AircraftIdentity.GearRetractable,
                Name = this.Name,
                VersionNumber = this.VersionNumber,
                Manufacturer = this.Manufacturer,
                Category = this.Category,
                IsVanilla = this.IsVanilla,
                IncludeInWorldPopulation = this.IncludeInWorldPopulation,
                NeedsCoPilot = this.NeedsCoPilot,
                NeedsFlightEngineer = this.NeedsFlightEngineer,
                RequiresManualFuelling = this.RequiresManualFuelling,
                RequiresManualLoading = this.RequiresManualLoading,
                IsVariantOf = this.IsVariantOf?.Id,
                MinimumRunwayLength = this.MinimumRunwayLength,
                MinPrice = this.MinimumPrice,
                MaxPrice = this.MaximumPrice,
                MaxPayloadDeltaAllowed = this.MaxPayloadDeltaAllowed,
                Comments = this.Comments
            };

            this.LoadingText = "Adding new aircraft type";
            try
            {
                var result = AgentOpenSkyService.Instance.AddAircraftTypeAsync(newAircraftType).Result;
                if (!result.IsError)
                {
                    this.AddAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("New aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            this.ViewReference.ShowNotification(notification);
                            this.CancelAddAircraft(); // This resets the input form and hides the groupbox
                            this.RefreshAircraftTypesCommand.DoExecute(null);
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

                            var notification = new OpenSkyNotification("Error adding new aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.AddAircraftTypeCommand, "Error adding new aircraft type");
            }
            finally
            {
                this.LoadingText = null;
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
            this.Manufacturer = null;
            this.Category = AircraftTypeCategory.SEP;
            this.IsVanilla = false;
            this.IncludeInWorldPopulation = false;
            this.NeedsCoPilot = false;
            this.NeedsFlightEngineer = false;
            this.IsVariantOf = null;
            this.MinimumPrice = 0;
            this.MaximumPrice = 0;
            this.MinimumRunwayLength = 0;
            this.Comments = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cancel edit aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CancelEditAircraft()
        {
            this.EditedAircraftType = null;
            this.EditedIsVariantOf = null;
            this.EditedNextVersion = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the next version of the edited type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearNextVersionOfEdited()
        {
            this.EditedNextVersion = null;
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
        /// Clears the VariantOf property of the edited aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearVariantOfEdited()
        {
            this.EditedIsVariantOf = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the IsVariantOf property of the new type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearVariantOfNew()
        {
            this.IsVariantOf = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the selected aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DeleteType()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.DeleteTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
                return;
            }

            ExtendedMessageBoxResult? answer = null;
            this.DeleteTypeCommand.ReportProgress(
                () =>
                {
                    var messageBox = new OpenSkyMessageBox(
                        "Delete type?",
                        $"Are you sure you want to delete the aircraft type: {this.SelectedAircraftType}",
                        MessageBoxButton.YesNo,
                        ExtendedMessageBoxImage.Question);
                    messageBox.Closed += (_, _) =>
                    {
                        answer = messageBox.Result;
                    };
                    this.ViewReference.ShowMessageBox(messageBox);
                });
            while (answer == null && !SleepScheduler.IsShutdownInProgress)
            {
                Thread.Sleep(500);
            }

            if (answer != ExtendedMessageBoxResult.Yes)
            {
                return;
            }

            this.LoadingText = "Deleting aircraft type";
            try
            {
                var result = AgentOpenSkyService.Instance.DeleteAircraftTypeAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.DeleteTypeCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.DeleteTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error deleting aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error deleting aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.DeleteTypeCommand, "Error deleting aircraft type");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Disables the select aircraft type's detailed checks.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DisableDetailedChecks()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.DisableDetailedChecksCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
                return;
            }

            this.LoadingText = "Enabling aircraft type detailed checks";
            try
            {
                var result = AgentOpenSkyService.Instance.DisableAircraftTypeDetailedChecksAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.DisableDetailedChecksCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.DisableDetailedChecksCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error disabling aircraft type detailed checks: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error disabling aircraft type detailed checks", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.DisableDetailedChecksCommand, "Error disabling aircraft type detailed checks");
            }
            finally
            {
                this.LoadingText = null;
            }
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
                this.DisableTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
                return;
            }

            this.LoadingText = "Disabling aircraft type";
            try
            {
                var result = AgentOpenSkyService.Instance.DisableAircraftTypeAsync(this.SelectedAircraftType.Id).Result;
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

                            var notification = new OpenSkyNotification("Error disabling aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.DisableTypeCommand, "Error disabling aircraft type");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Enables the select aircraft type's detailed checks.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void EnableDetailedChecks()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.EnableDetailedChecksCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
                return;
            }

            this.LoadingText = "Enabling aircraft type detailed checks";
            try
            {
                var result = AgentOpenSkyService.Instance.EnableAircraftTypeDetailedChecksAsync(this.SelectedAircraftType.Id).Result;
                if (!result.IsError)
                {
                    this.EnableDetailedChecksCommand.ReportProgress(
                        () => { this.RefreshAircraftTypesCommand.DoExecute(null); });
                }
                else
                {
                    this.EnableDetailedChecksCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error enabling aircraft type detailed checks: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error enabling aircraft type detailed checks", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.EnableDetailedChecksCommand, "Error enabling aircraft type detailed checks");
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
                this.EnableTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
                return;
            }

            this.LoadingText = "Enabling aircraft type";
            try
            {
                var result = AgentOpenSkyService.Instance.EnableAircraftTypeAsync(this.SelectedAircraftType.Id).Result;
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

                            var notification = new OpenSkyNotification("Error enabling aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.EnableTypeCommand, "Error enabling aircraft type");
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
                this.GetUserRolesCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Error fetching your user roles.", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
            }

            this.LoadingText = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Identify aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void IdentifyAircraft()
        {
            if (!this.Simulator.Connected)
            {
                var notification = new OpenSkyNotification("Identify aircraft", "Not connected to sim!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                notification.SetErrorColorStyle();
                this.ViewReference.ShowNotification(notification);
                return;
            }

            var foundMatch = false;
            foreach (var type in this.ExistingAircraftTypes)
            {
                if (type.MatchesAircraftInSimulator())
                {
                    foundMatch = true;
                    this.SelectedAircraftType = type;
                    var notification = new OpenSkyNotification("Identify aircraft", $"Aircraft identified as: {type}", MessageBoxButton.OK, ExtendedMessageBoxImage.Information, 30);
                    this.ViewReference.ShowNotification(notification);
                }
            }

            if (!foundMatch)
            {
                var notification = new OpenSkyNotification("Identify aircraft", "No matching aircraft type found.", MessageBoxButton.OK, ExtendedMessageBoxImage.Warning, 30);
                notification.SetWarningColorStyle();
                this.ViewReference.ShowNotification(notification);
            }
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
                var result = AgentOpenSkyService.Instance.GetSimulatorAircraftTypesAsync(this.Simulator.SimulatorType).Result;
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

                            var notification = new OpenSkyNotification("Error refreshing aircraft types", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.RefreshAircraftTypesCommand, "Error refreshing aircraft types");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Saves the edited aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SaveEditedAircraftType()
        {
            if (this.EditedAircraftType == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.EditedAircraftType.Name) || this.EditedAircraftType.Name.Length < 5)
            {
                this.SaveEditedAircraftTypeCommand.ReportProgress(() =>
                {
                    var notification = new OpenSkyNotification("Error", "Name not specified or less than 5 characters!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                    notification.SetErrorColorStyle();
                    this.ViewReference.ShowNotification(notification);
                });
                return;
            }

            this.EditedAircraftType.IsVariantOf = this.EditedIsVariantOf?.Id;
            this.EditedAircraftType.NextVersion = this.EditedNextVersion?.Id;

            this.LoadingText = "Saving changed aircraft type";
            try
            {
                var result = AgentOpenSkyService.Instance.UpdateAircraftTypeAsync(this.EditedAircraftType).Result;
                if (!result.IsError)
                {
                    this.SaveEditedAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Update aircraft type ", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            this.ViewReference.ShowNotification(notification);
                            this.CancelEditAircraft(); // This resets the input form and hides the groupbox
                            this.RefreshAircraftTypesCommand.DoExecute(null);
                        });
                }
                else
                {
                    this.SaveEditedAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error saving changed aircraft type: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error saving changed aircraft type", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.SaveEditedAircraftTypeCommand, "Error saving changed aircraft type");
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
            if (!this.Simulator.Connected)
            {
                var notification = new OpenSkyNotification("Add aircraft type", "Not connected to sim!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                notification.SetErrorColorStyle();
                this.ViewReference.ShowNotification(notification);
                return;
            }

            AircraftType matchedType = null;
            foreach (var type in this.ExistingAircraftTypes)
            {
                if (type.MatchesAircraftInSimulator())
                {
                    matchedType = type;
                    break;
                }
            }

            if (matchedType != null)
            {
                var messageBox = new OpenSkyMessageBox(
                    "Add aircraft type",
                    $"The aircraft currently loaded in the sim seems to be a match for the existing type: {matchedType}\r\n\r\nAre you sure you want to add another type?",
                    MessageBoxButton.YesNo,
                    ExtendedMessageBoxImage.Question);
                messageBox.Closed += (_, _) =>
                {
                    if (messageBox.Result == ExtendedMessageBoxResult.Yes)
                    {
                        this.AddAircraftVisibility = Visibility.Visible;
                    }
                };
                this.ViewReference.ShowMessageBox(messageBox);
            }
            else
            {
                this.AddAircraftVisibility = Visibility.Visible;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts editing the selected aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartEditAircraft()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                notification.SetErrorColorStyle();
                this.ViewReference.ShowNotification(notification);
                return;
            }

            this.EditedAircraftType = new AircraftType(this.SelectedAircraftType);
            if (this.SelectedAircraftType.IsVariantOf.HasValue)
            {
                this.EditedIsVariantOf = this.ExistingAircraftTypes.SingleOrDefault(t => t.Id == this.SelectedAircraftType.IsVariantOf);
            }

            if (this.SelectedAircraftType.NextVersion.HasValue)
            {
                this.EditedNextVersion = this.ExistingAircraftTypes.SingleOrDefault(t => t.Id == this.SelectedAircraftType.NextVersion);
            }
        }
    }
}