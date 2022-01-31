// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeOfDay.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// SimConnect time of day enum.
    /// </summary>
    /// <remarks>
    /// sushi.at, 14/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum TimeOfDay
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Day.
        /// </summary>
        Day = 1,

        /// <summary>
        /// Dusk/Dawn.
        /// </summary>
        DuskDawn = 2,

        /// <summary>
        /// Night.
        /// </summary>
        Night = 3
    }
}