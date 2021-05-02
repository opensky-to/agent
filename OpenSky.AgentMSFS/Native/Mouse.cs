﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mouse.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Native
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using OpenSky.AgentMSFS.Native.PInvoke;
    using OpenSky.AgentMSFS.Native.PInvoke.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Global mouse functions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class Mouse
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets screen mouse position.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/03/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <returns>
        /// The screen mouse position.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke", Justification = "Reviewed, ok.")]
        public static System.Windows.Point GetScreenMousePosition()
        {
            var point = default(Point);
            if (User32.GetCursorPos(ref point))
            {
                return new System.Windows.Point(point.x, point.y);
            }

            throw new Exception("Error retrieving screen mouse position: " + Marshal.GetLastWin32Error());
        }
    }
}