// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTrailLocation.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Models
{
    using System;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.SimConnect.Structs;
    using OpenSky.FlightLogXML;

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
        /// The position report we are wrapping around.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly PositionReport position = new();

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
            this.position.Timestamp = timestamp;
            this.position.Latitude = primary.Latitude;
            this.position.Longitude = primary.Longitude;
            this.position.Altitude = (int)primary.Altitude;
            this.position.Airspeed = primary.AirspeedTrue;
            this.position.Groundspeed = primary.GroundSpeed;
            this.position.OnGround = primary.OnGround;
            this.position.RadioAlt = primary.RadioHeight;
            this.position.Heading = primary.Heading;
            this.position.FuelOnBoard = fuelOnBoard;
            this.position.SimulationRate = primary.SimulationRate;
            this.position.TimeOfDay = (TimeOfDay)secondary.TimeOfDay;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTrailLocation"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="position">
        /// The position report we are wrapping around, restored from a flight log xml file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTrailLocation(PositionReport position) : base(position.Latitude, position.Longitude, position.Altitude)
        {
            this.position = position;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the position.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PositionReport Position => this.position;
    }
}