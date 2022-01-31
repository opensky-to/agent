// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Process.FlightPhases.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Media;
    using System.Reflection;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client - data processing code.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The currently active flight phase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FlightPhase flightPhase = FlightPhase.Unknown;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The next flight step instruction for the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string nextFlightStep;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to set next step flashing (last value).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool nextStepFlashing;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if taxi in started.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool taxiInStarted;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if the plane turned during the taxi in (if not, was the engine turned off on the runway?).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool taxiInTurned;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Was the plane airborne?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool wasAirborne;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when next step flashing changed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<bool> NextStepFlashingChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current flight phase.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FlightPhase FlightPhase
        {
            get => this.flightPhase;

            private set
            {
                if (Equals(this.flightPhase, value))
                {
                    return;
                }

                this.flightPhase = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the next flight step instruction for the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string NextFlightStep
        {
            get => this.nextFlightStep;

            private set
            {
                if (Equals(this.nextFlightStep, value))
                {
                    return;
                }

                this.nextFlightStep = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the next step should be flashing.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool NextStepFlashing
        {
            get => this.nextStepFlashing;

            private set
            {
                if (Equals(this.nextStepFlashing, value))
                {
                    return;
                }

                this.nextStepFlashing = value;
                this.OnPropertyChanged();
                this.NextStepFlashingChanged?.Invoke(this, value);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the plane was airborne.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool WasAirborne
        {
            get => this.wasAirborne;

            private set
            {
                if (Equals(this.wasAirborne, value))
                {
                    return;
                }

                this.wasAirborne = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Transition between flight phases.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void TransitionFlightPhase()
        {
            if (this.TrackingStatus != TrackingStatus.GroundOperations && this.TrackingStatus != TrackingStatus.Tracking && this.TrackingStatus != TrackingStatus.Resuming)
            {
                this.WasAirborne = false;
                this.FlightPhase = FlightPhase.UnTracked;
                return;
            }

            var unknownFlightPhase = true;
            var newNextStepFlashing = false;
            var currentPosition = this.PrimaryTracking.GeoCoordinate;
            var distanceToDepartureAirport = new GeoCoordinate(this.Flight?.Origin.Latitude ?? 0, this.Flight?.Origin.Longitude ?? 0).GetDistanceTo(currentPosition) / 1000 * 0.539957;
            var distanceToDestinationAirport = new GeoCoordinate(this.Flight?.Destination.Latitude ?? 0, this.Flight?.Destination.Longitude ?? 0).GetDistanceTo(currentPosition) / 1000 * 0.539957;
            var distanceToAlternateAirport = new GeoCoordinate(this.Flight?.Alternate.Latitude ?? 0, this.Flight?.Alternate.Longitude ?? 0).GetDistanceTo(currentPosition) / 1000 * 0.539957;

            if (!this.WasAirborne && this.PrimaryTracking.RadioHeight >= 50)
            {
                this.WasAirborne = true;
                this.AddTrackingEvent(this.PrimaryTracking, this.SecondaryTracking, FlightTrackingEventType.Airborne, OpenSkyColors.OpenSkyTeal, "Airborne");
            }

            // InMenu
            if (this.PrimaryTracking.OnGround && Math.Abs(Math.Round(this.PrimaryTracking.Latitude, 1)) < 0.5 && Math.Abs(Math.Round(this.PrimaryTracking.Longitude, 1)) < 0.5)
            {
                this.FlightPhase = FlightPhase.Briefing;
                this.NextFlightStep = string.Empty;
                unknownFlightPhase = false;
                if (this.TrackingStatus is TrackingStatus.GroundOperations or TrackingStatus.Tracking)
                {
                    Debug.WriteLine("Tracking aborted, sim returned to main menu.");
                    var assembly = Assembly.GetExecutingAssembly();
                    var player = new SoundPlayer(assembly.GetManifestResourceStream("OpenSky.AgentMSFS.Resources.OSnegative.wav"));
                    player.Play();
                    SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.AbortedSimMainMenu);
                    this.StopTracking(false);
                    this.fsConnect.SetText("Tracking aborted, sim returned to main menu.", 5);
                }
            }

            // PreFlight
            if (!this.WasAirborne && this.PrimaryTracking.OnGround && !this.SecondaryTracking.EngineRunning && this.SecondaryTracking.Pushback == Pushback.NoPushback)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.PreFlight;
                    this.NextFlightStep = this.TrackingStatus == TrackingStatus.GroundOperations ? "Next step: Wait for ground handling to complete" : $"Next step: Turn on the engine{(this.PlaneIdentity.EngineCount > 1 ? "s" : string.Empty)}";
                    newNextStepFlashing = this.TrackingStatus != TrackingStatus.GroundOperations;
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: PreFlight, last was {this.FlightPhase}");
                }
            }

            // Pushback
            if (this.SecondaryTracking.Pushback != Pushback.NoPushback)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.PushBack;
                    this.NextFlightStep = "Next step: Complete pushback";
                    newNextStepFlashing = true;
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Pushback, last was {this.FlightPhase}");
                }
            }

            // TaxiOut
            if (!this.WasAirborne && this.SecondaryTracking.EngineRunning && this.PrimaryTracking.GroundSpeed < 40 && this.PrimaryTracking.OnGround && this.SecondaryTracking.Pushback == Pushback.NoPushback)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.TaxiOut;
                    this.NextFlightStep = "Next step: Taxi to the runway and take off";
                    newNextStepFlashing = true;
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: TaxiOut, last was {this.FlightPhase}");
                }
            }

            // Takeoff
            var departureRadioHeight = this.PlaneIdentity.EngineType is EngineType.Jet or EngineType.Turboprop ? 1000 : 100;
            if (!this.WasAirborne && this.PrimaryTracking.GroundSpeed > 40 && this.PrimaryTracking.RadioHeight <= departureRadioHeight)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.Takeoff;
                    this.NextFlightStep = "Next step: Navigate to destination";
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Takeoff, last was {this.FlightPhase}");
                }
            }

            // Departure
            if (distanceToDepartureAirport < 10 && this.VerticalProfile == VerticalProfile.Climbing && !this.PrimaryTracking.OnGround)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.Departure;
                    this.NextFlightStep = "Next step: Navigate to destination";
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Departure, last was {this.FlightPhase}");
                }
            }

            // Climb
            var approachDistance = this.PlaneIdentity.EngineType is EngineType.Jet or EngineType.Turboprop ? 40 : 10;
            if (distanceToDepartureAirport >= approachDistance && this.VerticalProfile == VerticalProfile.Climbing && !this.PrimaryTracking.OnGround)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.Climb;
                    this.NextFlightStep = "Next step: Navigate to destination";
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Climb, last was {this.FlightPhase}");
                }
            }

            // Cruise
            if (distanceToDestinationAirport >= approachDistance && distanceToAlternateAirport >= approachDistance && this.VerticalProfile == VerticalProfile.Level && !this.PrimaryTracking.OnGround)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.Cruise;
                    this.NextFlightStep = "Next step: Navigate to destination";
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Cruise, last was {this.FlightPhase}");
                }
            }

            // Descent
            if (distanceToDestinationAirport >= approachDistance && distanceToAlternateAirport >= approachDistance && this.VerticalProfile == VerticalProfile.Descending && !this.PrimaryTracking.OnGround)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.Descent;
                    this.NextFlightStep = "Next step: Navigate to destination";
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Descent, last was {this.FlightPhase}");
                }
            }

            // Approach
            if ((distanceToDestinationAirport < approachDistance || distanceToAlternateAirport < approachDistance) && !this.PrimaryTracking.OnGround && this.PrimaryTracking.RadioHeight > 500)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.Approach;
                    this.NextFlightStep = "Next step: Approach the airport";
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Approach, last was {this.FlightPhase}");
                }
            }

            // Landing
            if (this.WasAirborne && this.PrimaryTracking.RadioHeight <= 500 && this.PrimaryTracking.GroundSpeed >= 40 && this.SecondaryTracking.EngineRunning)
            {
                // Landing is allowed to override descent in case of return to origin airport or I guess landing anywhere but the destination/alternate
                if (unknownFlightPhase || this.FlightPhase == FlightPhase.Descent)
                {
                    this.FlightPhase = FlightPhase.Landing;
                    this.NextFlightStep = "Next step: Butter that landing, which means you shouldn't be reading this";
                    newNextStepFlashing = true;
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Landing, last was {this.FlightPhase}");
                }
            }

            // GoAround todo

            // TaxiIn
            if (this.WasAirborne && this.SecondaryTracking.EngineRunning && this.PrimaryTracking.GroundSpeed < 40 && this.PrimaryTracking.OnGround)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.TaxiIn;
                    this.NextFlightStep = "Next step: Taxi to parking and turn off the engine";
                    newNextStepFlashing = true;
                    if (!this.taxiInStarted)
                    {
                        this.taxiInStarted = true;
                        this.taxiInTurned = false;
                    }

                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: TaxiIn, last was {this.FlightPhase}");
                }
            }

            // PostFlight
            if (this.WasAirborne && this.PrimaryTracking.OnGround && !this.SecondaryTracking.EngineRunning)
            {
                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.PostFlight;
                    this.NextFlightStep = "All done, saving report and submitting to OpenSky...";
                    newNextStepFlashing = true;
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: PostFlight, last was {this.FlightPhase}");
                }
            }

            if (this.PrimaryTracking.CrashSequence != CrashSequence.Off)
            {
                // todo save pre-crash flight phase to submit as part of crash report? implement this in the systems section?

                if (unknownFlightPhase)
                {
                    this.FlightPhase = FlightPhase.Crashed;
                    this.NextFlightStep = "Evacuate! Saving crash report and submitting to OpenSky...";
                    newNextStepFlashing = true;
                    unknownFlightPhase = false;
                }
                else
                {
                    Debug.WriteLine($"Potential flight phase conflict detected: Crashed, last was {this.FlightPhase}");
                }
            }

            if (unknownFlightPhase)
            {
                this.FlightPhase = FlightPhase.Unknown;
                Debug.WriteLine("Unable to determine current flight phase!");
            }

            this.NextStepFlashing = newNextStepFlashing;
        }
    }
}