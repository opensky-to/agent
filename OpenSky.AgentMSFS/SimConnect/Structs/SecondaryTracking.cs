// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecondaryTracking.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    using OpenSky.AgentMSFS.SimConnect.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Secondary flight tracking struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SecondaryTracking
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The UTC time in seconds since midnight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double UtcTime { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The UTC day of the month.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int UtcDay { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The UTC month of the year.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int UtcMonth { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The UTC year.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int UtcYear { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The time of day.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public TimeOfDay TimeOfDay { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is crash detection turned on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool CrashDetection { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is unlimited fuel turned on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool UnlimitedFuel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the electrical master battery turned on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ElectricalMasterBattery { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The engine1 combustion flag.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EngineCombustion1 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The engine1 combustion flag.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EngineCombustion2 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The engine1 combustion flag.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EngineCombustion3 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The engine1 combustion flag.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EngineCombustion4 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Pushback status.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Pushback Pushback { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the APU running?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ApuGenerator { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are the beacon lights on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool LightBeacon { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are the nav lights on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool LightNav { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are the strobe lights on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool LightStrobe { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are the taxi lights on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool LightTaxi { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are the landing lights on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool LightLanding { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flaps handle percentage.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FlapsHandle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flaps (trailing) percentage.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FlapsPercentage { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gear handle in down position?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool GearHandle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gear extended percentage.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GearPercentage { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the auto pilot engaged?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AutoPilot { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the parking brake set?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ParkingBrake { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Are the spoilers armed?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool SpoilersArmed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Seat belt sign on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool SeatBeltSign { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// No smoking sign on?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool NoSmokingSign { get; set; }


        // ==================================================================================================
        // END OF STRUCT PROPERTIES - BELOW ARE GET-ONLY COMPUTED PROPERTIES FOR OPENSKY
        // ==================================================================================================

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The UTC date/time.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime UtcDateTime
        {
            get
            {
                try
                {
                    return new DateTime(this.UtcYear, this.UtcMonth, this.UtcDay).AddSeconds(this.UtcTime);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is there a running engine?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool EngineRunning => this.EngineCombustion1 || this.EngineCombustion2 || this.EngineCombustion3 || this.EngineCombustion4;
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The secondary tracking struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class SecondaryTrackingDefinition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the definition list of sim properties.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<SimVar> Definition =>
            new()
            {
                new SimVar("ZULU TIME", "Seconds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("ZULU DAY OF MONTH", "Number", SIMCONNECT_DATATYPE.INT32),
                new SimVar("ZULU MONTH OF YEAR", "Number", SIMCONNECT_DATATYPE.INT32),
                new SimVar("ZULU YEAR", "Number", SIMCONNECT_DATATYPE.INT32),
                new SimVar("TIME OF DAY", "Enum", SIMCONNECT_DATATYPE.INT32),
                new SimVar("REALISM CRASH DETECTION", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("UNLIMITED FUEL", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("ELECTRICAL MASTER BATTERY", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG COMBUSTION:1", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG COMBUSTION:2", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG COMBUSTION:3", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG COMBUSTION:4", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("PUSHBACK STATE", "Enum", SIMCONNECT_DATATYPE.INT32),
                new SimVar("APU GENERATOR ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("LIGHT BEACON", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("LIGHT NAV", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("LIGHT TAXI", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("LIGHT LANDING", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("FLAPS HANDLE PERCENT", "Percentage", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("TRAILING EDGE FLAPS LEFT PERCENT", "Percentage", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("GEAR HANDLE POSITION", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GEAR TOTAL PCT EXTENDED", "Percentage", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("AUTOPILOT MASTER", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("BRAKE PARKING INDICATOR", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("CABIN SEATBELTS ALERT SWITCH", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("CABIN NO SMOKING ALERT SWITCH", "Bool", SIMCONNECT_DATATYPE.INT32),
            };
    }
}