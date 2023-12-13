// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingReportViewModel.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using JetBrains.Annotations;

    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Landing report view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 30/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Agent.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class LandingReportViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The landing reports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<TouchDown> landingReports = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight number header.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string flightNumberHeader = "Flight\r\nLanding Report";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Information describing the landing grade.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string landingGradeDescription;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LandingReportViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public LandingReportViewModel()
        {
            this.FlightNumberHeader = $"Flight #{this.Simulator.Flight?.FullFlightNumber}\r\nLanding Report";

            // Fetch the initial already existing landing report(s)
            if (this.Simulator.FinalTouchDownIndex >= 0)
            {
                for (var i = this.Simulator.FinalTouchDownIndex; i < this.Simulator.LandingReports.Count; i++)
                {
                    this.landingReports.Add(this.Simulator.LandingReports[i]);
                }
            }

            // Subscribe to changes
            this.Simulator.LandingReports.CollectionChanged += this.LandingReportsCollectionChanged;

            // Create dismiss command and notification timeout thread
            this.DismissLandingReportCommand = new Command(this.DismissLandingReport);
            new Thread(this.NotificationTimeout) { Name = "OpenSky.LandingReportNotificationTimeout" }.Start();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler CloseWindow;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the airspeed (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Airspeed => this.landingReports.Count > 0 ? this.landingReports[0].Airspeed : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the bounces.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Bounces => this.landingReports.Count > 1 ? this.landingReports.Count - 1 : 0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cross wind (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string CrossWind => this.landingReports.Count > 0 ? this.landingReports[0].CrossWind.ToString("Left, 0.0;Right, 0.0;None, 0") : "0";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the dismiss landing report command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command DismissLandingReportCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight number header.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FlightNumberHeader
        {
            get => this.flightNumberHeader;

            private set
            {
                if (Equals(this.flightNumberHeader, value))
                {
                    return;
                }

                this.flightNumberHeader = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ground speed (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundSpeed => this.landingReports.Count > 0 ? this.landingReports[0].GroundSpeed : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the head-wind (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string HeadWind => this.landingReports.Count > 0 ? this.landingReports[0].HeadWind.ToString("Tail, 0.0;Head, 0.0;None, 0") : "0";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing grade (also sets/updates LandingGradeDescription).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandingGrade
        {
            get
            {
                // ----------------------------------------------------
                // Landing rate (absolute)
                // ----------------------------------------------------
                // A+ Butter <=80 (Other)
                // A+ Perfect <=250 >=80 (WBA)
                // A+ Perfect <=160 >=80 (NBA)
                // A+ Perfect <=160 >=50 (JET engine)
                // A- Too soft <=80 (NBA, WBA)
                // A- Too soft <=50 (JET engine)
                // B  OK >350 (WBA)
                // B  OK >250 (NBA)
                // B  OK >220 (JET engine)
                // B  OK >200 (Other)
                // D  Hard >600 <=840 (Inspection)
                // E  Severe Hard >840 <=1000 (Inspection, possible repair)
                // F  Crash >1000 (Repair)

                // ----------------------------------------------------
                // G-Force
                // ----------------------------------------------------
                // A  Good >=0.75 <=1.25
                // B  OK >1.25 <=1.5
                // B- Low-G <0.75
                // B- Uncomfortable >1.5 <=2.1
                // C  Rough >2.1 <=2.6
                // D  Hard >2.6 <=2.86
                // E  Severed Hard >2.86 (Inspection, possible repair)
                // F  Crash >3 (Repair)

                // ----------------------------------------------------
                // Bounces
                // ----------------------------------------------------
                // C  Bouncy >1
                // D  Porpoising >2

                // ----------------------------------------------------
                // Wind
                // ----------------------------------------------------
                // E  Dangerous Cross >40 (Airliners) >20 (GA)
                // E  Dangerous Tail >15

                // ----------------------------------------------------
                // Sideslip
                // ----------------------------------------------------
                // E  Dangerous <-16 or >16

                // ----------------------------------------------------
                // Bank angle
                // ----------------------------------------------------
                // E  Dangerous <-5 or >5

                if (this.landingReports.Count == 0)
                {
                    this.LandingGradeDescription = "Unknown";
                    return "?";
                }

                var landingRateAbs = Math.Abs(this.MaxLandingRate);
                var crossWindAbs = Math.Abs(this.landingReports[0].CrossWind);
                var grade = "?";
                var desc = "Unknown";

                if (landingRateAbs > 1000 || this.MaxGForce > 3)
                {
                    grade = "F";
                    desc = "Crash landing";
                }

                if (grade == "?" && (this.MaxBankAngle is < -5.0 or > 5.0))
                {
                    grade = "E";
                    desc = "Dangerous bank angle";
                }

                if (grade == "?" && (this.MaxSideSlipAngle is < -65.0 or > 16.0))
                {
                    grade = "E";
                    desc = "Dangerous sideslip angle";
                }

                if (grade == "?" && (landingRateAbs is > 840 and <= 1000 || this.MaxGForce > 2.86))
                {
                    grade = "E";
                    desc = "Severe hard landing";
                }

                if (grade == "?" && crossWindAbs > 40)
                {
                    grade = "E";
                    desc = "Dangerous crosswind";
                }

                if (grade == "?" && crossWindAbs > 20 && this.Simulator.Flight?.Aircraft.Type.Category is AircraftTypeCategory.SEP or AircraftTypeCategory.MEP or AircraftTypeCategory.SET or AircraftTypeCategory.MET or AircraftTypeCategory.HEL)
                {
                    grade = "E";
                    desc = "Dangerous crosswind";
                }

                if (grade == "?" && this.landingReports[0].HeadWind > 15)
                {
                    grade = "E";
                    desc = "Dangerous tailwind";
                }

                if (grade == "A+" && (landingRateAbs is > 600 and <= 840 || this.MaxGForce is > 2.6 and <= 2.86))
                {
                    grade = "D";
                    desc = "Hard landing";
                }

                if (grade == "?" && this.Bounces > 2)
                {
                    grade = "D";
                    desc = "Porpoising landing";
                }

                if (grade == "?" && this.MaxGForce is > 2.1 and <= 2.6)
                {
                    grade = "C";
                    desc = "Rough landing";
                }

                if (grade == "?" && this.Bounces > 1)
                {
                    grade = "C";
                    desc = "Bouncy landing";
                }

                if (grade == "?" && this.MaxGForce is > 1.5 and <= 2.1)
                {
                    grade = "B-";
                    desc = "Uncomfortable landing";
                }

                if (grade == "?" && this.MaxGForce is > 1.25 and <= 1.5)
                {
                    grade = "B";
                    desc = "Good landing";
                }

                if (grade == "?" && this.MinGForce >= 0.75 && this.MaxGForce <= 1.25)
                {
                    grade = "A";
                    desc = "Great landing";

                    if (this.Simulator.Flight?.Aircraft.Type.Category is AircraftTypeCategory.WBA)
                    {
                        if (landingRateAbs > 350)
                        {
                            grade = "B";
                            desc = "OK landing";
                        }

                        if (landingRateAbs is <= 250 and >= 80)
                        {
                            grade = "A+";
                            desc = "Perfect landing";
                        }

                        if (landingRateAbs < 80)
                        {
                            grade = "A-";
                            desc = "Lading too soft";
                        }
                    }
                    else if (this.Simulator.Flight?.Aircraft.Type.Category is AircraftTypeCategory.NBA)
                    {
                        if (landingRateAbs > 250)
                        {
                            grade = "B";
                            desc = "OK landing";
                        }

                        if (landingRateAbs is <= 160 and >= 80)
                        {
                            grade = "A+";
                            desc = "Perfect landing";
                        }

                        if (landingRateAbs < 80)
                        {
                            grade = "A-";
                            desc = "Lading too soft";
                        }
                    }
                    else if (this.Simulator.AircraftIdentity.EngineType == EngineType.Jet)
                    {
                        if (landingRateAbs > 220)
                        {
                            grade = "B";
                            desc = "OK landing";
                        }

                        if (landingRateAbs is <= 160 and >= 50)
                        {
                            grade = "A+";
                            desc = "Perfect landing";
                        }

                        if (landingRateAbs < 50)
                        {
                            grade = "A-";
                            desc = "Lading too soft";
                        }
                    }
                    else
                    {
                        if (landingRateAbs > 200)
                        {
                            grade = "B";
                            desc = "OK landing";
                        }

                        if (landingRateAbs < 80)
                        {
                            grade = "A+";
                            desc = "Butter landing";
                        }
                    }
                }

                if (grade == "?" && this.MinGForce < 0.75)
                {
                    grade = "B-";
                    desc = "Low-G landing";
                }

                this.LandingGradeDescription = desc;
                return grade;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the information string describing the landing grade.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LandingGradeDescription
        {
            get => this.landingGradeDescription;

            private set
            {
                if (Equals(this.landingGradeDescription, value))
                {
                    return;
                }

                this.landingGradeDescription = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum bank angle.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxBankAngle
        {
            get
            {
                if (this.landingReports.Count > 0)
                {
                    var max = this.landingReports.Max(lr => lr.BankAngle);
                    var min = this.landingReports.Min(lr => lr.BankAngle);
                    return Math.Abs(min) > Math.Abs(max) ? min : max;
                }

                return 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum bank angle info string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string MaxBankAngleInfo => this.MaxBankAngle.ToString("Left, 0.0;Right, 0.0;None, 0");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum g-force.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxGForce => this.landingReports.Count > 0 ? this.landingReports.Max(lr => lr.GForce) : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum landing rate (Max landing rate is actually MIN (as landing rates are negative)).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxLandingRate => this.landingReports.Count > 0 ? this.landingReports.Min(lr => lr.LandingRate) : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum side-slip angle.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxSideSlipAngle
        {
            get
            {
                if (this.landingReports.Count > 0)
                {
                    var max = this.landingReports.Max(lr => lr.SideSlipAngle);
                    var min = this.landingReports.Min(lr => lr.SideSlipAngle);
                    return Math.Abs(min) > Math.Abs(max) ? min : max;
                }

                return 0;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum side-slip angle info text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string MaxSideSlipAngleInfo => this.MaxSideSlipAngle.ToString("Left, 0.0;Right, 0.0;None, 0");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the minimum g-force.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MinGForce => this.landingReports.Count > 0 ? this.landingReports.Min(lr => lr.GForce) : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulator instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Agent.Simulator.Simulator Simulator => Agent.Simulator.Simulator.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the wind angle (compound, only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindAngle => this.landingReports.Count > 0 ? this.landingReports[0].WindAngle : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the wind knots (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindKnots => this.landingReports.Count > 0 ? this.landingReports[0].WindKnots : 0.0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Dismiss landing report.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void DismissLandingReport()
        {
            this.CloseWindow?.Invoke(this, EventArgs.Empty);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The landing reports collection was changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LandingReportsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // New flight, unsubscribe from further changes
                Debug.WriteLine("Landing reports reset in SimConnect class, unsubscribing from events for notification window");
                this.Simulator.LandingReports.CollectionChanged -= this.LandingReportsCollectionChanged;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Debug.WriteLine("New landing report added, updating notification");

                foreach (var item in e.NewItems)
                {
                    if (item is TouchDown report)
                    {
                        this.landingReports.Add(report);
                    }
                }

                this.NotifyPropertyChanged(nameof(this.MaxLandingRate));
                this.NotifyPropertyChanged(nameof(this.MaxGForce));
                this.NotifyPropertyChanged(nameof(this.MinGForce));
                this.NotifyPropertyChanged(nameof(this.MaxSideSlipAngleInfo));
                this.NotifyPropertyChanged(nameof(this.Bounces));
                this.NotifyPropertyChanged(nameof(this.MaxBankAngleInfo));
                this.NotifyPropertyChanged(nameof(this.HeadWind));
                this.NotifyPropertyChanged(nameof(this.CrossWind));
                this.NotifyPropertyChanged(nameof(this.WindAngle));
                this.NotifyPropertyChanged(nameof(this.WindKnots));
                this.NotifyPropertyChanged(nameof(this.LandingGrade));
                this.NotifyPropertyChanged(nameof(this.Airspeed));
                this.NotifyPropertyChanged(nameof(this.GroundSpeed));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Notification timeout.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void NotificationTimeout()
        {
            SleepScheduler.SleepFor(TimeSpan.FromSeconds(120));
            this.CloseWindow?.Invoke(this, EventArgs.Empty);
        }
    }
}