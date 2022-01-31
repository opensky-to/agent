// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingViewModel.FuelTanks.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Views.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Specialized;
    using System.Diagnostics;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.AgentMSFS.Controls;
    using OpenSky.AgentMSFS.Controls.Models;
    using OpenSky.AgentMSFS.MVVM;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The view model for the flight tracking view - fuel tanks code.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.AgentMSFS.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightTrackingViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if the fuel section is expanded, false if not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isFuelExpanded;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel tank infos dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableConcurrentDictionary<FuelTank, string> FuelTankInfos { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel tank quantities dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ObservableConcurrentDictionary<FuelTank, double> FuelTankQuantities { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the fuel section is expanded or not.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsFuelExpanded
        {
            get => this.isFuelExpanded;

            set
            {
                if (Equals(this.isFuelExpanded, value))
                {
                    return;
                }

                this.isFuelExpanded = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the set fuel tanks command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command SetFuelTanksCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the suggest fuel command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command SuggestFuelCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Quantities collection changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Notify collection changed event information.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void QuantitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (FuelTank tank in Enum.GetValues(typeof(FuelTank)))
            {
                this.FuelTankInfos[tank] =
                    $"{(this.FuelTankQuantities[tank] / this.Simulator.FuelTanks.Capacities[tank] * 100):F0} % ({this.FuelTankQuantities[tank]:F1} gallons, {(this.FuelTankQuantities[tank] * this.Simulator.WeightAndBalance.FuelWeightPerGallon):F1} lbs / {(this.FuelTankQuantities[tank] * 3.78541):F1} l, {(this.FuelTankQuantities[tank] * this.Simulator.WeightAndBalance.FuelWeightPerGallon * 0.453592):F1} kg)";
            }

            this.NotifyPropertyChanged(nameof(this.FuelTankInfos));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets fuel tanks quantities in the sim according to the distribution here.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SetFuelTanks()
        {
            try
            {
                if (this.Simulator.Flight == null || this.Simulator.Flight.Aircraft.Type.RequiresManualFuelling)
                {
                    return;
                }

                var fuelTanks = this.Simulator.FuelTanks;
                var quantities = fuelTanks.Quantities;
                foreach (FuelTank tank in Enum.GetValues(typeof(FuelTank)))
                {
                    quantities[tank] = this.FuelTankQuantities[tank];
                }

                fuelTanks.UpdateQuantitiesFromDictionary(quantities);

                this.Simulator.SetFuelTanks(fuelTanks);
                this.Simulator.RefreshModelNow(Requests.FuelTanks);
                this.Simulator.RefreshModelNow(Requests.WeightAndBalance);
            }
            catch (Exception ex)
            {
                var notification = new OpenSkyNotification(new ErrorDetails { DetailedMessage = ex.Message, Exception = ex }, "Error setting fuel", ex.Message, ExtendedMessageBoxImage.Error, 30);
                notification.SetErrorColorStyle();
                this.ViewReference.ShowNotification(notification);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Suggest fuel loading according to the following priorities.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void SuggestFuel()
        {
            Debug.WriteLine("Calculating suggested fuel distribution");
            var fuelToLoad = this.Simulator.Flight?.FuelGallons ?? 0.0;

            // Priority 1: Left/Right wingtip tanks
            if (this.Simulator.FuelTanks.FuelTankLeftTipCapacity > 0 && Math.Abs(this.Simulator.FuelTanks.FuelTankLeftTipCapacity - this.Simulator.FuelTanks.FuelTankRightTipCapacity) == 0)
            {
                var intoWingTips = Math.Min(this.Simulator.FuelTanks.FuelTankLeftTipCapacity * 2, fuelToLoad) / 2;
                this.FuelTankQuantities[FuelTank.LeftTip] = intoWingTips;
                this.FuelTankQuantities[FuelTank.RightTip] = intoWingTips;
                fuelToLoad -= intoWingTips * 2;
            }

            // Priority 2: Left/Right main tanks
            if (this.Simulator.FuelTanks.FuelTankLeftMainCapacity > 0 && Math.Abs(this.Simulator.FuelTanks.FuelTankLeftMainCapacity - this.Simulator.FuelTanks.FuelTankRightMainCapacity) == 0)
            {
                var intoMains = Math.Min(this.Simulator.FuelTanks.FuelTankLeftMainCapacity * 2, fuelToLoad) / 2;
                this.FuelTankQuantities[FuelTank.LeftMain] = intoMains;
                this.FuelTankQuantities[FuelTank.RightMain] = intoMains;
                fuelToLoad -= intoMains * 2;
            }

            // Priority 3: Left/Right aux tanks
            if (this.Simulator.FuelTanks.FuelTankLeftAuxCapacity > 0 && Math.Abs(this.Simulator.FuelTanks.FuelTankLeftAuxCapacity - this.Simulator.FuelTanks.FuelTankRightAuxCapacity) == 0)
            {
                var intoAuxs = Math.Min(this.Simulator.FuelTanks.FuelTankLeftAuxCapacity * 2, fuelToLoad) / 2;
                this.FuelTankQuantities[FuelTank.LeftAux] = intoAuxs;
                this.FuelTankQuantities[FuelTank.RightAux] = intoAuxs;
                fuelToLoad -= intoAuxs * 2;
            }

            // Priority 4: Center tanks x3
            var fueledCenter3 = false;
            if (this.Simulator.FuelTanks.FuelTankCenterCapacity > 0 && Math.Abs(this.Simulator.FuelTanks.FuelTankCenterCapacity - this.Simulator.FuelTanks.FuelTankCenter2Capacity) == 0 &&
                Math.Abs(this.Simulator.FuelTanks.FuelTankCenterCapacity - this.Simulator.FuelTanks.FuelTankCenter3Capacity) == 0)
            {
                var intoCenter = Math.Min(this.Simulator.FuelTanks.FuelTankCenterCapacity * 3, fuelToLoad) / 3;
                this.FuelTankQuantities[FuelTank.Center] = intoCenter;
                this.FuelTankQuantities[FuelTank.Center2] = intoCenter;
                this.FuelTankQuantities[FuelTank.Center3] = intoCenter;
                fuelToLoad -= intoCenter;
                fueledCenter3 = true;
            }

            // Priority 5: Center tanks x2 
            var fueledCenter2 = false;
            if (this.Simulator.FuelTanks.FuelTankCenterCapacity > 0 && Math.Abs(this.Simulator.FuelTanks.FuelTankCenterCapacity - this.Simulator.FuelTanks.FuelTankCenter2Capacity) == 0)
            {
                var intoCenter = Math.Min(this.Simulator.FuelTanks.FuelTankCenterCapacity * 2, fuelToLoad) / 2;
                this.FuelTankQuantities[FuelTank.Center] = intoCenter;
                this.FuelTankQuantities[FuelTank.Center2] = intoCenter;
                fuelToLoad -= intoCenter;
                fueledCenter2 = true;
            }

            // Priority 6: Center tank alone
            if (this.Simulator.FuelTanks.FuelTankCenterCapacity > 0 && !fueledCenter3 && !fueledCenter2)
            {
                var intoCenter = Math.Min(this.Simulator.FuelTanks.FuelTankCenterCapacity, fuelToLoad);
                this.FuelTankQuantities[FuelTank.Center] = intoCenter;
                fuelToLoad -= intoCenter;
            }

            // Priority 7: Center2 tank alone
            if (this.Simulator.FuelTanks.FuelTankCenter2Capacity > 0 && !fueledCenter3 && !fueledCenter2)
            {
                var intoCenter = Math.Min(this.Simulator.FuelTanks.FuelTankCenter2Capacity, fuelToLoad);
                this.FuelTankQuantities[FuelTank.Center2] = intoCenter;
                fuelToLoad -= intoCenter;
            }

            // Priority 8: Center3 tank alone
            if (this.Simulator.FuelTanks.FuelTankCenter3Capacity > 0 && !fueledCenter3)
            {
                var intoCenter = Math.Min(this.Simulator.FuelTanks.FuelTankCenter3Capacity, fuelToLoad);
                this.FuelTankQuantities[FuelTank.Center3] = intoCenter;
                fuelToLoad -= intoCenter;
            }

            // Priority 9: External tanks x2
            var fueledExternal = false;
            if (this.Simulator.FuelTanks.FuelTankCenterCapacity > 0 && Math.Abs(this.Simulator.FuelTanks.FuelTankExternal1Capacity - this.Simulator.FuelTanks.FuelTankExternal2Capacity) == 0)
            {
                var intoExternal = Math.Min(this.Simulator.FuelTanks.FuelTankExternal1Capacity * 2, fuelToLoad) / 2;
                this.FuelTankQuantities[FuelTank.External1] = intoExternal;
                this.FuelTankQuantities[FuelTank.External2] = intoExternal;
                fuelToLoad -= intoExternal * 2;
                fueledExternal = true;
            }

            // Priority 10: External 1 alone
            if (this.Simulator.FuelTanks.FuelTankExternal1Capacity > 0 && !fueledExternal)
            {
                var intoExternal = Math.Min(this.Simulator.FuelTanks.FuelTankExternal1Capacity, fuelToLoad);
                this.FuelTankQuantities[FuelTank.External1] = intoExternal;
                fuelToLoad -= intoExternal;
            }

            // Priority 11: External 2 alone
            if (this.Simulator.FuelTanks.FuelTankExternal2Capacity > 0 && !fueledExternal)
            {
                var intoExternal = Math.Min(this.Simulator.FuelTanks.FuelTankExternal2Capacity, fuelToLoad);
                this.FuelTankQuantities[FuelTank.External2] = intoExternal;
                fuelToLoad -= intoExternal;
            }

            this.NotifyPropertyChanged(nameof(this.FuelTankQuantities));
            this.IsFuelExpanded = true;
            Debug.WriteLine($"Fuel distribution complete, still have {fuelToLoad:F2} gallons to load!");
        }
    }
}