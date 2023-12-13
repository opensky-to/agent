// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddAircraftViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
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
    /// Add aircraft view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 12/12/2023.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AddAircraftViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The optional comments (what mod was loaded, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string comments = string.Empty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Information describing the identified aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string identifiedAircraftInfo = "Unknown";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The name of the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string name = string.Empty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AddAircraftViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AddAircraftViewModel()
        {
            // Initialize data structures
            this.ExistingAircraftTypes = new ObservableCollection<AircraftType>();

            // Initialize commands
            this.RefreshAircraftTypesCommand = new AsynchronousCommand(this.RefreshAircraftTypes);
            this.IdentifyAircraftCommand = new Command(this.IdentifyAircraft);
            this.AddAircraftTypeCommand = new AsynchronousCommand(this.AddAircraftType);
            this.CancelCommand = new Command(this.CancelAddAircraft);

            // Execute initial commands
            this.RefreshAircraftTypesCommand.DoExecute(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<object> CloseWindow;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the add aircraft type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AddAircraftTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command CancelCommand { get; }

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
        /// Gets a list of the existing aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> ExistingAircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets information describing the identified aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string IdentifiedAircraftInfo
        {
            get => this.identifiedAircraftInfo;

            set
            {
                if (Equals(this.identifiedAircraftInfo, value))
                {
                    return;
                }

                this.identifiedAircraftInfo = value;
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
        /// Gets the refresh aircraft types command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAircraftTypesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulator object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Agent.Simulator.Simulator Simulator => Agent.Simulator.Simulator.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds a new aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
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
                ExtendedMessageBoxResult? answer = null;
                this.AddAircraftTypeCommand.ReportProgress(
                    () =>
                    {
                        var messageBox = new OpenSkyMessageBox(
                            "Add aircraft type",
                            $"The aircraft currently loaded in the sim seems to be a match for the existing type: {matchedType}\r\n\r\nAre you sure you want to add another type?",
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
                Comments = this.Comments,
                Simulator = this.Simulator.SimulatorType
            };

            this.LoadingText = "Adding new aircraft";
            try
            {
                var result = AgentOpenSkyService.Instance.AddAircraftTypeAsync(null, newAircraftType).Result;
                if (!result.IsError)
                {
                    this.AddAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            var messageBox = new OpenSkyMessageBox("New aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Check, 10);
                            this.ViewReference.ShowMessageBox(messageBox);
                            messageBox.Closed += (_, _) => { this.CloseWindow?.Invoke(this, null); };
                        });
                }
                else
                {
                    this.AddAircraftTypeCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error adding new aircraft: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            var notification = new OpenSkyNotification("Error adding new aircraft", result.Message, MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
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
        /// sushi.at, 13/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CancelAddAircraft()
        {
            this.CloseWindow?.Invoke(this, null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Identify aircraft currently loaded in the sim.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void IdentifyAircraft()
        {
            if (!this.Simulator.Connected)
            {
                this.IdentifiedAircraftInfo = "Not connected";
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
                    this.IdentifiedAircraftInfo = $"{type.Name} [v{type.VersionNumber}]";
                }
            }

            if (!foundMatch)
            {
                this.IdentifiedAircraftInfo = "No matching aircraft found!";
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

                            this.IdentifyAircraftCommand.DoExecute(null);
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
    }
}