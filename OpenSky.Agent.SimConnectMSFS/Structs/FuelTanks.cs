﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelTanks.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS.Structs
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

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
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Fuel tanks converter (simConnect struct to simulator model).
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class FuelTanksConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The FuelTanks extension method that converts the given tanks struct.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="tanks">
        /// The tanks to act on.
        /// </param>
        /// <returns>
        /// The simulator model fuel tanks.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Simulator.Models.FuelTanks Convert(this FuelTanks tanks)
        {
            return new Simulator.Models.FuelTanks
            {
                FuelTankCenterCapacity = tanks.FuelTankCenterCapacity,
                FuelTankCenter2Capacity = tanks.FuelTankCenter2Capacity,
                FuelTankCenter3Capacity = tanks.FuelTankCenter3Capacity,
                FuelTankLeftMainCapacity = tanks.FuelTankLeftMainCapacity,
                FuelTankLeftAuxCapacity = tanks.FuelTankLeftAuxCapacity,
                FuelTankLeftTipCapacity = tanks.FuelTankLeftTipCapacity,
                FuelTankRightMainCapacity = tanks.FuelTankRightMainCapacity,
                FuelTankRightAuxCapacity = tanks.FuelTankRightAuxCapacity,
                FuelTankRightTipCapacity = tanks.FuelTankRightTipCapacity,
                FuelTankExternal1Capacity = tanks.FuelTankExternal1Capacity,
                FuelTankExternal2Capacity = tanks.FuelTankExternal2Capacity,

                FuelTankCenterQuantity = tanks.FuelTankCenterQuantity,
                FuelTankCenter2Quantity = tanks.FuelTankCenter2Quantity,
                FuelTankCenter3Quantity = tanks.FuelTankCenter3Quantity,
                FuelTankLeftMainQuantity = tanks.FuelTankLeftMainQuantity,
                FuelTankLeftAuxQuantity = tanks.FuelTankLeftAuxQuantity,
                FuelTankLeftTipQuantity = tanks.FuelTankLeftTipQuantity,
                FuelTankRightMainQuantity = tanks.FuelTankRightMainQuantity,
                FuelTankRightAuxQuantity = tanks.FuelTankRightAuxQuantity,
                FuelTankRightTipQuantity = tanks.FuelTankRightTipQuantity,
                FuelTankExternal1Quantity = tanks.FuelTankExternal1Quantity,
                FuelTankExternal2Quantity = tanks.FuelTankExternal2Quantity
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The FuelTanks extension method that converts the given tanks model.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="tanks">
        /// The tanks to act on.
        /// </param>
        /// <returns>
        /// The simConnect fuel tanks struct.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static FuelTanks ConvertBack(this Simulator.Models.FuelTanks tanks)
        {
            return new FuelTanks
            {
                FuelTankCenterCapacity = tanks.FuelTankCenterCapacity,
                FuelTankCenter2Capacity = tanks.FuelTankCenter2Capacity,
                FuelTankCenter3Capacity = tanks.FuelTankCenter3Capacity,
                FuelTankLeftMainCapacity = tanks.FuelTankLeftMainCapacity,
                FuelTankLeftAuxCapacity = tanks.FuelTankLeftAuxCapacity,
                FuelTankLeftTipCapacity = tanks.FuelTankLeftTipCapacity,
                FuelTankRightMainCapacity = tanks.FuelTankRightMainCapacity,
                FuelTankRightAuxCapacity = tanks.FuelTankRightAuxCapacity,
                FuelTankRightTipCapacity = tanks.FuelTankRightTipCapacity,
                FuelTankExternal1Capacity = tanks.FuelTankExternal1Capacity,
                FuelTankExternal2Capacity = tanks.FuelTankExternal2Capacity,

                FuelTankCenterQuantity = tanks.FuelTankCenterQuantity,
                FuelTankCenter2Quantity = tanks.FuelTankCenter2Quantity,
                FuelTankCenter3Quantity = tanks.FuelTankCenter3Quantity,
                FuelTankLeftMainQuantity = tanks.FuelTankLeftMainQuantity,
                FuelTankLeftAuxQuantity = tanks.FuelTankLeftAuxQuantity,
                FuelTankLeftTipQuantity = tanks.FuelTankLeftTipQuantity,
                FuelTankRightMainQuantity = tanks.FuelTankRightMainQuantity,
                FuelTankRightAuxQuantity = tanks.FuelTankRightAuxQuantity,
                FuelTankRightTipQuantity = tanks.FuelTankRightTipQuantity,
                FuelTankExternal1Quantity = tanks.FuelTankExternal1Quantity,
                FuelTankExternal2Quantity = tanks.FuelTankExternal2Quantity
            };
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
}