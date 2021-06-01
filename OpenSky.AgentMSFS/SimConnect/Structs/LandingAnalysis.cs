// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingAnalysis.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;
    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Landing analysis struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingAnalysis
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
        /// The altitude in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Altitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the plane on the ground?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The cross wind (knots).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindLat { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The headwind/tailwind (knots).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindLong { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The true airspeed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double AirspeedTrue { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground speed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundSpeed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The sidewards speed of the plane (feet/s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double SpeedLat { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The forward speed of the plane (feet/s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double SpeedLong { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The g-force.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Gforce { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical speed during touchdown (feet/s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double LandingRateSeconds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The bank angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double BankAngle { get; set; }

        // ==================================================================================================
        // END OF STRUCT PROPERTIES - BELOW ARE GET-ONLY COMPUTED PROPERTIES FOR OPENSKY
        // ==================================================================================================

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The landing rate in feet per minute.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double LandingRate => this.LandingRateSeconds * 60;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current map location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Location Location => new(this.Latitude, this.Longitude, this.Altitude);
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The landing analysis struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class LandingAnalysisDefinition
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
                new SimVar("PLANE ALTITUDE", "Feet", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("AIRCRAFT WIND X", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("AIRCRAFT WIND Z", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("AIRSPEED TRUE", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("GROUND VELOCITY", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("VELOCITY BODY X", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("VELOCITY BODY Z", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("G FORCE", "GForce", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE TOUCHDOWN NORMAL VELOCITY", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE BANK DEGREES", "Degrees", SIMCONNECT_DATATYPE.FLOAT64)
            };
    }
}