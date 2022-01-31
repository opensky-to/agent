// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WeightAndBalance.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System.Collections.Generic;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The weight and balance struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public struct WeightAndBalance
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Empty weight in pounds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double EmptyWeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Total weight in pounds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double TotalWeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Max gross weight in pounds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxGrossWeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Fuel total capacity in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTotalCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Fuel total quantity on board in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTotalQuantity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Fuel weight (pounds/gallon).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelWeightPerGallon { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The center of gravity aft limit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double CgAftLimit { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The center of gravity forward limit.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double CgFwdLimit { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The center of gravity (longitudinal)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double CgPercent { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The center of gravity (lateral)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double CgPercentLateral { get; set; }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The weight and balance struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 20/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class WeightAndBalanceDefinition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the definition list of sim properties.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<SimVar> Definition =>
            new()
            {
                new SimVar("EMPTY WEIGHT", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("TOTAL WEIGHT", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("MAX GROSS WEIGHT", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TOTAL CAPACITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL TOTAL QUANTITY", "Gallons", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("FUEL WEIGHT PER GALLON", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("CG AFT LIMIT", "Percent over 100", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("CG FWD LIMIT", "Percent over 100", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("CG PERCENT", "Percent over 100", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("CG PERCENT LATERAL", "Percent over 100", SIMCONNECT_DATATYPE.FLOAT64),
            };
    }
}