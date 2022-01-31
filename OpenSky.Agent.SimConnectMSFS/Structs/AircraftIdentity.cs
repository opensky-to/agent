// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftIdentity.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS.Structs
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft identity struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Uppercase naming for struct variables/mixed with some being properties")]
    public struct AircraftIdentity
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Plane type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Type;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Engine type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public EngineType EngineType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The engine count.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int EngineCount { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ATC plane type string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string AtcType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ATC plane model string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string AtcModel;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Does the plane have flaps?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool FlapsAvailable { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the landing gear retractable?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool GearRetractable { get; set; }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft identity converter (simConnect struct to simulator model).
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class AircraftIdentityConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An AircraftIdentity extension method that converts the given aircraft identity.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="identity">
        /// The identity to act on.
        /// </param>
        /// <returns>
        /// The simulator model aircraft identity.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Agent.Simulator.Models.AircraftIdentity Convert(this AircraftIdentity identity)
        {
            return new Agent.Simulator.Models.AircraftIdentity
            {
                Type = identity.Type,
                EngineType = identity.EngineType,
                EngineCount = identity.EngineCount,
                AtcType = identity.AtcType,
                AtcModel = identity.AtcModel,
                FlapsAvailable = identity.FlapsAvailable,
                GearRetractable = identity.GearRetractable
            };
        }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The aircraft identity struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class AircraftIdentityDefinition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the definition list of sim properties.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<SimVar> Definition =>
            new()
            {
                new SimVar("TITLE", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("ENGINE TYPE", "Enum", SIMCONNECT_DATATYPE.INT32),
                new SimVar("NUMBER OF ENGINES", "Number", SIMCONNECT_DATATYPE.INT32),
                new SimVar("ATC TYPE", null, SIMCONNECT_DATATYPE.STRING64),
                new SimVar("ATC MODEL", null, SIMCONNECT_DATATYPE.STRING64),
                new SimVar("FLAPS AVAILABLE", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("IS GEAR RETRACTABLE", "Bool", SIMCONNECT_DATATYPE.INT32),
            };
    }
}