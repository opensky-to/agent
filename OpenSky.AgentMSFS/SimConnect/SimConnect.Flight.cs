// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Flight.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    using JetBrains.Annotations;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.Models.API;
    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.AgentMSFS.SimConnect.Helpers;
    using OpenSky.AgentMSFS.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client - flight tracking code.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight save mutex.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex flightSaveMutex = new Mutex(false);

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
        /// When did we auto-save the flight log the last time?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastFlightLogAutoSave = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pause info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string pauseInfo;

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
                        allConditionsMet = false;
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

                Debug.WriteLine("SimConnect flight changing...");
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
                    new Thread(this.DoGroundHandling) { Name = "OpenSky.SimConnect.GroundHandling" }.Start();

                    if (!value.Resume)
                    {
                        Debug.WriteLine("Preparing to track new flight");
                        this.flightLoadingTempStructs = null;
                        this.TrackingStatus = TrackingStatus.Preparing;

                        this.WarpInfo = "No [*]";
                        this.TrackingConditions[(int)Models.TrackingConditions.Fuel].Expected = $"{value.FuelGallons:F2}";
                        this.TrackingConditions[(int)Models.TrackingConditions.Payload].Expected = $"{value.PayloadPounds:F2}";
                        this.TrackingConditions[(int)Models.TrackingConditions.PlaneModel].Expected = value.PlaneName;

                        // Add airport markers to map
                        UpdateGUIDelegate addAirports = () =>
                        {
                            lock (this.trackingEventMarkers)
                            {
                                var originMarker = new TrackingEventMarker(value.OriginCoordinates, value.OriginICAO, OpenSkyColors.OpenSkyTeal, Colors.White);
                                this.trackingEventMarkers.Add(originMarker);
                                this.TrackingEventMarkerAdded?.Invoke(this, originMarker);
                                var alternateMarker = new TrackingEventMarker(value.AlternateCoordinates, value.AlternateICAO, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                                this.trackingEventMarkers.Add(alternateMarker);
                                this.TrackingEventMarkerAdded?.Invoke(this, alternateMarker);
                                var destinationMarker = new TrackingEventMarker(value.DestinationCoordinates, value.DestinationICAO, OpenSkyColors.OpenSkyTeal, Colors.White);
                                this.trackingEventMarkers.Add(destinationMarker);
                                this.TrackingEventMarkerAdded?.Invoke(this, destinationMarker);
                            }
                        };
                        Application.Current.Dispatcher.BeginInvoke(addAirports);
                    }
                    else
                    {
                        Debug.WriteLine("Preparing to track resumed flight");
                        this.TrackingStatus = TrackingStatus.Resuming;
                        this.LoadFlight();
                    }
                }
                else
                {
                    Debug.WriteLine("Flight set to NULL, stopping tracking");
                    this.flightLoadingTempStructs = null;
                    this.StopTracking(false);
                    this.lastFlightLogAutoSave = DateTime.MinValue;
                    this.simbriefOfpLoaded = false;

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
        /// Gets the pause info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PauseInfo
        {
            get => this.pauseInfo;

            private set
            {
                if (Equals(this.pauseInfo, value))
                {
                    return;
                }

                this.pauseInfo = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the tracking conditions.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<int, TrackingCondition> TrackingConditions { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the duration of the tracking session.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string TrackingDuration
        {
            get => this.trackingDuration;

            private set
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

            private set
            {
                if (Equals(this.trackingStatus, value))
                {
                    return;
                }

                this.trackingStatus = value;
                this.OnPropertyChanged();
                this.TrackingStatusChanged?.Invoke(this, value);
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

            private set
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
        /// -------------------------------------------------------------------------------------------------
        public void StartTracking()
        {
            Debug.WriteLine("SimConnect asked to start/resume tracking...");
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
                this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, OpenSkyColors.OpenSkyTealLight, "Flight tracking started");
                this.Speech.SpeakAsync("Tracking started. But ground handling is still in progress. You can set up your plane but don't turn on the engines or start pushing back.");
            }
            else
            {
                if (this.TrackingStatus != TrackingStatus.Tracking)
                {
                    if (this.TrackingStatus == TrackingStatus.Preparing)
                    {
                        Debug.WriteLine("Flight tracking starting...");
                        this.Speech.SpeakAsync("Flight tracking started.");
                        this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, OpenSkyColors.OpenSkyTealLight, "Flight tracking started");
                    }

                    if (this.TrackingStatus == TrackingStatus.Resuming)
                    {
                        Debug.WriteLine("Flight tracking resuming...");
                        this.Speech.SpeakAsync("Flight tracking resumed");
                        this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, OpenSkyColors.OpenSkyTealLight, "Flight tracking resumed");
                    }

                    this.TrackingStatus = TrackingStatus.Tracking;
                    this.trackingStarted = DateTime.UtcNow;
                }
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
            Debug.WriteLine($"SimConnect asked to stop tracking...resume: {resumeLater}");
            if (this.Flight != null)
            {
                this.Flight.Resume = resumeLater;
                this.FlightChanged?.Invoke(this, this.Flight);
            }

            this.TrackingStatus = this.Flight != null ? (this.Flight.Resume ? TrackingStatus.Resuming : TrackingStatus.Preparing) : TrackingStatus.NotTracking;

            // Was the flight set to null? If so, also reset the tracking conditions
            if (this.Flight == null)
            {
                Debug.WriteLine("Flight is NULL, resetting tracking conditions");
                foreach (TrackingConditions condition in Enum.GetValues(typeof(TrackingConditions)))
                {
                    this.TrackingConditions[(int)condition].Reset();
                }

                this.TrackingConditions[(int)Models.TrackingConditions.RealismSettings].Expected = "No slew, No unlimited fuel,\r\nCrash detection, SimRate=1";
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

                this.DeleteSaveFile();

                // todo delete any cloud saves?

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
                            var originMarker = new TrackingEventMarker(this.Flight.OriginCoordinates, this.Flight.OriginICAO, OpenSkyColors.OpenSkyTeal, Colors.White);
                            this.trackingEventMarkers.Add(originMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, originMarker);
                            var alternateMarker = new TrackingEventMarker(this.Flight.AlternateCoordinates, this.Flight.AlternateICAO, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                            this.trackingEventMarkers.Add(alternateMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, alternateMarker);
                            var destinationMarker = new TrackingEventMarker(this.Flight.DestinationCoordinates, this.Flight.DestinationICAO, OpenSkyColors.OpenSkyTeal, Colors.White);
                            this.trackingEventMarkers.Add(destinationMarker);
                            this.TrackingEventMarkerAdded?.Invoke(this, destinationMarker);
                        }
                    }
                };
                Application.Current.Dispatcher.BeginInvoke(resetMarkersAndEvents);
            }
            else
            {
                this.SaveFlight();
                this.Speech.SpeakAsync("Flight saved.");

                // todo make sure we update the last know location on the api

                // todo cloud save

                this.flightLoadingTempStructs = new FlightLoadingTempStructs
                {
                    FuelTanks = this.FuelTanks,
                    PayloadStations = this.PayloadStations,
                    SlewPlaneIntoPosition = Structs.SlewPlaneIntoPosition.FromPrimaryTracking(this.PrimaryTracking)
                };
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
                var saveFileName = $"{flightSaveDirectory}\\opensky-flight-{this.Flight.FlightID}.save";

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
            this.SaveFlight();

            // todo submit final save file

            // Sleep 10 seconds for now to simulate backend upload and processing
            Thread.Sleep(10 * 1000);

            this.Flight = null;
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
                Debug.WriteLine($"Loading flight {this.Flight?.FlightID}");
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
                var saveFileName = $"{flightSaveDirectory}\\opensky-flight-{this.Flight.FlightID}.save";

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
        /// Saves the current flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SaveFlight()
        {
            try
            {
                if (this.Flight == null)
                {
                    throw new Exception("No flight loaded that could be saved.");
                }

                if (!this.flightSaveMutex.WaitOne(30 * 1000))
                {
                    throw new Exception("Timeout waiting for save flight mutex.");
                }

                this.RefreshStructNow(Requests.FuelTanks);
                this.RefreshStructNow(Requests.PayloadStations);
                Thread.Sleep(500);

                Debug.WriteLine($"Saving flight {this.Flight?.FlightID}");
                var flightSaveDirectory = "%localappdata%\\OpenSky\\Flights\\";
                flightSaveDirectory = Environment.ExpandEnvironmentVariables(flightSaveDirectory);
                if (!Directory.Exists(flightSaveDirectory))
                {
                    Directory.CreateDirectory(flightSaveDirectory);
                }

                var saveFile = this.GenerateSaveFile();

                // todo gzip the xml string
                if (saveFile != null && this.Flight != null)
                {
                    var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes($"{saveFile}"));
                    using (var fileStream = File.Create($"{flightSaveDirectory}\\opensky-flight-{this.Flight.FlightID}.save"))
                    {
                        using (var gzip = new GZipStream(fileStream, CompressionMode.Compress))
                        {
                            xmlStream.CopyTo(gzip);
                        }
                    }

                    // todo remove this after testing
                    File.WriteAllText($"{flightSaveDirectory}\\opensky-flight-{this.Flight.FlightID}.xml", $"{saveFile}");

                    this.lastFlightLogAutoSave = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving flight: " + ex);
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
            if (this.TrackingStatus == TrackingStatus.GroundOperations || this.TrackingStatus == TrackingStatus.Tracking)
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
                    this.WarpInfo = this.timeSavedBecauseOfSimRate.TotalSeconds >= 1 ? $"Yes, saved {this.timeSavedBecauseOfSimRate:hh\\:mm\\:ss} [*]" : "No [*]";
                    this.AddTrackingEvent(ppt.New, this.SecondaryTracking, Colors.DarkViolet, $"Simrate changed: {ppt.New.SimulationRate}");

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
                }

                // todo Check if user has paused the sim (but didn't use the pause button we provide) ... check if lat/lon isn't changing while in flight

                // Auto-save flight log?
                if ((DateTime.UtcNow - this.lastFlightLogAutoSave).TotalMinutes > 2)
                {
                    Debug.WriteLine("Auto-saving flight");
                    this.SaveFlight();
                }
            }
        }
    }
}