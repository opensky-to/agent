﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftIdentity.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft identity model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftIdentity
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ATC plane model string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AtcModel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ATC plane type string.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AtcType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The engine count.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int EngineCount { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Engine type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public EngineType EngineType { get; set; }

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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the plane type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Type { get; set; }
    }
}