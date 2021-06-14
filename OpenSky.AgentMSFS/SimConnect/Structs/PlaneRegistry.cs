// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaneRegistry.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Plane registry struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/04/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Uppercase naming for struct variables/mixed with some being properties")]
    public struct PlaneRegistry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The atc ID/plane registry number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string AtcID;
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The plane registry struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/04/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class PlaneRegistryDefinition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the definition list of sim properties.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<SimVar> Definition =>
            new()
            {
                new SimVar("ATC ID", null, SIMCONNECT_DATATYPE.STRING64)
            };
    }
}