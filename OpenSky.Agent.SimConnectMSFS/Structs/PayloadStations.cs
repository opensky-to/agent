// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayloadStations.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.SimConnectMSFS.Structs
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Payload stations struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Uppercase naming for struct variables/mixed with some being properties")]
    public struct PayloadStations
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the number of payload stations. 
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int Count { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 1.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name1;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 2.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name2;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 3.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name3;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 4.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name4;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 5.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name5;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 6.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name6;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 7.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name7;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 8.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name8;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 9.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name9;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 10.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name10;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name11;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 12.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name12;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 13.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name13;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 14.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name14;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 15.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name15;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 16.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name16;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 17.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name17;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 18.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name18;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 19.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name19;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station name 20.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name20;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 1.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight1 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 2.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight2 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 3.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight3 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 4.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight4 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 5.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight5 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 6.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight6 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 7.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight7 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 8.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight8 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 9.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight9 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 10.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight10 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight11 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 12.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight12 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 13.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight13 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 14.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight14 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 15.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight15 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 16.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight16 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 17.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight17 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 18.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight18 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 19.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight19 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The payload station weight 20.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Weight20 { get; set; }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Payload station converter (simConnect struct to simulator model).
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class PayloadStationsConverter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The PayloadStations extension method that converts the given payload stations struct.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="stations">
        /// The stations to act on.
        /// </param>
        /// <returns>
        /// The simulator model payload stations.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Simulator.Models.PayloadStations Convert(this PayloadStations stations)
        {
            return new Simulator.Models.PayloadStations
            {
                Count = stations.Count,
                Name1 = stations.Name1,
                Name2 = stations.Name2,
                Name3 = stations.Name3,
                Name4 = stations.Name4,
                Name5 = stations.Name5,
                Name6 = stations.Name6,
                Name7 = stations.Name7,
                Name8 = stations.Name8,
                Name9 = stations.Name9,
                Name10 = stations.Name10,
                Name11 = stations.Name11,
                Name12 = stations.Name12,
                Name13 = stations.Name13,
                Name14 = stations.Name14,
                Name15 = stations.Name15,
                Name16 = stations.Name16,
                Name17 = stations.Name17,
                Name18 = stations.Name18,
                Name19 = stations.Name19,
                Name20 = stations.Name20,
                Weight1 = stations.Weight1,
                Weight2 = stations.Weight2,
                Weight3 = stations.Weight3,
                Weight4 = stations.Weight4,
                Weight5 = stations.Weight5,
                Weight6 = stations.Weight6,
                Weight7 = stations.Weight7,
                Weight8 = stations.Weight8,
                Weight9 = stations.Weight9,
                Weight10 = stations.Weight10,
                Weight11 = stations.Weight11,
                Weight12 = stations.Weight12,
                Weight13 = stations.Weight13,
                Weight14 = stations.Weight14,
                Weight15 = stations.Weight15,
                Weight16 = stations.Weight16,
                Weight17 = stations.Weight17,
                Weight18 = stations.Weight18,
                Weight19 = stations.Weight19,
                Weight20 = stations.Weight20,
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The PayloadStations extension method that converts the given payload stations model.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="stations">
        /// The stations to act on.
        /// </param>
        /// <returns>
        /// The simConnect payload stations struct.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static PayloadStations ConvertBack(this Simulator.Models.PayloadStations stations)
        {
            return new PayloadStations
            {
                Count = stations.Count,
                Name1 = stations.Name1,
                Name2 = stations.Name2,
                Name3 = stations.Name3,
                Name4 = stations.Name4,
                Name5 = stations.Name5,
                Name6 = stations.Name6,
                Name7 = stations.Name7,
                Name8 = stations.Name8,
                Name9 = stations.Name9,
                Name10 = stations.Name10,
                Name11 = stations.Name11,
                Name12 = stations.Name12,
                Name13 = stations.Name13,
                Name14 = stations.Name14,
                Name15 = stations.Name15,
                Name16 = stations.Name16,
                Name17 = stations.Name17,
                Name18 = stations.Name18,
                Name19 = stations.Name19,
                Name20 = stations.Name20,
                Weight1 = stations.Weight1,
                Weight2 = stations.Weight2,
                Weight3 = stations.Weight3,
                Weight4 = stations.Weight4,
                Weight5 = stations.Weight5,
                Weight6 = stations.Weight6,
                Weight7 = stations.Weight7,
                Weight8 = stations.Weight8,
                Weight9 = stations.Weight9,
                Weight10 = stations.Weight10,
                Weight11 = stations.Weight11,
                Weight12 = stations.Weight12,
                Weight13 = stations.Weight13,
                Weight14 = stations.Weight14,
                Weight15 = stations.Weight15,
                Weight16 = stations.Weight16,
                Weight17 = stations.Weight17,
                Weight18 = stations.Weight18,
                Weight19 = stations.Weight19,
                Weight20 = stations.Weight20,
            };
        }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The payload stations struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 19/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class PayloadStationsDefinition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the definition list of sim properties.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<SimVar> Definition =>
            new()
            {
                new SimVar("PAYLOAD STATION COUNT", "Number", SIMCONNECT_DATATYPE.INT32),
                new SimVar("PAYLOAD STATION NAME:1", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:2", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:3", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:4", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:5", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:6", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:7", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:8", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:9", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:10", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:11", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:12", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:13", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:14", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:15", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:16", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:17", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:18", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:19", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION NAME:20", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar("PAYLOAD STATION WEIGHT:1", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:2", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:3", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:4", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:5", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:6", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:7", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:8", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:9", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:10", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:11", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:12", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:13", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:14", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:15", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:16", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:17", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:18", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:19", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PAYLOAD STATION WEIGHT:20", "Pounds", SIMCONNECT_DATATYPE.FLOAT64),
            };
    }
}