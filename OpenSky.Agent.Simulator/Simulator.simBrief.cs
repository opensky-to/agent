// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.simBrief.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.Agent.Simulator.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - simBrief.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simbrief waypoint markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<SimbriefWaypointMarker> simbriefWaypointMarkers = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when SimConnect adds a new simbrief waypoint marker.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<SimbriefWaypointMarker> SimbriefWaypointMarkerAdded;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Simbrief route location collection to draw a poly line on the map.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection SimbriefRouteLocations { get; }
    }
}