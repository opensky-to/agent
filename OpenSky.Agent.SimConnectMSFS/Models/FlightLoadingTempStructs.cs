// <copyright file="FlightLoadingTempStructs.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    using OpenSky.Agent.Simulator.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Temporary structs loaded from flight save file are kept here until the user clicks resume
    /// tracking.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/04/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FlightLoadingTempStructs
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tanks struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FuelTanks FuelTanks { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload stations struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PayloadStations PayloadStations { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the slew plane into position struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SlewPlaneIntoPosition SlewPlaneIntoPosition { get; set; }
    }
}