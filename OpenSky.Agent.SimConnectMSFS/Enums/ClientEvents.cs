// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientEvents.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Client event IDs.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum ClientEvents
    {
        /// <summary>
        /// The set UTC time event group
        /// </summary>
        SetTime = 100,

        /// <summary>
        /// Set Years event
        /// </summary>
        SetZuluYears = 101,

        /// <summary>
        /// Set days event
        /// </summary>
        SetZuluDays = 102,

        /// <summary>
        /// Set hours event
        /// </summary>
        SetZuluHours = 103,

        /// <summary>
        /// Set minute event
        /// </summary>
        SetZuluMinute = 104,

        /// <summary>
        /// Slew event group
        /// </summary>
        Slew = 200,

        /// <summary>
        /// Enable slew event
        /// </summary>
        SlewOn = 201,

        /// <summary>
        /// Disable slew event
        /// </summary>
        SlewOff = 202
    }
}