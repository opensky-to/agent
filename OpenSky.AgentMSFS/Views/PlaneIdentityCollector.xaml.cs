// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaneIdentityCollector.xaml.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Plane identity collector window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class PlaneIdentityCollector
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaneIdentityCollector"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public PlaneIdentityCollector()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static PlaneIdentityCollector Instance { get; set; }

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
                Instance = new PlaneIdentityCollector();
                Instance.Closed += (sender, e) => Instance = null;
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