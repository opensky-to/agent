// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrashSequence.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// SimConnect crash sequence enum.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum CrashSequence
    {
        /// <summary>
        /// No crash in progress - all good.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Crash complete? Not used by MSFS
        /// </summary>
        Complete = 1,

        /// <summary>
        /// Crash reset? Not used by MSFS
        /// </summary>
        Reset = 3,

        /// <summary>
        /// Crash pause? Not used by MSFS
        /// </summary>
        Pause = 4,

        /// <summary>
        /// Crash detected/started - oh oh - this state will stay like this until the user clicks restart/quit.
        /// </summary>
        Start = 11
    }
}