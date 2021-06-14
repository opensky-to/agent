// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingCondition.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tracking condition object.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class TrackingCondition : INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to automatically set the value in the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool autoSet;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if the condition is met.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool conditionMet;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current condition value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string current = "Unknown";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True to enable, false to disable the tracking condition.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool enabled = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The expected condition value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string expected = "Unknown";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to automatically set the value in the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool AutoSet
        {
            get => this.autoSet;

            set
            {
                if (Equals(this.autoSet, value))
                {
                    return;
                }

                this.autoSet = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the tracking condition is met.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ConditionMet
        {
            get => this.conditionMet;

            set
            {
                if (Equals(this.conditionMet, value))
                {
                    return;
                }

                this.conditionMet = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the current condition value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Current
        {
            get => this.current;

            set
            {
                if (Equals(this.current, value))
                {
                    return;
                }

                this.current = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this tracking condition is enabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool Enabled
        {
            get => this.enabled;

            set
            {
                if (Equals(this.enabled, value))
                {
                    return;
                }

                this.enabled = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.RowHeight));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the expected condition value.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Expected
        {
            get => this.expected;

            set
            {
                if (Equals(this.expected, value))
                {
                    return;
                }

                this.expected = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the visibility of this tracking condition.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GridLength RowHeight => this.Enabled ? new GridLength(1, GridUnitType.Auto) : new GridLength(0, GridUnitType.Pixel);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resets this object (except for AutoSet).
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Reset()
        {
            this.Current = "Unknown";
            this.Expected = "Unknown";
            this.ConditionMet = false;
            this.Enabled = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// (Optional) Name of the property.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}