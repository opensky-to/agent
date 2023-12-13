// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypesViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using Microsoft.Win32;

    using OpenSky.Agent.Controls;
    using OpenSky.Agent.Controls.Models;
    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.Agent.Tools;

    using OpenSkyApi;

    using TomsToolbox.Essentials;

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
        /// The aircraft type being updated.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Guid? aircraftTypeBeingUpdated;

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
        /// The engine model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string engineModel;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ICAO type designator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string icaoTypeDesignator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to include, false to exclude the type in the world population.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool includeInWorldPopulation;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if is historic, false if not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isHistoric;

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
        /// The manufacturer delivery airport ICAO(s) (comma separated).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string manufacturerDeliveryAirportICAOs;

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
        private int maxPayloadDeltaAllowed = 5;

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
        /// The override fuel type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FuelType overrideFuelType = FuelType.NotUsed;

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
        /// The selected manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftManufacturer selectedManufacturer;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The upgrade aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility upgradeAircraftVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Does the aircraft use the strobe for beacon?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool usesStrobeForBeacon;

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
            this.Manufacturers = new ObservableCollection<AircraftManufacturer>();
            this.AircraftUpgrades = new ObservableCollection<AircraftTypeUpgrade>();
            this.MatchingVisibilities = new ObservableCollection<Visibility>();
            for (var i = 0; i < 34; i++)
            {
                this.MatchingVisibilities.Add(Visibility.Collapsed);
            }

            this.GetUserRolesCommand = new AsynchronousCommand(this.GetUserRoles);
            this.RefreshAircraftTypesCommand = new AsynchronousCommand(this.RefreshAircraftTypes);
            this.AddAircraftTypeCommand = new AsynchronousCommand(this.AddAircraftType);
            this.StartAddAircraftCommand = new Command(this.StartAddAircraft);
            this.StartUpdateAircraftCommand = new Command(this.StartUpdateAircraft, false);
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
            this.UploadImageCommand = new AsynchronousCommand(this.UploadImage);
            this.GetAircraftManufacturersCommand = new AsynchronousCommand(this.GetAircraftManufacturers);
            this.GetAircraftUpgradesCommand = new AsynchronousCommand(this.GetAircraftUpgrades, false);
            this.CloseUpgradeAircraftCommand = new Command(this.CloseUpgradeAircraft);
            this.UpdateAircraftTypeCommand = new AsynchronousCommand(this.UpdateAircraftType);

            this.GetUserRolesCommand.DoExecute(null);
            this.GetAircraftManufacturersCommand.DoExecute(null);

            // Make sure we can update the matches visibilities
            this.SelectedAircraftTypes.CollectionChanged += this.SelectedAircraftTypesCollectionChanged;
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
        /// Gets the aircraft upgrades.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftTypeUpgrade> AircraftUpgrades { get; }

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
        /// Gets the close upgrade aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CloseUpgradeAircraftCommand { get; }

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
        /// Gets or sets the engine model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string EngineModel
        {
            get => this.engineModel;

            set
            {
                if (Equals(this.engineModel, value))
                {
                    return;
                }

                this.engineModel = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of the existing aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> ExistingAircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get aircraft manufacturers command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetAircraftManufacturersCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get aircraft upgrades command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetAircraftUpgradesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the get user roles command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand GetUserRolesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ICAO type designator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(4)]
        public string IcaoTypeDesignator
        {
            get => this.icaoTypeDesignator;

            set
            {
                if (Equals(this.icaoTypeDesignator, value))
                {
                    return;
                }

                this.icaoTypeDesignator = value;
                this.NotifyPropertyChanged();
            }
        }

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
        /// Gets or sets a value indicating whether this aircraft is historic.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsHistoric
        {
            get => this.isHistoric;

            set
            {
                if (Equals(this.isHistoric, value))
                {
                    return;
                }

                this.isHistoric = value;
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
        /// Gets or sets the manufacturer delivery airport ICAO(s) (comma separated).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ManufacturerDeliveryAirportICAOs
        {
            get => this.manufacturerDeliveryAirportICAOs;

            set
            {
                if (Equals(this.manufacturerDeliveryAirportICAOs, value))
                {
                    return;
                }

                this.manufacturerDeliveryAirportICAOs = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the manufacturers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftManufacturer> Manufacturers { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the matching visibilities (when comparing two aircraft types).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<Visibility> MatchingVisibilities { get; }

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
        /// Gets or sets the override fuel type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FuelType OverrideFuelType
        {
            get => this.overrideFuelType;

            set
            {
                if (Equals(this.overrideFuelType, value))
                {
                    return;
                }

                this.overrideFuelType = value;
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
        /// Gets or sets the selected aircraft type.
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
                this.StartUpdateAircraftCommand.CanExecute = value != null;

                if (UserSessionService.Instance.IsModerator)
                {
                    this.EnableTypeCommand.CanExecute = value != null;
                    this.DisableTypeCommand.CanExecute = value != null;
                    this.EnableDetailedChecksCommand.CanExecute = value != null;
                    this.DisableDetailedChecksCommand.CanExecute = value != null;
                    this.StartEditAircraftCommand.CanExecute = value != null;
                    this.UploadImageCommand.CanExecute = value != null;
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
        /// Gets or sets the selected manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftManufacturer SelectedManufacturer
        {
            get => this.selectedManufacturer;

            set
            {
                if (Equals(this.selectedManufacturer, value))
                {
                    return;
                }

                this.selectedManufacturer = value;
                this.NotifyPropertyChanged();
            }
        }

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
        /// Gets the start update aircraft command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command StartUpdateAircraftCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the update aircraft type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand UpdateAircraftTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the upgrade aircraft visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility UpgradeAircraftVisibility
        {
            get => this.upgradeAircraftVisibility;

            set
            {
                if (Equals(this.upgradeAircraftVisibility, value))
                {
                    return;
                }

                this.upgradeAircraftVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the upload image command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand UploadImageCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft uses the strobe for the beacon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool UsesStrobeForBeacon
        {
            get => this.usesStrobeForBeacon;

            set
            {
                if (Equals(this.usesStrobeForBeacon, value))
                {
                    return;
                }

                this.usesStrobeForBeacon = value;
                this.NotifyPropertyChanged();
            }
        }

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
        /// Closes the upgrade aircraft view and clears the upgrades collection.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void CloseUpgradeAircraft()
        {
            this.UpgradeAircraftVisibility = Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the current aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AddAircraftType()
        {
            if (!this.Simulator.Connected)
            {
                this.AddAircraftTypeCommand.ReportProgress(
                    () =>
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
                            "ATC aircraft model or type not set, are you connected to the simulator and is the aircraft loaded correctly? Are you sure you want to continue?",
                            MessageBoxButton.YesNo,
                            ExtendedMessageBoxImage.Hand);
                        messageBox.SetWarningColorStyle();
                        messageBox.Closed += (_, _) => { answer = messageBox.Result; };
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
                this.AddAircraftTypeCommand.ReportProgress(
                    () =>
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
                EmptyWeight = Math.Round(this.Simulator.WeightAndBalance.EmptyWeight, 0),
                FuelTotalCapacity = Math.Round(this.Simulator.WeightAndBalance.FuelTotalCapacity, 1),
                FuelWeightPerGallon = Math.Round(this.Simulator.WeightAndBalance.FuelWeightPerGallon, 1),
                MaxGrossWeight = Math.Round(this.Simulator.WeightAndBalance.MaxGrossWeight, 0),
                FlapsAvailable = this.Simulator.AircraftIdentity.FlapsAvailable,
                IsGearRetractable = this.Simulator.AircraftIdentity.GearRetractable,
                Name = this.Name,
                VersionNumber = this.VersionNumber,
                ManufacturerID = this.SelectedManufacturer?.Id,
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
                Comments = this.Comments,
                Simulator = this.Simulator.SimulatorType,
                EngineModel = this.EngineModel,
                OverrideFuelType = this.OverrideFuelType,
                IsHistoric = this.IsHistoric,
                UsesStrobeForBeacon = this.UsesStrobeForBeacon,
                IcaoTypeDesignator = this.IcaoTypeDesignator
            };

            if (!string.IsNullOrEmpty(this.ManufacturerDeliveryAirportICAOs))
            {
                newAircraftType.DeliveryLocations = new List<AircraftManufacturerDeliveryLocation>();
                var icaos = this.ManufacturerDeliveryAirportICAOs.Split(',');
                foreach (var icao in icaos)
                {
                    if (!string.IsNullOrEmpty(icao.Trim()))
                    {
                        newAircraftType.DeliveryLocations.Add(
                            new AircraftManufacturerDeliveryLocation
                            {
                                AircraftTypeID = Guid.Empty,
                                ManufacturerID = "empty",
                                AirportICAO = icao.Trim()
                            });
                    }
                }
            }

            this.LoadingText = "Adding new aircraft type";
            try
            {
                var result = AgentOpenSkyService.Instance.AddAircraftTypeAsync(this.aircraftTypeBeingUpdated, newAircraftType).Result;
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
            this.SelectedManufacturer = null;
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
            this.OverrideFuelType = FuelType.NotUsed;
            this.EngineModel = null;
            this.IsHistoric = false;
            this.RequiresManualFuelling = false;
            this.RequiresManualLoading = false;
            this.IcaoTypeDesignator = null;
            this.UsesStrobeForBeacon = false;

            this.aircraftTypeBeingUpdated = null;
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
        /// Copies the selected aircraft type to be updated using the add aircraft panel.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CopySelectedTypeToBeUpdated()
        {
            this.Name = this.SelectedAircraftType.Name;
            this.VersionNumber = this.SelectedAircraftType.VersionNumber + 1;
            this.SelectedManufacturer = this.SelectedAircraftType.Manufacturer;
            this.ManufacturerDeliveryAirportICAOs = this.SelectedAircraftType.ManufacturerDeliveryLocationICAOs;
            this.Category = this.SelectedAircraftType.Category;
            this.IsVanilla = this.SelectedAircraftType.IsVanilla;
            this.IncludeInWorldPopulation = this.SelectedAircraftType.IncludeInWorldPopulation;
            this.NeedsCoPilot = this.SelectedAircraftType.NeedsCoPilot;
            this.NeedsFlightEngineer = this.SelectedAircraftType.NeedsFlightEngineer;
            this.RequiresManualFuelling = this.SelectedAircraftType.RequiresManualFuelling;
            this.RequiresManualLoading = this.SelectedAircraftType.RequiresManualLoading;
            this.MinimumRunwayLength = this.SelectedAircraftType.MinimumRunwayLength;
            this.MinimumPrice = this.SelectedAircraftType.MinPrice;
            this.MaximumPrice = this.SelectedAircraftType.MaxPrice;
            this.MaxPayloadDeltaAllowed = this.SelectedAircraftType.MaxPayloadDeltaAllowed;
            this.Comments = this.SelectedAircraftType.Comments;
            this.EngineModel = this.SelectedAircraftType.EngineModel;
            this.OverrideFuelType = this.SelectedAircraftType.OverrideFuelType;
            this.IsHistoric = this.SelectedAircraftType.IsHistoric;
            if (this.SelectedAircraftType.IsVariantOf.HasValue)
            {
                var variantType = this.ExistingAircraftTypes.SingleOrDefault(t => t.Id == this.SelectedAircraftType.IsVariantOf.Value);
                if (variantType != null)
                {
                    this.IsVariantOf = variantType;
                }
            }

            this.aircraftTypeBeingUpdated = this.SelectedAircraftType.Id;

            this.AddAircraftVisibility = Visibility.Visible;
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
                this.DeleteTypeCommand.ReportProgress(
                    () =>
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
                    messageBox.Closed += (_, _) => { answer = messageBox.Result; };
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
                this.DisableDetailedChecksCommand.ReportProgress(
                    () =>
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
                this.DisableTypeCommand.ReportProgress(
                    () =>
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
                this.EnableDetailedChecksCommand.ReportProgress(
                    () =>
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
                this.EnableTypeCommand.ReportProgress(
                    () =>
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
        /// Gets the list of aircraft manufacturers.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetAircraftManufacturers()
        {
            this.LoadingText = "Fetching aircraft manufacturers";
            try
            {
                var result = AgentOpenSkyService.Instance.GetAircraftManufacturersAsync().Result;
                if (!result.IsError)
                {
                    this.GetAircraftManufacturersCommand.ReportProgress(
                        () =>
                        {
                            this.Manufacturers.Clear();
                            this.Manufacturers.AddRange(result.Data);
                        });
                }
                else
                {
                    this.GetAircraftManufacturersCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error fetching aircraft manufacturers: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error fetching aircraft manufacturers", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.GetAircraftManufacturersCommand, "Error fetching aircraft manufacturers");
            }
            finally
            {
                this.LoadingText = null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available aircraft upgrades.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void GetAircraftUpgrades()
        {
            if (!UserSessionService.Instance.IsModerator)
            {
                return;
            }

            this.LoadingText = "Checking for available aircraft type upgrades...";
            try
            {
                var result = AgentOpenSkyService.Instance.GetAircraftTypeUpgradesAsync().Result;
                if (!result.IsError)
                {
                    this.GetAircraftUpgradesCommand.ReportProgress(
                        () =>
                        {
                            this.AircraftUpgrades.Clear();
                            this.AircraftUpgrades.AddRange(result.Data);
                            this.UpgradeAircraftVisibility = Visibility.Visible;
                        });
                }
                else
                {
                    this.GetAircraftUpgradesCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error checking for aircraft type upgrades: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error checking for aircraft type upgrades", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.GetAircraftUpgradesCommand, "Error checking for aircraft type upgrades");
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
                this.GetUserRolesCommand.ReportProgress(
                    () =>
                    {
                        this.RefreshAircraftTypesCommand.DoExecute(null);
                        this.GetAircraftUpgradesCommand.CanExecute = UserSessionService.Instance.IsModerator;
                    });
            }
            else
            {
                this.GetUserRolesCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification("Error", "Error fetching your user roles.", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        this.ViewReference.ShowNotification(notification);

                        this.GetAircraftUpgradesCommand.CanExecute = false;
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
                            foreach (var type in result.Data.OrderBy(t => t.Name).ThenBy(t => t.VersionNumber))
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
                this.SaveEditedAircraftTypeCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification("Error", "Name not specified or less than 5 characters!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        this.ViewReference.ShowNotification(notification);
                    });
                return;
            }

            this.EditedAircraftType.IsVariantOf = this.EditedIsVariantOf?.Id;
            this.EditedAircraftType.NextVersion = this.EditedNextVersion?.Id;
            this.EditedAircraftType.ManufacturerID = this.EditedAircraftType.Manufacturer?.Id;

            this.EditedAircraftType.DeliveryLocations = new List<AircraftManufacturerDeliveryLocation>();
            if (!string.IsNullOrEmpty(this.ManufacturerDeliveryAirportICAOs))
            {
                var icaos = this.ManufacturerDeliveryAirportICAOs.Split(',');
                foreach (var icao in icaos)
                {
                    if (!string.IsNullOrEmpty(icao.Trim()))
                    {
                        this.EditedAircraftType.DeliveryLocations.Add(
                            new AircraftManufacturerDeliveryLocation
                            {
                                AircraftTypeID = Guid.Empty,
                                ManufacturerID = "empty",
                                AirportICAO = icao.Trim()
                            });
                    }
                }
            }

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
        /// Selected aircraft types collection changed - let's compare what is equal and what isn't.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SelectedAircraftTypesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.SelectedAircraftTypes.Count != 2)
            {
                for (var i = 0; i < 34; i++)
                {
                    this.MatchingVisibilities[i] = Visibility.Collapsed;
                }
            }
            else
            {
                // 0 ... name, version, etc., doesn't make sense to compare
                this.MatchingVisibilities[1] = !string.Equals(this.SelectedAircraftTypes[0].ManufacturerID, this.SelectedAircraftTypes[1].ManufacturerID, StringComparison.InvariantCultureIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[2] = !string.Equals(this.SelectedAircraftTypes[0].ManufacturerDeliveryLocationICAOs, this.SelectedAircraftTypes[1].ManufacturerDeliveryLocationICAOs, StringComparison.InvariantCultureIgnoreCase)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                this.MatchingVisibilities[3] = !Equals(this.SelectedAircraftTypes[0].Category, this.SelectedAircraftTypes[1].Category) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[4] = !Equals(this.SelectedAircraftTypes[0].Simulator, this.SelectedAircraftTypes[1].Simulator) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[5] = !Equals(this.SelectedAircraftTypes[0].Enabled, this.SelectedAircraftTypes[1].Enabled) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[6] = !Equals(this.SelectedAircraftTypes[0].DetailedChecksDisabled, this.SelectedAircraftTypes[1].DetailedChecksDisabled) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[7] = !Equals(this.SelectedAircraftTypes[0].IsVanilla, this.SelectedAircraftTypes[1].IsVanilla) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[8] = !Equals(this.SelectedAircraftTypes[0].IncludeInWorldPopulation, this.SelectedAircraftTypes[1].IncludeInWorldPopulation) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[9] = !Equals(this.SelectedAircraftTypes[0].IsHistoric, this.SelectedAircraftTypes[1].IsHistoric) ? Visibility.Visible : Visibility.Collapsed;

                // 10 ... next version, doesn't make sense to compare
                // 11 ... variant of, doesn't make sensor to compare
                this.MatchingVisibilities[12] = !Equals(this.SelectedAircraftTypes[0].AtcType, this.SelectedAircraftTypes[1].AtcType) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[13] = !Equals(this.SelectedAircraftTypes[0].AtcModel, this.SelectedAircraftTypes[1].AtcModel) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[14] = !Equals(this.SelectedAircraftTypes[0].EmptyWeight, this.SelectedAircraftTypes[1].EmptyWeight) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[15] = !Equals(this.SelectedAircraftTypes[0].OverrideFuelType, this.SelectedAircraftTypes[1].OverrideFuelType) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[16] = !Equals(this.SelectedAircraftTypes[0].FuelTotalCapacity, this.SelectedAircraftTypes[1].FuelTotalCapacity) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[17] = !Equals(this.SelectedAircraftTypes[0].FuelWeightPerGallon, this.SelectedAircraftTypes[1].FuelWeightPerGallon) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[18] = !Equals(this.SelectedAircraftTypes[0].MaxGrossWeight, this.SelectedAircraftTypes[1].MaxGrossWeight) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[19] = !Equals(this.SelectedAircraftTypes[0].EngineType, this.SelectedAircraftTypes[1].EngineType) || !Equals(this.SelectedAircraftTypes[0].EngineCount, this.SelectedAircraftTypes[1].EngineCount)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                this.MatchingVisibilities[20] = !string.Equals(this.SelectedAircraftTypes[0].EngineModel, this.SelectedAircraftTypes[1].EngineModel, StringComparison.InvariantCultureIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[21] = !Equals(this.SelectedAircraftTypes[0].FlapsAvailable, this.SelectedAircraftTypes[1].FlapsAvailable) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[22] = !Equals(this.SelectedAircraftTypes[0].IsGearRetractable, this.SelectedAircraftTypes[1].IsGearRetractable) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[23] = !Equals(this.SelectedAircraftTypes[0].MinimumRunwayLength, this.SelectedAircraftTypes[1].MinimumRunwayLength) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[24] = !Equals(this.SelectedAircraftTypes[0].MinPrice, this.SelectedAircraftTypes[1].MinPrice) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[25] = !Equals(this.SelectedAircraftTypes[0].MaxPrice, this.SelectedAircraftTypes[1].MaxPrice) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[26] = !Equals(this.SelectedAircraftTypes[0].NeedsCoPilot, this.SelectedAircraftTypes[1].NeedsCoPilot) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[27] = !Equals(this.SelectedAircraftTypes[0].NeedsFlightEngineer, this.SelectedAircraftTypes[1].NeedsFlightEngineer) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[28] = !Equals(this.SelectedAircraftTypes[0].RequiresManualFuelling, this.SelectedAircraftTypes[1].RequiresManualFuelling) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[29] = !Equals(this.SelectedAircraftTypes[0].RequiresManualLoading, this.SelectedAircraftTypes[1].RequiresManualLoading) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[30] = !Equals(this.SelectedAircraftTypes[0].MaxPayloadDeltaAllowed, this.SelectedAircraftTypes[1].MaxPayloadDeltaAllowed) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[31] = !Equals(this.SelectedAircraftTypes[0].UsesStrobeForBeacon, this.SelectedAircraftTypes[1].UsesStrobeForBeacon) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[32] = !Equals(this.SelectedAircraftTypes[0].IcaoTypeDesignator, this.SelectedAircraftTypes[1].IcaoTypeDesignator) ? Visibility.Visible : Visibility.Collapsed;
                this.MatchingVisibilities[33] = !Equals(this.SelectedAircraftTypes[0].HasAircraftImage, this.SelectedAircraftTypes[1].HasAircraftImage) ? Visibility.Visible : Visibility.Collapsed;
            }

            this.NotifyPropertyChanged(nameof(this.MatchingVisibilities));
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

            this.ManufacturerDeliveryAirportICAOs = string.Empty;
            if (this.SelectedAircraftType.DeliveryLocations != null)
            {
                foreach (var deliveryLocation in this.SelectedAircraftType.DeliveryLocations)
                {
                    this.ManufacturerDeliveryAirportICAOs += $"{deliveryLocation.AirportICAO},";
                }
            }

            this.ManufacturerDeliveryAirportICAOs = this.ManufacturerDeliveryAirportICAOs.TrimEnd(',');
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts updating the selected aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/11/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartUpdateAircraft()
        {
            if (!this.Simulator.Connected)
            {
                var notification = new OpenSkyNotification("Update aircraft type", "Not connected to sim!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
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
                    "Update aircraft type",
                    $"The aircraft currently loaded in the sim seems to be a match for the existing type: {matchedType}\r\n\r\nAre you sure you an update is necessary?",
                    MessageBoxButton.YesNo,
                    ExtendedMessageBoxImage.Question);
                messageBox.Closed += (_, _) =>
                {
                    if (messageBox.Result == ExtendedMessageBoxResult.Yes)
                    {
                        this.CopySelectedTypeToBeUpdated();
                    }
                };
                this.ViewReference.ShowMessageBox(messageBox);
            }
            else
            {
                this.CopySelectedTypeToBeUpdated();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Perform the specified aircraft type upgrade.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void UpdateAircraftType(object parameter)
        {
            if (parameter is AircraftTypeUpgrade upgrade)
            {
                this.LoadingText = "Upgrading aircraft...";
                try
                {
                    var result = AgentOpenSkyService.Instance.UpgradeAircraftTypeAsync(upgrade).Result;
                    if (!result.IsError)
                    {
                        this.UpdateAircraftTypeCommand.ReportProgress(() => this.GetAircraftUpgradesCommand.DoExecute(null));
                    }
                    else
                    {
                        this.UpdateAircraftTypeCommand.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine("Error performing aircraft type upgrade: " + result.Message);
                                if (!string.IsNullOrEmpty(result.ErrorDetails))
                                {
                                    Debug.WriteLine(result.ErrorDetails);
                                }

                                var notification = new OpenSkyNotification("Error performing aircraft type upgrade", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                notification.SetErrorColorStyle();
                                this.ViewReference.ShowNotification(notification);
                            });
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.ViewReference, this.UpdateAircraftTypeCommand, "Error performing aircraft type upgrade");
                }
                finally
                {
                    this.LoadingText = null;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Upload aircraft type image.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UploadImage()
        {
            if (this.SelectedAircraftTypes.Count != 1)
            {
                this.UploadImageCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification("Error", "Please select exactly one aircraft type!", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        this.ViewReference.ShowNotification(notification);
                    });
                return;
            }

            bool? answer = null;
            string fileName = null;
            this.UploadImageCommand.ReportProgress(
                () =>
                {
                    var openDialog = new OpenFileDialog
                    {
                        Title = "Select new aircraft image",
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
                var result = AgentOpenSkyService.Instance.UpdateAircraftTypeImageAsync(this.SelectedAircraftType.Id, new FileParameter(File.OpenRead(fileName), fileName, fileName.ToLowerInvariant().EndsWith(".png") ? "image/png" : "image/jpeg"))
                                                .Result;
                if (result.IsError)
                {
                    this.UploadImageCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error uploading aircraft image: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error uploading aircraft image", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
                else
                {
                    this.UploadImageCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification("Aircraft image upload", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 10);
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.ViewReference, this.UploadImageCommand, "Error uploading aircraft image.");
            }
        }
    }
}