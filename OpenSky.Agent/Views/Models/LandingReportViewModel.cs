// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingReportViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using JetBrains.Annotations;

    using OpenSky.Agent.Simulator.Tools;
    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.FlightLogXML;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Landing report view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 30/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
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
            foreach (var item in this.Simulator.LandingReports)
            {
                this.landingReports.Add(item);
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
        public string CrossWind => this.landingReports.Count > 0 ? this.landingReports[0].CrossWind.ToString("Left, 0.00;Right, 0.00;None, 0") : "0";

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
        /// Gets the head wind (only from first touchdown).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string HeadWind => this.landingReports.Count > 0 ? this.landingReports[0].HeadWind.ToString("Tail, 0.00;Head, 0.00;None, 0") : "0";

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
                // A+ Butter <=60 (PISTON,TURBO)
                // A+ Perfect >60 <=130 (JET)
                // A- Too soft <=60 (JET)
                // A  Good >60 <=180
                // B  OK >180 <=240
                // C  Hard >240 <=600
                // D  Rough >600 <=1000 (Inspection)
                // F  Crash >1000 (Repair)

                // ----------------------------------------------------
                // G-Force
                // ----------------------------------------------------
                // C  Hard >2.1

                // ----------------------------------------------------
                // Bounces
                // ----------------------------------------------------
                // C  Bouncy >1
                // D  Porpoising >2

                // ----------------------------------------------------
                // Wind todo add wind ratings
                // ----------------------------------------------------
                // E  Dangerous Cross >40 (JET) >20 (PISTON,TURBO)
                // E  Dangerous Tail >15 (JET) >5 (PISTON,TURBO)

                // ----------------------------------------------------
                // Sideslip
                // ----------------------------------------------------
                // E  Dangerous <-15 or >15

                // ----------------------------------------------------
                // Bank angle
                // ----------------------------------------------------
                // E  Dangerous <-5 or >5

                var landingRateAbs = Math.Abs(this.MaxLandingRate);
                var grade = "A+";
                var desc = "Butter landing";

                if (landingRateAbs > 1000)
                {
                    grade = "F";
                    desc = "Crash landing";
                }

                if (grade == "A+" && (this.MaxBankAngle is < -5.0 or > 5.0))
                {
                    grade = "E";
                    desc = "Dangerous bank angle";
                }

                if (grade == "A+" && (this.MaxSideSlipAngle is < -15.0 or > 15.0))
                {
                    grade = "E";
                    desc = "Dangerous sideslip angle";
                }

                if (grade == "A+" && landingRateAbs is > 600 and <= 1000)
                {
                    grade = "D";
                    desc = "Rough landing";
                }

                if (grade == "A+" && this.Bounces > 2)
                {
                    grade = "D";
                    desc = "Porpoising landing";
                }

                if (grade == "A+" && (this.MaxGForce is < -2.1 or > 2.1))
                {
                    grade = "C";
                    desc = "Hard landing";
                }

                if (grade == "A+" && landingRateAbs is > 240 and <= 600)
                {
                    grade = "C";
                    desc = "Hard landing";
                }

                if (grade == "A+" && this.Bounces > 1)
                {
                    grade = "C";
                    desc = "Bouncy landing";
                }

                if (grade == "A+" && landingRateAbs is > 180 and <= 240)
                {
                    grade = "B";
                    desc = "OK landing";
                }

                if (grade == "A+" && landingRateAbs is > 60 and <= 180)
                {
                    grade = "A";
                    desc = "Good landing";

                    if (landingRateAbs <= 130 && this.Simulator.AircraftIdentity.EngineType == EngineType.Jet)
                    {
                        grade = "A+";
                        desc = "Perfect landing";
                    }
                }

                if (grade == "A+" && this.Simulator.AircraftIdentity.EngineType == EngineType.Jet)
                {
                    grade = "A-";
                    desc = "Lading too soft";
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
        public string MaxBankAngleInfo => this.MaxBankAngle.ToString("Left, 0.00;Right, 0.00;None, 0");

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
        /// Gets the maximum side slip angle.
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
        /// Gets the maximum side slip angle info text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string MaxSideSlipAngleInfo => this.MaxSideSlipAngle.ToString("Left, 0.00;Right, 0.00;None, 0");

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