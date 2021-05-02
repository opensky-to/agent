// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingDebugViewModel.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using JetBrains.Annotations;

    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.SimConnect;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The view model for the tracking debug view.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class TrackingDebugViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the SimConnect instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public SimConnect SimConnect => SimConnect.Instance;
    }
}