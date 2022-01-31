// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapPositionUpdate.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Mep position event data.
    /// </summary>
    /// <remarks>
    /// sushi.at, 25/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class MapPositionUpdate
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPositionUpdate"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/03/2021.
        /// </remarks>
        /// <param name="location">
        /// The location.
        /// </param>
        /// <param name="isUserAction">
        /// (Optional) True if is a user action that always should re-center the map, false if it is the plane moving and we should an out-of-bounds check.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public MapPositionUpdate(Location location, bool isUserAction = false)
        {
            this.Location = location;
            this.IsUserAction = isUserAction;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this is a user action that always should re-center the map, false if it is the plane moving and we should an out-of-bounds check.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsUserAction { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Location Location { get; }
    }
}