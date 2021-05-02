﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ABE.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Native.PInvoke.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// ABE enumeration.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum ABE : uint
    {
        /// <summary>
        /// Taskbar is on the left side of the screen
        /// </summary>
        Left = 0,

        /// <summary>
        /// Taskbar is at the top of the screen
        /// </summary>
        Top = 1,

        /// <summary>
        /// Taskbar is on the right side of the screen
        /// </summary>
        Right = 2,

        /// <summary>
        /// Taskbar is on the bottom of the screen
        /// </summary>
        Bottom = 3
    }
}