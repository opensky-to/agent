// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelTanks.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    using OpenSky.AgentMSFS.SimConnect.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Fuel tanks struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FuelTanks
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenterCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 2 capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenter2Capacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 3 capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenter3Capacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left main capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftMainCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left auxiliary capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftAuxCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left tip capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftTipCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right main capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightMainCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right auxiliary capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightAuxCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right tip capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightTipCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 1 capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankExternal1Capacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 2 capacity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankExternal2Capacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenterQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 2 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenter2Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank center 3 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankCenter3Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left main quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftMainQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left auxiliary quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftAuxQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank left tip quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankLeftTipQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right main quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightMainQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right auxiliary quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightAuxQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank right tip quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankRightTipQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 1 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankExternal1Quantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel tank external 2 quantity.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTankExternal2Quantity { get; set; }

        // ==================================================================================================
        // END OF STRUCT PROPERTIES - BELOW ARE GET-ONLY COMPUTED PROPERTIES FOR OPENSKY
        // ==================================================================================================

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

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The fuel tanks struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 18/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class FuelTanksDefinition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the definition list of sim properties.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<SimVar> Definition =>
            new()
            {
                new SimVar("FUEL TANK CENTER CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK CENTER2 CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK CENTER3 CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK LEFT MAIN CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK LEFT AUX CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK LEFT TIP CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK RIGHT MAIN CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK RIGHT AUX CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK RIGHT TIP CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK EXTERNAL1 CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK EXTERNAL2 CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),

                new SimVar("FUEL TANK CENTER QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK CENTER2 QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK CENTER3 QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK LEFT MAIN QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK LEFT AUX QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK LEFT TIP QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK RIGHT MAIN QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK RIGHT AUX QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK RIGHT TIP QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK EXTERNAL1 QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TANK EXTERNAL2 QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
            };
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Fuel tanks save/restore helper class.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/04/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class FuelTanksSaver
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets fuel tanks for saving to a file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="tanks">
        /// The tanks.
        /// </param>
        /// <returns>
        /// The fuel tanks for saving.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static XElement GetFuelTanksForSave(FuelTanks tanks)
        {
            var fuelTanks = new XElement("FuelTanks");
            foreach (FuelTank tank in Enum.GetValues(typeof(FuelTank)))
            {
                fuelTanks.Add(new XElement(tank.ToString(), tanks.Quantities[tank]));
            }

            return fuelTanks;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Restore fuel tanks from save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="tanksFromSave">
        /// The tanks from the save file.
        /// </param>
        /// <returns>
        /// The FuelTanks struct restored (only quantities, not capacities!).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static FuelTanks RestoreFuelTanksFromSave(XElement tanksFromSave)
        {
            var tanks = new FuelTanks();
            var quantities = tanks.Quantities;
            foreach (FuelTank tank in Enum.GetValues(typeof(FuelTank)))
            {
                quantities[tank] = double.Parse(tanksFromSave.Element(tank.ToString())?.Value ?? "missing");
            }

            tanks.UpdateQuantitiesFromDictionary(quantities);

            return tanks;
        }
    }
}