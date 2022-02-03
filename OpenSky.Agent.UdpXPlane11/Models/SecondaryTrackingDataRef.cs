// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecondaryTrackingDataRef.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using OpenSky.Agent.Simulator.Enums;
    using OpenSky.FlightLogXML;

    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane 11 dataref enabled version of secondary tracking model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.Simulator.Models.SecondaryTracking"/>
    /// -------------------------------------------------------------------------------------------------
    public class SecondaryTrackingDataRef : Simulator.Models.SecondaryTracking
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondaryTrackingDataRef"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public SecondaryTrackingDataRef()
        {
            // These can't be changed in XPlane
            this.CrashDetection = true;
            this.UnlimitedFuel = false;

            this.UtcYear = DateTime.UtcNow.Year; // Doesn't seem to exist in xplane
            this.TimeOfDay = TimeOfDay.Unknown; // todo check if we can determine this somehow
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
            connector.Subscribe(DataRefs.TimeZuluTimeSec, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Cockpit2ClockTimerCurrentDay, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Cockpit2ClockTimerCurrentMonth, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalBatteryOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Flightmodel2EnginesEngineIsBurningFuel, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.GraphicsAnimationGroundTrafficTowbarHeadingDeg, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Cockpit2ElectricalAPURunning, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalBeaconLightsOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalNavLightsOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalStrobeLightsOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalTaxiLightOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalLandingLightsOn, 1000 / sampleRate, this.DataRefUpdated);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OnChange subscribed datarefs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<string> subscribed = new();

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
            if (!this.subscribed.Contains(element.DataRef))
            {
                Debug.WriteLine($"SUBSCRIBED TO: {element.DataRef} : {element.Value}");
                this.subscribed.Add(element.DataRef);
                element.OnValueChange += this.DatarefOnValueChange;
            }
            else
            {
                Debug.WriteLine($"REPEAT VALUE?: {element.DataRef} : {element.Value}");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Dataref on value change.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/02/2022.
        /// </remarks>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void DatarefOnValueChange(DataRefElement sender, float newValue)
        {
            if (sender.DataRef == DataRefs.TimeZuluTimeSec.DataRef)
            {
                this.UtcTime = newValue;
            }
            if (sender.DataRef == DataRefs.Cockpit2ClockTimerCurrentDay.DataRef)
            {
                this.UtcDay = (int)newValue;
            }
            if (sender.DataRef == DataRefs.Cockpit2ClockTimerCurrentMonth.DataRef)
            {
                this.UtcMonth = (int)newValue;
            }
            if (sender.DataRef == DataRefs.CockpitElectricalBatteryOn.DataRef)
            {
                this.ElectricalMasterBattery = (int)newValue == 1;
            }
            if (sender.DataRef == DataRefs.Flightmodel2EnginesEngineIsBurningFuel.DataRef)
            {
                this.EngineCombustion1 = (int)newValue == 1;
                this.EngineCombustion2 = (int)newValue == 1;
                this.EngineCombustion3 = (int)newValue == 1;
                this.EngineCombustion4 = (int)newValue == 1;
            }
            if (sender.DataRef == DataRefs.GraphicsAnimationGroundTrafficTowbarHeadingDeg.DataRef)
            {
                this.Pushback = newValue != 0 ? Pushback.Straight : Pushback.NoPushback; // todo check
            }
            if (sender.DataRef == DataRefs.Cockpit2ElectricalAPURunning.DataRef)
            {
                this.ApuGenerator = (int)newValue == 1;
            }
            if (sender.DataRef == DataRefs.CockpitElectricalBeaconLightsOn.DataRef)
            {
                this.LightBeacon = (int)newValue == 1;
            }
            if (sender.DataRef == DataRefs.CockpitElectricalNavLightsOn.DataRef)
            {
                this.LightNav = (int)newValue == 1;
            }
            if (sender.DataRef == DataRefs.CockpitElectricalStrobeLightsOn.DataRef)
            {
                this.LightStrobe = (int)newValue == 1;
            }
            if (sender.DataRef == DataRefs.CockpitElectricalTaxiLightOn.DataRef)
            {
                this.LightTaxi = (int)newValue == 1;
            }
            if (sender.DataRef == DataRefs.CockpitElectricalLandingLightsOn.DataRef)
            {
                this.LightLanding = (int)newValue == 1;
            }
        }

        public Simulator.Models.SecondaryTracking Clone()
        {
            return new Simulator.Models.SecondaryTracking
            {
                UtcTime = this.UtcTime,
                UtcDay = this.UtcDay,
                UtcMonth = this.UtcMonth,
                UtcYear = this.UtcYear,
                TimeOfDay = this.TimeOfDay,
                CrashDetection = this.CrashDetection,
                UnlimitedFuel = this.UnlimitedFuel,
                ElectricalMasterBattery = this.ElectricalMasterBattery,
                EngineCombustion1 = this.EngineCombustion1,
                EngineCombustion2 = this.EngineCombustion2,
                EngineCombustion3 = this.EngineCombustion3,
                EngineCombustion4 = this.EngineCombustion4,
                Pushback = this.Pushback,
                ApuGenerator = this.ApuGenerator,
                LightBeacon = this.LightBeacon,
                LightNav = this.LightNav,
                LightStrobe = this.LightStrobe,
                LightTaxi = this.LightTaxi,
                LightLanding = this.LightLanding,
                FlapsHandle = this.FlapsHandle,
                FlapsPercentage = this.FlapsPercentage,
                GearHandle = this.GearHandle,
                GearPercentage = this.GearPercentage,
                AutoPilot = this.AutoPilot,
                ParkingBrake = this.ParkingBrake,
                SpoilersArmed = this.SpoilersArmed,
                SeatBeltSign = this.SeatBeltSign,
                NoSmokingSign = this.NoSmokingSign
            };
        }
    }
}