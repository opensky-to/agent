// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.xaml.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
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
    }
}