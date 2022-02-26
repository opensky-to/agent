// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingViewModel.CustomModules.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using System.Windows;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The view model for the flight tracking view - custom modules code.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Agent.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightTrackingViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The custom module visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Visibility customModuleVisibility = Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the custom module visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility CustomModuleVisibility
        {
            get => this.customModuleVisibility;
        
            set
            {
                if(Equals(this.customModuleVisibility, value))
                {
                   return;
                }
        
                this.customModuleVisibility = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The custom module content.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FrameworkElement customModuleContent;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the custom module content.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FrameworkElement CustomModuleContent
        {
            get => this.customModuleContent;
        
            set
            {
                if(Equals(this.customModuleContent, value))
                {
                   return;
                }
        
                this.customModuleContent = value;
                this.NotifyPropertyChanged();
            }
        }
    }
}
