// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessPrimaryTracking.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Helpers
{
    using OpenSky.Agent.SimConnectMSFS.Structs;
    using OpenSky.AgentMSFS.SimConnect.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Process primary tracking (for queue processing)
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ProcessPrimaryTracking
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the new SimConnect primary tracking struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PrimaryTracking New { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the old SimConnect primary tracking struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PrimaryTracking Old { get; set; }
    }
}