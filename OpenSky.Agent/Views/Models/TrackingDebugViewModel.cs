// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingDebugViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using JetBrains.Annotations;

    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The view model for the tracking debug view.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Agent.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class TrackingDebugViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulator instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Simulator Simulator => Simulator.Instance;
    }
}