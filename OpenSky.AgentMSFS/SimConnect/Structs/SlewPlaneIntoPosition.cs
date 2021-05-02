// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SlewPlaneIntoPosition.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect.Structs
{
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    using CTrue.FsConnect;

    using Microsoft.FlightSimulator.SimConnect;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Slew plane into position struct.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SlewPlaneIntoPosition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The latitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The longitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The radio height in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double RadioHeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The magnetic heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Heading { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True airspeed in knots.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double AirspeedTrue { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The pitch angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PitchAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The bank angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double BankAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical speed in feet per second.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double VerticalSpeedSeconds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the plane on the ground?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; set; }

        // ==================================================================================================
        // END OF STRUCT PROPERTIES - BELOW ARE GET-ONLY COMPUTED PROPERTIES FOR OPENSKY
        // ==================================================================================================

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the geo coordinate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GeoCoordinate GeoCoordinate => new GeoCoordinate(this.Latitude, this.Longitude, this.RadioHeight);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes this object from the given from primary tracking struct.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/04/2021.
        /// </remarks>
        /// <param name="primary">
        /// The primary tracking struct.
        /// </param>
        /// <returns>
        /// A SlewPlaneIntoPosition struct.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static SlewPlaneIntoPosition FromPrimaryTracking(PrimaryTracking primary)
        {
            var slew = new SlewPlaneIntoPosition
            {
                Latitude = primary.Latitude,
                Longitude = primary.Longitude,
                RadioHeight = primary.RadioHeight,
                Heading = primary.Heading,
                AirspeedTrue = primary.AirspeedTrue,
                PitchAngle = primary.PitchAngle,
                BankAngle = primary.BankAngle,
                VerticalSpeedSeconds = primary.VerticalSpeedSeconds,
                OnGround = primary.OnGround
            };
            return slew;
        }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The slew plane into position struct SimConnect properties definition.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class SlewPlaneIntoPositionDefinition
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the definition list of sim properties.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static List<SimVar> Definition =>
            new List<SimVar>
            {
                new SimVar("PLANE LATITUDE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE LONGITUDE", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE ALT ABOVE GROUND", "Feet", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE HEADING DEGREES MAGNETIC", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("AIRSPEED TRUE", "Knots", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE PITCH DEGREES", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("PLANE BANK DEGREES", "Degrees", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("VERTICAL SPEED", "Feet per second", SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32),
            };
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Slew plane into position save/restore helper class.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/04/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class SlewPlaneIntoPositionSaver
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets slew plane into position for saving to file - Note we are actually using primary
        /// tracking struct for this as this struct is not using a sample rate and only loaded on request.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="primary">
        /// The primary.
        /// </param>
        /// <returns>
        /// The primary tracking struct containing the slew plane into position info for saving.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static XElement GetSlewPlaneIntoPositionForSave(PrimaryTracking primary)
        {
            var position = new XElement("ResumePosition");
            position.SetAttributeValue("Lat", primary.Latitude);
            position.SetAttributeValue("Lon", primary.Longitude);
            position.SetAttributeValue("Alt", $"{primary.RadioHeight:F0}");
            position.SetAttributeValue("Hdg", $"{primary.Heading:F2}");
            position.SetAttributeValue("Tas", $"{primary.AirspeedTrue:F0}");
            position.SetAttributeValue("Pit", $"{primary.PitchAngle:F2}");
            position.SetAttributeValue("Bnk", $"{primary.BankAngle:F2}");
            position.SetAttributeValue("Vss", $"{primary.VerticalSpeedSeconds:F2}");
            position.SetAttributeValue("Gnd", primary.OnGround);
            return position;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Restore slew plane into position from save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="positionFromSave">
        /// The position from the save file.
        /// </param>
        /// <returns>
        /// A SlewPlaneIntoPosition.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static SlewPlaneIntoPosition RestoreSlewPlaneIntoPositionFromSave(XElement positionFromSave)
        {
            var slew = new SlewPlaneIntoPosition
            {
                Latitude = double.Parse(positionFromSave.Attribute("Lat")?.Value ?? "missing"),
                Longitude = double.Parse(positionFromSave.Attribute("Lon")?.Value ?? "missing"),
                RadioHeight = double.Parse(positionFromSave.Attribute("Alt")?.Value ?? "missing"),
                Heading = double.Parse(positionFromSave.Attribute("Hdg")?.Value ?? "missing"),
                AirspeedTrue = double.Parse(positionFromSave.Attribute("Tas")?.Value ?? "missing"),
                PitchAngle = double.Parse(positionFromSave.Attribute("Pit")?.Value ?? "missing"),
                BankAngle = double.Parse(positionFromSave.Attribute("Bnk")?.Value ?? "missing"),
                VerticalSpeedSeconds = double.Parse(positionFromSave.Attribute("Vss")?.Value ?? "missing"),
                OnGround = bool.Parse(positionFromSave.Attribute("Gnd")?.Value ?? "missing")
            };
            return slew;
        }
    }
}