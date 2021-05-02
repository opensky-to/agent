// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelTank.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Enums
{
    using OpenSky.AgentMSFS.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Fuel tank enum (used to access fuel tank dictionaries).
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum FuelTank
    {
        /// <summary>
        /// Center tank
        /// </summary>
        [StringValue("Center")]
        Center,

        /// <summary>
        /// Center2 tank
        /// </summary>
        [StringValue("Center 2")]
        Center2,

        /// <summary>
        /// Center3 tank
        /// </summary>
        [StringValue("Center 3")]
        Center3,

        /// <summary>
        /// Left main tank
        /// </summary>
        [StringValue("Left main")]
        LeftMain,

        /// <summary>
        /// Left aux tank
        /// </summary>
        [StringValue("Left aux")]
        LeftAux,

        /// <summary>
        /// Left wingtip tank
        /// </summary>
        [StringValue("Left wingtip")]
        LeftTip,

        /// <summary>
        /// Right main tank
        /// </summary>
        [StringValue("Right main")]
        RightMain,

        /// <summary>
        /// Right aux tank
        /// </summary>
        [StringValue("Right aux")]
        RightAux,

        /// <summary>
        /// Right wingtip tank
        /// </summary>
        [StringValue("Right wingtip")]
        RightTip,

        /// <summary>
        /// External1 tank
        /// </summary>
        [StringValue("External 1")]
        External1,

        /// <summary>
        /// External2 tank
        /// </summary>
        [StringValue("External 2")]
        External2
    }
}