// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayloadStations.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

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

        // ==================================================================================================
        // END OF STRUCT PROPERTIES - BELOW ARE GET-ONLY COMPUTED PROPERTIES FOR OPENSKY
        // ==================================================================================================

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload stations names list.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<string> Names =>
            new()
            {
                this.Name1?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name2?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name3?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name4?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name5?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name6?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name7?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name8?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name9?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name10?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name11?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name12?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name13?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name14?.Replace("TT:MENU.PAYLOAD.", string.Empty),
                this.Name15?.Replace("TT:MENU.PAYLOAD.", string.Empty)
            };
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
            };
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Payload stations save/restore helper class.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/04/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class PayloadStationsSaver
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets payload stations for saving to a file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="stations">
        /// The stations.
        /// </param>
        /// <returns>
        /// The payload stations for saving.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static XElement GetPayloadStationsForSave(PayloadStations stations)
        {
            var payLoadStations = new XElement("PayloadStations");
            payLoadStations.Add(new XElement("Weight1", stations.Weight1));
            payLoadStations.Add(new XElement("Weight2", stations.Weight2));
            payLoadStations.Add(new XElement("Weight3", stations.Weight3));
            payLoadStations.Add(new XElement("Weight4", stations.Weight4));
            payLoadStations.Add(new XElement("Weight5", stations.Weight5));
            payLoadStations.Add(new XElement("Weight6", stations.Weight6));
            payLoadStations.Add(new XElement("Weight7", stations.Weight7));
            payLoadStations.Add(new XElement("Weight8", stations.Weight8));
            payLoadStations.Add(new XElement("Weight9", stations.Weight9));
            payLoadStations.Add(new XElement("Weight10", stations.Weight10));
            payLoadStations.Add(new XElement("Weight11", stations.Weight11));
            payLoadStations.Add(new XElement("Weight12", stations.Weight12));
            payLoadStations.Add(new XElement("Weight13", stations.Weight13));
            payLoadStations.Add(new XElement("Weight14", stations.Weight14));
            payLoadStations.Add(new XElement("Weight15", stations.Weight15));
            return payLoadStations;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Restore payload stations from save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="stationsFromSave">
        /// The stations from save file.
        /// </param>
        /// <returns>
        /// The PayloadStations struct restored (only weights, not names or count!).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static PayloadStations RestorePayloadStationsFromSave(XElement stationsFromSave)
        {
            var payloadStations = new PayloadStations
            {
                Weight1 = double.Parse(stationsFromSave.Element("Weight1")?.Value ?? "missing"),
                Weight2 = double.Parse(stationsFromSave.Element("Weight2")?.Value ?? "missing"),
                Weight3 = double.Parse(stationsFromSave.Element("Weight3")?.Value ?? "missing"),
                Weight4 = double.Parse(stationsFromSave.Element("Weight4")?.Value ?? "missing"),
                Weight5 = double.Parse(stationsFromSave.Element("Weight5")?.Value ?? "missing"),
                Weight6 = double.Parse(stationsFromSave.Element("Weight6")?.Value ?? "missing"),
                Weight7 = double.Parse(stationsFromSave.Element("Weight7")?.Value ?? "missing"),
                Weight8 = double.Parse(stationsFromSave.Element("Weight8")?.Value ?? "missing"),
                Weight9 = double.Parse(stationsFromSave.Element("Weight9")?.Value ?? "missing"),
                Weight10 = double.Parse(stationsFromSave.Element("Weight10")?.Value ?? "missing"),
                Weight11 = double.Parse(stationsFromSave.Element("Weight11")?.Value ?? "missing"),
                Weight12 = double.Parse(stationsFromSave.Element("Weight12")?.Value ?? "missing"),
                Weight13 = double.Parse(stationsFromSave.Element("Weight13")?.Value ?? "missing"),
                Weight14 = double.Parse(stationsFromSave.Element("Weight14")?.Value ?? "missing"),
                Weight15 = double.Parse(stationsFromSave.Element("Weight15")?.Value ?? "missing")
            };

            return payloadStations;
        }
    }
}