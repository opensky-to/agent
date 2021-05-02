// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Pushback.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// SimConnect pushback enum.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum Pushback
    {
        /// <summary>
        /// Pushing back straight.
        /// </summary>
        Straight = 0,

        /// <summary>
        /// Pushing back left.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Pushing back right.
        /// </summary>
        Right = 2,

        /// <summary>
        /// Not pushing back.
        /// </summary>
        NoPushback = 3
    }
}