// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingViewModel.PayloadStations.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    using OpenSky.Agent.Controls;
    using OpenSky.Agent.Controls.Models;
    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// The view model for the flight tracking view - payload stations code.
    /// </content>
    /// <remarks>
    /// sushi.at, 19/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Agent.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightTrackingViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if the payload section is expanded, false if not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isPayloadExpanded;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the payload section is expanded or not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsPayloadExpanded
        {
            get => this.isPayloadExpanded;

            set
            {
                if (Equals(this.isPayloadExpanded, value))
                {
                    return;
                }

                this.isPayloadExpanded = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload station weights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableCollection<double> PayloadStationWeights { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the set payload stations command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command SetPayloadStationsCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the suggest payload command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command SuggestPayloadCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets payload station weights in the sim according to the distribution here.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SetPayloadStations()
        {
            if (this.Simulator.Flight == null || this.Simulator.Flight.Aircraft.Type.RequiresManualLoading)
            {
                return;
            }

            try
            {
                var payloadStations = this.Simulator.PayloadStations;
                payloadStations.Weight1 = this.PayloadStationWeights[0];
                payloadStations.Weight2 = this.PayloadStationWeights[1];
                payloadStations.Weight3 = this.PayloadStationWeights[2];
                payloadStations.Weight4 = this.PayloadStationWeights[3];
                payloadStations.Weight5 = this.PayloadStationWeights[4];
                payloadStations.Weight6 = this.PayloadStationWeights[5];
                payloadStations.Weight7 = this.PayloadStationWeights[6];
                payloadStations.Weight8 = this.PayloadStationWeights[7];
                payloadStations.Weight9 = this.PayloadStationWeights[8];
                payloadStations.Weight10 = this.PayloadStationWeights[9];
                payloadStations.Weight11 = this.PayloadStationWeights[10];
                payloadStations.Weight12 = this.PayloadStationWeights[11];
                payloadStations.Weight13 = this.PayloadStationWeights[12];
                payloadStations.Weight14 = this.PayloadStationWeights[13];
                payloadStations.Weight15 = this.PayloadStationWeights[14];
                payloadStations.Weight16 = this.PayloadStationWeights[15];
                payloadStations.Weight17 = this.PayloadStationWeights[16];
                payloadStations.Weight18 = this.PayloadStationWeights[17];
                payloadStations.Weight19 = this.PayloadStationWeights[18];
                payloadStations.Weight20 = this.PayloadStationWeights[19];

                this.Simulator.SetPayloadStations(payloadStations);
                this.Simulator.RefreshModelNow(Requests.PayloadStations);
                this.Simulator.RefreshModelNow(Requests.WeightAndBalance);
            }
            catch (Exception ex)
            {
                var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error setting payload", ex.Message, ExtendedMessageBoxImage.Error, 30);
                notification.SetErrorColorStyle();
                this.ViewReference.ShowNotification(notification);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Suggest payload loading.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SuggestPayload()
        {
            Debug.WriteLine("Calculating suggested payload distribution");
            var payloadToLoad = this.Simulator.Flight?.PayloadPounds ?? 0.0;

            // Check for pilot/copilot first
            var nonPilotStations = 0;
            for (var i = 0; i < this.Simulator.PayloadStations.Count; i++)
            {
                if (this.Simulator.PayloadStations.Names[i].Contains("PILOT"))
                {
                    var payload = Math.Min(170, payloadToLoad);
                    this.PayloadStationWeights[i] = payload;
                    payloadToLoad -= payload;
                }
                else
                {
                    nonPilotStations++;
                }
            }

            // Distribute the rest evenly
            if (nonPilotStations == 0)
            {
                nonPilotStations = this.Simulator.PayloadStations.Count;
            }

            if (nonPilotStations > 0)
            {
                var payloadShare = payloadToLoad / nonPilotStations;
                for (var i = 0; i < this.Simulator.PayloadStations.Count; i++)
                {
                    if (!this.Simulator.PayloadStations.Names[i].Contains("PILOT"))
                    {
                        this.PayloadStationWeights[i] = payloadShare;
                        payloadToLoad -= payloadShare;
                    }
                }
            }

            this.IsPayloadExpanded = true;
            Debug.WriteLine($"Payload distribution complete, still have {payloadToLoad:F2} pounds to load!");
        }
    }
}