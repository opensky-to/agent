// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypesViewModel.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows;

    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.SimConnect;
    using OpenSky.AgentMSFS.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft types view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftTypesViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The optional comments (what mod was loaded, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string comments = string.Empty;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftTypesViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftTypesViewModel()
        {
            this.ExistingAircraftTypes = new ObservableCollection<AircraftType>();

            this.RefreshAircraftTypesCommand = new AsynchronousCommand(this.RefreshAircraftTypes);
            this.AddAircraftTypeCommand = new AsynchronousCommand(this.AddAircraftType);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes the list of existing aircraft types.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void RefreshAircraftTypes()
        {
            try
            {
                var result = OpenSkyService.Instance.GetAllAircraftTypesAsync().Result;
                if (!result.IsError)
                {
                    this.ExistingAircraftTypes.Clear();
                    foreach (var type in result.Data)
                    {
                        this.ExistingAircraftTypes.Add(type);
                    }
                }
                else
                {
                    this.RefreshAircraftTypesCommand.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine("Error refreshing aircraft types: " + result.Message);
                            if (!string.IsNullOrEmpty(result.ErrorDetails))
                            {
                                Debug.WriteLine(result.ErrorDetails);
                            }

                            ModernWpf.MessageBox.Show(result.Message, "Error refreshing aircraft types", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            catch (Exception ex)
            {
                ex.HandleApiCallException(this.RefreshAircraftTypesCommand, "Error refreshing aircraft types");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the add aircraft type command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand AddAircraftTypeCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh aircraft types command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AsynchronousCommand RefreshAircraftTypesCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of of the existing aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<AircraftType> ExistingAircraftTypes { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the optional comments (what mod was loaded, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Comments
        {
            get => this.comments;

            set
            {
                if (Equals(this.comments, value))
                {
                    return;
                }

                this.comments = value;
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
        private void AddAircraftType()
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

            
            // todo
        }
    }
}