// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTrailLocation.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Models
{
    using System;
    using System.Globalization;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.SimConnect.Enums;
    using OpenSky.AgentMSFS.SimConnect.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// An aircraft trail location.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/03/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.Maps.MapControl.WPF.Location"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTrailLocation : Location
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTrailLocation"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/03/2021.
        /// </remarks>
        /// <param name="timestamp">
        /// The timestamp of the location.
        /// </param>
        /// <param name="primary">
        /// The primary tracking struct.
        /// </param>
        /// <param name="secondary">
        /// The secondary tracking struct.
        /// </param>
        /// <param name="fuelOnBoard">
        /// The fuel on board in gallons.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTrailLocation(DateTime timestamp, PrimaryTracking primary, SecondaryTracking secondary, double fuelOnBoard) : base(primary.MapLocation.Latitude, primary.MapLocation.Longitude, primary.MapLocation.Altitude)
        {
            this.Timestamp = timestamp;
            this.Airspeed = primary.AirspeedTrue;
            this.Groundspeed = primary.GroundSpeed;
            this.OnGround = primary.OnGround;
            this.RadioAlt = primary.RadioHeight;
            this.Heading = primary.Heading;
            this.FuelOnBoard = fuelOnBoard;
            this.SimulationRate = primary.SimulationRate;
            this.TimeOfDay = secondary.TimeOfDay;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTrailLocation"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="locationFromSave">
        /// The location from a save file to restore.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTrailLocation(XElement locationFromSave)
        {
            this.Timestamp = DateTime.ParseExact(locationFromSave.Attribute("Timestamp")?.Value, "O", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            this.Latitude = double.Parse(locationFromSave.Attribute("Lat")?.Value ?? "missing");
            this.Longitude = double.Parse(locationFromSave.Attribute("Lon")?.Value ?? "missing");
            this.Altitude = double.Parse(locationFromSave.Attribute("Alt")?.Value ?? "missing");
            this.Airspeed = double.Parse(locationFromSave.Attribute("Airspeed")?.Value ?? "missing");
            this.Groundspeed = double.Parse(locationFromSave.Attribute("Groundspeed")?.Value ?? "missing");
            this.OnGround = bool.Parse(locationFromSave.Attribute("OnGround")?.Value ?? "missing");
            this.RadioAlt = double.Parse(locationFromSave.Attribute("RadioAlt")?.Value ?? "missing");
            this.Heading = double.Parse(locationFromSave.Attribute("Heading")?.Value ?? "missing");
            this.FuelOnBoard = double.Parse(locationFromSave.Attribute("FuelOnBoard")?.Value ?? "missing");
            this.SimulationRate = double.Parse(locationFromSave.Attribute("SimulationRate")?.Value ?? "missing");
            Enum.TryParse(locationFromSave.Attribute("TimeOfDay")?.Value, out TimeOfDay timeOfDay);
            this.TimeOfDay = timeOfDay;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the true airspeed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Airspeed { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel on board in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelOnBoard { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ground speed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Groundspeed { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Heading { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the plane is on the ground.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the radio altitude (AGL).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double RadioAlt { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulation rate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double SimulationRate { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the time of day.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public TimeOfDay TimeOfDay { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the timestamp of the location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Timestamp { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets location XElement for save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <returns>
        /// The location for save.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public XElement GetLocationForSave()
        {
            var locationElement = new XElement("Location");
            locationElement.SetAttributeValue("Timestamp", $"{this.Timestamp:O}");
            locationElement.SetAttributeValue("Lat", $"{this.Latitude}");
            locationElement.SetAttributeValue("Lon", $"{this.Longitude}");
            locationElement.SetAttributeValue("Alt", $"{this.Altitude:F0}");
            locationElement.SetAttributeValue("Airspeed", $"{this.Airspeed:F0}");
            locationElement.SetAttributeValue("Groundspeed", $"{this.Groundspeed:F0}");
            locationElement.SetAttributeValue("OnGround", $"{this.OnGround}");
            locationElement.SetAttributeValue("RadioAlt", $"{this.RadioAlt:F0}");
            locationElement.SetAttributeValue("Heading", $"{this.Heading:F0}");
            locationElement.SetAttributeValue("FuelOnBoard", $"{this.FuelOnBoard:F2}");
            locationElement.SetAttributeValue("SimulationRate", $"{this.SimulationRate:F1}");
            locationElement.SetAttributeValue("TimeOfDay", $"{this.TimeOfDay}");
            return locationElement;
        }
    }
}