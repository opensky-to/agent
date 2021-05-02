// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaneIdentityCollectorViewModel.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System.Windows;

    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.SimConnect;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Plane identity collector view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class PlaneIdentityCollectorViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The CSV we are collecting.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string csv;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The description we can add to the plane record (what mod was loaded, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string description = string.Empty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaneIdentityCollectorViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public PlaneIdentityCollectorViewModel()
        {
            this.AddPlaneIdentityCommand = new Command(this.AddPlaneIdentity);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the add plane identity command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command AddPlaneIdentityCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the CSV we are collecting.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Csv
        {
            get => this.csv;

            set
            {
                if (Equals(this.csv, value))
                {
                    return;
                }

                this.csv = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the description we can add to the plane record (what mod was loaded, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Description
        {
            get => this.description;

            set
            {
                if (Equals(this.description, value))
                {
                    return;
                }

                this.description = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the SimConnect object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SimConnect SimConnect => SimConnect.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the current plane identity to the CSV.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void AddPlaneIdentity()
        {
            if (!this.SimConnect.Connected)
            {
                ModernWpf.MessageBox.Show("Not connected to sim!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(this.SimConnect.PlaneIdentity.AtcModel) || string.IsNullOrEmpty(this.SimConnect.PlaneIdentity.AtcType))
            {
                ModernWpf.MessageBox.Show("ATC plane model or type not set, are you connected to the simulator and is the plane loaded correctly?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (this.Description.Contains(","))
            {
                ModernWpf.MessageBox.Show("You cannot add commas into the description text!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var csvRecord = $"{this.SimConnect.PlaneIdentity.TypeProperty},{this.SimConnect.PlaneIdentity.AtcType},{this.SimConnect.PlaneIdentity.AtcModel},{this.SimConnect.PlaneIdentifierHash},{this.Description}\r\n";
            this.Csv += csvRecord;

            this.Description = string.Empty;
        }
    }
}