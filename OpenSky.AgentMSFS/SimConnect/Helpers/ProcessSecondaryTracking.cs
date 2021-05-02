// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessSecondaryTracking.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Helpers
{
    using OpenSky.AgentMSFS.SimConnect.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Process secondary tracking (for queue processing)
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ProcessSecondaryTracking
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the new SimConnect secondary tracking struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SecondaryTracking New { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the old SimConnect secondary tracking struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SecondaryTracking Old { get; set; }
    }
}