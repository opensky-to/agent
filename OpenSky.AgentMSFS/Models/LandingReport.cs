// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingReport.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Models
{
    using System;
    using System.Globalization;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Landing report.
    /// </summary>
    /// <remarks>
    /// sushi.at, 26/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class LandingReport
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LandingReport"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="timestamp">
        /// The timestamp of the landing.
        /// </param>
        /// <param name="location">
        /// The location.
        /// </param>
        /// <param name="landingRate">
        /// The landing rate.
        /// </param>
        /// <param name="gForce">
        /// The G-force.
        /// </param>
        /// <param name="forwardSpeed">
        /// The forward speed.
        /// </param>
        /// <param name="sidewardsSpeed">
        /// The sidewards speed.
        /// </param>
        /// <param name="headWind">
        /// The head wind.
        /// </param>
        /// <param name="crossWind">
        /// The cross wind.
        /// </param>
        /// <param name="bankAngle">
        /// The bank angle.
        /// </param>
        /// <param name="groundSpeed">
        /// The ground speed.
        /// </param>
        /// <param name="airspeed">
        /// The airspeed.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public LandingReport(DateTime timestamp, Location location, double landingRate, double gForce, double forwardSpeed, double sidewardsSpeed, double headWind, double crossWind, double bankAngle, double groundSpeed, double airspeed)
        {
            this.Timestamp = timestamp;
            this.Location = location;
            this.LandingRate = landingRate;
            this.GForce = gForce;
            this.SideSlipAngle = Math.Atan(sidewardsSpeed / forwardSpeed) * 180.0 / Math.PI;
            this.HeadWind = headWind;
            this.CrossWind = crossWind;
            this.BankAngle = bankAngle;
            this.GroundSpeed = groundSpeed;
            this.Airspeed = airspeed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LandingReport"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="landingFromSave">
        /// The landing report to restore from a save file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public LandingReport(XElement landingFromSave)
        {
            this.Timestamp = DateTime.ParseExact(landingFromSave.Attribute("Timestamp")?.Value, "O", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var lat = double.Parse(landingFromSave.Attribute("Lat")?.Value ?? "missing");
            var lon = double.Parse(landingFromSave.Attribute("Lon")?.Value ?? "missing");
            var alt = double.Parse(landingFromSave.Attribute("Alt")?.Value ?? "missing");
            this.Location = new Location(lat, lon, alt);
            this.LandingRate = double.Parse(landingFromSave.Attribute("LandingRate")?.Value ?? "missing");
            this.GForce = double.Parse(landingFromSave.Attribute("GForce")?.Value ?? "missing");
            this.SideSlipAngle = double.Parse(landingFromSave.Attribute("SideSlipAngle")?.Value ?? "missing");
            this.HeadWind = double.Parse(landingFromSave.Attribute("HeadWind")?.Value ?? "missing");
            this.CrossWind = double.Parse(landingFromSave.Attribute("CrossWind")?.Value ?? "missing");
            this.BankAngle = double.Parse(landingFromSave.Attribute("BankAngle")?.Value ?? "missing");
            this.GroundSpeed = double.Parse(landingFromSave.Attribute("GroundSpeed")?.Value ?? "missing");
            this.Airspeed = double.Parse(landingFromSave.Attribute("Airspeed")?.Value ?? "missing");
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airspeed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Airspeed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the bank angle.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double BankAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the cross wind.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double CrossWind { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the G-force.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GForce { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ground speed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundSpeed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the head wind.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double HeadWind { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the landing rate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double LandingRate { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Location Location { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the side slip angle.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double SideSlipAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the timestamp of the landing.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Timestamp { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the wind angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindAngle => Math.Atan2(this.CrossWind, this.HeadWind) * 180.0 / Math.PI;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the wind in knots (direction in WindAngle!).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindKnots => Math.Sqrt(Math.Pow(this.CrossWind, 2) + Math.Pow(this.HeadWind, 2));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets landing report for saving in a file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <returns>
        /// The landing report for saving.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public XElement GetLandingReportForSave()
        {
            var landing = new XElement("Touchdown");
            landing.SetAttributeValue("Timestamp", $"{this.Timestamp:O}");
            landing.SetAttributeValue("Lat", $"{this.Location.Latitude}");
            landing.SetAttributeValue("Lon", $"{this.Location.Longitude}");
            landing.SetAttributeValue("Alt", $"{this.Location.Altitude:F0}");
            landing.SetAttributeValue("LandingRate", $"{this.LandingRate:F0}");
            landing.SetAttributeValue("GForce", $"{this.GForce:F2}");
            landing.SetAttributeValue("SideSlipAngle", $"{this.SideSlipAngle:F2}");
            landing.SetAttributeValue("HeadWind", $"{this.HeadWind:F2}");
            landing.SetAttributeValue("CrossWind", $"{this.CrossWind:F2}");
            landing.SetAttributeValue("BankAngle", $"{this.CrossWind:F2}");
            landing.SetAttributeValue("GroundSpeed", $"{this.GroundSpeed:F0}");
            landing.SetAttributeValue("Airspeed", $"{this.Airspeed:F0}");
            return landing;
        }
    }
}