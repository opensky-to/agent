// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightExtensions.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Models.API
{
    using System;
    using System.Diagnostics;
    using System.Xml.Linq;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky flight extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/04/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class FlightExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A Flight extension method that checks it matches a flight restored from a save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="flight">
        /// The flight to act on.
        /// </param>
        /// <param name="flightFromSave">
        /// The flight restored to compare to.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void CheckSaveMatchesFlight(this Flight flight, XElement flightFromSave)
        {
            var flightID = flightFromSave.EnsureChildElement("ID");
            var planeIdentifier = flightFromSave.EnsureChildElement("PlaneIdentifierHash");
            var atcType = flightFromSave.EnsureChildElement("AtcType");
            var atcModel = flightFromSave.EnsureChildElement("AtcModel");
            if (flight.FlightID != int.Parse(flightID.Value) || !flight.PlaneIdentifier.Equals(planeIdentifier.Value) || !flight.AtcType.Equals(atcType.Value) || !flight.AtcModel.Equals(atcModel.Value))
            {
                Debug.WriteLine("Flight basics mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Origin check
            var origin = flightFromSave.EnsureChildElement("Origin");
            if (!flight.OriginICAO.Equals(origin.Attribute("ICAO")?.Value) || Math.Abs(flight.OriginCoordinates.Latitude - double.Parse(origin.Attribute("Lat")?.Value ?? "-1")) > 0.01 ||
                Math.Abs(flight.OriginCoordinates.Longitude - double.Parse(origin.Attribute("Lon")?.Value ?? "-1")) > 0.01)
            {
                Debug.WriteLine("Origin mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Destination check
            var destination = flightFromSave.EnsureChildElement("Destination");
            if (!flight.DestinationICAO.Equals(destination.Attribute("ICAO")?.Value) || Math.Abs(flight.DestinationCoordinates.Latitude - double.Parse(destination.Attribute("Lat")?.Value ?? "-1")) > 0.01 ||
                Math.Abs(flight.DestinationCoordinates.Longitude - double.Parse(destination.Attribute("Lon")?.Value ?? "-1")) > 0.01)
            {
                Debug.WriteLine("Destination mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Alternate check
            var alternate = flightFromSave.EnsureChildElement("Alternate");
            if (!flight.AlternateICAO.Equals(alternate.Attribute("ICAO")?.Value) || Math.Abs(flight.AlternateCoordinates.Latitude - double.Parse(alternate.Attribute("Lat")?.Value ?? "-1")) > 0.01 ||
                Math.Abs(flight.AlternateCoordinates.Longitude - double.Parse(alternate.Attribute("Lon")?.Value ?? "-1")) > 0.01)
            {
                Debug.WriteLine("Alternate mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Payload/fuel check
            var payload = flightFromSave.EnsureChildElement("PayloadPounds");
            var fuelGallons = flightFromSave.EnsureChildElement("FuelGallons");
            if (Math.Abs(flight.PayloadPounds - double.Parse(payload.Value)) > 0.01 || Math.Abs(flight.FuelGallons - double.Parse(fuelGallons.Value)) > 0.01)
            {
                Debug.WriteLine("Starting fuel/payload mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Extension method that ensures that a specific child XElement of a parent one exists - throws exception if not.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="parent">
        /// The parent. This cannot be null.
        /// </param>
        /// <param name="child">
        /// The child. This cannot be null.
        /// </param>
        /// <returns>
        /// An XElement. This will never be null.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public static XElement EnsureChildElement(this XElement parent, [NotNull] string child)
        {
            var childElement = parent.Element(child);
            if (childElement == null)
            {
                Debug.WriteLine($"Save file malformed: Missing element {child}");
                throw new Exception("Save file malformed!");
            }

            return childElement;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A Flight extension method that generates a flight XElement for saving to a file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="flight">
        /// The flight to act on.
        /// </param>
        /// <returns>
        /// The flight to save.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static XElement GenerateFlightForSave(this Flight flight)
        {
            var flightElement = new XElement("Flight");
            flightElement.Add(new XElement("ID", $"{flight.FlightID}"));
            flightElement.Add(new XElement("PlaneName", $"{flight.PlaneName}"));
            flightElement.Add(new XElement("PlaneIdentifierHash", $"{flight.PlaneIdentifier}"));
            flightElement.Add(new XElement("AtcType", $"{flight.AtcType}"));
            flightElement.Add(new XElement("AtcModel", $"{flight.AtcModel}"));
            flightElement.Add(new XElement("UtcOffset", $"{flight.UtcOffset:F1}"));

            var origin = new XElement("Origin");
            flightElement.Add(origin);
            origin.SetAttributeValue("ICAO", $"{flight.OriginICAO}");
            origin.SetAttributeValue("Name", $"{flight.Origin}");
            origin.SetAttributeValue("Lat", $"{flight.OriginCoordinates.Latitude}");
            origin.SetAttributeValue("Lon", $"{flight.OriginCoordinates.Longitude}");

            var destination = new XElement("Destination");
            flightElement.Add(destination);
            destination.SetAttributeValue("ICAO", $"{flight.DestinationICAO}");
            destination.SetAttributeValue("Name", $"{flight.Destination}");
            destination.SetAttributeValue("Lat", $"{flight.DestinationCoordinates.Latitude}");
            destination.SetAttributeValue("Lon", $"{flight.DestinationCoordinates.Longitude}");

            var alternate = new XElement("Alternate");
            flightElement.Add(alternate);
            alternate.SetAttributeValue("ICAO", $"{flight.AlternateICAO}");
            alternate.SetAttributeValue("Name", $"{flight.Alternate}");
            alternate.SetAttributeValue("Lat", $"{flight.AlternateCoordinates.Latitude}");
            alternate.SetAttributeValue("Lon", $"{flight.AlternateCoordinates.Longitude}");

            flightElement.Add(new XElement("FuelGallons", $"{flight.FuelGallons:F2}"));
            flightElement.Add(new XElement("Payload", $"{flight.Payload}"));
            flightElement.Add(new XElement("PayloadPounds", $"{flight.PayloadPounds}"));

            return flightElement;
        }
    }
}