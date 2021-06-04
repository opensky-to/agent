// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftType.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
{
    using OpenSky.AgentMSFS.SimConnect;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type model - local extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
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
    }
}