﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingDebug.xaml.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Tracking debug view.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class TrackingDebug
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingDebug"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public TrackingDebug()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single instance of this view.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private static TrackingDebug Instance { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Open the tracking debug view or bring the existing instance into view.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public static void Open()
        {
            if (Instance == null)
            {
                Instance = new TrackingDebug();
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