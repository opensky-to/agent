// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestFlight.xaml.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Create test flight window.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class TestFlight
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFlight"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public TestFlight()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static TestFlight Instance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open the test flight view or bring the existing instance into view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static void Open()
        {
            if (Instance == null)
            {
                Instance = new TestFlight();
                Instance.Closed += (sender, e) => Instance = null;
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
        /// Tests flight view model on close window.
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
        private void TestFlightViewModelOnCloseWindow(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}