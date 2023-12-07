﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingStatus.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Values that represent the tracking status options.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum TrackingStatus
    {
        /// <summary>
        /// Not tracking
        /// </summary>
        NotTracking,

        /// <summary>
        /// Preparing to track (displaying current/expected location, plane, date/time, etc., offer slew to location)
        /// </summary>
        Preparing,

        /// <summary>
        /// Tracking started in ground operations mode (fueling, loading cargo/pax)
        /// </summary>
        GroundOperations,

        /// <summary>
        /// Tracking flight in progress
        /// </summary>
        Tracking,

        /// <summary>
        /// Resuming flight (basically the same as preparing but disabling certain features and changing certain display texts)
        /// </summary>
        Resuming
    }
}