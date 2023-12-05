// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Flight.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Device.Location;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.Agent.Simulator.Models;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    using PositionReport = OpenSkyApi.PositionReport;
    using TrackingEventMarker = Models.TrackingEventMarker;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - flight.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The auto-save upload mutex.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex autoSaveUploadMutex = new(false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight auto-save primary tracking mutex (to prevent multiple threads saving if it takes
        /// longer than 50ms), to prevent holding up the primary tracking queue processing.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex flightAutoSaveTrackFlightMutex = new(false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight save mutex.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex flightSaveMutex = new(false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The position report upload mutex.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex positionReportUploadMutex = new(false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky flight the player wants to track.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        private Flight flight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last active simulation rate (above 1).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double? lastActiveSimRate;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When was the last active simulation rate (above 1) activated?.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime? lastActiveSimRateActivated;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When was the last auto-save uploaded successfully?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastAutoSaveUpload = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When did we auto-save the flight log the last time?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastFlightLogAutoSave = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When was the last position report uploaded successfully?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastPositionReportUpload = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The time saved because of simulation rate above 1 (equals warp delay at the end of the flight).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private TimeSpan timeSavedBecauseOfSimRate = TimeSpan.Zero;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Duration of the tracking session.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string trackingDuration;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tracking started date/time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime? trackingStarted;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current tracking status.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private TrackingStatus trackingStatus = TrackingStatus.NotTracking;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The warp info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string warpInfo = "No [*]";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when flight changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<Flight> FlightChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the tracking status changed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<TrackingStatus> TrackingStatusChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether we can start tracking the current flight or not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool CanStartTracking
        {
            get
            {
                var allConditionsMet = this.Flight != null;
                foreach (TrackingConditions condition in Enum.GetValues(typeof(TrackingConditions)))
                {
                    if (this.TrackingConditions[(int)condition].Enabled && !this.TrackingConditions[(int)condition].ConditionMet)
                    {
                        // Date time and fuel aren't strict anymore, neither is new Vatsim one
                        var nonStrict = new[] { Models.TrackingConditions.DateTime, Models.TrackingConditions.Fuel, Models.TrackingConditions.Vatsim };
                        if (!nonStrict.Contains(condition))
                        {
                            allConditionsMet = false;
                        }
                    }
                }

                return allConditionsMet;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight the player wants to track.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public Flight Flight
        {
            get => this.flight;

            set
            {
                if (Equals(this.flight, value))
                {
                    return;
                }

                Debug.WriteLine("Simulator flight changing...");
                if (value != null && this.TrackingStatus != TrackingStatus.NotTracking)
                {
                    Debug.WriteLine("Throwing error: You need to abort the current flight before changing to a new one!");
                    throw new Exception("You need to abort the current flight before changing to a new one!");
                }

                this.flight = value;
                this.OnPropertyChanged();
                this.FlightChanged?.Invoke(this, value);

                if (value != null)
                {
                    // Start the ground handling thread for this flight
                    new Thread(this.DoGroundHandling) { Name = "OpenSky.Simulator.GroundHandling" }.Start();

                    // Add airport markers to map (do this in both new and restore, save file doesn't contain these cause the data would just be redundant)
                    UpdateGUIDelegate addAirports = () =>
                    {
                        lock (this.trackingEventMarkers)
                        {
                            var alternateMarker = new TrackingEventMarker(new GeoCoordinate(value.Alternate.Latitude, value.Alternate.Longitude), value.Alternate.Icao, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                            this.trackingEventMarkers.Add(alternateMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, alternateMarker);
                            var originMarker = new TrackingEventMarker(new GeoCoordinate(value.Origin.Latitude, value.Origin.Longitude), value.Origin.Icao, OpenSkyColors.OpenSkyTeal, Colors.White);
                            this.trackingEventMarkers.Add(originMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, originMarker);
                            var destinationMarker = new TrackingEventMarker(new GeoCoordinate(value.Destination.Latitude, value.Destination.Longitude), value.Destination.Icao, OpenSkyColors.OpenSkyTeal, Colors.White);
                            this.trackingEventMarkers.Add(destinationMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, destinationMarker);

                            var alternateDetailMarker = new TrackingEventMarker(value.Alternate, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                            this.trackingEventMarkers.Add(alternateDetailMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, alternateDetailMarker);
                            var originDetailMarker = new TrackingEventMarker(value.Origin, OpenSkyColors.OpenSkyTeal, Colors.White);
                            this.trackingEventMarkers.Add(originDetailMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, originDetailMarker);
                            var destinationDetailMarker = new TrackingEventMarker(value.Destination, OpenSkyColors.OpenSkyTeal, Colors.White);
                            this.trackingEventMarkers.Add(destinationDetailMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, destinationDetailMarker);

                            foreach (var runway in value.Alternate.Runways)
                            {
                                var runwayMarker = new TrackingEventMarker(runway);
                                this.trackingEventMarkers.Add(runwayMarker);
                                this.TrackingEventMarkerAdded?.Invoke(this, runwayMarker);
                            }

                            foreach (var runway in value.Origin.Runways)
                            {
                                var runwayMarker = new TrackingEventMarker(runway);
                                this.trackingEventMarkers.Add(runwayMarker);
                                this.TrackingEventMarkerAdded?.Invoke(this, runwayMarker);
                            }

                            foreach (var runway in value.Destination.Runways)
                            {
                                var runwayMarker = new TrackingEventMarker(runway);
                                this.trackingEventMarkers.Add(runwayMarker);
                                this.TrackingEventMarkerAdded?.Invoke(this, runwayMarker);
                            }
                        }
                    };

                    this.TrackingConditions[(int)Models.TrackingConditions.Fuel].Expected =
                        $"{value.FuelGallons:F1} gal, {value.FuelGallons * 3.78541:F1} liters ▶ {value.FuelGallons * value.Aircraft.Type.FuelWeightPerGallon:F1} lbs, {value.FuelGallons * value.Aircraft.Type.FuelWeightPerGallon * 0.453592:F1} kg";
                    this.TrackingConditions[(int)Models.TrackingConditions.Payload].Expected = $"{value.PayloadPounds:F1} lbs, {value.PayloadPounds * 0.453592:F1} kg";
                    this.TrackingConditions[(int)Models.TrackingConditions.PlaneModel].Expected = $"{value.Aircraft.Type.Name} (v{value.Aircraft.Type.VersionNumber})";

                    this.TrackingConditions[(int)Models.TrackingConditions.Fuel].AutoSet = !value.Aircraft.Type.RequiresManualFuelling;
                    this.TrackingConditions[(int)Models.TrackingConditions.Payload].AutoSet = !value.Aircraft.Type.RequiresManualLoading;

                    this.OnlineNetworkConnectionDuration = TimeSpan.Zero;
                    if (value.OnlineNetwork == OnlineNetwork.Vatsim)
                    {
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].Enabled = true;
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].Current = "Unknown";
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].Expected = $"True, Callsign: {value.AtcCallsign}, Flight plan: {value.Origin.Icao}-{value.Destination.Icao}, Location: <50 km";
                    }
                    else
                    {
                        this.TrackingConditions[(int)Models.TrackingConditions.Vatsim].Enabled = false;
                    }

                    if (!value.Resume)
                    {
                        Debug.WriteLine("Preparing to track new flight");
                        this.flightLoadingTempModels = null;
                        this.TrackingStatus = TrackingStatus.Preparing;
                        Application.Current.Dispatcher.BeginInvoke(addAirports);

                        this.WarpInfo = "No [*]";

                        // Add simbrief navlog to map
                        UpdateGUIDelegate addNavlog = () =>
                        {
                            if (value.NavlogFixes?.Count > 0)
                            {
                                this.SimbriefOfpLoaded = true;
                                foreach (var flightNavlogFix in value.NavlogFixes)
                                {
                                    this.SimbriefRouteLocations.Add(new Location(flightNavlogFix.Latitude, flightNavlogFix.Longitude));
                                    if (flightNavlogFix.Type != "apt")
                                    {
                                        var newMarker = new SimbriefWaypointMarker(flightNavlogFix.Latitude, flightNavlogFix.Longitude, flightNavlogFix.Ident, flightNavlogFix.Type);
                                        this.simbriefWaypointMarkers.Add(newMarker);
                                        this.SimbriefWaypointMarkerAdded?.Invoke(this, newMarker);
                                    }
                                }
                            }
                        };
                        Application.Current.Dispatcher.BeginInvoke(addNavlog);
                    }
                    else
                    {
                        Debug.WriteLine("Preparing to track resumed flight");
                        this.TrackingStatus = TrackingStatus.Resuming;
                        Application.Current.Dispatcher.BeginInvoke(addAirports);
                        this.CheckCloudSave();
                        this.LoadFlight();

                        this.flightLoadingTempModels = new FlightLoadingTempModels
                        {
                            FuelTanks = new FuelTanks
                            {
                                FuelTankCenterQuantity = value.FuelTankCenterQuantity ?? 0,
                                FuelTankCenter2Quantity = value.FuelTankCenter2Quantity ?? 0,
                                FuelTankCenter3Quantity = value.FuelTankCenter3Quantity ?? 0,
                                FuelTankLeftMainQuantity = value.FuelTankLeftMainQuantity ?? 0,
                                FuelTankLeftAuxQuantity = value.FuelTankLeftAuxQuantity ?? 0,
                                FuelTankLeftTipQuantity = value.FuelTankLeftTipQuantity ?? 0,
                                FuelTankRightMainQuantity = value.FuelTankRightMainQuantity ?? 0,
                                FuelTankRightAuxQuantity = value.FuelTankRightAuxQuantity ?? 0,
                                FuelTankRightTipCapacity = value.FuelTankRightTipQuantity ?? 0,
                                FuelTankExternal1Quantity = value.FuelTankExternal1Quantity ?? 0,
                                FuelTankExternal2Quantity = value.FuelTankExternal2Quantity ?? 0
                            },
                            PayloadStations = new PayloadStations(), // todo restore payload stations once we have that, all 0s for now
                            SlewAircraftIntoPosition = new SlewAircraftIntoPosition
                            {
                                Latitude = value.Latitude ?? 0,
                                Longitude = value.Longitude ?? 0,
                                Heading = value.Heading ?? 0,
                                BankAngle = value.BankAngle,
                                PitchAngle = value.PitchAngle,
                                OnGround = value.OnGround,
                                AirspeedTrue = value.AirspeedTrue ?? 0,
                                Altitude = value.Altitude ?? 0,
                                RadioHeight = value.RadioHeight ?? 0,
                                VerticalSpeedSeconds = value.VerticalSpeedSeconds
                            }
                        };

                        this.TrackingConditions[(int)Models.TrackingConditions.Fuel].Expected =
                            $"{this.flightLoadingTempModels.FuelTanks.TotalQuantity:F1} gal, {this.flightLoadingTempModels.FuelTanks.TotalQuantity * 3.78541:F1} liters ▶ {this.flightLoadingTempModels.FuelTanks.TotalQuantity * value.Aircraft.Type.FuelWeightPerGallon:F1} lbs, {this.flightLoadingTempModels.FuelTanks.TotalQuantity * value.Aircraft.Type.FuelWeightPerGallon * 0.453592:F1} kg";
                    }
                }
                else
                {
                    Debug.WriteLine("Flight set to NULL, stopping tracking");
                    this.flightLoadingTempModels = null;
                    this.StopTracking(false);
                    this.lastFlightLogAutoSave = DateTime.MinValue;
                    this.simbriefOfpLoaded = false;
                    this.OnlineNetworkConnectionDuration = TimeSpan.Zero;
                    this.OnlineNetworkConnectionStarted = null;
                    this.VatsimClientConnection = null;

                    // Reset ground handling
                    this.GroundHandlingComplete = false;
                    this.FuelLoadingComplete = false;
                    this.PayloadLoadingComplete = false;
                    this.FuelLoadingEstimateMinutes = "??";
                    this.PayloadLoadingEstimateMinutes = "??";
                    this.fuelLoadingCompleteSoundPlayed = false;
                    this.payloadLoadingCompleteSoundPlayed = false;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the duration of the tracking session.
        /// </summary>
        /// <seealso cref="P:OpenSky.Agent.Simulator.Simulator.TrackingDuration"/>
        /// -------------------------------------------------------------------------------------------------
        public string TrackingDuration
        {
            get => this.trackingDuration;

            protected set
            {
                if (Equals(this.trackingDuration, value))
                {
                    return;
                }

                this.trackingDuration = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current tracking status.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public TrackingStatus TrackingStatus
        {
            get => this.trackingStatus;

            protected set
            {
                if (Equals(this.trackingStatus, value))
                {
                    return;
                }

                this.trackingStatus = value;
                this.OnPropertyChanged();
                this.TrackingStatusChanged?.Invoke(this, value);

                // Reduce the loading and fuel sample rates while preparing to make it easier for the user to adjust them for manual aircraft
                if (value is not TrackingStatus.NotTracking and not TrackingStatus.Tracking)
                {
                    this.SampleRates[Requests.FuelTanks] = 2000;
                    this.SampleRates[Requests.PayloadStations] = 2000;
                    this.SampleRates[Requests.WeightAndBalance] = 2000;
                }
                else
                {
                    this.SampleRates[Requests.FuelTanks] = 15000;
                    this.SampleRates[Requests.PayloadStations] = 15000;
                    this.SampleRates[Requests.WeightAndBalance] = 15000;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the warp info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string WarpInfo
        {
            get => this.warpInfo;

            protected set
            {
                if (Equals(this.warpInfo, value))
                {
                    return;
                }

                this.warpInfo = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts flight tracking.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <seealso cref="M:OpenSky.Agent.Simulator.Simulator.StartTracking()"/>
        /// -------------------------------------------------------------------------------------------------
        public void StartTracking()
        {
            Debug.WriteLine("Simulator asked to start/resume tracking...");
            if (!this.CanStartTracking)
            {
                throw new Exception("Not all tracking conditions met, cannot start tracking.");
            }

            if (!this.GroundHandlingComplete)
            {
                // Are the engines already running?
                if (this.SecondaryTracking.EngineRunning)
                {
                    Debug.WriteLine("Ground handling not done, but engines running");
                    throw new Exception("You cannot start tracking if your engines are running and the ground handling is not complete!");
                }

                this.TrackingStatus = TrackingStatus.GroundOperations;
                this.trackingStarted = DateTime.UtcNow;
                this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, FlightTrackingEventType.TrackingStarted, OpenSkyColors.OpenSkyTealLight, "Flight tracking started");
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.TrackingStartedGroundHandling);
            }
            else
            {
                if (this.TrackingStatus != TrackingStatus.Tracking)
                {
                    if (this.TrackingStatus == TrackingStatus.Preparing)
                    {
                        this.TrackingStatus = TrackingStatus.Tracking;
                        Debug.WriteLine("Flight tracking starting...");
                        SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.TrackingStarted);
                        this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, FlightTrackingEventType.TrackingStarted, OpenSkyColors.OpenSkyTealLight, "Flight tracking started");
                    }

                    if (this.TrackingStatus == TrackingStatus.Resuming)
                    {
                        this.TrackingStatus = TrackingStatus.Tracking;
                        Debug.WriteLine("Flight tracking resuming...");
                        SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.TrackingResumed);
                        this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, FlightTrackingEventType.TrackingResumed, OpenSkyColors.OpenSkyTealLight, "Flight tracking resumed");

                        // Check if we just resumed a save that failed to upload
                        if (this.FlightPhase == FlightPhase.PostFlight)
                        {
                            this.FinishUpFlightTracking();
                        }
                    }

                    this.trackingStarted = DateTime.UtcNow;
                }
            }

            // Is the pilot already connected to Vatsim?
            if (this.Flight?.OnlineNetwork == OnlineNetwork.Vatsim && this.VatsimClientConnection != null)
            {
                this.OnlineNetworkConnectionStarted = DateTime.UtcNow;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Stops the flight tracking session, doesn't abort flight but cleans up tracking events.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// <param name="resumeLater">
        /// True if the user decided to save and resume later.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void StopTracking(bool resumeLater)
        {
            Debug.WriteLine($"Asked to stop tracking...resume: {resumeLater}");

            this.TrackingStatus = this.Flight != null ? (this.Flight.Resume ? TrackingStatus.Resuming : TrackingStatus.Preparing) : TrackingStatus.NotTracking;

            // Was the flight set to null? If so, also reset the tracking conditions
            if (this.Flight == null)
            {
                Debug.WriteLine("Flight is NULL, resetting tracking conditions");
                foreach (TrackingConditions condition in Enum.GetValues(typeof(TrackingConditions)))
                {
                    this.TrackingConditions[(int)condition].Reset();
                }

                this.TrackingConditions[(int)Models.TrackingConditions.RealismSettings].Expected = "No slew, No unlimited fuel,\r\nCrash detection, SimRate=0 or 1";
            }

            if (!resumeLater)
            {
                this.trackingStarted = null;
                this.timeSavedBecauseOfSimRate = TimeSpan.Zero;
                this.TrackingDuration = string.Empty;
                this.lastActiveSimRate = null;
                this.lastActiveSimRateActivated = null;
                this.WarpInfo = string.Empty;
                this.PauseInfo = string.Empty;
                this.totalPaused = TimeSpan.Zero;
                this.lastFlightLogAutoSave = DateTime.MinValue;
                this.lastPositionReportUpload = DateTime.MinValue;
                this.lastAutoSaveUpload = DateTime.MinValue;

                this.DeleteSaveFile();
                if (this.Flight != null)
                {
                    // Abort flight
                    try
                    {
                        var result = this.openSkyServiceInstance.AbortFlightAsync(this.Flight.Id).Result;
                        if (result.IsError)
                        {
                            Debug.WriteLine("Error aborting flight on OpenSky: " + result.Message + "\r\n" + result.ErrorDetails);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error aborting flight on OpenSky: " + ex);
                    }
                }

                UpdateGUIDelegate resetMarkersAndEvents = () =>
                {
                    Debug.WriteLine("Resetting markers and events");
                    this.TrackingEventLogEntries.Clear();
                    this.AircraftTrailLocations.Clear();
                    lock (this.trackingEventMarkers)
                    {
                        this.trackingEventMarkers.Clear();
                    }

                    this.LandingReports.Clear();

                    if (this.Flight == null)
                    {
                        this.SimbriefRouteLocations.Clear();
                        this.simbriefWaypointMarkers.Clear();
                    }
                    else
                    {
                        lock (this.trackingEventMarkers)
                        {
                            // Add airport markers to map
                            var originMarker = new TrackingEventMarker(new GeoCoordinate(this.Flight.Origin.Latitude, this.Flight.Origin.Longitude), this.Flight.Origin.Icao, OpenSkyColors.OpenSkyTeal, Colors.White);
                            this.trackingEventMarkers.Add(originMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, originMarker);
                            var alternateMarker = new TrackingEventMarker(new GeoCoordinate(this.Flight.Alternate.Latitude, this.Flight.Alternate.Longitude), this.Flight.Alternate.Icao, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                            this.trackingEventMarkers.Add(alternateMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, alternateMarker);
                            var destinationMarker = new TrackingEventMarker(new GeoCoordinate(this.Flight.Destination.Latitude, this.Flight.Destination.Longitude), this.Flight.Destination.Icao, OpenSkyColors.OpenSkyTeal, Colors.White);
                            this.trackingEventMarkers.Add(destinationMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, destinationMarker);
                        }
                    }
                };
                Application.Current.Dispatcher.BeginInvoke(resetMarkersAndEvents);
            }
            else
            {
                this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, FlightTrackingEventType.TrackingPaused, OpenSkyColors.OpenSkyTealLight, "Flight tracking paused");
                this.SaveFlight();
                SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.FlightSavedPaused);
                this.UploadPositionReport();
                this.UploadAutoSave();

                try
                {
                    if (!this.flightSaveMutex.WaitOne(30 * 1000))
                    {
                        throw new Exception("Timeout waiting for save flight mutex.");
                    }

                    this.PauseFlight();

                    this.flightLoadingTempModels = new FlightLoadingTempModels
                    {
                        FuelTanks = this.FuelTanks,
                        PayloadStations = this.PayloadStations,
                        SlewAircraftIntoPosition = SlewAircraftIntoPosition.FromPrimaryTracking(this.PrimaryTracking)
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error waiting for saves and uploads before pausing flight: {ex}");
                }
                finally
                {
                    try
                    {
                        this.flightSaveMutex.ReleaseMutex();
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check if there is a (newer) cloud save for this flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void CheckCloudSave()
        {
            Debug.WriteLine($"Checking if cloud-save for flight {this.Flight?.Id} is more recent than local save...");
            try
            {
                if (this.Flight == null)
                {
                    throw new Exception("No flight loaded that could be restored!");
                }

                if (!this.Flight.HasAutoSaveLog || !this.Flight.LastAutoSave.HasValue)
                {
                    // Flight has no cloud-save
                    return;
                }

                if (!this.flightSaveMutex.WaitOne(30 * 1000))
                {
                    throw new Exception("Timeout waiting for save flight mutex.");
                }

                var flightSaveDirectory = "%localappdata%\\OpenSky\\Flights\\";
                flightSaveDirectory = Environment.ExpandEnvironmentVariables(flightSaveDirectory);
                var saveFileName = $"{flightSaveDirectory}\\opensky-flight-{this.Flight.Id}.save";

                var downloadCloudSave = false;
                if (!File.Exists(saveFileName))
                {
                    downloadCloudSave = true;
                }
                else
                {
                    var localSaveTime = File.GetLastWriteTimeUtc(saveFileName);
                    if (this.Flight.LastAutoSave.Value.UtcDateTime > localSaveTime)
                    {
                        downloadCloudSave = true;
                    }
                }

                if (downloadCloudSave)
                {
                    var result = this.openSkyServiceInstance.DownloadFlightAutoSaveAsync(this.Flight.Id).Result;
                    if (!result.IsError)
                    {
                        var base64 = result.Data;
                        if (base64 != null)
                        {
                            var binary = Convert.FromBase64String(base64);
                            File.WriteAllBytes(saveFileName, binary);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error downloading cloud save: " + result.Message + "\r\n" + result.ErrorDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error downloading cloud save: " + ex);
                throw;
            }
            finally
            {
                try
                {
                    this.flightSaveMutex.ReleaseMutex();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Delete save file for current flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/04/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// -------------------------------------------------------------------------------------------------
        private void DeleteSaveFile()
        {
            try
            {
                if (this.Flight == null)
                {
                    throw new Exception("No flight loaded, so not sure which file to delete.");
                }

                if (!this.flightSaveMutex.WaitOne(30 * 1000))
                {
                    throw new Exception("Timeout waiting for save flight mutex.");
                }

                var flightSaveDirectory = "%localappdata%\\OpenSky\\Flights\\";
                flightSaveDirectory = Environment.ExpandEnvironmentVariables(flightSaveDirectory);
                var saveFileName = $"{flightSaveDirectory}\\opensky-flight-{this.Flight.Id}.save";

                if (!File.Exists(saveFileName))
                {
                    throw new Exception("No save file to delete found for this flight!");
                }

                Debug.WriteLine($"Deleting flight save file: {saveFileName}");
                File.Delete(saveFileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error deleting flight save file: " + ex);
            }
            finally
            {
                try
                {
                    this.flightSaveMutex.ReleaseMutex();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Finishes up the flight tracking.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void FinishUpFlightTracking()
        {
            Debug.WriteLine("SimConnect finishing up flight tracking started");

            var callStopFlight = false;
            if (this.Flight != null)
            {
                try
                {
                    Debug.WriteLine("Creating final position report...");
                    var onlineForDuration = this.OnlineNetworkConnectionDuration;
                    if (this.OnlineNetworkConnectionStarted.HasValue)
                    {
                        onlineForDuration += (DateTime.UtcNow - this.OnlineNetworkConnectionStarted.Value);
                    }

                    var positionReport = new PositionReport
                    {
                        Id = this.Flight.Id,
                        AirspeedTrue = this.PrimaryTracking.AirspeedTrue,
                        Altitude = this.PrimaryTracking.Altitude,
                        BankAngle = this.PrimaryTracking.BankAngle,
                        FlightPhase = this.PrimaryTracking.Crash ? FlightPhase.Crashed : this.FlightPhase,
                        GroundSpeed = this.PrimaryTracking.GroundSpeed,
                        Heading = this.PrimaryTracking.Heading,
                        Latitude = this.PrimaryTracking.Latitude,
                        Longitude = this.PrimaryTracking.Longitude,
                        OnGround = this.PrimaryTracking.OnGround,
                        PitchAngle = this.PrimaryTracking.PitchAngle,
                        RadioHeight = this.PrimaryTracking.RadioHeight,
                        VerticalSpeedSeconds = this.PrimaryTracking.VerticalSpeedSeconds,
                        TimeWarpTimeSavedSeconds = (int)this.timeSavedBecauseOfSimRate.TotalSeconds,
                        ConnectedToOnlineNetworkSeconds = (int)onlineForDuration.TotalSeconds,

                        FuelTankCenterQuantity = this.FuelTanks.FuelTankCenterQuantity,
                        FuelTankCenter2Quantity = this.FuelTanks.FuelTankCenter2Quantity,
                        FuelTankCenter3Quantity = this.FuelTanks.FuelTankCenter3Quantity,
                        FuelTankLeftMainQuantity = this.FuelTanks.FuelTankLeftMainQuantity,
                        FuelTankLeftAuxQuantity = this.FuelTanks.FuelTankLeftAuxQuantity,
                        FuelTankLeftTipQuantity = this.FuelTanks.FuelTankLeftTipQuantity,
                        FuelTankRightMainQuantity = this.FuelTanks.FuelTankRightMainQuantity,
                        FuelTankRightAuxQuantity = this.FuelTanks.FuelTankRightAuxQuantity,
                        FuelTankRightTipQuantity = this.FuelTanks.FuelTankRightTipQuantity,
                        FuelTankExternal1Quantity = this.FuelTanks.FuelTankExternal1Quantity,
                        FuelTankExternal2Quantity = this.FuelTanks.FuelTankExternal2Quantity
                    };

                    this.RefreshModelNow(Requests.FuelTanks);
                    this.RefreshModelNow(Requests.PayloadStations);
                    Thread.Sleep(500);

                    var saveFile = this.GenerateSaveFile();
                    if (saveFile != null)
                    {
                        var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes($"{saveFile}"));
                        xmlStream.Seek(0, SeekOrigin.Begin);
                        var targetMemoryStream = new MemoryStream();
                        byte[] zippedBytes;
                        using (var gzip = new GZipStream(targetMemoryStream, CompressionMode.Compress))
                        {
                            xmlStream.CopyTo(gzip);
                            gzip.Close();
                            zippedBytes = targetMemoryStream.ToArray();
                        }

                        var base64String = Convert.ToBase64String(zippedBytes);

                        Debug.WriteLine("Submitting final report to OpenSky server...");
                        var finalReport = new FinalReport
                        {
                            FinalPositionReport = positionReport,
                            FlightLog = base64String
                        };

                        var result = this.openSkyServiceInstance.CompleteFlightAsync(finalReport).Result;
                        if (result.IsError)
                        {
                            Debug.WriteLine("Error submitting final flight report: " + result.Message + "\r\n" + result.ErrorDetails);

                            // todo how to handle this? save to a special file or only offer retry? Or does the user have to resume from the last save?
                        }
                        else
                        {
                            Debug.WriteLine("Submitting final report to OpenSky server successfully!");
                            callStopFlight = true;
                        }
                    }
                }
                catch (AbandonedMutexException)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error submitting final flight report: " + ex);

                    // todo how to handle this? save to a special file or only offer retry? Or does the user have to resume from the last save?
                }
                finally
                {
                    try
                    {
                        this.flightSaveMutex.ReleaseMutex();
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
            else
            {
                Debug.WriteLine("CRITICAL: Unable to finish up flight tracking and submitting final log because SimConnect.Flight is NULL!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                callStopFlight = true;
            }

            // todo only call this if the saving/etc. worked, ask user for retry etc.
            if (callStopFlight)
            {
                this.DeleteSaveFile();
                this.Flight = null;
                this.StopTracking(false);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads the tracking details of a saved flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LoadFlight()
        {
            try
            {
                Debug.WriteLine($"Loading flight {this.Flight?.Id}");
                if (this.Flight == null)
                {
                    throw new Exception("No flight loaded that could be restored!");
                }

                if (!this.flightSaveMutex.WaitOne(30 * 1000))
                {
                    throw new Exception("Timeout waiting for save flight mutex.");
                }

                var flightSaveDirectory = "%localappdata%\\OpenSky\\Flights\\";
                flightSaveDirectory = Environment.ExpandEnvironmentVariables(flightSaveDirectory);
                var saveFileName = $"{flightSaveDirectory}\\opensky-flight-{this.Flight.Id}.save";

                if (!File.Exists(saveFileName))
                {
                    throw new Exception("No save file found for this flight!");
                }

                var xmlStream = new MemoryStream();
                using (var fileStream = File.OpenRead(saveFileName))
                {
                    using (var gzip = new GZipStream(fileStream, CompressionMode.Decompress))
                    {
                        gzip.CopyTo(xmlStream);
                    }
                }

                xmlStream.Seek(0, SeekOrigin.Begin);
                var saveFile = Encoding.UTF8.GetString(xmlStream.ToArray());
                this.RestoreSaveFile(saveFile);

                this.lastFlightLogAutoSave = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading flight: " + ex);
                throw;
            }
            finally
            {
                try
                {
                    this.flightSaveMutex.ReleaseMutex();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Pause the current flight on OpenSky.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/11/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// -------------------------------------------------------------------------------------------------
        private void PauseFlight()
        {
            if (this.Flight == null)
            {
                throw new Exception("No flight loaded that could be paused.");
            }

            new Thread(
                    () =>
                    {
                        try
                        {
                            var result = this.openSkyServiceInstance.PauseFlightAsync(this.Flight.Id).Result;
                            if (result.IsError)
                            {
                                Debug.WriteLine("Error pausing flight: " + result.Message + "\r\n" + result.ErrorDetails);
                            }
                            else
                            {
                                this.Flight = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error pausing flight: " + ex);
                        }
                    })
            { Name = "SimConnect.Flight.Pause" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Saves the current flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SaveFlight()
        {
            if (this.Flight == null)
            {
                throw new Exception("No flight loaded that could be saved.");
            }

            new Thread(
                    () =>
                    {
                        try
                        {
                            if (!this.flightAutoSaveTrackFlightMutex.WaitOne(50))
                            {
                                return;
                            }

                            if (!this.flightSaveMutex.WaitOne(30 * 1000))
                            {
                                throw new Exception("Timeout waiting for save flight mutex.");
                            }

                            this.RefreshModelNow(Requests.FuelTanks);
                            this.RefreshModelNow(Requests.PayloadStations);
                            Thread.Sleep(500);

                            Debug.WriteLine($"Saving flight {this.Flight?.Id}");
                            var flightSaveDirectory = "%localappdata%\\OpenSky\\Flights\\";
                            flightSaveDirectory = Environment.ExpandEnvironmentVariables(flightSaveDirectory);
                            if (!Directory.Exists(flightSaveDirectory))
                            {
                                Directory.CreateDirectory(flightSaveDirectory);
                            }

                            var saveFile = this.GenerateSaveFile();

                            if (saveFile != null && this.Flight != null)
                            {
                                var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes($"{saveFile}"));
                                using (var fileStream = File.Create($"{flightSaveDirectory}\\opensky-flight-{this.Flight.Id}.save"))
                                {
                                    using (var gzip = new GZipStream(fileStream, CompressionMode.Compress))
                                    {
                                        xmlStream.CopyTo(gzip);
                                    }
                                }

                                // todo remove this after testing
                                File.WriteAllText($"{flightSaveDirectory}\\opensky-flight-{this.Flight.Id}.xml", $"{saveFile}");

                                this.lastFlightLogAutoSave = DateTime.UtcNow;
                            }
                        }
                        catch (AbandonedMutexException)
                        {
                            // Ignore
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error saving flight: " + ex);
                        }
                        finally
                        {
                            try
                            {
                                this.flightAutoSaveTrackFlightMutex.ReleaseMutex();
                            }
                            catch
                            {
                                // Ignore
                            }

                            try
                            {
                                this.flightSaveMutex.ReleaseMutex();
                            }
                            catch
                            {
                                // Ignore
                            }
                        }
                    })
            { Name = "SimConnect.Flight.SaveFlight" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Track flight parameters like SimRate, pausing, etc.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/03/2021.
        /// </remarks>
        /// <param name="ppt">
        /// The primary Simconnect tracking data (old and new).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void TrackFlight(ProcessPrimaryTracking ppt)
        {
            if (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
            {
                // Update info strings
                if (this.trackingStarted.HasValue)
                {
                    var duration = DateTime.UtcNow - this.trackingStarted.Value;
                    this.TrackingDuration = duration.TotalDays >= 1 ? duration.ToString("d\\.hh\\:mm\\:ss") : duration.ToString("hh\\:mm\\:ss");
                }

                this.PauseInfo = this.totalPaused.TotalSeconds >= 1 ? (this.totalPaused.TotalDays >= 1 ? $"Yes, for {this.totalPaused:d\\.hh\\:mm\\:ss} [*]" : $"Yes, for {this.totalPaused:hh\\:mm\\:ss} [*]") : "No [*]";

                // Check for SimRate change
                if (Math.Abs(ppt.Old.SimulationRate - ppt.New.SimulationRate) > 0)
                {
                    Debug.WriteLine($"SimRate changed to {ppt.New.SimulationRate}");
                    this.AddTrackingEvent(ppt.New, this.SecondaryTracking, FlightTrackingEventType.SimRateChanged, Colors.DarkViolet, $"Simrate changed: {ppt.New.SimulationRate}");

                    // Was there an active simrate above 1?
                    if (this.lastActiveSimRate.HasValue && this.lastActiveSimRateActivated.HasValue)
                    {
                        var duration = DateTime.UtcNow - this.lastActiveSimRateActivated.Value;
                        this.timeSavedBecauseOfSimRate = this.timeSavedBecauseOfSimRate.Add(new TimeSpan((long)(duration.Ticks * this.lastActiveSimRate.Value)));
                        this.lastActiveSimRate = null;
                        this.lastActiveSimRateActivated = null;
                    }

                    if (ppt.New.SimulationRate > 1)
                    {
                        this.lastActiveSimRate = ppt.New.SimulationRate;
                        this.lastActiveSimRateActivated = DateTime.UtcNow;
                    }

                    this.WarpInfo = this.timeSavedBecauseOfSimRate.TotalSeconds >= 1 ? $"Yes, saved {this.timeSavedBecauseOfSimRate:hh\\:mm\\:ss} [*]" : "No [*]";
                }

                // todo Check if user has paused the sim (but didn't use the pause button we provide) ... check if lat/lon isn't changing while in flight

                if (this.PrimaryTracking.Crash)
                {
                    // Plane crashed
                    this.AddTrackingEvent(ppt.New, this.SecondaryTracking, FlightTrackingEventType.Crashed, OpenSkyColors.OpenSkyRed, "Aircraft crashed");

                    // todo play some ELT sound? to be proper annoying :)
                    this.FinishUpFlightTracking();
                }

                // Auto-save flight log?
                if ((DateTime.UtcNow - this.lastFlightLogAutoSave).TotalMinutes > 2)
                {
                    Debug.WriteLine("Auto-saving flight");
                    this.SaveFlight();
                }

                // Do a position report?
                if ((DateTime.UtcNow - this.lastPositionReportUpload).TotalSeconds > 30)
                {
                    this.UploadPositionReport();
                }

                // Upload auto-save?
                if ((DateTime.UtcNow - this.lastAutoSaveUpload).TotalMinutes > 10)
                {
                    this.UploadAutoSave();
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Upload auto-save to OpenSky.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UploadAutoSave()
        {
            if (this.Flight == null)
            {
                throw new Exception("No flight loaded that could be cloud saved.");
            }

            new Thread(
                    () =>
                    {
                        try
                        {
                            if (!this.autoSaveUploadMutex.WaitOne(50))
                            {
                                return;
                            }

                            if (!this.flightSaveMutex.WaitOne(30 * 1000))
                            {
                                throw new Exception("Timeout waiting for save flight mutex.");
                            }

                            var flightSaveDirectory = "%localappdata%\\OpenSky\\Flights\\";
                            flightSaveDirectory = Environment.ExpandEnvironmentVariables(flightSaveDirectory);
                            var saveFileName = $"{flightSaveDirectory}\\opensky-flight-{this.Flight.Id}.save";

                            if (!File.Exists(saveFileName))
                            {
                                // Wait one minute before trying this again
                                this.lastAutoSaveUpload = DateTime.UtcNow.AddMinutes(-9);
                                return;
                            }

                            var bytes = File.ReadAllBytes(saveFileName);
                            var base64String = Convert.ToBase64String(bytes);

                            var result = this.openSkyServiceInstance.UploadFlightAutoSaveAsync(this.Flight.Id, base64String).Result;
                            if (!result.IsError)
                            {
                                this.lastAutoSaveUpload = DateTime.UtcNow;
                            }
                            else
                            {
                                Debug.WriteLine("Error uploading auto-save: " + result.Message + "\r\n" + result.ErrorDetails);
                            }
                        }
                        catch (AbandonedMutexException)
                        {
                            // Ignore
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error uploading auto-save: " + ex);
                        }
                        finally
                        {
                            try
                            {
                                this.autoSaveUploadMutex.ReleaseMutex();
                            }
                            catch
                            {
                                // Ignore
                            }

                            try
                            {
                                this.flightSaveMutex.ReleaseMutex();
                            }
                            catch
                            {
                                // Ignore
                            }
                        }
                    })
            { Name = "SimConnect.Flight.UploadAutoSave" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Upload flight position report to OpenSky.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void UploadPositionReport()
        {
            if (this.Flight == null)
            {
                throw new Exception("No flight loaded that could be position reported.");
            }

            new Thread(
                    () =>
                    {
                        try
                        {
                            if (!this.positionReportUploadMutex.WaitOne(50))
                            {
                                return;
                            }

                            var onlineForDuration = this.OnlineNetworkConnectionDuration;
                            if (this.OnlineNetworkConnectionStarted.HasValue)
                            {
                                onlineForDuration += (DateTime.UtcNow - this.OnlineNetworkConnectionStarted.Value);
                            }

                            var positionReport = new PositionReport
                            {
                                Id = this.Flight.Id,
                                AirspeedTrue = this.PrimaryTracking.AirspeedTrue,
                                Altitude = this.PrimaryTracking.Altitude,
                                BankAngle = this.PrimaryTracking.BankAngle,
                                FlightPhase = this.PrimaryTracking.Crash ? FlightPhase.Crashed : this.FlightPhase,
                                GroundSpeed = this.PrimaryTracking.GroundSpeed,
                                Heading = this.PrimaryTracking.Heading,
                                Latitude = this.PrimaryTracking.Latitude,
                                Longitude = this.PrimaryTracking.Longitude,
                                OnGround = this.PrimaryTracking.OnGround,
                                PitchAngle = this.PrimaryTracking.PitchAngle,
                                RadioHeight = this.PrimaryTracking.RadioHeight,
                                VerticalSpeedSeconds = this.PrimaryTracking.VerticalSpeedSeconds,
                                TimeWarpTimeSavedSeconds = (int)this.timeSavedBecauseOfSimRate.TotalSeconds,
                                ConnectedToOnlineNetworkSeconds = (int)onlineForDuration.TotalSeconds,

                                FuelTankCenterQuantity = this.FuelTanks.FuelTankCenterQuantity,
                                FuelTankCenter2Quantity = this.FuelTanks.FuelTankCenter2Quantity,
                                FuelTankCenter3Quantity = this.FuelTanks.FuelTankCenter3Quantity,
                                FuelTankLeftMainQuantity = this.FuelTanks.FuelTankLeftMainQuantity,
                                FuelTankLeftAuxQuantity = this.FuelTanks.FuelTankLeftAuxQuantity,
                                FuelTankLeftTipQuantity = this.FuelTanks.FuelTankLeftTipQuantity,
                                FuelTankRightMainQuantity = this.FuelTanks.FuelTankRightMainQuantity,
                                FuelTankRightAuxQuantity = this.FuelTanks.FuelTankRightAuxQuantity,
                                FuelTankRightTipQuantity = this.FuelTanks.FuelTankRightTipQuantity,
                                FuelTankExternal1Quantity = this.FuelTanks.FuelTankExternal1Quantity,
                                FuelTankExternal2Quantity = this.FuelTanks.FuelTankExternal2Quantity
                            };

                            var result = this.openSkyServiceInstance.PositionReportAsync(positionReport).Result;

                            if (!result.IsError)
                            {
                                this.lastPositionReportUpload = DateTime.UtcNow;
                            }
                            else
                            {
                                Debug.WriteLine("Error uploading position report: " + result.Message + "\r\n" + result.ErrorDetails);
                            }
                        }
                        catch (AbandonedMutexException)
                        {
                            // Ignore
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error uploading position report: " + ex);
                        }
                        finally
                        {
                            try
                            {
                                this.positionReportUploadMutex.ReleaseMutex();
                            }
                            catch
                            {
                                // Ignore
                            }
                        }
                    })
            { Name = "SimConnect.Flight.UploadPositionReport" }.Start();
        }
    }
}