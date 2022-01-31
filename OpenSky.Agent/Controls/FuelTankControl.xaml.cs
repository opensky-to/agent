// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelTankControl.xaml.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Controls
{
    using System.ComponentModel;
    using System.Windows;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Fuel tank custom control.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FuelTankControl
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel tangent capacity property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty FuelTankCapacityProperty = DependencyProperty.Register("FuelTankCapacity", typeof(double), typeof(FuelTankControl), new UIPropertyMetadata(0.0));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel tank name property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty FuelTankNameProperty = DependencyProperty.Register("FuelTankName", typeof(string), typeof(FuelTankControl), new UIPropertyMetadata("Unknown tank"));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel tank info property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty FuelTankInfoProperty = DependencyProperty.Register("FuelTankInfo", typeof(string), typeof(FuelTankControl), new UIPropertyMetadata("??"));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel tank quantity property.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly DependencyProperty FuelTankQuantityProperty = DependencyProperty.Register("FuelTankQuantity", typeof(double), typeof(FuelTankControl), new UIPropertyMetadata(0.0));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Controls.FuelTankControl"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FuelTankControl()
        {
            this.InitializeComponent();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public double FuelTankCapacity
        {
            get => (double)this.GetValue(FuelTankCapacityProperty);
            set => this.SetValue(FuelTankCapacityProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the fuel tank.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public string FuelTankName
        {
            get => (string)this.GetValue(FuelTankNameProperty);
            set => this.SetValue(FuelTankNameProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the info string for the fuel tank.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public string FuelTankInfo
        {
            get => (string)this.GetValue(FuelTankInfoProperty);
            set => this.SetValue(FuelTankInfoProperty, value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Bindable(true)]
        public double FuelTankQuantity
        {
            get => (double)this.GetValue(FuelTankQuantityProperty);
            set => this.SetValue(FuelTankQuantityProperty, value);
        }
    }
}