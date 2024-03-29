﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowExtensions.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Tools
{
    using System.Windows;

    using OpenSky.Agent.Native;
    using OpenSky.Agent.Native.PInvoke.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Window extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class WindowExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Positions the window next to notification area of the taskbar.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="window">
        /// The window to act on.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void PositionWindowToNotificationArea(this Window window)
        {
            var taskbarInfo = Taskbar.TaskbarInfo;

            if (taskbarInfo.Position == TaskbarPosition.Top)
            {
                window.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width - window.Width;
                window.Top = taskbarInfo.Bounds.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Bottom)
            {
                window.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width - window.Width;
                window.Top = taskbarInfo.Bounds.Y - window.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Left)
            {
                window.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width;
                window.Top = taskbarInfo.Bounds.Y + taskbarInfo.Bounds.Height - window.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Right)
            {
                window.Left = taskbarInfo.Bounds.X - window.Width;
                window.Top = taskbarInfo.Bounds.Y + taskbarInfo.Bounds.Height - window.Height;
            }
        }
    }
}