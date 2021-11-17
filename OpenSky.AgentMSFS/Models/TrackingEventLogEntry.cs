// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingEventLogEntry.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Models
{
    using System;
    using System.Windows.Media;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.Tools;
    using OpenSky.FlightLogXML;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky tracking event.
    /// </summary>
    /// <remarks>
    /// sushi.at, 16/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class TrackingEventLogEntry : FlightLogXML.TrackingEventLogEntry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventLogEntry"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 16/03/2021.
        /// </remarks>
        /// <param name="type">
        /// The tracking event type.
        /// </param>
        /// <param name="eventTime">
        /// Gets the date/time of the event.
        /// </param>
        /// <param name="eventColor">
        /// The event color.
        /// </param>
        /// <param name="logMessage">
        /// Gets the log message for the flight events log.
        /// </param>
        /// <param name="location">
        /// Gets the location of the event.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventLogEntry(FlightTrackingEventType type, DateTime eventTime, Color eventColor, string logMessage, Location location)
        {
            this.EventType = type;
            this.EventTime = eventTime;

            this.Latitude = location.Latitude;
            this.Longitude = location.Longitude;
            this.Altitude = (int)location.Altitude;

            this.EventColor = eventColor.ToDrawingColor();
            this.LogMessage = logMessage;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventLogEntry"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="entry">
        /// The entry, restored from a flight log xml file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventLogEntry(FlightLogXML.TrackingEventLogEntry entry)
        {
            this.EventType = entry.EventType;
            this.EventTime = entry.EventTime;

            this.Latitude = entry.Latitude;
            this.Longitude = entry.Longitude;
            this.Altitude = entry.Altitude;

            this.EventColor = entry.EventColor;
            this.LogMessage = entry.LogMessage;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the color brush of the event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Brush EventColorBrush => new SolidColorBrush(this.EventColor.ToMediaColor());

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Location Location => new(this.Latitude, this.Longitude, this.Altitude);
    }
}