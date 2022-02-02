// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimaryTracking.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using System.Collections.Generic;
    using System.Diagnostics;

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
            connector.Subscribe(DataRefs.FlightmodelPositionVhIndFpm, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Cockpit2AnnunciatorsStallWarning, 1000 / sampleRate, this.DataRefUpdated);
            // todo detect overspeed possible?
            connector.Subscribe(DataRefs.Flightmodel2MiscGforceNormal, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.TimeSimSpeed, 1000 / sampleRate, this.DataRefUpdated);
            // no slew mode really
            connector.Subscribe(DataRefs.Flightmodel2MiscHasCrashed, 1000 / sampleRate, this.DataRefUpdated);
        }

        private readonly List<string> subscribed = new();

        private void DataRefUpdated(DataRefElement element, float value)
        {
            if (!this.subscribed.Contains(element.DataRef))
            {
                Debug.WriteLine($"{element.DataRef} : {element.Value}");
                this.subscribed.Add(element.DataRef);
                element.OnValueChange += this.Element_OnValueChange;
            }
            
        }

        private void Element_OnValueChange(DataRefElement sender, float newValue)
        {
            if (sender.DataRef == DataRefs.FlightmodelPositionLatitude.DataRef)
            {
                this.Latitude = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionLongitude.DataRef)
            {
                this.Longitude = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionElevation.DataRef)
            {
                this.Altitude = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionYAgl.DataRef)
            {
                this.RadioHeight = newValue; // todo is this in meters?
                this.OnGround = newValue < 1; // todo there has to be a better way
            }
            if (sender.DataRef == DataRefs.FlightmodelMiscHInd2.DataRef)
            {
                this.IndicatedAltitude = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionMagPsi.DataRef)
            {
                this.Heading = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionTrueAirspeed.DataRef)
            {
                this.AirspeedTrue = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionGroundspeed.DataRef)
            {
                this.GroundSpeed = newValue; // todo unit? m/s?
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionIndicatedAirspeed.DataRef)
            {
                this.AirspeedIndicated = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionTrueTheta.DataRef)
            {
                this.PitchAngle = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionTruePhi.DataRef)
            {
                this.BankAngle = newValue;
            }
            if (sender.DataRef == DataRefs.FlightmodelPositionVhIndFpm.DataRef)
            {
                this.VerticalSpeedSeconds = newValue;
            }
            if (sender.DataRef == DataRefs.Cockpit2AnnunciatorsStallWarning.DataRef)
            {
                this.StallWarning = (int)newValue == 1;
            }
            if (sender.DataRef == DataRefs.Flightmodel2MiscGforceNormal.DataRef)
            {
                this.GForce = newValue;
            }
            if (sender.DataRef == DataRefs.TimeSimSpeed.DataRef)
            {
                this.SimulationRate = newValue;
            }
            if (sender.DataRef == DataRefs.Flightmodel2MiscHasCrashed.DataRef)
            {
                this.Crash = (int)newValue == 1;
            }
        }

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
    }
}