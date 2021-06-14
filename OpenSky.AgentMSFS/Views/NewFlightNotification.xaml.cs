// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewFlightNotification.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    using System;
    using System.Windows;

    using OpenSky.AgentMSFS.Tools;
    using OpenSky.AgentMSFS.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// New flight notification window.
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
            this.PositionWindowToNotificationArea();
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
    }
}