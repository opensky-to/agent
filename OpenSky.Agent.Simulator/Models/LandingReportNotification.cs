// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingReportNotification.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// A landing report notification.
    /// </summary>
    /// <remarks>
    /// sushi.at, 30/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class LandingReportNotification
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// After turning the engines off.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly LandingReportNotification AfterTurningEnginesOff = new(1, "After turning engines off");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// As soon as possible.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly LandingReportNotification AsSoonAsPossible = new(0, "As soon as available");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The notification is disabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static readonly LandingReportNotification Disable = new(2, "Disable");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LandingReportNotification"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// <param name="id">
        /// The identifier.
        /// </param>
        /// <param name="setting">
        /// The setting.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private LandingReportNotification(uint id, string setting)
        {
            this.NotificationID = id;
            this.NotificationSetting = setting;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the identifier of the notification.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public uint NotificationID { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the notification setting.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public string NotificationSetting { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets landing report notification options.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// <returns>
        /// The landing report notifications.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static List<LandingReportNotification> GetLandingReportNotifications()
        {
            return new()
            {
                AsSoonAsPossible,
                AfterTurningEnginesOff,
                Disable
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Parses the given ID.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// <param name="id">
        /// The identifier.
        /// </param>
        /// <returns>
        /// The matching LandingReportNotification. This may be null.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public static LandingReportNotification Parse(uint id)
        {
            return GetLandingReportNotifications().SingleOrDefault(n => n.NotificationID == id);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
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

            return this.Equals((LandingReportNotification)obj);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        /// -------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return this.NotificationID.GetHashCode();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        /// -------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return this.NotificationSetting;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/03/2021.
        /// </remarks>
        /// <param name="other">
        /// The landing report notification to compare to this object. This cannot be null.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected bool Equals([NotNull] LandingReportNotification other)
        {
            return this.NotificationID == other.NotificationID;
        }
    }
}