﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppBarData.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Native.PInvoke.Structs
{
    using System;
    using System.Runtime.InteropServices;

    using OpenSky.Agent.Native.PInvoke.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// App bar data struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential)]
    public struct AppBarData
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Size of the struct.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public uint cbSize;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Window pointer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IntPtr hWnd;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Callback message.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public uint uCallbackMessage;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// ABE edge coordinate (where is the taskbar?).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ABE uEdge;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Rect of the app bar (bounds).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Rect rc;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Additional parameters.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int lParam;
    }
}