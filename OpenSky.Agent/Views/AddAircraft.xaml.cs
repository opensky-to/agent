// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddAircraft.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views
{
    using System.Windows;

    using OpenSky.Agent.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simple add aircraft window for users.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AddAircraft
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AddAircraft"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private AddAircraft()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static AddAircraft Instance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open the add aircraft view or bring the existing instance into view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static void Open()
        {
            if (Instance == null)
            {
                if (!UserSessionService.Instance.IsUserLoggedIn)
                {
                    LoginNotification.Open();
                    return;
                }

                Instance = new AddAircraft();
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
        /// Add aircraft on loaded.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void AddAircraftLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AddAircraftViewModel viewModel)
            {
                viewModel.ViewReference = this;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// View model fired close window event.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/12/2023.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// An object to process.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ViewModelCloseWindow(object sender, object e)
        {
            this.Close();
        }
    }
}