// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoUpdate.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views
{
    using System;
    using System.Windows;

    using OpenSky.Agent.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Auto update window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AutoUpdate
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoUpdate"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AutoUpdate()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Automatic update on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/01/2022.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AutoUpdateOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AutoUpdateViewModel viewModel)
            {
                viewModel.ViewReference = this;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The viewmodel wants to close the window.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AutoUpdateViewModelOnClose(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}