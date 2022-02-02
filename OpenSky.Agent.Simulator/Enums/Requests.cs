// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Requests.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// SimConnect request types.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum Requests : uint
    {
        /// <summary>
        /// Primary tracking request
        /// </summary>
        Primary = 0,


        /// <summary>
        /// Secondary tracking request
        /// </summary>
        Secondary = 1,


        /// <summary>
        /// Fuel tanks request
        /// </summary>
        FuelTanks = 2,


        /// <summary>
        /// Payload stations request
        /// </summary>
        PayloadStations = 3,
        
        /// <summary>
        /// Plane identity request
        /// </summary>
        PlaneIdentity = 4,

        /// <summary>
        /// Weight and balance request
        /// </summary>
        WeightAndBalance = 5,

        /// <summary>
        /// Landing analysis request
        /// </summary>
        LandingAnalysis = 6,

        /// <summary>
        /// Slew plane into position request
        /// </summary>
        SlewPlaneIntoPosition = 7,

        /// <summary>
        /// Plane registry request
        /// </summary>
        PlaneRegistry = 8
    }
}