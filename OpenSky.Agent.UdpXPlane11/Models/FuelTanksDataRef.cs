// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FuelTanksDataRef.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OpenSky.Agent.Simulator.Enums;

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
        /// The tank indices.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        internal Dictionary<FuelTank, int> tankIndices = new();

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
        private readonly float[] tankRatios = { -9999, -9999, -9999, -9999, -9999, -9999, -9999, -9999, -9999, };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) the fuel tank X-axis coordinates.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly float[] tankX = { -9999, -9999, -9999, -9999, -9999, -9999, -9999, -9999, -9999 };

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

                var tankXRef = DataRefs.AircraftOverflowAcfTankX;
                tankXRef.DataRef += $"[{i}]";
                connector.Subscribe(tankXRef, 1000 / sampleRate, this.DataRefUpdated);
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
            var updateIndices = false;
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

            if (element.DataRef.StartsWith(DataRefs.AircraftOverflowAcfTankX.DataRef))
            {
                if (element.DataRef.Contains("[") && element.DataRef.EndsWith("]"))
                {
                    var indexString = element.DataRef.Split('[')[1].Replace("]", string.Empty);
                    if (int.TryParse(indexString, out var index) && index is >= 0 and < 9)
                    {
                        this.tankX[index] = value;
                        updateIndices = true;
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

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if ((updateIndices || updateCapacities) && this.tankX.All(t => t != -9999) && this.tankRatios.All(t => t != -9999))
            {
                this.tankIndices.Clear();
                var leftTanks = new List<TankIndexWithRatio>();
                var rightTanks = new List<TankIndexWithRatio>();
                for (var i = 0; i < 9; i++)
                {
                    // Is the tank used?
                    if (this.tankRatios[i] == 0)
                    {
                        continue;
                    }

                    // A center tank
                    if (this.tankX[i] < 0.5 && this.tankX[i] > -0.5)
                    {
                        if (!this.tankIndices.ContainsKey(FuelTank.Center))
                        {
                            this.tankIndices.Add(FuelTank.Center, i);
                            continue;
                        }

                        if (!this.tankIndices.ContainsKey(FuelTank.Center2))
                        {
                            this.tankIndices.Add(FuelTank.Center2, i);
                            continue;
                        }

                        if (!this.tankIndices.ContainsKey(FuelTank.Center3))
                        {
                            this.tankIndices.Add(FuelTank.Center3, i);
                            continue;
                        }

                        throw new Exception("Can't support more than 3 center tanks!");
                    }

                    // A right side tank
                    if (this.tankX[i] >= 0.5)
                    {
                        rightTanks.Add(new TankIndexWithRatio { Index = i, Ratio = this.tankRatios[i] });
                        continue;
                    }

                    // A left side tank
                    {
                        leftTanks.Add(new TankIndexWithRatio { Index = i, Ratio = this.tankRatios[i] });
                    }
                }

                // Add left and right tanks in order of their size
                foreach (var tankIndexWithRatio in leftTanks.OrderByDescending(t => t.Ratio))
                {
                    if (!this.tankIndices.ContainsKey(FuelTank.LeftMain))
                    {
                        this.tankIndices.Add(FuelTank.LeftMain, tankIndexWithRatio.Index);
                        continue;
                    }

                    if (!this.tankIndices.ContainsKey(FuelTank.LeftTip))
                    {
                        this.tankIndices.Add(FuelTank.LeftTip, tankIndexWithRatio.Index);
                        continue;
                    }

                    if (!this.tankIndices.ContainsKey(FuelTank.LeftAux))
                    {
                        this.tankIndices.Add(FuelTank.LeftAux, tankIndexWithRatio.Index);
                        continue;
                    }

                    if (!this.tankIndices.ContainsKey(FuelTank.External1))
                    {
                        this.tankIndices.Add(FuelTank.External1, tankIndexWithRatio.Index);
                        continue;
                    }

                    throw new Exception("Can't support more than 4 left side tanks!");
                }

                foreach (var tankIndexWithRatio in rightTanks.OrderByDescending(t => t.Ratio))
                {
                    if (!this.tankIndices.ContainsKey(FuelTank.RightMain))
                    {
                        this.tankIndices.Add(FuelTank.RightMain, tankIndexWithRatio.Index);
                        continue;
                    }

                    if (!this.tankIndices.ContainsKey(FuelTank.RightTip))
                    {
                        this.tankIndices.Add(FuelTank.RightTip, tankIndexWithRatio.Index);
                        continue;
                    }

                    if (!this.tankIndices.ContainsKey(FuelTank.RightAux))
                    {
                        this.tankIndices.Add(FuelTank.RightAux, tankIndexWithRatio.Index);
                        continue;
                    }

                    if (!this.tankIndices.ContainsKey(FuelTank.External2))
                    {
                        this.tankIndices.Add(FuelTank.External2, tankIndexWithRatio.Index);
                        continue;
                    }

                    throw new Exception("Can't support more than 4 right side tanks!");
                }
            }

            if (updateCapacities)
            {
                var capacities = new Dictionary<FuelTank, double>();
                foreach (FuelTank tank in Enum.GetValues(typeof(FuelTank)))
                {
                    capacities.Add(tank, 0);
                }

                var totalCapacityGallons = (this.fuelMaxWeight * 2.205) / this.fuelWeight;
                foreach (var tankIndex in this.tankIndices)
                {
                    capacities[tankIndex.Key] = this.tankRatios[tankIndex.Value] * totalCapacityGallons;
                }

                this.UpdateCapactiesFromDictionary(capacities);
            }

            if (updateQuantities)
            {
                var quantities = new Dictionary<FuelTank, double>();
                foreach (FuelTank tank in Enum.GetValues(typeof(FuelTank)))
                {
                    quantities.Add(tank, 0);
                }

                foreach (var tankIndex in this.tankIndices)
                {
                    quantities[tankIndex.Key] = this.tankFuelWeight[tankIndex.Value] * 2.205 / this.fuelWeight;
                }

                this.UpdateQuantitiesFromDictionary(quantities);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A tank index with ratio (for sorting).
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private class TankIndexWithRatio
        {
            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets the zero-based index of the fuel tank.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public int Index { get; set; }

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets the ratio.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public float Ratio { get; set; }
        }
    }
}