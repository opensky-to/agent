// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingViewModel.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Media;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    using JetBrains.Annotations;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.SimConnect;
    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.AgentMSFS.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The view model for the flight tracking view.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
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
        /// The skip ground handling visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility skipGroundHandlingVisibility = Visibility.Collapsed;

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
                if(Equals(this.skipGroundHandlingVisibility, value))
                {
                   return;
                }
        
                this.skipGroundHandlingVisibility = value;
                this.NotifyPropertyChanged();
            }
        }



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
            for (var i = 0; i < 15; i++)
            {
                this.PayloadStationWeights.Add(0.0);
            }

            // Set up event log listeners
            this.SimConnect.TrackingEventMarkerAdded += this.SimConnectTrackingEventMarkerAdded;
            this.SimConnect.SimbriefWaypointMarkerAdded += this.SimConnectSimbriefWaypointMarkerAdded;
            this.SimConnect.TrackingStatusChanged += this.SimConnectTrackingStatusChanged;
            this.SimConnect.FlightChanged += this.SimConnectFlightChanged;
            this.SimConnect.LocationChanged += this.SimConnectLocationChanged;

            // Create commands
            this.SetFuelAndPayloadCommand = new Command(this.SetFuelAndPayload);
            this.ToggleAdvancedWeightAndBalanceCommand = new Command(this.ToggleAdvancedWeightAndBalance);
            this.SuggestFuelCommand = new Command(this.SuggestFuel);
            this.SetFuelTanksCommand = new Command(this.SetFuelTanks);
            this.SuggestPayloadCommand = new Command(this.SuggestPayload);
            this.SetPayloadStationsCommand = new Command(this.SetPayloadStations);
            this.StartTrackingCommand = new AsynchronousCommand(this.StartTracking, false);
            this.AbortFlightCommand = new AsynchronousCommand(this.AbortFlight, false);
            this.ToggleFlightPauseCommand = new Command(this.ToggleFlightPause, false);
            this.StopTrackingCommand = new AsynchronousCommand(this.StopTracking, false);
            this.ImportSimbriefCommand = new AsynchronousCommand(this.ImportSimbrief, false);
            this.MoveMapToCoordinateCommand = new Command(this.MoveMapToCoordinate);
            this.SlewIntoPositionCommand = new AsynchronousCommand(this.SlewIntoPosition);
            this.ToggleOfpCommand = new Command(this.ToggleOfp);
            this.SpeedUpGroundHandlingCommand = new Command(this.SpeedUpGroundHandling);
            this.SkipGroundHandlingCommand = new Command(this.SkipGroundHandling);

            // Are we already preparing/resuming/tracking?
            this.SimConnectTrackingStatusChanged(this, this.SimConnect.TrackingStatus);
            this.SimConnectFlightChanged(this, this.SimConnect.Flight);
            if (this.SimConnect.SimbriefOfpLoaded)
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
        /// Gets the SimConnect instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public SimConnect SimConnect => SimConnect.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the skip ground handling command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command SkipGroundHandlingCommand { get; }

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
        public Command ToggleFlightPauseCommand { get; }

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
            if (this.SimConnect.Flight == null)
            {
                return;
            }

            MessageBoxResult? answer = MessageBoxResult.None;
            this.AbortFlightCommand.ReportProgress(
                () =>
                {
                    answer = ModernWpf.MessageBox.Show(
                        "Are you sure you want to cancel the current flight?\r\n\r\nYou will loose any saved progress and have to return the OpenSky client to start another flight.",
                        "Cancel flight?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                },
                true);

            if (answer == MessageBoxResult.Yes)
            {
                this.LoadingText = "Aborting flight...";
                try
                {
                    this.SimConnect.StopTracking(false);
                    this.SimConnect.Flight = null;
                    this.IsFuelExpanded = false;
                    this.IsPayloadExpanded = false;
                    this.WeightAndBalancesVisibility = Visibility.Visible;
                    this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    ex.HandleApiCallException(this.AbortFlightCommand, "Error aborting flight");
                }
                finally
                {
                    this.LoadingText = null;
                }
            }
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
        /// SimConnect flight changed.
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
        private void SimConnectFlightChanged(object sender, Flight e)
        {
            UpdateGUIDelegate updateCommands = () =>
            {
                this.AbortFlightCommand.CanExecute = e != null;
                this.StartTrackingCommand.CanExecute = e != null;
                this.ImportSimbriefCommand.CanExecute = e != null;
                this.NoFlightVisibility = e != null ? Visibility.Collapsed : Visibility.Visible;
                this.StartTrackingButtonText = e?.Resume == true ? "Resume Tracking" : "Start Tracking";
            };

            Application.Current.Dispatcher.BeginInvoke(updateCommands);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// SimConnect tracking status changed.
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
        private void SimConnectTrackingStatusChanged(object sender, TrackingStatus e)
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
                if (!this.SimConnect.SimbriefOfpLoaded)
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
            if (this.SimConnect.SkipGroundHandling(false))
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
                this.SimConnect.SlewPlaneToFlightPosition();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error slewing plane into position: " + ex);
                this.SlewIntoPositionCommand.ReportProgress(() => ModernWpf.MessageBox.Show(ex.Message, "Error slewing into position", MessageBoxButton.OK, MessageBoxImage.Error));
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
            this.SimConnect.SpeedUpGroundHandling();
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
                if (this.SimConnect.TrackingStatus == TrackingStatus.Preparing)
                {
                    Debug.WriteLine("User clicked on start tracking, performing checks");
                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = false);

                    // Check conditions met
                    if (!this.SimConnect.CanStartTracking)
                    {
                        Debug.WriteLine("Tracking conditions not met");
                        this.StartTrackingCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Not all tracking conditions are met, please review, correct and try again.", "Start tracking", MessageBoxButton.OK, MessageBoxImage.Warning));
                        this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                        return;
                    }

                    // Set time in sim?
                    if (this.SimConnect.TrackingConditions[(int)TrackingConditions.DateTime].AutoSet && this.SimConnect.Flight != null)
                    {
                        this.SimConnect.SetTime(DateTime.UtcNow.AddHours(this.SimConnect.Flight.UtcOffset));
                    }

                    // Set fuel?
                    if (this.SimConnect.TrackingConditions[(int)TrackingConditions.Fuel].AutoSet)
                    {
                        this.StartTrackingCommand.ReportProgress(() => this.SetFuelTanksCommand.DoExecute(null), true);
                    }

                    // Set payload?
                    if (this.SimConnect.TrackingConditions[(int)TrackingConditions.Payload].AutoSet)
                    {
                        this.StartTrackingCommand.ReportProgress(() => this.SetPayloadStationsCommand.DoExecute(null), true);
                    }

                    // Set the plane registration
                    this.SimConnect.SetPlaneRegistry(this.SimConnect.Flight?.Aircraft.Registry);

                    // Wait a bit to make sure all structs have updated, especially time in sim
                    Thread.Sleep(this.SimConnect.SampleRates[Requests.Secondary] + 1000);

                    // Check weights and balance
                    if (this.SimConnect.WeightAndBalance.CgPercent < this.SimConnect.WeightAndBalance.CgFwdLimit || this.SimConnect.WeightAndBalance.CgPercent > this.SimConnect.WeightAndBalance.CgAftLimit)
                    {
                        Debug.WriteLine("CG outside limits, double checking with user...");
                        MessageBoxResult? answer = MessageBoxResult.None;
                        this.StartTrackingCommand.ReportProgress(
                            () => answer = ModernWpf.MessageBox.Show("The current CG is outside of the limits specified for this plane, are you sure you want to continue?", "CG limit", MessageBoxButton.YesNo, MessageBoxImage.Warning),
                            true);
                        if (answer != MessageBoxResult.Yes)
                        {
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }
                    }

                    if (Math.Abs(this.SimConnect.WeightAndBalance.CgPercentLateral) > 0.01)
                    {
                        Debug.WriteLine("Lateral CG outside limits, double checking with user...");
                        MessageBoxResult? answer = MessageBoxResult.None;
                        this.StartTrackingCommand.ReportProgress(
                            () => answer = ModernWpf.MessageBox.Show("The current lateral CG is outside of the limits specified for this plane, are you sure you want to continue?", "CG limit", MessageBoxButton.YesNo, MessageBoxImage.Warning),
                            true);
                        if (answer != MessageBoxResult.Yes)
                        {
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }
                    }

                    if (this.SimConnect.WeightAndBalance.PayloadWeight > this.SimConnect.WeightAndBalance.MaxPayloadWeight)
                    {
                        Debug.WriteLine("Payload weight outside limits, double checking with user...");
                        MessageBoxResult? answer = MessageBoxResult.None;
                        this.StartTrackingCommand.ReportProgress(
                            () => answer = ModernWpf.MessageBox.Show("The payload weight exceeds the limits specified for this plane, are you sure you want to continue?", "Payload weight limit", MessageBoxButton.YesNo, MessageBoxImage.Warning),
                            true);
                        if (answer != MessageBoxResult.Yes)
                        {
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }
                    }

                    if (this.SimConnect.WeightAndBalance.TotalWeight > this.SimConnect.WeightAndBalance.MaxGrossWeight)
                    {
                        Debug.WriteLine("Total weight outside limits, double checking with user...");
                        MessageBoxResult? answer = MessageBoxResult.None;
                        this.StartTrackingCommand.ReportProgress(
                            () => answer = ModernWpf.MessageBox.Show("The total weight exceeds the limits specified for this plane, are you sure you want to continue?", "Total weight limit", MessageBoxButton.YesNo, MessageBoxImage.Warning),
                            true);
                        if (answer != MessageBoxResult.Yes)
                        {
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }
                    }

                    if (!this.SimConnect.GroundHandlingComplete)
                    {
                        Debug.WriteLine("Ground handling not complete, ask the user about skipping...");
                        MessageBoxResult? answer = MessageBoxResult.None;
                        this.StartTrackingCommand.ReportProgress(
                            () => answer = ModernWpf.MessageBox.Show("Ground handling not yet completed, do you want to skip it?", "Ground handling", MessageBoxButton.YesNo, MessageBoxImage.Warning),
                            true);
                        if (answer != MessageBoxResult.Yes)
                        {
                            this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                            return;
                        }

                        this.SimConnect.SkipGroundHandling(true);
                    }

                    this.SimConnect.StartTracking();
                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                }

                // Resume flight
                if (this.SimConnect.TrackingStatus == TrackingStatus.Resuming)
                {
                    Debug.WriteLine("User clicked on resume tracking, performing checks");
                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = false);

                    // Check conditions met
                    if (!this.SimConnect.CanStartTracking)
                    {
                        Debug.WriteLine("Tracking conditions not met");
                        this.StartTrackingCommand.ReportProgress(() => ModernWpf.MessageBox.Show("Not all tracking conditions are met, please review, correct and try again.", "Start tracking", MessageBoxButton.OK, MessageBoxImage.Warning));
                        this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                        return;
                    }

                    // Set time in sim?
                    if (this.SimConnect.TrackingConditions[(int)TrackingConditions.DateTime].AutoSet && this.SimConnect.Flight != null)
                    {
                        this.SimConnect.SetTime(DateTime.UtcNow.AddHours(this.SimConnect.Flight.UtcOffset));
                    }

                    // Set fuel and payload back to what they were when we saved the flight
                    this.SimConnect.SetFuelAndPayloadFromSave();

                    // Set the plane registration
                    this.SimConnect.SetPlaneRegistry(this.SimConnect.Flight?.Aircraft.Registry);

                    // Start five second countdown?
                    if (this.SimConnect.PrimaryTracking.SlewActive)
                    {
                        Debug.WriteLine("Starting 5 second resume timer...");
                        var assembly = Assembly.GetExecutingAssembly();
                        var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSbeepbeepstart.wav"));
                        player.Play();
                        var secondsToGo = 5;
                        this.StartTrackingButtonText = "Resuming in 5";
                        while (secondsToGo > 0)
                        {
                            Thread.Sleep(1000);
                            secondsToGo--;
                            this.StartTrackingButtonText = secondsToGo > 0 ? $"Resuming in {secondsToGo}" : "Resuming now";
                        }

                        this.SimConnect.SetSlew(false);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        // Wait a bit to make sure all structs have updated, especially time in sim
                        Thread.Sleep(this.SimConnect.SampleRates[Requests.Secondary] + 1000);
                    }

                    this.SimConnect.StartTracking();
                    this.StartTrackingCommand.ReportProgress(() => this.StartTrackingCommand.CanExecute = true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting/resuming tracking: " + ex);
                this.StartTrackingCommand.ReportProgress(() => ModernWpf.MessageBox.Show(ex.Message, "Error starting or resuming tracking", MessageBoxButton.OK, MessageBoxImage.Error));
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
            MessageBoxResult? answer = MessageBoxResult.None;
            this.StopTrackingCommand.ReportProgress(
                () => answer = ModernWpf.MessageBox.Show(
                    "WARNING: You are about to stop tracking your OpenSky flight!\r\n\r\nDo you want to save your current progress so that you can resume the flight later?",
                    "Stop tracking?",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Hand),
                true);
            if (answer is MessageBoxResult.None or MessageBoxResult.Cancel)
            {
                return;
            }

            if (answer == MessageBoxResult.Yes)
            {
                try
                {
                    this.SimConnect.StopTracking(true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error saving flight: " + ex);
                    this.StopTrackingCommand.ReportProgress(() => ModernWpf.MessageBox.Show(ex.Message, "Error saving flight", MessageBoxButton.OK, MessageBoxImage.Error));
                }
            }
            else
            {
                try
                {
                    this.SimConnect.StopTracking(false);
                    this.SimConnect.Flight = null;
                    this.IsFuelExpanded = false;
                    this.IsPayloadExpanded = false;
                    this.WeightAndBalancesVisibility = Visibility.Visible;
                    this.WeightAndBalancesAdvancedVisibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error stopping tracking: " + ex);
                    this.StopTrackingCommand.ReportProgress(() => ModernWpf.MessageBox.Show(ex.Message, "Error stopping tracking", MessageBoxButton.OK, MessageBoxImage.Error));
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
            Debug.WriteLine("Toggling simconnect flight pause");
            this.SimConnect.Pause(!this.SimConnect.IsPaused);
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