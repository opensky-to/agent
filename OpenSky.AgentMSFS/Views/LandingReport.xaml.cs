// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingReport.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    using System;
    using System.Windows;

    using OpenSky.AgentMSFS.Native;
    using OpenSky.AgentMSFS.Native.PInvoke.Enums;
    using OpenSky.AgentMSFS.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Landing report notification window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class LandingReport
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LandingReport"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public LandingReport()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Landing report notification loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LandingReportNotificationOnLoaded(object sender, RoutedEventArgs e)
        {
            this.PositionthisToNotificationArea();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LandingReportViewModelOnClose(object sender, EventArgs e)
        {
            UpdateGUIDelegate closethis = () =>
            {
                try
                {
                    this.Close();
                }
                catch
                {
                    // Ignore
                }
            };
            this.Dispatcher.BeginInvoke(closethis);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Positions the this next to notification area of the taskbar.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void PositionthisToNotificationArea()
        {
            var taskbarInfo = Taskbar.TaskbarInfo;

            if (taskbarInfo.Position == TaskbarPosition.Top)
            {
                this.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width - this.Width;
                this.Top = taskbarInfo.Bounds.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Bottom)
            {
                this.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width - this.Width;
                this.Top = taskbarInfo.Bounds.Y - this.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Left)
            {
                this.Left = taskbarInfo.Bounds.X + taskbarInfo.Bounds.Width;
                this.Top = taskbarInfo.Bounds.Y + taskbarInfo.Bounds.Height - this.Height;
            }

            if (taskbarInfo.Position == TaskbarPosition.Right)
            {
                this.Left = taskbarInfo.Bounds.X - this.Width;
                this.Top = taskbarInfo.Bounds.Y + taskbarInfo.Bounds.Height - this.Height;
            }
        }
    }
}