// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecondaryTracking.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS.Structs
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.FlightLogXML;

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
        public bool ApuRunning { get; set; }

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
        /// Is the auto-pilot engaged?
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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the aircraft has "latitude longitude freeze" on, GSX
        /// sets this during pushback.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsLatitudeLongitudeFreezeOn { get; set; }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Secondary tracking converter (simConnect struct to simulator model).
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class SecondaryTrackingConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A SecondaryTracking extension method that converts the given secondary tracking.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="secondary">
        /// The secondary to act on.
        /// </param>
        /// <returns>
        /// The simulator model secondary tracking.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Simulator.Models.SecondaryTracking Convert(this SecondaryTracking secondary)
        {
            // Detect GSX pushback
            var pushBack = secondary.Pushback;
            if (pushBack == Pushback.NoPushback && secondary.IsLatitudeLongitudeFreezeOn)
            {
                pushBack = Pushback.Straight;
            }
            
            return new Simulator.Models.SecondaryTracking
            {
                UtcTime = secondary.UtcTime,
                UtcDay = secondary.UtcDay,
                UtcMonth = secondary.UtcMonth,
                UtcYear = secondary.UtcYear,
                TimeOfDay = secondary.TimeOfDay,
                CrashDetection = secondary.CrashDetection,
                UnlimitedFuel = secondary.UnlimitedFuel,
                ElectricalMasterBattery = secondary.ElectricalMasterBattery,
                EngineCombustion1 = secondary.EngineCombustion1,
                EngineCombustion2 = secondary.EngineCombustion2,
                EngineCombustion3 = secondary.EngineCombustion3,
                EngineCombustion4 = secondary.EngineCombustion4,
                Pushback = pushBack,
                ApuRunning = secondary.ApuRunning,
                LightBeacon = secondary.LightBeacon,
                LightNav = secondary.LightNav,
                LightStrobe = secondary.LightStrobe,
                LightTaxi = secondary.LightTaxi,
                LightLanding = secondary.LightLanding,
                FlapsHandle = secondary.FlapsHandle,
                FlapsPercentage = secondary.FlapsPercentage,
                GearHandle = secondary.GearHandle,
                GearPercentage = secondary.GearPercentage,
                AutoPilot = secondary.AutoPilot,
                ParkingBrake = secondary.ParkingBrake,
                SpoilersArmed = secondary.SpoilersArmed,
                SeatBeltSign = secondary.SeatBeltSign,
                NoSmokingSign = secondary.NoSmokingSign
            };
        }
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
                new SimVar("APU SWITCH", "Bool", SIMCONNECT_DATATYPE.INT32),
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
                new SimVar("IS LATITUDE LONGITUDE FREEZE ON", "Bool", SIMCONNECT_DATATYPE.INT32)
            };
    }
}