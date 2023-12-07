// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingAnalysisDataRef.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using System;

    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane 11 dataref enabled version of landing analysis model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 07/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.Simulator.Models.LandingAnalysis"/>
    /// -------------------------------------------------------------------------------------------------
    public class LandingAnalysisDataRef : Simulator.Models.LandingAnalysis
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The heading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private float heading;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The wind degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private float windDegrees;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The wind speed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private float windSpeed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LandingAnalysisDataRef"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public LandingAnalysisDataRef()
        {
            // Todo figure out how to calculate that correctly in XP11
            this.SpeedLat = 0;
            this.SpeedLong = 1;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Makes a copy of this object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/02/2022.
        /// </remarks>
        /// <returns>
        /// A copy of this object.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public Simulator.Models.LandingAnalysis Clone()
        {
            return new Simulator.Models.LandingAnalysis
            {
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                Altitude = this.Altitude,
                OnGround = this.OnGround,
                WindLat = this.WindLat,
                WindLong = this.WindLong,
                AirspeedTrue = this.AirspeedTrue,
                GroundSpeed = this.GroundSpeed,
                SpeedLat = this.SpeedLat,
                SpeedLong = this.SpeedLong,
                Gforce = this.Gforce,
                LandingRateSeconds = this.LandingRateSeconds,
                BankAngle = this.BankAngle
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Register with Xplane connector.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/02/2022.
        /// </remarks>
        /// <param name="connector">
        /// The Xplane connector.
        /// </param>
        /// <param name="sampleRate">
        /// The configured sample rate.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void RegisterWithConnector(XPlaneConnector connector, int sampleRate)
        {
            connector.Subscribe(DataRefs.FlightmodelPositionLatitude, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionLongitude, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionElevation, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionYAgl, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionMagPsi, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.WeatherWindDirectionDegt, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.WeatherWindSpeedKt, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionTrueAirspeed, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionGroundspeed, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Flightmodel2MiscGforceNormal, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionVhInd, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionTruePhi, 1000 / sampleRate, this.DataRefUpdated);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Dataref subscription updated.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/02/2022.
        /// </remarks>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void DataRefUpdated(DataRefElement element, float value)
        {
            if (element.DataRef == DataRefs.FlightmodelPositionLatitude.DataRef)
            {
                this.Latitude = value;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionLongitude.DataRef)
            {
                this.Longitude = value;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionElevation.DataRef)
            {
                this.Altitude = value * 3.281;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionYAgl.DataRef)
            {
                this.OnGround = value < 1; // todo there has to be a better way
            }

            if (element.DataRef == DataRefs.FlightmodelPositionTrueAirspeed.DataRef)
            {
                this.AirspeedTrue = value * 1.944;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionGroundspeed.DataRef)
            {
                this.GroundSpeed = value * 1.944;
            }

            if (element.DataRef == DataRefs.Flightmodel2MiscGforceNormal.DataRef)
            {
                this.Gforce = value;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionVhInd.DataRef)
            {
                this.LandingRateSeconds = value * 3.281;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionTruePhi.DataRef)
            {
                this.BankAngle = value;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionMagPsi.DataRef)
            {
                this.heading = value;
            }

            if (element.DataRef == DataRefs.WeatherWindDirectionDegt.DataRef)
            {
                this.windDegrees = value;
            }

            if (element.DataRef == DataRefs.WeatherWindSpeedKt.DataRef)
            {
                this.windSpeed = value * 1.944f;
            }

            var windDelta = Math.Abs(this.heading - this.windDegrees);
            this.WindLat = Math.Sin((Math.PI / 180) * windDelta) * this.windSpeed;
            this.WindLong = Math.Cos((Math.PI / 180) * windDelta) * this.windSpeed;
        }
    }
}