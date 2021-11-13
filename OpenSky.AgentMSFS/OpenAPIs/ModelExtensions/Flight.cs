// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightExtensions.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
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
    public partial class Flight
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the flight can resume (all necessary parameters are present).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool Resume
        {
            get
            {
                // Fuel tanks have values?
                if (!this.FuelTankCenterQuantity.HasValue || !this.FuelTankCenter2Quantity.HasValue || !this.FuelTankCenter3Quantity.HasValue || !this.FuelTankLeftMainQuantity.HasValue || !this.FuelTankLeftAuxQuantity.HasValue ||
                    !this.FuelTankLeftTipQuantity.HasValue || !this.FuelTankRightMainQuantity.HasValue || !this.FuelTankRightTipQuantity.HasValue || !this.FuelTankExternal1Quantity.HasValue || !this.FuelTankExternal2Quantity.HasValue)
                {
                    return false;
                }

                // todo check payload stations

                if (!this.Latitude.HasValue || !this.Longitude.HasValue || !this.RadioHeight.HasValue || !this.Heading.HasValue || !this.AirspeedTrue.HasValue)
                {
                    return false;
                }

                return true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the total payload pounds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PayloadPounds
        {
            get
            {
                // todo sum up the total payloads to be transported

                return 0;
            }
        }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 13/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class FlightExtensionMethods
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
            var planeRegistry = flightFromSave.EnsureChildElement("PlaneRegistry");
            var atcType = flightFromSave.EnsureChildElement("AtcType");
            var atcModel = flightFromSave.EnsureChildElement("AtcModel");
            if (flight.Id != Guid.Parse(flightID.Value) || !flight.Aircraft.Registry.Equals(planeRegistry.Value) || !flight.Aircraft.Type.AtcType.Equals(atcType.Value) || !flight.Aircraft.Type.AtcModel.Equals(atcModel.Value))
            {
                Debug.WriteLine("Flight basics mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Origin check
            var origin = flightFromSave.EnsureChildElement("Origin");
            if (!flight.Origin.Icao.Equals(origin.Attribute("ICAO")?.Value) || Math.Abs(flight.Origin.Latitude - double.Parse(origin.Attribute("Lat")?.Value ?? "-1")) > 0.01 ||
                Math.Abs(flight.Origin.Longitude - double.Parse(origin.Attribute("Lon")?.Value ?? "-1")) > 0.01)
            {
                Debug.WriteLine("Origin mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Destination check
            var destination = flightFromSave.EnsureChildElement("Destination");
            if (!flight.Destination.Icao.Equals(destination.Attribute("ICAO")?.Value) || Math.Abs(flight.Destination.Latitude - double.Parse(destination.Attribute("Lat")?.Value ?? "-1")) > 0.01 ||
                Math.Abs(flight.Destination.Longitude - double.Parse(destination.Attribute("Lon")?.Value ?? "-1")) > 0.01)
            {
                Debug.WriteLine("Destination mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Alternate check
            var alternate = flightFromSave.EnsureChildElement("Alternate");
            if (!flight.Alternate.Icao.Equals(alternate.Attribute("ICAO")?.Value) || Math.Abs(flight.Alternate.Latitude - double.Parse(alternate.Attribute("Lat")?.Value ?? "-1")) > 0.01 ||
                Math.Abs(flight.Alternate.Longitude - double.Parse(alternate.Attribute("Lon")?.Value ?? "-1")) > 0.01)
            {
                Debug.WriteLine("Alternate mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Payload/fuel check
            var payload = flightFromSave.EnsureChildElement("PayloadPounds");
            var fuelGallons = flightFromSave.EnsureChildElement("FuelGallons");
            if (Math.Abs(flight.PayloadPounds - double.Parse(payload.Value)) > 0.01 || Math.Abs(flight.FuelGallons ?? 0 - double.Parse(fuelGallons.Value)) > 0.01)
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
            flightElement.Add(new XElement("ID", $"{flight.Id}"));
            flightElement.Add(new XElement("PlaneRegistry", $"{flight.Aircraft.Registry}"));
            flightElement.Add(new XElement("AtcType", $"{flight.Aircraft.Type.AtcType}"));
            flightElement.Add(new XElement("AtcModel", $"{flight.Aircraft.Type.AtcModel}"));
            flightElement.Add(new XElement("UtcOffset", $"{flight.UtcOffset:F1}"));

            var origin = new XElement("Origin");
            flightElement.Add(origin);
            origin.SetAttributeValue("ICAO", $"{flight.Origin.Icao}");
            origin.SetAttributeValue("Name", $"{flight.Origin.Name}");
            origin.SetAttributeValue("Lat", $"{flight.Origin.Latitude}");
            origin.SetAttributeValue("Lon", $"{flight.Origin.Longitude}");

            var destination = new XElement("Destination");
            flightElement.Add(destination);
            destination.SetAttributeValue("ICAO", $"{flight.Destination.Icao}");
            destination.SetAttributeValue("Name", $"{flight.Destination.Name}");
            destination.SetAttributeValue("Lat", $"{flight.Destination.Latitude}");
            destination.SetAttributeValue("Lon", $"{flight.Destination.Longitude}");

            var alternate = new XElement("Alternate");
            flightElement.Add(alternate);
            alternate.SetAttributeValue("ICAO", $"{flight.Alternate.Icao}");
            alternate.SetAttributeValue("Name", $"{flight.Alternate.Name}");
            alternate.SetAttributeValue("Lat", $"{flight.Alternate.Latitude}");
            alternate.SetAttributeValue("Lon", $"{flight.Alternate.Longitude}");

            flightElement.Add(new XElement("FuelGallons", $"{flight.FuelGallons:F2}"));
            flightElement.Add(new XElement("Payload", "None")); // todo add payload once we have it
            flightElement.Add(new XElement("PayloadPounds", $"{flight.PayloadPounds:F2}"));

            return flightElement;
        }
    }
}