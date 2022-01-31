// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Navigation;

    using OpenSky.AgentMSFS.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Settings window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Settings
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private Settings()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static Settings Instance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open the settings view or bring the existing instance into view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static void Open()
        {
            if (Instance == null)
            {
                Instance = new Settings();
                Instance.Closed += (_, _) => Instance = null;
                Instance.Show();
            }
            else
            {
                Instance.BringIntoView();
                Instance.Activate();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Hyperlink on request navigate.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Request navigate event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void HyperlinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Profile image on mouse enter.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ProfileImageOnMouseEnter(object sender, MouseEventArgs e)
        {
            this.CameraCanvas.Visibility = this.ProfileImage.IsMouseOver || this.CameraCanvas.IsMouseOver ? Visibility.Visible : Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Profile image on mouse leave.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/11/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Mouse event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ProfileImageOnMouseLeave(object sender, MouseEventArgs e)
        {
            this.CameraCanvas.Visibility = this.ProfileImage.IsMouseOver || this.CameraCanvas.IsMouseOver ? Visibility.Visible : Visibility.Collapsed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Settings on loaded.
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
        private void SettingsOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SettingsViewModel viewModel)
            {
                viewModel.ViewReference = this;
            }
        }
    }
}