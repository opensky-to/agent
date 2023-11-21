// --------------------------------------------------------------------------------------------------------------------
// <copyright file="A32NX.xaml.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Controls.CustomModules
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// A32NX custom module control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class A32NX : INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) the payload to load property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty PayloadToLoadProperty = DependencyProperty.Register("PayloadToLoad", typeof(double), typeof(A32NX), new UIPropertyMetadata(0.0, PayloadChanged));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) the current payload property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty CurrentPayloadProperty = DependencyProperty.Register("CurrentPayload", typeof(double), typeof(A32NX), new UIPropertyMetadata(0.0, PayloadChanged));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="A32NX"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public A32NX()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the cargo in kilograms to complete loading to the exact payload weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Cargo => $"{(this.PayloadToLoad - this.CurrentPayload) / 2.20462:F1} kg, type {(this.PayloadToLoad - this.CurrentPayload) / 2.20462 / 1000.0:F4} into MCDU and\r\nselect CARGO HOLD, followed by BOARDING START";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the current payload.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public double CurrentPayload
        {
            get => (double)this.GetValue(CurrentPayloadProperty);
            set => this.SetValue(CurrentPayloadProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the number of passengers we recommend to load.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int PassengerCount => (int)((this.PayloadToLoad / 2.20462) / 104.0);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload in kilograms.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PayloadKilograms => $"{this.PayloadToLoad / 2.20462:F1} kg";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload to load.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public double PayloadToLoad
        {
            get => (double)this.GetValue(PayloadToLoadProperty);
            set => this.SetValue(PayloadToLoadProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/02/2022.
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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Payload changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/02/2022.
        /// </remarks>
        /// <param name="d">
        /// A DependencyObject to process.
        /// </param>
        /// <param name="e">
        /// Dependency property changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private static void PayloadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is A32NX a32NX)
            {
                a32NX.OnPropertyChanged(nameof(a32NX.PayloadKilograms));
                a32NX.OnPropertyChanged(nameof(a32NX.PassengerCount));
                a32NX.OnPropertyChanged(nameof(a32NX.Cargo));
            }
        }
    }
}