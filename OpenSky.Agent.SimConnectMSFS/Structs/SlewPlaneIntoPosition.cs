// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SlewPlaneIntoPosition.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    using System.Device.Location;
    using System.Runtime.InteropServices;

    using OpenSky.Agent.SimConnectMSFS.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Slew plane into position struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SlewPlaneIntoPosition
    {
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
        /// The radio height in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double RadioHeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The magnetic heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Heading { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True airspeed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double AirspeedTrue { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pitch angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PitchAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The bank angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double BankAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical speed in feet per second.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double VerticalSpeedSeconds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the plane on the ground?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; set; }

        // ==================================================================================================
        // END OF STRUCT PROPERTIES - BELOW ARE GET-ONLY COMPUTED PROPERTIES FOR OPENSKY
        // ==================================================================================================

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the geo coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GeoCoordinate GeoCoordinate => new(this.Latitude, this.Longitude, this.RadioHeight);

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
        public static SlewPlaneIntoPosition FromPrimaryTracking(PrimaryTracking primary)
        {
            var slew = new SlewPlaneIntoPosition
            {
                Latitude = primary.Latitude,
                Longitude = primary.Longitude,
                RadioHeight = primary.RadioHeight,
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

    /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The slew plane into position struct SimConnect properties definition.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static class SlewPlaneIntoPositionDefinition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the definition list of sim properties.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<SimVar> Definition =>
            new()
            {
                new SimVar("PLANE LATITUDE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE LONGITUDE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE ALT ABOVE GROUND", "Feet", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE HEADING DEGREES MAGNETIC", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("AIRSPEED TRUE", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE PITCH DEGREES", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE BANK DEGREES", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("VERTICAL SPEED", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32),
            };
    }
}