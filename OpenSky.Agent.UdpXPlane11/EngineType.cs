// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelWeightHelper.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane engine type.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/11/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class EngineType
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets fuel weight for engine type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2023.
        /// </remarks>
        /// <param name="engineType">
        /// Type of the engine.
        /// </param>
        /// <returns>
        /// The fuel weight in lbs per gallon.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static double GetFuelWeightForEngineType(int engineType)
        {
            return engineType switch
            {
                // Reciprocating carburetor
                0 => 6,

                // Reciprocating injected
                1 => 6,

                // Free turbo deprecated
                2 => 6.7,

                // Electric engine
                3 => 0,

                // Lo Bypass Jet deprecated
                4 => 6.7,

                // Single spool jet
                5 => 6.7,

                // Rocket
                6 => 0,

                // Multi spool jet
                7 => 6.7,

                // Turbo Prop Fixed deprecated
                8 => 6.7,

                // Free turbo prop
                9 => 6.7,

                // Fixed turbo prop
                10 => 6.7,

                // Unknown engine type
                _ => 0
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Convert engine type to OpenSky enum.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2023.
        /// </remarks>
        /// <param name="engineType">
        /// Type of the engine from xplane.
        /// </param>
        /// <returns>
        /// The engine type converted to OpenSky.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static OpenSkyApi.EngineType ConvertEngineType(int engineType)
        {
            return engineType switch
            {
                // Reciprocating carburetor
                0 => OpenSkyApi.EngineType.Piston,

                // Reciprocating injected
                1 => OpenSkyApi.EngineType.Piston,

                // Free turbo deprecated
                2 => OpenSkyApi.EngineType.Turboprop,

                // Electric engine
                3 => OpenSkyApi.EngineType.Unsupported,

                // Lo Bypass Jet deprecated
                4 => OpenSkyApi.EngineType.Jet,

                // Single spool jet
                5 => OpenSkyApi.EngineType.Jet,

                // Rocket
                6 => OpenSkyApi.EngineType.Unsupported,

                // Multi spool jet
                7 => OpenSkyApi.EngineType.Jet,

                // Turbo Prop Fixed deprecated
                8 => OpenSkyApi.EngineType.Turboprop,

                // Free turbo prop
                9 => OpenSkyApi.EngineType.Turboprop,

                // Fixed turbo prop
                10 => OpenSkyApi.EngineType.Turboprop,

                // Unknown engine type
                _ => OpenSkyApi.EngineType.None
            };
        }
    }
}