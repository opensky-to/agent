// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightLoadingTempModels.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight loading temporary models (loaded from flight save and kept here until the user clicks
    /// resume tracking.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FlightLoadingTempModels
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tanks model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FuelTanks FuelTanks { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload stations model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PayloadStations PayloadStations { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the slew aircraft into position model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SlewAircraftIntoPosition SlewAircraftIntoPosition { get; set; }
    }
}