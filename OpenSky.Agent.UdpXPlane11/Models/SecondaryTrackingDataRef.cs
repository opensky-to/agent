// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecondaryTrackingDataRef.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using System;
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
        /// Number of engines.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private int numberOfEngines;

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
        /// Makes a copy of this object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/02/2022.
        /// </remarks>
        /// <returns>
        /// A copy of this object.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
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
                ApuRunning = this.ApuRunning,
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
            connector.Subscribe(DataRefs.AircraftEngineAcfNumEngines, 1000 / sampleRate, this.DataRefUpdated);
            var engine1Running = DataRefs.FlightmodelEngineENGNRunning;
            engine1Running.DataRef += "[0]";
            connector.Subscribe(engine1Running, 1000 / sampleRate, this.DataRefUpdated);
            var engine2Running = DataRefs.FlightmodelEngineENGNRunning;
            engine2Running.DataRef += "[1]";
            connector.Subscribe(engine2Running, 1000 / sampleRate, this.DataRefUpdated);
            var engine3Running = DataRefs.FlightmodelEngineENGNRunning;
            engine3Running.DataRef += "[2]";
            connector.Subscribe(engine3Running, 1000 / sampleRate, this.DataRefUpdated);
            var engine4Running = DataRefs.FlightmodelEngineENGNRunning;
            engine4Running.DataRef += "[3]";
            connector.Subscribe(engine4Running, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.GraphicsAnimationGroundTrafficTowbarHeadingDeg, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Cockpit2ElectricalAPURunning, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalBeaconLightsOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalNavLightsOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalStrobeLightsOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalTaxiLightOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitElectricalLandingLightsOn, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelControlsFlaprqst, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelControlsFlaprat, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitSwitchesGearHandleStatus, 1000 / sampleRate, this.DataRefUpdated);
            var gearDeploy = DataRefs.Flightmodel2GearDeployRatio;
            gearDeploy.DataRef += "[0]";
            connector.Subscribe(gearDeploy, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitAutopilotAutopilotMode, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelControlsParkbrake, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelControlsSbrkrqst, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitSwitchesFastenSeatBelts, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.CockpitSwitchesNoSmoking, 1000 / sampleRate, this.DataRefUpdated);

            for (var i = 0; i < 40; i++)
            {
                var tailNum = DataRefs.AircraftViewAcfTailnum;
                tailNum.DataRef += $"[{i}]";
                connector.Subscribe(tailNum, 1000 / sampleRate, this.DataRefUpdated);
            }
        }

        private readonly char[] tailNumber = new char[40];

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
            if (element.DataRef.StartsWith(DataRefs.AircraftViewAcfTailnum.DataRef))
            {
                if (element.DataRef.Contains("[") && element.DataRef.EndsWith("]"))
                {
                    var indexString = element.DataRef.Split('[')[1].Replace("]", string.Empty);
                    if (int.TryParse(indexString, out var index) && index is >= 0 and < 40)
                    {
                        this.tailNumber[index] = (char)value;
                        var tailString = new string(this.tailNumber).Replace("\0", string.Empty);
                        Debug.WriteLine($"Registry: {tailString}");
                    }
                }
            }

            if (element.DataRef == DataRefs.TimeZuluTimeSec.DataRef)
            {
                this.UtcTime = value;
            }

            if (element.DataRef == DataRefs.Cockpit2ClockTimerCurrentDay.DataRef)
            {
                this.UtcDay = (int)value;
            }

            if (element.DataRef == DataRefs.Cockpit2ClockTimerCurrentMonth.DataRef)
            {
                this.UtcMonth = (int)value;
            }

            if (element.DataRef == DataRefs.CockpitElectricalBatteryOn.DataRef)
            {
                this.ElectricalMasterBattery = (int)value == 1;
            }

            if (element.DataRef == DataRefs.AircraftEngineAcfNumEngines.DataRef)
            {
                this.numberOfEngines = (int)value;
                if (this.numberOfEngines < 1)
                {
                    this.EngineCombustion1 = false;
                }

                if (this.numberOfEngines < 2)
                {
                    this.EngineCombustion2 = false;
                }

                if (this.numberOfEngines < 3)
                {
                    this.EngineCombustion3 = false;
                }

                if (this.numberOfEngines < 4)
                {
                    this.EngineCombustion4 = false;
                }
            }

            if (element.DataRef.StartsWith(DataRefs.FlightmodelEngineENGNRunning.DataRef))
            {
                if (element.DataRef.EndsWith("[0]"))
                {
                    this.EngineCombustion1 = (int)value == 1 && this.numberOfEngines >= 1;
                }

                if (element.DataRef.EndsWith("[1]"))
                {
                    this.EngineCombustion2 = (int)value == 1 && this.numberOfEngines >= 2;
                }

                if (element.DataRef.EndsWith("[2]"))
                {
                    this.EngineCombustion3 = (int)value == 1 && this.numberOfEngines >= 3;
                }

                if (element.DataRef.EndsWith("[3]"))
                {
                    this.EngineCombustion4 = (int)value == 1 && this.numberOfEngines >= 4;
                }
            }

            if (element.DataRef == DataRefs.GraphicsAnimationGroundTrafficTowbarHeadingDeg.DataRef)
            {
                this.Pushback = value != 0 ? Pushback.Straight : Pushback.NoPushback; // todo check
            }

            if (element.DataRef == DataRefs.Cockpit2ElectricalAPURunning.DataRef)
            {
                this.ApuRunning = (int)value == 1;
            }

            if (element.DataRef == DataRefs.CockpitElectricalBeaconLightsOn.DataRef)
            {
                this.LightBeacon = (int)value == 1;
            }

            if (element.DataRef == DataRefs.CockpitElectricalNavLightsOn.DataRef)
            {
                this.LightNav = (int)value == 1;
            }

            if (element.DataRef == DataRefs.CockpitElectricalStrobeLightsOn.DataRef)
            {
                this.LightStrobe = (int)value == 1;
            }

            if (element.DataRef == DataRefs.CockpitElectricalTaxiLightOn.DataRef)
            {
                this.LightTaxi = (int)value == 1;
            }

            if (element.DataRef == DataRefs.CockpitElectricalLandingLightsOn.DataRef)
            {
                this.LightLanding = (int)value == 1;
            }

            if (element.DataRef == DataRefs.FlightmodelControlsFlaprqst.DataRef)
            {
                this.FlapsHandle = value * 100;
            }

            if (element.DataRef == DataRefs.FlightmodelControlsFlaprat.DataRef)
            {
                this.FlapsPercentage = value * 100;
            }

            if (element.DataRef == DataRefs.CockpitSwitchesGearHandleStatus.DataRef)
            {
                this.GearHandle = (int)value == 1;
            }

            if (element.DataRef.StartsWith(DataRefs.Flightmodel2GearDeployRatio.DataRef))
            {
                this.GearPercentage = value;
            }

            if (element.DataRef == DataRefs.CockpitAutopilotAutopilotMode.DataRef)
            {
                this.AutoPilot = (int)value == 2;
            }

            if (element.DataRef == DataRefs.FlightmodelControlsParkbrake.DataRef)
            {
                this.ParkingBrake = value > 0.1;
            }

            if (element.DataRef == DataRefs.FlightmodelControlsSbrkrqst.DataRef)
            {
                this.SpoilersArmed = value < 0;
            }

            if (element.DataRef == DataRefs.CockpitSwitchesFastenSeatBelts.DataRef)
            {
                this.SeatBeltSign = (int)value == 1;
            }

            if (element.DataRef == DataRefs.CockpitSwitchesNoSmoking.DataRef)
            {
                this.NoSmokingSign = (int)value == 1;
            }
        }
    }
}