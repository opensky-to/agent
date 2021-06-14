// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingEventLogEntry.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Models
{
    using System;
    using System.Globalization;
    using System.Windows.Media;
    using System.Xml.Linq;

    using JetBrains.Annotations;

    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky tracking event.
    /// </summary>
    /// <remarks>
    /// sushi.at, 16/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class TrackingEventLogEntry
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventLogEntry"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 16/03/2021.
        /// </remarks>
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
        public TrackingEventLogEntry(DateTime eventTime, Color eventColor, string logMessage, Location location)
        {
            this.EventTime = eventTime;
            this.Location = location;
            this.EventColor = eventColor;
            this.EventColorBrush = new SolidColorBrush(eventColor);
            this.LogMessage = logMessage;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventLogEntry"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <param name="entryFromSave">
        /// The entry from a save file to restore.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public TrackingEventLogEntry(XElement entryFromSave)
        {
            this.EventTime = DateTime.ParseExact(entryFromSave.Attribute("Time")?.Value, "O", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var lat = double.Parse(entryFromSave.Attribute("Lat")?.Value ?? "missing");
            var lon = double.Parse(entryFromSave.Attribute("Lon")?.Value ?? "missing");
            var alt = double.Parse(entryFromSave.Attribute("Alt")?.Value ?? "missing");
            this.Location = new Location(lat, lon, alt);
            var color = ColorConverter.ConvertFromString(entryFromSave.Attribute("Color")?.Value ?? "missing") as Color? ?? Colors.Red;
            this.EventColor = color;
            this.EventColorBrush = new SolidColorBrush(color);
            this.LogMessage = entryFromSave.Attribute("Message")?.Value ?? "missing";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the color of the event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Color EventColor { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the color brush of the event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Brush EventColorBrush { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the date/time of the event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime EventTime { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the location of the event.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public Location Location { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the log message for the flight events log.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotNull]
        public string LogMessage { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets log entry for saving in a file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <returns>
        /// The log entry for saving.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public XElement GetLogEntryForSave()
        {
            var entryElement = new XElement("LogEntry");
            entryElement.SetAttributeValue("Time", $"{this.EventTime:O}");
            entryElement.SetAttributeValue("Lat", $"{this.Location.Latitude}");
            entryElement.SetAttributeValue("Lon", $"{this.Location.Longitude}");
            entryElement.SetAttributeValue("Alt", $"{this.Location.Altitude:F0}");
            entryElement.SetAttributeValue("Color", $"{this.EventColor}");
            entryElement.SetAttributeValue("Message", $"{this.LogMessage}");
            return entryElement;
        }

        // todo define some kind of event ID enum that we share with the API, so that these events make sense there too (no log message parsing!)
    }
}