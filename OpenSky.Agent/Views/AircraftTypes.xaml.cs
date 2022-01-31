// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypes.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    using System.Windows;

    using DataGridExtensions;

    using OpenSky.AgentMSFS.Views.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Plane identity collector window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftTypes
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTypes"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private AircraftTypes()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static AircraftTypes Instance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open the plane identity collector view or bring the existing instance into view.
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

                Instance = new AircraftTypes();
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
        /// Aircraft types on loaded.
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
        private void AircraftTypesOnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AircraftTypesViewModel viewModel)
            {
                viewModel.ViewReference = this;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears all filters on click.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/06/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void ClearAllFiltersOnClick(object sender, RoutedEventArgs e)
        {
            this.AircraftTypesGrid.GetFilter().Clear();
        }
    }
}