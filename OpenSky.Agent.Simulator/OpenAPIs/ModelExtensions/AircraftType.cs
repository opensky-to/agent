// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftType.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System;
    using System.Collections.Generic;

    using TomsToolbox.Essentials;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type model - local extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [Serializable]
    public partial class AircraftType
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftType"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftType"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="copyFrom">
        /// The other type to copy from.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType(AircraftType copyFrom)
        {
            this.AtcModel = copyFrom.AtcModel;
            this.AtcType = copyFrom.AtcType;
            this.Category = copyFrom.Category;
            this.Comments = copyFrom.Comments;
            this.DetailedChecksDisabled = copyFrom.DetailedChecksDisabled;
            this.EmptyWeight = copyFrom.EmptyWeight;
            this.Enabled = copyFrom.Enabled;
            this.EngineCount = copyFrom.EngineCount;
            this.EngineType = copyFrom.EngineType;
            this.FlapsAvailable = copyFrom.FlapsAvailable;
            this.FuelTotalCapacity = copyFrom.FuelTotalCapacity;
            this.FuelWeightPerGallon = copyFrom.FuelWeightPerGallon;
            this.Id = copyFrom.Id;
            this.IsGearRetractable = copyFrom.IsGearRetractable;
            this.IsVanilla = copyFrom.IsVanilla;
            this.IncludeInWorldPopulation = copyFrom.IncludeInWorldPopulation;
            this.IsVariantOf = copyFrom.IsVariantOf;
            this.LastEditedByID = copyFrom.LastEditedByID;
            this.LastEditedByName = copyFrom.LastEditedByName;
            this.MaxGrossWeight = copyFrom.MaxGrossWeight;
            this.MinimumRunwayLength = copyFrom.MinimumRunwayLength;
            this.MaxPrice = copyFrom.MaxPrice;
            this.MinPrice = copyFrom.MinPrice;
            this.Name = copyFrom.Name;
            this.ManufacturerID = copyFrom.ManufacturerID;
            this.Manufacturer = copyFrom.Manufacturer;
            this.NeedsCoPilot = copyFrom.NeedsCoPilot;
            this.NeedsFlightEngineer = copyFrom.NeedsFlightEngineer;
            this.RequiresManualFuelling = copyFrom.RequiresManualFuelling;
            this.RequiresManualLoading = copyFrom.RequiresManualLoading;
            this.NextVersion = copyFrom.NextVersion;
            this.Simulator = copyFrom.Simulator;
            this.UploaderID = copyFrom.UploaderID;
            this.UploaderName = copyFrom.UploaderName;
            this.VersionNumber = copyFrom.VersionNumber;
            this.MaxPayloadDeltaAllowed = copyFrom.MaxPayloadDeltaAllowed;
            this.HasAircraftImage = copyFrom.HasAircraftImage;
            this.EngineModel = copyFrom.EngineModel;
            this.OverrideFuelType = copyFrom.OverrideFuelType;
            this.IsHistoric = copyFrom.IsHistoric;
            this.DeliveryLocations = new List<AircraftManufacturerDeliveryLocation>();
            this.DeliveryLocations.AddRange(copyFrom.DeliveryLocations);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the manufacturer delivery location ICAO(s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ManufacturerDeliveryLocationICAOs
        {
            get
            {
                var icaos = string.Empty;
                if (this.DeliveryLocations != null)
                {
                    foreach (var deliveryLocation in this.DeliveryLocations)
                    {
                        icaos += $"{deliveryLocation.AirportICAO},";
                    }
                }

                return icaos.TrimEnd(',');
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((AircraftType)obj);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        /// -------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return this.Id.GetHashCode();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if this aircraft type matches the aircraft currently loaded in the simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <returns>
        /// True if it matches the aircraft in the simulator, false if not.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public bool MatchesAircraftInSimulator()
        {
            if (OpenSky.Agent.Simulator.Simulator.Instance.Connected)
            {
                if (!string.Equals(!string.IsNullOrEmpty(OpenSky.Agent.Simulator.Simulator.Instance.AircraftIdentity.AtcType) ? OpenSky.Agent.Simulator.Simulator.Instance.AircraftIdentity.AtcType : "MISSING", this.AtcType, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                if (!string.Equals(!string.IsNullOrEmpty(OpenSky.Agent.Simulator.Simulator.Instance.AircraftIdentity.AtcModel) ? OpenSky.Agent.Simulator.Simulator.Instance.AircraftIdentity.AtcModel : "MISSING", this.AtcModel, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                if (OpenSky.Agent.Simulator.Simulator.Instance.AircraftIdentity.EngineType != this.EngineType)
                {
                    return false;
                }

                if (OpenSky.Agent.Simulator.Simulator.Instance.AircraftIdentity.EngineCount != this.EngineCount)
                {
                    return false;
                }

                if (OpenSky.Agent.Simulator.Simulator.Instance.AircraftIdentity.FlapsAvailable != this.FlapsAvailable)
                {
                    return false;
                }

                if (OpenSky.Agent.Simulator.Simulator.Instance.AircraftIdentity.GearRetractable != this.IsGearRetractable)
                {
                    return false;
                }

                // Skip these checks if detailed checks are TEMPORARILY disabled
                if (!this.DetailedChecksDisabled)
                {
                    if (Math.Abs(OpenSky.Agent.Simulator.Simulator.Instance.WeightAndBalance.EmptyWeight - this.EmptyWeight) > 0.5)
                    {
                        return false;
                    }

                    if (Math.Abs(OpenSky.Agent.Simulator.Simulator.Instance.WeightAndBalance.FuelTotalCapacity - this.FuelTotalCapacity) > 0.5)
                    {
                        return false;
                    }

                    if (Math.Abs(OpenSky.Agent.Simulator.Simulator.Instance.WeightAndBalance.MaxGrossWeight - this.MaxGrossWeight) > 0.5)
                    {
                        return false;
                    }
                }

                // Passed all checks
                return true;
            }

            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return $"{this.Name} [v{this.VersionNumber}]";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// <param name="other">
        /// The aircraft type to compare to this object.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected bool Equals(AircraftType other)
        {
            return this.Id.Equals(other.Id);
        }
    }
}