// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayloadStationControl.xaml.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Controls
{
    using System.ComponentModel;
    using System.Windows;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Payload station custom control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class PayloadStationControl
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty PayloadStationNameProperty = DependencyProperty.Register("PayloadStationName", typeof(string), typeof(PayloadStationControl), new UIPropertyMetadata("Unknown"));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty PayloadStationWeightProperty = DependencyProperty.Register("PayloadStationWeight", typeof(double), typeof(PayloadStationControl), new UIPropertyMetadata(0.0));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadStationControl"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public PayloadStationControl()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the payload station.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public string PayloadStationName
        {
            get => (string)this.GetValue(PayloadStationNameProperty);
            set => this.SetValue(PayloadStationNameProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload station weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public string PayloadStationWeight
        {
            get => (string)this.GetValue(PayloadStationWeightProperty);
            set => this.SetValue(PayloadStationWeightProperty, value);
        }
    }
}