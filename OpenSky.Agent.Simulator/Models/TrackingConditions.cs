// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingConditions.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tracking conditions enum.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum TrackingConditions
    {
        /// <summary>
        /// Date and time condition.
        /// </summary>
        DateTime = 0,

        /// <summary>
        /// Fuel condition.
        /// </summary>
        Fuel = 1,

        /// <summary>
        /// Payload condition.
        /// </summary>
        Payload = 2,

        /// <summary>
        /// Plane model condition.
        /// </summary>
        PlaneModel = 3,

        /// <summary>
        /// Realism settings condition.
        /// </summary>
        RealismSettings = 4,

        /// <summary>
        /// Location condition.
        /// </summary>
        Location = 5,

        /// <summary>
        /// Vatsim option.
        /// </summary>
        Vatsim = 6
    }
}