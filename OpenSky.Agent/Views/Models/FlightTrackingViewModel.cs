﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Media;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Data;

    using JetBrains.Annotations;

    using OpenSky.Agent.Controls.CustomModules;
    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator.Controls;
    using OpenSky.Agent.Simulator.Controls.Models;
    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.Agent.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The view model for the flight tracking view.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Agent.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightTrackingViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground handling warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility groundHandlingWarningVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The loading text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string loadingText;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The no flight visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility noFlightVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ofp visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility ofpVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The resume flight tips visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility resumeFlightTipsVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show hide advanced weight and balance button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string showHideAdvancedWeightAndBalanceButtonText = "Show advanced";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The show hide ofp button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string showHideOFPButtonText = "Show OFP";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The skip ground handling visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility skipGroundHandlingVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The start/resume tracking button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string startTrackingButtonText = "Start Tracking";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tracking conditions visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility trackingConditionsVisibility = Visibility.Visible;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tracking conditions visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility trackingStatusVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The weight and balances advanced visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility weightAndBalancesAdvancedVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The weight and balances visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility weightAndBalancesVisibility = Visibility.Visible;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightTrackingViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FlightTrackingViewModel()
        {
            // Set up fuel tank dictionaries
            this.FuelTankQuantities = new ObservableConcurrentDictionary<FuelTank, double>();
            this.FuelTankInfos = new ObservableConcurrentDictionary<FuelTank, string>();
            foreach (FuelTank tank in Enum.GetValues(typeof(FuelTank)))
            {
                this.FuelTankQuantities.Add(tank, 0);
                this.FuelTankInfos.Add(tank, "??");
            }

            this.FuelTankQuantities.CollectionChanged += this.QuantitiesCollectionChanged;

            // Set up payload stations collection
            this.PayloadStationWeights = new ObservableCollection<double>();
            for (var i = 0; i < 20; i++)
            {
                this.PayloadStationWeights.Add(0.0);
            }

            // Set up event log listeners
            this.Simulator.TrackingEventMarkerAdded += this.SimulatorTrackingEventMarkerAdded;
            this.Simulator.SimbriefWaypointMarkerAdded += this.SimulatorSimbriefWaypointMarkerAdded;
            this.Simulator.TrackingStatusChanged += this.SimulatorTrackingStatusChanged;
            this.Simulator.FlightChanged += this.SimulatortFlightChanged;
            this.Simulator.LocationChanged += this.SimulatorLocationChanged;
            this.Simulator.SimbriefOfpLoadedChanged += this.SimulatorOfpLoadedChanged;

            // Create commands
            this.SetFuelAndPayloadCommand = new Command(this.SetFuelAndPayload);
            this.ToggleAdvancedWeightAndBalanceCommand = new Command(this.ToggleAdvancedWeightAndBalance);
            this.SuggestFuelCommand = new Command(this.SuggestFuel);
            this.SetFuelTanksCommand = new Command(this.SetFuelTanks);
            this.SuggestPayloadCommand = new Command(this.SuggestPayload);
            this.SetPayloadStationsCommand = new Command(this.SetPayloadStations);
            this.StartTrackingCommand = new AsynchronousCommand(this.StartTracking, false);
            this.AbortFlightCommand = new AsynchronousCommand(this.AbortFlight, false);
            this.ToggleFlightPauseCommand = new AsynchronousCommand(this.ToggleFlightPause, false);
            this.StopTrackingCommand = new AsynchronousCommand(this.StopTracking, false);
            this.ImportSimbriefCommand = new AsynchronousCommand(this.ImportSimbrief, false);
            this.MoveMapToCoordinateCommand = new Command(this.MoveMapToCoordinate);
            this.SlewIntoPositionCommand = new AsynchronousCommand(this.SlewIntoPosition);
            this.ToggleOfpCommand = new Command(this.ToggleOfp);
            this.SpeedUpGroundHandlingCommand = new Command(this.SpeedUpGroundHandling);
            this.SkipGroundHandlingCommand = new Command(this.SkipGroundHandling);
            this.RefreshMetarCommand = new Command(this.RefreshMetar);
            this.CompleteFlightCommand = new AsynchronousCommand(this.CompleteFlight);

            // Are we already preparing/resuming/tracking?
            this.SimulatorTrackingStatusChanged(this, this.Simulator.TrackingStatus);
            this.SimulatortFlightChanged(this, this.Simulator.Flight);
            if (this.Simulator.SimbriefOfpLoaded)
            {
                this.ImportSimbriefVisibility = Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to reset the tracking map.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler ResetTrackingMap;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the abort flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AbortFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the complete flight command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand CompleteFlightCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ground handling warning visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility GroundHandlingWarningVisibility
        {
            get => this.groundHandlingWarningVisibility;

            private set
            {
                if (Equals(this.groundHandlingWarningVisibility, value))
                {
                    return;
                }

                this.groundHandlingWarningVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the import simbrief visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility ImportSimbriefVisibility
        {
            get => this.importSimbriefVisibility;

            private set
            {
                if (Equals(this.importSimbriefVisibility, value))
                {
                    return;
                }

                this.importSimbriefVisibility = value;
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
        /// Gets the no flight visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility NoFlightVisibility
        {
            get => this.noFlightVisibility;

            private set
            {
                if (Equals(this.noFlightVisibility, value))
                {
                    return;
                }

                this.noFlightVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ofp visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility OfpVisibility
        {
            get => this.ofpVisibility;

            set
            {
                if (Equals(this.ofpVisibility, value))
                {
                    return;
                }

                this.ofpVisibility = value;
                this.NotifyPropertyChanged();
                this.ShowHideOFPButtonText = value == Visibility.Collapsed ? "Show OFP" : "Hide OFP";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh metar command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command RefreshMetarCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the resume flight tips visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility ResumeFlightTipsVisibility
        {
            get => this.resumeFlightTipsVisibility;

            private set
            {
                if (Equals(this.resumeFlightTipsVisibility, value))
                {
                    return;
                }

                this.resumeFlightTipsVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the set fuel and payload command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command SetFuelAndPayloadCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the show hide advanced weight and balance button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ShowHideAdvancedWeightAndBalanceButtonText
        {
            get => this.showHideAdvancedWeightAndBalanceButtonText;

            private set
            {
                if (Equals(this.showHideAdvancedWeightAndBalanceButtonText, value))
                {
                    return;
                }

                this.showHideAdvancedWeightAndBalanceButtonText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the show hide ofp button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ShowHideOFPButtonText
        {
            get => this.showHideOFPButtonText;

            set
            {
                if (Equals(this.showHideOFPButtonText, value))
                {
                    return;
                }

                this.showHideOFPButtonText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulator instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Agent.Simulator.Simulator Simulator => Agent.Simulator.Simulator.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the skip ground handling command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command SkipGroundHandlingCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the skip ground handling visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility SkipGroundHandlingVisibility
        {
            get => this.skipGroundHandlingVisibility;

            set
            {
                if (Equals(this.skipGroundHandlingVisibility, value))
                {
                    return;
                }

                this.skipGroundHandlingVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the slew into position command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand SlewIntoPositionCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the speed up ground handling command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command SpeedUpGroundHandlingCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start/resume tracking button text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string StartTrackingButtonText
        {
            get => this.startTrackingButtonText;

            private set
            {
                if (Equals(this.startTrackingButtonText, value))
                {
                    return;
                }

                this.startTrackingButtonText = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the start tracking command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand StartTrackingCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the startup view model instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public StartupViewModel StartupViewModel => StartupViewModel.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the stop tracking command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand StopTrackingCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the toggle advanced weight and balance command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ToggleAdvancedWeightAndBalanceCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the toggle flight pause command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand ToggleFlightPauseCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the toggle ofp command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ToggleOfpCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking conditions visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility TrackingConditionsVisibility
        {
            get => this.trackingConditionsVisibility;

            private set
            {
                if (Equals(this.trackingConditionsVisibility, value))
                {
                    return;
                }

                this.trackingConditionsVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking conditions visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility TrackingStatusVisibility
        {
            get => this.trackingStatusVisibility;

            private set
            {
                if (Equals(this.trackingStatusVisibility, value))
                {
                    return;
                }

                this.trackingStatusVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the weight and balances advanced visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility WeightAndBalancesAdvancedVisibility
        {
            get => this.weightAndBalancesAdvancedVisibility;

            private set
            {
                if (Equals(this.weightAndBalancesAdvancedVisibility, value))
                {
                    return;
                }

                this.weightAndBalancesAdvancedVisibility = value;
                this.NotifyPropertyChanged();
                this.ShowHideAdvancedWeightAndBalanceButtonText = value == Visibility.Collapsed ? "Show advanced" : "Hide advanced";
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the weight and balances visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility WeightAndBalancesVisibility
        {
            get => this.weightAndBalancesVisibility;

            private set
            {
                if (Equals(this.weightAndBalancesVisibility, value))
                {
                    return;
                }

                this.weightAndBalancesVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Abort the current flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AbortFlight()
        {
            Debug.WriteLine("User clicked on abort flight, confirming...");
            if (this.Simulator.Flight == null)
            {
                return;
            }

            ExtendedMessageBoxResult? answer = null;
            this.AbortFlightCommand.ReportProgress(
                () =>
                {
                    var messageBox = new OpenSkyMessageBox(
                        "Cancel flight?",
                        "Are you sure you want to cancel the current flight?\r\n\r\nYou will loose any saved progress and have to return the OpenSky client to start another flight.",
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

            if (answer == ExtendedMessageBoxResult.Yes)
            {
                this.LoadingText = "Aborting flight...";
                try
                {
                    this.Simulator.StopTracking(false);
                    this.Simulator.Flight = null;
                    this.IsFuelExpanded = false;
                    this.IsPayloadExpanded = false;
                    this.WeightAndBalancesVisibility = Visibility.Visible;
                    this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.ViewReference, this.AbortFlightCommand, "Error aborting flight");
                }
                finally
                {
                    this.LoadingText = null;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Complete the flight and submit it to the OpenSky server.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CompleteFlight()
        {
            if (this.Simulator.CanFinishTracking)
            {
                this.Simulator.FinishUpFlightTracking();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh metar.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshMetar()
        {
            this.Simulator.RefreshMetar();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets fuel and payload.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SetFuelAndPayload()
        {
            this.SetFuelTanks();
            this.SetPayloadStations();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simulator ofp loaded changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="loaded">
        /// True if the data was loaded.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimulatorOfpLoadedChanged(object sender, bool loaded)
        {
            this.ImportSimbriefVisibility = loaded ? Visibility.Collapsed : Visibility.Visible;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simulator flight changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// A Flight to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimulatortFlightChanged(object sender, Flight e)
        {
            UpdateGUIDelegate updateCommands = () =>
            {
                this.AbortFlightCommand.CanExecute = e != null;
                this.StartTrackingCommand.CanExecute = e != null;
                this.ImportSimbriefCommand.CanExecute = e != null;
                this.NoFlightVisibility = e != null ? Visibility.Collapsed : Visibility.Visible;
                this.StartTrackingButtonText = e?.Resume == true ? "Resume Tracking" : "Start Tracking";

                this.SetFuelTanksCommand.CanExecute = e != null && !e.Aircraft.Type.RequiresManualFuelling;
                this.SetPayloadStationsCommand.CanExecute = e != null && !e.Aircraft.Type.RequiresManualLoading;
                this.SetFuelAndPayloadCommand.CanExecute = e != null && (!e.Aircraft.Type.RequiresManualFuelling || !e.Aircraft.Type.RequiresManualLoading);
                this.ToggleAdvancedWeightAndBalanceCommand.CanExecute = e != null && (!e.Aircraft.Type.RequiresManualFuelling || !e.Aircraft.Type.RequiresManualLoading);

                // Show/hide any custom modules?
                if (e != null && e.Aircraft.Type.CustomAgentModule == "A32NX")
                {
                    this.CustomModuleContent = new A32NX();
                    var payloadBinding = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("Simulator.Flight.PayloadPounds"),
                    };
                    BindingOperations.SetBinding(this.CustomModuleContent, A32NX.PayloadToLoadProperty, payloadBinding);
                    var currentPayloadBinding = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("Simulator.WeightAndBalance.PayloadWeight")
                    };
                    BindingOperations.SetBinding(this.CustomModuleContent, A32NX.CurrentPayloadProperty, currentPayloadBinding);
                    this.CustomModuleVisibility = Visibility.Visible;
                }
                else
                {
                    this.CustomModuleVisibility = Visibility.Collapsed;
                    this.CustomModuleContent = null;
                }
            };

            Application.Current.Dispatcher.BeginInvoke(updateCommands);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simulator tracking status changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// The new TrackingStatus.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void SimulatorTrackingStatusChanged(object sender, TrackingStatus e)
        {
            if (e == TrackingStatus.Preparing)
            {
                this.SuggestFuel();
                this.SuggestPayload();

                this.WeightAndBalancesVisibility = Visibility.Visible;
                this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                this.TrackingConditionsVisibility = Visibility.Visible;
                this.TrackingStatusVisibility = Visibility.Collapsed;
                this.SkipGroundHandlingVisibility = Visibility.Collapsed;
                if (!this.Simulator.SimbriefOfpLoaded)
                {
                    this.ImportSimbriefVisibility = Visibility.Visible;
                }

                this.ResumeFlightTipsVisibility = Visibility.Collapsed;
                this.GroundHandlingWarningVisibility = Visibility.Collapsed;
                UpdateGUIDelegate updateTrackingCommands = () =>
                {
                    this.ToggleFlightPauseCommand.CanExecute = false;
                    this.StopTrackingCommand.CanExecute = false;
                    this.ResetTrackingMap?.Invoke(this, EventArgs.Empty);
                };
                Application.Current.Dispatcher.BeginInvoke(updateTrackingCommands);
            }

            if (e == TrackingStatus.Resuming)
            {
                this.WeightAndBalancesVisibility = Visibility.Collapsed;
                this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                this.TrackingConditionsVisibility = Visibility.Visible;
                this.TrackingStatusVisibility = Visibility.Collapsed;
                this.ImportSimbriefVisibility = Visibility.Collapsed;
                this.ResumeFlightTipsVisibility = Visibility.Visible;
                this.GroundHandlingWarningVisibility = Visibility.Collapsed;
                this.SkipGroundHandlingVisibility = Visibility.Collapsed;
            }

            if (e == TrackingStatus.GroundOperations)
            {
                this.WeightAndBalancesVisibility = Visibility.Collapsed;
                this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                this.TrackingConditionsVisibility = Visibility.Collapsed;
                this.TrackingStatusVisibility = Visibility.Visible;
                this.ImportSimbriefVisibility = Visibility.Collapsed;
                this.ResumeFlightTipsVisibility = Visibility.Collapsed;
                this.GroundHandlingWarningVisibility = Visibility.Visible;
                this.SkipGroundHandlingVisibility = Visibility.Visible;
                UpdateGUIDelegate updateTrackingCommands = () =>
                {
                    this.ToggleFlightPauseCommand.CanExecute = true;
                    this.StopTrackingCommand.CanExecute = true;
                };
                Application.Current.Dispatcher.BeginInvoke(updateTrackingCommands);
            }

            if (e == TrackingStatus.Tracking)
            {
                this.WeightAndBalancesVisibility = Visibility.Collapsed;
                this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                this.TrackingConditionsVisibility = Visibility.Collapsed;
                this.TrackingStatusVisibility = Visibility.Visible;
                this.ImportSimbriefVisibility = Visibility.Collapsed;
                this.ResumeFlightTipsVisibility = Visibility.Collapsed;
                this.GroundHandlingWarningVisibility = Visibility.Collapsed;
                this.SkipGroundHandlingVisibility = Visibility.Collapsed;
                UpdateGUIDelegate updateTrackingCommands = () =>
                {
                    this.ToggleFlightPauseCommand.CanExecute = true;
                    this.StopTrackingCommand.CanExecute = true;
                };
                Application.Current.Dispatcher.BeginInvoke(updateTrackingCommands);
            }

            if (e == TrackingStatus.NotTracking)
            {
                this.WeightAndBalancesVisibility = Visibility.Visible;
                this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                this.TrackingConditionsVisibility = Visibility.Visible;
                this.TrackingStatusVisibility = Visibility.Collapsed;
                this.ImportSimbriefVisibility = Visibility.Visible;
                this.ResumeFlightTipsVisibility = Visibility.Collapsed;
                this.GroundHandlingWarningVisibility = Visibility.Collapsed;
                this.SkipGroundHandlingVisibility = Visibility.Collapsed;
                UpdateGUIDelegate updateTrackingCommands = () =>
                {
                    this.ToggleFlightPauseCommand.CanExecute = false;
                    this.StopTrackingCommand.CanExecute = false;
                    this.ResetTrackingMap?.Invoke(this, EventArgs.Empty);
                };
                Application.Current.Dispatcher.BeginInvoke(updateTrackingCommands);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Skip ground handling.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SkipGroundHandling()
        {
            if (this.Simulator.SkipGroundHandling(false))
            {
                this.SkipGroundHandlingVisibility = Visibility.Collapsed;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Slew plane into position.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SlewIntoPosition()
        {
            Debug.WriteLine("User requested slew into position");
            try
            {
                this.Simulator.SlewPlaneToFlightPosition();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error slewing plane into position: " + ex);
                this.SlewIntoPositionCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error slewing into position", ex.Message, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        this.ViewReference.ShowNotification(notification);
                    });
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Speed up ground handling.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SpeedUpGroundHandling()
        {
            this.Simulator.SpeedUpGroundHandling();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Start flight tracking.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StartTracking()
        {
            try
            {
                // Start tracking a new flight
                if (this.Simulator.TrackingStatus == TrackingStatus.Preparing)
                {
                    Debug.WriteLine("User clicked on start tracking, performing checks");
                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = false);

                    // Check conditions met
                    if (!this.Simulator.CanStartTracking)
                    {
                        Debug.WriteLine("Tracking conditions not met");
                        this.StartTrackingCommand.ReportProgress(
                            () =>
                            {
                                var notification = new OpenSkyNotification("Start tracking", "Not all tracking conditions are met, please review, correct and try again.", MessageBoxButton.OK, ExtendedMessageBoxImage.Warning, 10);
                                notification.SetWarningColorStyle();
                                this.ViewReference.ShowNotification(notification);
                            });
                        this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                        return;
                    }

                    // Set time in sim?
                    if (this.Simulator.TrackingConditions[(int)TrackingConditions.DateTime].AutoSet && this.Simulator.Flight != null)
                    {
                        this.Simulator.SetTime(DateTime.UtcNow);
                    }

                    // Set fuel?
                    if (this.Simulator.TrackingConditions[(int)TrackingConditions.Fuel].AutoSet)
                    {
                        this.StartTrackingCommand.ReportProgress(() => this.SetFuelTanksCommand.DoExecute(null), true);
                    }

                    // Set payload?
                    if (this.Simulator.TrackingConditions[(int)TrackingConditions.Payload].AutoSet)
                    {
                        this.StartTrackingCommand.ReportProgress(() => this.SetPayloadStationsCommand.DoExecute(null), true);
                    }

                    // Set the plane registration
                    this.Simulator.SetAircraftRegistry(this.Simulator.Flight?.Aircraft.Registry.RemoveSimPrefix());

                    // Wait a bit to make sure all structs have updated, especially time in sim
                    Thread.Sleep(this.Simulator.SampleRates[Requests.Secondary] + 1000);

                    // Check fuel
                    if (this.Simulator.Flight != null)
                    {
                        if (this.Simulator.WeightAndBalance.FuelTotalQuantity < (this.Simulator.Flight.FuelGallons ?? 0) && ((this.Simulator.Flight.FuelGallons ?? 0) - this.Simulator.WeightAndBalance.FuelTotalQuantity) > 0.2)
                        {
                            Debug.WriteLine("Fuel below flight plan, double checking with user...");
                            ExtendedMessageBoxResult? answer = null;
                            this.StartTrackingCommand.ReportProgress(
                                () =>
                                {
                                    var messageBox = new OpenSkyMessageBox(
                                        "Low fuel",
                                        "The fuel currently loaded into the aircraft is lower then specified in the flight plan, are you sure you want to continue?",
                                        MessageBoxButton.YesNo,
                                        ExtendedMessageBoxImage.Warning);
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
                                this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                                return;
                            }
                        }
                        else if (this.Simulator.WeightAndBalance.FuelTotalQuantity > (this.Simulator.Flight.FuelGallons ?? 0) + 1)
                        {
                            var gallons = this.Simulator.WeightAndBalance.FuelTotalQuantity - (this.Simulator.Flight.FuelGallons ?? 0);
                            Debug.WriteLine($"Aircraft has more fuel loaded then planned, charge the pilot/airline for the difference of {gallons} gallons...");

                            try
                            {
                                var result = AgentOpenSkyService.Instance.PurchaseLastMinuteFuelAsync(this.Simulator.Flight.Id, gallons).Result;
                                if (result.IsError)
                                {
                                    this.StartTrackingCommand.ReportProgress(
                                        () =>
                                        {
                                            var notification = new OpenSkyNotification("Fuel error", $"Error purchasing last minute fuel: {result.Message}", MessageBoxButton.OK, ExtendedMessageBoxImage.Error, 30);
                                            notification.SetErrorColorStyle();
                                            this.ViewReference.ShowNotification(notification);
                                        });

                                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                                    return;
                                }

                                // Is ground handling still running? If yes add time to that, otherwise add it to the warp time saved
                                if (!this.Simulator.GroundHandlingComplete && this.Simulator.Flight.FuelLoadingComplete.HasValue)
                                {
                                    this.Simulator.Flight.FuelLoadingComplete = this.Simulator.Flight.FuelLoadingComplete.Value.AddMinutes(result.Data);
                                }
                                else
                                {
                                    this.Simulator.Flight.TimeWarpTimeSavedSeconds += (int)(result.Data * 60);
                                }
                            }
                            catch (Exception ex)
                            {
                                this.StartTrackingCommand.ReportProgress(
                                    () =>
                                    {
                                        var notification = new OpenSkyNotification(new ErrorDetails { Exception = ex }, "Fuel error", "Error purchasing last minute fuel.", ExtendedMessageBoxImage.Error, 30);
                                        this.ViewReference.ShowNotification(notification);
                                    });

                                this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                                return;
                            }
                        }
                    }

                    // Check weights and balance
                    if (this.Simulator.WeightAndBalance.CgPercent < this.Simulator.WeightAndBalance.CgFwdLimit || this.Simulator.WeightAndBalance.CgPercent > this.Simulator.WeightAndBalance.CgAftLimit)
                    {
                        Debug.WriteLine("CG outside limits, double checking with user...");
                        ExtendedMessageBoxResult? answer = null;
                        this.StartTrackingCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "CG limit",
                                    "The current CG is outside of the limits specified for this aircraft, are you sure you want to continue?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Warning);
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
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }
                    }

                    if (Math.Abs(this.Simulator.WeightAndBalance.CgPercentLateral) > 0.01)
                    {
                        Debug.WriteLine("Lateral CG outside limits, double checking with user...");
                        ExtendedMessageBoxResult? answer = null;
                        this.StartTrackingCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "CG limit",
                                    "The current lateral CG is outside of the limits specified for this aircraft, are you sure you want to continue?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Warning);
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
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }
                    }

                    if (this.Simulator.WeightAndBalance.PayloadWeight > this.Simulator.WeightAndBalance.MaxPayloadWeight)
                    {
                        Debug.WriteLine("Payload weight outside limits, double checking with user...");
                        ExtendedMessageBoxResult? answer = null;
                        this.StartTrackingCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "Payload weight limit",
                                    "The payload weight exceeds the limits specified for this aircraft, are you sure you want to continue?",
                                    MessageBoxButton.YesNo,
                                    ExtendedMessageBoxImage.Warning);
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
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }
                    }

                    if (this.Simulator.WeightAndBalance.TotalWeight > this.Simulator.WeightAndBalance.MaxGrossWeight)
                    {
                        Debug.WriteLine("Total weight outside limits, double checking with user...");
                        ExtendedMessageBoxResult? answer = null;
                        this.StartTrackingCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "Total weight limit",
                                    "The total weight exceeds the limits specified for this aircraft, are you sure you want to continue?",
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
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }
                    }

                    if (!this.Simulator.GroundHandlingComplete && this.Simulator.SecondaryTracking.EngineRunning)
                    {
                        Debug.WriteLine("Ground handling not complete, ask the user about skipping...");
                        ExtendedMessageBoxResult? answer = null;
                        this.StartTrackingCommand.ReportProgress(
                            () =>
                            {
                                var messageBox = new OpenSkyMessageBox(
                                    "Ground handling",
                                    "Ground handling not yet completed, do you want to skip it?",
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
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }

                        this.Simulator.SkipGroundHandling(true);
                    }

                    this.Simulator.StartTracking();
                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                }

                // Resume flight
                if (this.Simulator.TrackingStatus == TrackingStatus.Resuming)
                {
                    Debug.WriteLine("User clicked on resume tracking, performing checks");
                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = false);

                    // Check conditions met
                    if (!this.Simulator.CanStartTracking)
                    {
                        Debug.WriteLine("Tracking conditions not met");
                        this.StartTrackingCommand.ReportProgress(
                            () =>
                            {
                                var notification = new OpenSkyNotification("Start tracking", "Not all tracking conditions are met, please review, correct and try again.", MessageBoxButton.OK, ExtendedMessageBoxImage.Warning, 10);
                                notification.SetWarningColorStyle();
                                this.ViewReference.ShowNotification(notification);
                            });
                        this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                        return;
                    }

                    // Set time in sim?
                    if (this.Simulator.TrackingConditions[(int)TrackingConditions.DateTime].AutoSet && this.Simulator.Flight != null)
                    {
                        this.Simulator.SetTime(DateTime.UtcNow);
                    }

                    // Set fuel and payload back to what they were when we saved the flight
                    this.Simulator.SetFuelAndPayloadFromSave();

                    // Set the plane registration
                    this.Simulator.SetAircraftRegistry(this.Simulator.Flight?.Aircraft.Registry.RemoveSimPrefix());

                    // Start five second countdown?
                    if (this.Simulator.PrimaryTracking.SlewActive)
                    {
                        Debug.WriteLine("Starting 5 second resume timer...");
                        var assembly = Assembly.GetExecutingAssembly();
                        var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.Agent.Resources.OSbeepbeepstart.wav"));
                        player.Play();
                        var secondsToGo = 5;
                        this.StartTrackingButtonText = "Resuming in 5";
                        while (secondsToGo > 0)
                        {
                            Thread.Sleep(1000);
                            secondsToGo--;
                            this.StartTrackingButtonText = secondsToGo > 0 ? $"Resuming in {secondsToGo}" : "Resuming now";
                        }

                        this.Simulator.SetSlew(false);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        // Wait a bit to make sure all structs have updated, especially time in sim
                        Thread.Sleep(this.Simulator.SampleRates[Requests.Secondary] + 1000);
                    }

                    this.Simulator.StartTracking();
                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting/resuming tracking: " + ex);
                this.StartTrackingCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error starting tracking", ex.Message, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        this.ViewReference.ShowNotification(notification);
                    });
                this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Stops the flight tracking.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void StopTracking()
        {
            Debug.WriteLine("User wants to stop tracking, confirming...");
            ExtendedMessageBoxResult? answer = null;
            this.StopTrackingCommand.ReportProgress(
                () =>
                {
                    var messageBox = new OpenSkyMessageBox(
                        "Stop tracking?",
                        "WARNING: You are about to stop tracking your OpenSky flight!\r\n\r\nDo you want to save your current progress so that you can resume the flight later?",
                        MessageBoxButton.YesNoCancel,
                        ExtendedMessageBoxImage.Hand);
                    messageBox.SetWarningColorStyle();
                    messageBox.Closed += (_, _) => { answer = messageBox.Result; };
                    this.ViewReference.ShowMessageBox(messageBox);
                });
            while (answer == null && !SleepScheduler.IsShutdownInProgress)
            {
                Thread.Sleep(500);
            }

            if (answer is ExtendedMessageBoxResult.None or ExtendedMessageBoxResult.Cancel)
            {
                return;
            }

            if (answer == ExtendedMessageBoxResult.Yes)
            {
                try
                {
                    this.Simulator.StopTracking(true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error saving flight: " + ex);
                    this.StopTrackingCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error saving flight", ex.Message, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
            else
            {
                try
                {
                    this.Simulator.StopTracking(false);
                    this.Simulator.Flight = null;
                    this.IsFuelExpanded = false;
                    this.IsPayloadExpanded = false;
                    this.WeightAndBalancesVisibility = Visibility.Visible;
                    this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error stopping tracking: " + ex);
                    this.StopTrackingCommand.ReportProgress(
                        () =>
                        {
                            var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error stopping tracking", ex.Message, ExtendedMessageBoxImage.Error, 30);
                            notification.SetErrorColorStyle();
                            this.ViewReference.ShowNotification(notification);
                        });
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Toggle advanced weight and balance visibility.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ToggleAdvancedWeightAndBalance()
        {
            Debug.WriteLine("Advanced W&B toggled");
            this.WeightAndBalancesAdvancedVisibility = this.WeightAndBalancesAdvancedVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Toggle flight pause.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ToggleFlightPause()
        {
            try
            {
                Debug.WriteLine("Toggling simconnect flight pause");
                this.Simulator.Pause(!this.Simulator.IsPaused);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error pausing/resuming sim: " + ex);
                this.ToggleFlightPauseCommand.ReportProgress(
                    () =>
                    {
                        var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error pausing/resuming simulator", ex.Message, ExtendedMessageBoxImage.Error, 30);
                        notification.SetErrorColorStyle();
                        this.ViewReference.ShowNotification(notification);
                    });
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Toggle OFP visibility.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ToggleOfp()
        {
            Debug.WriteLine("OFP toggled");
            this.OfpVisibility = this.OfpVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}