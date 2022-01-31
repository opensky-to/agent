﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaneIdentity.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Plane identity struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Uppercase naming for struct variables/mixed with some being properties")]
    public struct PlaneIdentity
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
    /// The plane identity struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class PlaneIdentityDefinition
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