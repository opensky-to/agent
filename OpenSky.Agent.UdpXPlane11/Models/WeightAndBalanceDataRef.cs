// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WeightAndBalanceDataRef.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane 11 dataref enabled version of weight and balance model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 07/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.Simulator.Models.WeightAndBalance"/>
    /// -------------------------------------------------------------------------------------------------
    public class WeightAndBalanceDataRef : Simulator.Models.WeightAndBalance
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel maximum weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private float fuelMaxWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private float fuelWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WeightAndBalanceDataRef"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public WeightAndBalanceDataRef()
        {
            // XPlane doesn't have a variables for the current CG
            this.CgPercentLateral = 0;
            this.CgPercent = 0;
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
        public Simulator.Models.WeightAndBalance Clone()
        {
            return new Simulator.Models.WeightAndBalance
            {
                EmptyWeight = this.EmptyWeight,
                TotalWeight = this.TotalWeight,
                MaxGrossWeight = this.MaxGrossWeight,
                FuelTotalCapacity = this.FuelTotalCapacity,
                FuelTotalQuantity = this.FuelTotalQuantity,
                FuelWeightPerGallon = this.FuelWeightPerGallon,
                CgAftLimit = this.CgAftLimit,
                CgFwdLimit = this.CgFwdLimit,
                CgPercent = this.CgPercent,
                CgPercentLateral = this.CgPercentLateral
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
            connector.Subscribe(DataRefs.AircraftWeightAcfMEmpty, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelWeightMTotal, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.AircraftWeightAcfMMax, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.AircraftWeightAcfMFuelTot, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelWeightMFuelTotal, 1000 / sampleRate, this.DataRefUpdated);
            var engineType = DataRefs.AircraftPropAcfEnType;
            engineType.DataRef += "[0]";
            connector.Subscribe(engineType, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.AircraftOverflowAcfCgzAft, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.AircraftOverflowAcfCgzFwd, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.Cockpit2GaugesIndicatorsCGIndicator, 1000 / sampleRate, this.DataRefUpdated);
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
            var updateFuelValues = false;
            if (element.DataRef == DataRefs.AircraftWeightAcfMEmpty.DataRef)
            {
                this.EmptyWeight = value * 2.205;
            }

            if (element.DataRef == DataRefs.FlightmodelWeightMTotal.DataRef)
            {
                this.TotalWeight = value * 2.205;
            }

            if (element.DataRef == DataRefs.AircraftWeightAcfMMax.DataRef)
            {
                this.MaxGrossWeight = value * 2.205;
            }

            if (element.DataRef == DataRefs.AircraftWeightAcfMFuelTot.DataRef)
            {
                this.fuelMaxWeight = value;
                updateFuelValues = true;
            }

            if (element.DataRef == DataRefs.FlightmodelWeightMFuelTotal.DataRef)
            {
                this.fuelWeight = value;
                updateFuelValues = true;
            }

            if (element.DataRef.StartsWith(DataRefs.AircraftPropAcfEnType.DataRef))
            {
                this.FuelWeightPerGallon = EngineType.GetFuelWeightForEngineType((int)value);
                updateFuelValues = true;
            }

            if (element.DataRef == DataRefs.AircraftOverflowAcfCgzAft.DataRef)
            {
                this.CgAftLimit = value;
            }

            if (element.DataRef == DataRefs.AircraftOverflowAcfCgzFwd.DataRef)
            {
                this.CgFwdLimit = value;
            }

            if (updateFuelValues)
            {
                this.FuelTotalCapacity = (this.fuelMaxWeight * 2.205) / this.FuelWeightPerGallon;
                this.FuelTotalQuantity = (this.fuelWeight * 2.205) / this.FuelWeightPerGallon;
            }
        }
    }
}