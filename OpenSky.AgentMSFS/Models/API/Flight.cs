// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Flight.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Models.API
{
    using System;
    using System.Device.Location;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// This is a placeholder flight class until we can get the proper OpenSky API one implemented.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Flight
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Alternate { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate airport coordinates.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GeoCoordinate AlternateCoordinates { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the alternate ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AlternateICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the atc model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AtcModel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the atc.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string AtcType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the destination airport name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Destination { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Destination airport coordinates.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GeoCoordinate DestinationCoordinates { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets destination ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string DestinationICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Identifier for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int FlightID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelGallons { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when fuel loading will be complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime FuelLoadingComplete { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin airport name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Origin { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The origin airport coordinates.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public GeoCoordinate OriginCoordinates { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the origin ICAO code.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OriginICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload description text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Payload { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when payload loading will be complete.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime PayloadLoadingComplete { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the payload in pounds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double PayloadPounds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier hash code of the plane.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PlaneIdentifier { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the plane name.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PlaneName { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the plane registry.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string PlaneRegistry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to resume this flight or start a new one.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool Resume { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the UTC offset for the flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double UtcOffset { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a debug flight for testing LOWS-LOWW.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <returns>
        /// The new demo flight.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Flight CreateDebugJob()
        {
            return new()
            {
                FlightID = 6681,
                PlaneIdentifier = "cded57279be863637c6b57c4658862e02eb6acc424fe19d8fe5613e53557a73b",
                AtcType = "TT:ATCCOM.ATC_NAME CESSNA.0.text",
                AtcModel = "TT:ATCCOM.AC_MODEL_C25C.0.text",
                PlaneName = "Cessna CJ4 Citation WorkingTitle",
                PlaneRegistry = "SUSI-CJ4",
                UtcOffset = 1,

                Resume = false,

                Origin = "Salzburg Airport",
                OriginICAO = "LOWS",
                OriginCoordinates = new GeoCoordinate(47.7933006287, 13.0043001175),

                Destination = "Vienna International Airport",
                DestinationICAO = "LOWW",
                DestinationCoordinates = new GeoCoordinate(48.110298156738, 16.569700241089),

                Alternate = "Graz Airport",
                AlternateICAO = "LOWG",
                AlternateCoordinates = new GeoCoordinate(46.991100311279, 15.439599990845),

                Payload = "5 Pax: Crash Test Dummies",
                PayloadPounds = 170 * 5 + 170,
                FuelGallons = 300.0,
                FuelLoadingComplete = DateTime.UtcNow.AddSeconds(30),
                PayloadLoadingComplete = DateTime.UtcNow.AddSeconds(130)
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="obj">
        /// The object to compare with the current object.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <seealso cref="M:System.Object.Equals(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            var otherJob = obj as Flight;
            return otherJob?.FlightID.Equals(this.FlightID) == true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        /// -------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return this.FlightID.GetHashCode();
        }
    }
}