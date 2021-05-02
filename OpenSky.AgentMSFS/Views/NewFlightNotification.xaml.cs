// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewFlightNotification.xaml.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    using System;
    using System.Windows;

    using OpenSky.AgentMSFS.Native;
    using OpenSky.AgentMSFS.Native.PInvoke.Enums;
    using OpenSky.AgentMSFS.Tools;
    using OpenSky.AgentMSFS.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// New flight notification this.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class NewFlightNotification
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="NewFlightNotification"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="timeout">
        /// The timeout for the notification in milliseconds.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public NewFlightNotification(int timeout = 60 * 1000)
        {
            this.InitializeComponent();

            if (this.DataContext is NewFlightNotificationViewModel viewModel)
            {
                viewModel.Timeout = timeout;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// New flight notification loaded.
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
        private void NewFlightNotificationOnLoaded(object sender, RoutedEventArgs e)
        {
            this.PositionthisToNotificationArea();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void NewFlightNotificationViewModelOnCloseWindow(object sender, EventArgs e)
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