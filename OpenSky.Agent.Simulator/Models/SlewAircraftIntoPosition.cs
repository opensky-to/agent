// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SlewAircraftIntoPosition.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    using System.Device.Location;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Slew aircraft into position model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class SlewAircraftIntoPosition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True airspeed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double AirspeedTrue { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the altitude.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Altitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The bank angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double BankAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the geo coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GeoCoordinate GeoCoordinate => new(this.Latitude, this.Longitude, this.RadioHeight);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The magnetic heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Heading { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The latitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The longitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the plane on the ground?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pitch angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PitchAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The radio height in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double RadioHeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical speed in feet per second.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double VerticalSpeedSeconds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes this object from the given from primary tracking struct.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/04/2021.
        /// </remarks>
        /// <param name="primary">
        /// The primary tracking struct.
        /// </param>
        /// <returns>
        /// A SlewPlaneIntoPosition struct.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static SlewAircraftIntoPosition FromPrimaryTracking(PrimaryTracking primary)
        {
            var slew = new SlewAircraftIntoPosition
            {
                Latitude = primary.Latitude,
                Longitude = primary.Longitude,
                RadioHeight = primary.RadioHeight,
                Altitude = primary.Altitude,
                Heading = primary.Heading,
                AirspeedTrue = primary.AirspeedTrue,
                PitchAngle = primary.PitchAngle,
                BankAngle = primary.BankAngle,
                VerticalSpeedSeconds = primary.VerticalSpeedSeconds,
                OnGround = primary.OnGround
            };
            return slew;
        }
    }
}