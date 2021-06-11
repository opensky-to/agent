// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimaryTracking.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;
    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.SimConnect.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Primary flight tracking struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 12/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PrimaryTracking
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
        /// The radio height in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double RadioHeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the plane on the ground?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; set; }

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
        /// Ground speed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundSpeed { get; set; }

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
        /// The stall warning.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool StallWarning { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The over-speed warning.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OverspeedWarning { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The g-force.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GForce { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simulation rate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double SimulationRate { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is slew mode active?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool SlewActive { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Has the simulator detected a plane crash?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public CrashSequence CrashSequence { get; set; }

        // todo Maybe add "AMBIENT IN CLOUD	Bool", so we further refine lights on requirements (say no ldg lights below 10000 if in the clouds?)

        // ==================================================================================================
        // END OF STRUCT PROPERTIES - BELOW ARE GET-ONLY COMPUTED PROPERTIES FOR OPENSKY
        // ==================================================================================================

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical speed in feet per minute.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double VerticalSpeed => this.VerticalSpeedSeconds * 60;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current map location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Location MapLocation => new(this.Latitude, this.Longitude, this.Altitude);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the geo coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GeoCoordinate GeoCoordinate => new(this.Latitude, this.Longitude, this.Altitude);
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The primary tracking struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class PrimaryTrackingDefinition
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
                new SimVar("PLANE ALT ABOVE GROUND", "Feet", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("PLANE HEADING DEGREES MAGNETIC", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("AIRSPEED TRUE", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("GROUND VELOCITY", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE PITCH DEGREES", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE BANK DEGREES", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("VERTICAL SPEED", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("STALL WARNING", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("OVERSPEED WARNING", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("G FORCE", "GForce", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("SIMULATION RATE", "Number", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("IS SLEW ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("CRASH SEQUENCE", "Enum", SIMCONNECT_DATATYPE.INT32),
            };
    }
}