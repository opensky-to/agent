// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginNotification.xaml.cs" company="OpenSky">
// sushi.at for OpenSky 2021
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
    /// Login notification window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class LoginNotification
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginNotification"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="timeout">
        /// The timeout for the notification in milliseconds.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public LoginNotification(int timeout = 60 * 1000)
        {
            this.InitializeComponent();

            if (this.DataContext is LoginNotificationViewModel viewModel)
            {
                viewModel.Timeout = timeout;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Login notification loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LoginNotificationOnLoaded(object sender, RoutedEventArgs e)
        {
            this.PositionWindowToNotificationArea();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the view model wants to close the window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void LoginNotificationViewModelOnCloseWindow(object sender, EventArgs e)
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