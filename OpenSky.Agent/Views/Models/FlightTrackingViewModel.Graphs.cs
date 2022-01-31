// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingViewModel.Graphs.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System.Windows;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The view model for the flight tracking view - graphs.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/01/2022.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightTrackingViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart altitude is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartAltitudeChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart fuel is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartFuelChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart ground speed is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartGroundSpeedChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart simulation rate is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartSimRateChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the altitude series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AltitudeAxisAndSeriesVisibility => this.ChartAltitudeChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart altitude is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartAltitudeChecked
        {
            get => this.chartAltitudeChecked;

            set
            {
                if (Equals(this.chartAltitudeChecked, value))
                {
                    return;
                }

                this.chartAltitudeChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.AltitudeAxisAndSeriesVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart fuel is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartFuelChecked
        {
            get => this.chartFuelChecked;

            set
            {
                if (Equals(this.chartFuelChecked, value))
                {
                    return;
                }

                this.chartFuelChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.FuelAxisAndSeriesVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart ground speed is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartGroundSpeedChecked
        {
            get => this.chartGroundSpeedChecked;

            set
            {
                if (Equals(this.chartGroundSpeedChecked, value))
                {
                    return;
                }

                this.chartGroundSpeedChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.GroundSpeedAxisAndSeriesVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart simulation rate is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartSimRateChecked
        {
            get => this.chartSimRateChecked;

            set
            {
                if (Equals(this.chartSimRateChecked, value))
                {
                    return;
                }

                this.chartSimRateChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.SimRateAxisAndSeriesVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel axis and series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility FuelAxisAndSeriesVisibility => this.ChartFuelChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ground speed series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility GroundSpeedAxisAndSeriesVisibility => this.ChartGroundSpeedChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulation rate axis and series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility SimRateAxisAndSeriesVisibility => this.ChartSimRateChecked ? Visibility.Visible : Visibility.Collapsed;
    }
}