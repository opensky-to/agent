// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessLandingAnalysis.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Helpers
{
    using OpenSky.AgentMSFS.SimConnect.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Process landing analysis (for queue processing).
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ProcessLandingAnalysis
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the new SimConnect landing analysis struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LandingAnalysis New { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the old SimConnect landing analysis struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LandingAnalysis Old { get; set; }
    }
}