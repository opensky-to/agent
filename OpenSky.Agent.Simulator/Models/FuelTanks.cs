// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelTanks.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    using System.Collections.Generic;

    using OpenSky.Agent.Simulator.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Fuel tanks model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FuelTanks
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel tank capacities dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<FuelTank, double> Capacities =>
            new()
            {
                { FuelTank.Center, this.FuelTankCenterCapacity },
                { FuelTank.Center2, this.FuelTankCenter2Capacity },
                { FuelTank.Center3, this.FuelTankCenter3Capacity },
                { FuelTank.LeftMain, this.FuelTankLeftMainCapacity },
                { FuelTank.LeftAux, this.FuelTankLeftAuxCapacity },
                { FuelTank.LeftTip, this.FuelTankLeftTipCapacity },
                { FuelTank.RightMain, this.FuelTankRightMainCapacity },
                { FuelTank.RightAux, this.FuelTankRightAuxCapacity },
                { FuelTank.RightTip, this.FuelTankRightTipCapacity },
                { FuelTank.External1, this.FuelTankExternal1Capacity },
                { FuelTank.External2, this.FuelTankExternal2Capacity }
            };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 2 capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenter2Capacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 2 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenter2Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 3 capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenter3Capacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 3 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenter3Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenterCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenterQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 1 capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankExternal1Capacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 1 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankExternal1Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 2 capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankExternal2Capacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 2 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankExternal2Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left auxiliary capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftAuxCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left auxiliary quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftAuxQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left main capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftMainCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left main quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftMainQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left tip capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftTipCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left tip quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftTipQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right auxiliary capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightAuxCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right auxiliary quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightAuxQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right main capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightMainCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right main quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightMainQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right tip capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightTipCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right tip quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightTipQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel tank quantities dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<FuelTank, double> Quantities =>
            new()
            {
                { FuelTank.Center, this.FuelTankCenterQuantity },
                { FuelTank.Center2, this.FuelTankCenter2Quantity },
                { FuelTank.Center3, this.FuelTankCenter3Quantity },
                { FuelTank.LeftMain, this.FuelTankLeftMainQuantity },
                { FuelTank.LeftAux, this.FuelTankLeftAuxQuantity },
                { FuelTank.LeftTip, this.FuelTankLeftTipQuantity },
                { FuelTank.RightMain, this.FuelTankRightMainQuantity },
                { FuelTank.RightAux, this.FuelTankRightAuxQuantity },
                { FuelTank.RightTip, this.FuelTankRightTipQuantity },
                { FuelTank.External1, this.FuelTankExternal1Quantity },
                { FuelTank.External2, this.FuelTankExternal2Quantity }
            };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the total quantity of fuel in all tanks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double TotalQuantity
        {
            get
            {
                var total = 0.0;
                foreach (var quantity in this.Quantities)
                {
                    total += quantity.Value;
                }

                return total;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the capacities from a modified dictionary.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/02/2022.
        /// </remarks>
        /// <param name="modifiedDictionary">
        /// The modified dictionary containing the new capacity values.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void UpdateCapactiesFromDictionary(Dictionary<FuelTank, double> modifiedDictionary)
        {
            this.FuelTankCenterCapacity = modifiedDictionary[FuelTank.Center];
            this.FuelTankCenter2Capacity = modifiedDictionary[FuelTank.Center2];
            this.FuelTankCenter3Capacity = modifiedDictionary[FuelTank.Center3];
            this.FuelTankLeftMainCapacity = modifiedDictionary[FuelTank.LeftMain];
            this.FuelTankLeftAuxCapacity = modifiedDictionary[FuelTank.LeftAux];
            this.FuelTankLeftTipCapacity = modifiedDictionary[FuelTank.LeftTip];
            this.FuelTankRightMainCapacity = modifiedDictionary[FuelTank.RightMain];
            this.FuelTankRightAuxCapacity = modifiedDictionary[FuelTank.RightAux];
            this.FuelTankRightTipCapacity = modifiedDictionary[FuelTank.RightTip];
            this.FuelTankExternal1Capacity = modifiedDictionary[FuelTank.External1];
            this.FuelTankExternal2Capacity = modifiedDictionary[FuelTank.External2];
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the quantities from a modified dictionary.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="modifiedDictionary">
        /// The modified dictionary containing the new quantity values.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void UpdateQuantitiesFromDictionary(Dictionary<FuelTank, double> modifiedDictionary)
        {
            this.FuelTankCenterQuantity = modifiedDictionary[FuelTank.Center];
            this.FuelTankCenter2Quantity = modifiedDictionary[FuelTank.Center2];
            this.FuelTankCenter3Quantity = modifiedDictionary[FuelTank.Center3];
            this.FuelTankLeftMainQuantity = modifiedDictionary[FuelTank.LeftMain];
            this.FuelTankLeftAuxQuantity = modifiedDictionary[FuelTank.LeftAux];
            this.FuelTankLeftTipQuantity = modifiedDictionary[FuelTank.LeftTip];
            this.FuelTankRightMainQuantity = modifiedDictionary[FuelTank.RightMain];
            this.FuelTankRightAuxQuantity = modifiedDictionary[FuelTank.RightAux];
            this.FuelTankRightTipQuantity = modifiedDictionary[FuelTank.RightTip];
            this.FuelTankExternal1Quantity = modifiedDictionary[FuelTank.External1];
            this.FuelTankExternal2Quantity = modifiedDictionary[FuelTank.External2];
        }
    }
}