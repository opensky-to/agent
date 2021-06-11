// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftType.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    using OpenSky.AgentMSFS.SimConnect;

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

        public bool MatchesAircraftInSimulator()
        {
            if (SimConnect.Instance.Connected)
            {
            }

            return false;
        }

        public AircraftType()
        {

        }

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
            this.Id = copyFrom.Id;
            this.IsGearRetractable = copyFrom.IsGearRetractable;
            this.IsVanilla = copyFrom.IsVanilla;
            this.IsVariantOf = copyFrom.IsVariantOf;
            this.LastEditedByID = copyFrom.LastEditedByID;
            this.LastEditedByName = copyFrom.LastEditedByName;
            this.MaxGrossWeight = copyFrom.MaxGrossWeight;
            this.MaxPrice = copyFrom.MaxPrice;
            this.MinPrice = copyFrom.MinPrice;
            this.Name = copyFrom.Name;
            this.NeedsCoPilot = copyFrom.NeedsCoPilot;
            this.NextVersion = copyFrom.NextVersion;
            this.Simulator = copyFrom.Simulator;
            this.UploaderID = copyFrom.UploaderID;
            this.UploaderName = copyFrom.UploaderName;
            this.VersionNumber = copyFrom.VersionNumber;
        }
    }
}