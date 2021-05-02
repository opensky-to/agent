// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineType.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// SimConnect engine types.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum EngineType
    {
        /// <summary>
        /// Piston engine.
        /// </summary>
        Piston = 0,

        /// <summary>
        /// Jet engine.
        /// </summary>
        Jet = 1,

        /// <summary>
        /// No engine.
        /// </summary>
        None = 2,

        /// <summary>
        /// Helicopter turbine.
        /// </summary>
        HeloBellTurbine = 3,

        /// <summary>
        /// Unsupported engine.
        /// </summary>
        Unsupported = 4,

        /// <summary>
        /// Turboprop engine.
        /// </summary>
        Turboprop = 5
    }
}