// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypesViewModel.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
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

            this.RefreshAircraftTypesCommand = new AsynchronousCommand(this.RefreshAircraftTypes);
            this.AddAircraftTypeCommand = new AsynchronousCommand(this.AddAircraftType);
            this.ClearVariantOfCommand = new Command(this.ClearVariantOf);

            this.RefreshAircraftTypesCommand.DoExecute(null);
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
        /// Gets the add aircraft type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AddAircraftTypeCommand { get; }

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
        /// Gets a list of of the existing aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> ExistingAircraftTypes { get; }

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
        [Required]
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
        /// Gets the clear VariantOf command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearVariantOfCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the SimConnect object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SimConnect SimConnect => SimConnect.Instance;

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
                        () => { ModernWpf.MessageBox.Show(result.Message, "New aircraft type", MessageBoxButton.OK, MessageBoxImage.Error); });
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
        /// Refreshes the list of existing aircraft types.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAircraftTypes()
        {
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
        }
    }
}