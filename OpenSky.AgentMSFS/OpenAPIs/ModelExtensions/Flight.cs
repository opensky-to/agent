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
                // Just the pilot in the beginning
                var totalPayload = 190.0;

                if (this.Aircraft.Type.NeedsCoPilot)
                {
                    totalPayload += 190;
                }

                if (this.Aircraft.Type.NeedsFlightEngineer)
                {
                    totalPayload += 190;
                }

                foreach (var flightPayload in this.FlightPayloads)
                {
                    totalPayload += flightPayload.Payload.Weight;
                }

                return totalPayload;
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
        /// <param name="log">
        /// The flight log restored from the save file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void CheckSaveMatchesFlight(this Flight flight, OpenSky.FlightLogXML.FlightLog log)
        {
            if (flight.Id != log.FlightID || !flight.Aircraft.Registry.Equals(log.AircraftRegistry))
            {
                Debug.WriteLine("Flight basics mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Origin check
            if (!flight.Origin.Icao.Equals(log.Origin.Icao) || Math.Abs(flight.Origin.Latitude - log.Origin.Latitude) > 0.01 ||
                Math.Abs(flight.Origin.Longitude - log.Origin.Longitude) > 0.01)
            {
                Debug.WriteLine("Origin mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Destination check
            if (!flight.Destination.Icao.Equals(log.Destination.Icao) || Math.Abs(flight.Destination.Latitude - log.Destination.Latitude) > 0.01 ||
                Math.Abs(flight.Destination.Longitude - log.Destination.Longitude) > 0.01)
            {
                Debug.WriteLine("Destination mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Alternate check
            if (!flight.Alternate.Icao.Equals(log.Alternate.Icao) || Math.Abs(flight.Alternate.Latitude - log.Alternate.Latitude) > 0.01 ||
                Math.Abs(flight.Alternate.Longitude - log.Alternate.Longitude) > 0.01)
            {
                Debug.WriteLine("Alternate mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }

            // Payload/fuel check
            if (Math.Abs(flight.PayloadPounds - log.PayloadPounds) > 0.01 || Math.Abs((flight.FuelGallons ?? 0) - log.FuelGallons) > 1.0)
            {
                Debug.WriteLine("Starting fuel/payload mismatch");

                // todo flag this to the api? is user editing save files?
                throw new Exception("Flight mismatch");
            }
        }
    }
}