// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WeightAndBalance.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS.Structs
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
    /// Weight and balance converter (simConnect struct to simulator model).
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class WeightAndBalanceConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A WeightAndBalance extension method that converts the given weight and balance.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="wab">
        /// The wab to act on.
        /// </param>
        /// <returns>
        /// The simulator model weight and balance.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Simulator.Models.WeightAndBalance Convert(this WeightAndBalance wab)
        {
            return new Simulator.Models.WeightAndBalance
            {
                EmptyWeight = wab.EmptyWeight,
                TotalWeight = wab.TotalWeight,
                MaxGrossWeight = wab.MaxGrossWeight,
                FuelTotalCapacity = wab.FuelTotalCapacity,
                FuelTotalQuantity = wab.FuelTotalQuantity,
                FuelWeightPerGallon = wab.FuelWeightPerGallon,
                CgAftLimit = wab.CgAftLimit,
                CgFwdLimit = wab.CgFwdLimit,
                CgPercent = wab.CgPercent,
                CgPercentLateral = wab.CgPercentLateral
            };
        }
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