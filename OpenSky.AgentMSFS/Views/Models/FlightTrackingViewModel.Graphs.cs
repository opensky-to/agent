// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingViewModel.Graphs.cs" company="OpenSky">
// OpenSky project 2021
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
        /// True if chart airspeed is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartAirspeedChecked = true;

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
        /// True if chart radio altitude is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartRadioAltChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if chart simulation rate is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool chartSimRateChecked = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the airspeed series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AirspeedSeriesVisibility => this.ChartAirspeedChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the altitude axis visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AltitudeAxisVisibility => this.ChartAltitudeChecked || this.ChartRadioAltChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the altitude series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility AltitudeSeriesVisibility => this.ChartAltitudeChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart airspeed is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartAirspeedChecked
        {
            get => this.chartAirspeedChecked;

            set
            {
                if (Equals(this.chartAirspeedChecked, value))
                {
                    return;
                }

                this.chartAirspeedChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.AirspeedSeriesVisibility));
                this.NotifyPropertyChanged(nameof(this.SpeedAxisVisibility));
            }
        }

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
                this.NotifyPropertyChanged(nameof(this.AltitudeSeriesVisibility));
                this.NotifyPropertyChanged(nameof(this.AltitudeAxisVisibility));
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
                this.NotifyPropertyChanged(nameof(this.GroundSpeedSeriesVisibility));
                this.NotifyPropertyChanged(nameof(this.SpeedAxisVisibility));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the chart radio altitude is checked.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ChartRadioAltChecked
        {
            get => this.chartRadioAltChecked;

            set
            {
                if (Equals(this.chartRadioAltChecked, value))
                {
                    return;
                }

                this.chartRadioAltChecked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.RadioAltSeriesVisibility));
                this.NotifyPropertyChanged(nameof(this.AltitudeAxisVisibility));
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
        public Visibility GroundSpeedSeriesVisibility => this.ChartGroundSpeedChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the radio altitude series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility RadioAltSeriesVisibility => this.ChartRadioAltChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the simulation rate axis and series visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility SimRateAxisAndSeriesVisibility => this.ChartSimRateChecked ? Visibility.Visible : Visibility.Collapsed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the speed axis visibility.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Visibility SpeedAxisVisibility => this.ChartAirspeedChecked || this.chartGroundSpeedChecked ? Visibility.Visible : Visibility.Collapsed;
    }
}