// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SlewAircraftIntoPosition.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS.Structs
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Slew plane into position struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SlewAircraftIntoPosition
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
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Slew aircraft into position converter (simConnect struct to simulator model).
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class SlewAircraftIntoPositionConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A SlewAircraftIntoPosition extension method that converts the given slew aircraft into position struct.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="position">
        /// The position to act on.
        /// </param>
        /// <returns>
        /// The simulator model slew aircraft into position.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Agent.Simulator.Models.SlewAircraftIntoPosition Convert(this SlewAircraftIntoPosition position)
        {
            return new Agent.Simulator.Models.SlewAircraftIntoPosition
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude,
                RadioHeight = position.RadioHeight,
                Heading = position.Heading,
                AirspeedTrue = position.AirspeedTrue,
                PitchAngle = position.PitchAngle,
                BankAngle = position.BankAngle,
                VerticalSpeedSeconds = position.VerticalSpeedSeconds,
                OnGround = position.OnGround
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An SlewAircraftIntoPosition extension method that converts the given slew aircraft into position model.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="position">
        /// The position to act on.
        /// </param>
        /// <returns>
        /// The simConnect slew aircraft into position struct.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static SlewAircraftIntoPosition ConvertBack(this Agent.Simulator.Models.SlewAircraftIntoPosition position)
        {
            return new SlewAircraftIntoPosition
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude,
                RadioHeight = position.RadioHeight,
                Heading = position.Heading,
                AirspeedTrue = position.AirspeedTrue,
                PitchAngle = position.PitchAngle,
                BankAngle = position.BankAngle,
                VerticalSpeedSeconds = position.VerticalSpeedSeconds,
                OnGround = position.OnGround
            };
        }
    }

    /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The slew aircraft into position struct SimConnect properties definition.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static class SlewAircraftIntoPositionDefinition
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