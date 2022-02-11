// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimaryTrackingDataRef.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane 11 dataref enabled version of primary tracking model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.Simulator.Models.PrimaryTracking"/>
    /// -------------------------------------------------------------------------------------------------
    public class PrimaryTrackingDataRef : Simulator.Models.PrimaryTracking
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Makes a copy of this object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/02/2022.
        /// </remarks>
        /// <returns>
        /// A copy of this object.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public Simulator.Models.PrimaryTracking Clone()
        {
            return new Simulator.Models.PrimaryTracking
            {
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                Altitude = this.Altitude,
                RadioHeight = this.RadioHeight,
                IndicatedAltitude = this.IndicatedAltitude,
                OnGround = this.OnGround,
                Heading = this.Heading,
                AirspeedTrue = this.AirspeedTrue,
                GroundSpeed = this.GroundSpeed,
                AirspeedIndicated = this.AirspeedIndicated,
                PitchAngle = this.PitchAngle,
                BankAngle = this.BankAngle,
                VerticalSpeedSeconds = this.VerticalSpeedSeconds,
                StallWarning = this.StallWarning,
                OverspeedWarning = this.OverspeedWarning,
                GForce = this.GForce,
                SimulationRate = this.SimulationRate,
                SlewActive = this.SlewActive,
                Crash = this.Crash
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Register with Xplane connector.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/02/2022.
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
            connector.Subscribe(DataRefs.FlightmodelMiscHInd2, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionMagPsi, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionTrueAirspeed, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionGroundspeed, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionIndicatedAirspeed, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionTrueTheta, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionTruePhi, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionVhInd, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Cockpit2AnnunciatorsStallWarning, 1000 / sampleRate, this.DataRefUpdated);

            // todo detect overspeed possible?
            connector.Subscribe(DataRefs.Flightmodel2MiscGforceNormal, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.TimeSimSpeed, 1000 / sampleRate, this.DataRefUpdated);

            // no slew mode really
            connector.Subscribe(DataRefs.Flightmodel2MiscHasCrashed, 1000 / sampleRate, this.DataRefUpdated);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Dataref subscription updated.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/02/2022.
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
                this.RadioHeight = value * 3.281;
                this.OnGround = value < 1; // todo there has to be a better way
            }

            if (element.DataRef == DataRefs.FlightmodelMiscHInd2.DataRef)
            {
                this.IndicatedAltitude = value;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionMagPsi.DataRef)
            {
                this.Heading = value;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionTrueAirspeed.DataRef)
            {
                this.AirspeedTrue = value * 1.944;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionGroundspeed.DataRef)
            {
                this.GroundSpeed = value * 1.944;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionIndicatedAirspeed.DataRef)
            {
                this.AirspeedIndicated = value * 1.944;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionTrueTheta.DataRef)
            {
                this.PitchAngle = value;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionTruePhi.DataRef)
            {
                this.BankAngle = value;
            }

            if (element.DataRef == DataRefs.FlightmodelPositionVhInd.DataRef)
            {
                this.VerticalSpeedSeconds = value * 3.281;
            }

            if (element.DataRef == DataRefs.Cockpit2AnnunciatorsStallWarning.DataRef)
            {
                this.StallWarning = (int)value == 1;
            }

            if (element.DataRef == DataRefs.Flightmodel2MiscGforceNormal.DataRef)
            {
                this.GForce = value;
            }

            if (element.DataRef == DataRefs.TimeSimSpeed.DataRef)
            {
                this.SimulationRate = value;
            }

            if (element.DataRef == DataRefs.Flightmodel2MiscHasCrashed.DataRef)
            {
                this.Crash = (int)value == 1;
            }
        }
    }
}