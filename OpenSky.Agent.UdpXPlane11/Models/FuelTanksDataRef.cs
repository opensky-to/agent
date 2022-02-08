// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelTanksDataRef.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane 11 dataref enabled version of fuel tanks model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.Simulator.Models.FuelTanks"/>
    /// -------------------------------------------------------------------------------------------------
    public class FuelTanksDataRef : Agent.Simulator.Models.FuelTanks
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) The current fuel weight for each tank.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly float[] tankFuelWeight = new float[9];

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) The fuel tank rations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly float[] tankRatios = new float[9];

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel maximum weight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private float fuelMaxWeight;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel weight lbs/gal.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double fuelWeight;

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
        public Simulator.Models.FuelTanks Clone()
        {
            return new Simulator.Models.FuelTanks
            {
                // Omitted external tanks since XPlane only supports 9 tanks
                FuelTankCenterCapacity = this.FuelTankCenterCapacity,
                FuelTankCenter2Capacity = this.FuelTankCenter2Capacity,
                FuelTankCenter3Capacity = this.FuelTankCenter3Capacity,
                FuelTankLeftMainCapacity = this.FuelTankLeftMainCapacity,
                FuelTankLeftAuxCapacity = this.FuelTankLeftAuxCapacity,
                FuelTankLeftTipCapacity = this.FuelTankLeftTipCapacity,
                FuelTankRightMainCapacity = this.FuelTankRightMainCapacity,
                FuelTankRightAuxCapacity = this.FuelTankRightAuxCapacity,
                FuelTankRightTipCapacity = this.FuelTankRightTipCapacity,

                FuelTankCenterQuantity = this.FuelTankCenterQuantity,
                FuelTankCenter2Quantity = this.FuelTankCenter2Quantity,
                FuelTankCenter3Quantity = this.FuelTankCenter3Quantity,
                FuelTankLeftMainQuantity = this.FuelTankLeftMainQuantity,
                FuelTankLeftAuxQuantity = this.FuelTankLeftAuxQuantity,
                FuelTankLeftTipQuantity = this.FuelTankLeftTipQuantity,
                FuelTankRightMainQuantity = this.FuelTankRightMainQuantity,
                FuelTankRightAuxQuantity = this.FuelTankRightAuxQuantity,
                FuelTankRightTipQuantity = this.FuelTankRightTipQuantity,
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
            for (var i = 0; i < 9; i++)
            {
                var tankRatio = DataRefs.AircraftOverflowAcfTankRat;
                tankRatio.DataRef += $"[{i}]";
                connector.Subscribe(tankRatio, 1000 / sampleRate, this.DataRefUpdated);

                var tankWeight = DataRefs.FlightmodelWeightMFuel;
                tankWeight.DataRef += $"[{i}]";
                connector.Subscribe(tankWeight, 1000 / sampleRate, this.DataRefUpdated);
            }

            connector.Subscribe(DataRefs.AircraftWeightAcfMFuelTot, 1000 / sampleRate, this.DataRefUpdated);
            var engineType = DataRefs.AircraftPropAcfEnType;
            engineType.DataRef += "[0]";
            connector.Subscribe(engineType, 1000 / sampleRate, this.DataRefUpdated);
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
            var updateCapacities = false;
            var updateQuantities = false;
            if (element.DataRef.StartsWith(DataRefs.AircraftOverflowAcfTankRat.DataRef))
            {
                if (element.DataRef.Contains("[") && element.DataRef.EndsWith("]"))
                {
                    var indexString = element.DataRef.Split('[')[1].Replace("]", string.Empty);
                    if (int.TryParse(indexString, out var index) && index is >= 0 and < 9)
                    {
                        this.tankRatios[index] = value;
                        updateCapacities = true;
                    }
                }
            }

            if (element.DataRef.StartsWith(DataRefs.FlightmodelWeightMFuel.DataRef))
            {
                if (element.DataRef.Contains("[") && element.DataRef.EndsWith("]"))
                {
                    var indexString = element.DataRef.Split('[')[1].Replace("]", string.Empty);
                    if (int.TryParse(indexString, out var index) && index is >= 0 and < 9)
                    {
                        this.tankFuelWeight[index] = value;
                        updateQuantities = true;
                    }
                }
            }

            if (element.DataRef == DataRefs.AircraftWeightAcfMFuelTot.DataRef)
            {
                this.fuelMaxWeight = value;
                updateCapacities = true;
            }

            if (element.DataRef.StartsWith(DataRefs.AircraftPropAcfEnType.DataRef))
            {
                this.fuelWeight = (int)value switch
                {
                    0 => 6,
                    1 => 6,
                    2 => 6.7,
                    3 => 0, // Electric engine
                    4 => 6.7,
                    5 => 6.7,
                    6 => 0, // Rocket
                    7 => 0, // Tip rockets
                    8 => 6.7,
                    _ => 0
                };
                updateCapacities = true;
                updateQuantities = true;
            }

            if (updateCapacities)
            {
                // todo need to check with larger plane if these tank index assignments make sense
                var totalCapacityGallons = (this.fuelMaxWeight * 2.205) / this.fuelWeight;
                this.FuelTankLeftMainCapacity = this.tankRatios[0] * totalCapacityGallons;
                this.FuelTankRightMainCapacity = this.tankRatios[1] * totalCapacityGallons;
                this.FuelTankLeftTipCapacity = this.tankRatios[2] * totalCapacityGallons;
                this.FuelTankRightTipCapacity = this.tankRatios[3] * totalCapacityGallons;
                this.FuelTankLeftAuxCapacity = this.tankRatios[4] * totalCapacityGallons;
                this.FuelTankRightAuxCapacity = this.tankRatios[5] * totalCapacityGallons;
                this.FuelTankCenterCapacity = this.tankRatios[6] * totalCapacityGallons;
                this.FuelTankCenter2Capacity = this.tankRatios[7] * totalCapacityGallons;
                this.FuelTankCenter3Capacity = this.tankRatios[8] * totalCapacityGallons;
            }

            if (updateQuantities)
            {
                // todo need to check with larger plane if these tank index assignments make sense
                this.FuelTankLeftMainQuantity = this.tankFuelWeight[0] * 2.205 / this.fuelWeight;
                this.FuelTankRightMainQuantity = this.tankFuelWeight[1] * 2.205 / this.fuelWeight;
                this.FuelTankLeftTipQuantity = this.tankFuelWeight[2] * 2.205 / this.fuelWeight;
                this.FuelTankRightTipQuantity = this.tankFuelWeight[3] * 2.205 / this.fuelWeight;
                this.FuelTankLeftAuxQuantity = this.tankFuelWeight[4] * 2.205 / this.fuelWeight;
                this.FuelTankRightAuxQuantity = this.tankFuelWeight[5] * 2.205 / this.fuelWeight;
                this.FuelTankCenterQuantity = this.tankFuelWeight[6] * 2.205 / this.fuelWeight;
                this.FuelTankCenter2Quantity = this.tankFuelWeight[7] * 2.205 / this.fuelWeight;
                this.FuelTankCenter3Quantity = this.tankFuelWeight[8] * 2.205 / this.fuelWeight;
            }
        }
    }
}