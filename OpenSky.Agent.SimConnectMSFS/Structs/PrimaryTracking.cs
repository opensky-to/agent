﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimaryTracking.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS.Structs
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    using OpenSky.Agent.SimConnectMSFS.Enums;

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
        /// The indicated altitude in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double IndicatedAltitude { get; set; }

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
        /// The indicated airspeed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double AirspeedIndicated { get; set; }

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
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Primary tracking converter (simConnect struct to simulator model).
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class PrimaryTrackingConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A PrimaryTracking extension method that converts the given primary tracking.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="primary">
        /// The primary to act on.
        /// </param>
        /// <returns>
        /// The simulator model primary tracking.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Simulator.Models.PrimaryTracking Convert(this PrimaryTracking primary)
        {
            return new Simulator.Models.PrimaryTracking
            {
                Latitude = primary.Latitude,
                Longitude = primary.Longitude,
                Altitude = primary.Altitude,
                RadioHeight = primary.RadioHeight,
                IndicatedAltitude = primary.IndicatedAltitude,
                OnGround = primary.OnGround,
                Heading = primary.Heading,
                AirspeedTrue = primary.AirspeedTrue,
                GroundSpeed = primary.GroundSpeed,
                AirspeedIndicated = primary.AirspeedIndicated,
                PitchAngle = primary.PitchAngle,
                BankAngle = primary.BankAngle,
                VerticalSpeedSeconds = primary.VerticalSpeedSeconds,
                StallWarning = primary.StallWarning,
                OverspeedWarning = primary.OverspeedWarning,
                GForce = primary.GForce,
                SimulationRate = primary.SimulationRate,
                SlewActive = primary.SlewActive,
                Crash = primary.CrashSequence != CrashSequence.Off
            };
        }
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
                new SimVar("INDICATED ALTITUDE", "Feet", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("PLANE HEADING DEGREES MAGNETIC", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("AIRSPEED TRUE", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("GROUND VELOCITY", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
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