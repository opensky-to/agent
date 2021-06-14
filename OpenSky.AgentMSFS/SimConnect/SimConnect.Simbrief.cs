// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Simbrief.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simconnect client - simbrief flight plan import code.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The simbrief waypoint markers.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<SimbriefWaypointMarker> simbriefWaypointMarkers = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if simbrief ofp was loaded for this flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool simbriefOfpLoaded;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when SimConnect adds a new simbrief waypoint marker.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event EventHandler<SimbriefWaypointMarker> SimbriefWaypointMarkerAdded;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether a simbrief ofp was loaded for the current flight.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool SimbriefOfpLoaded
        {
            get => this.simbriefOfpLoaded;

            private set
            {
                if (Equals(this.simbriefOfpLoaded, value))
                {
                    return;
                }

                this.simbriefOfpLoaded = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Simbrief route location collection to draw a poly line on the map.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LocationCollection SimbriefRouteLocations { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Import simbrief flight plan navlog fixes.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// <param name="ofp">
        /// The ofp.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void ImportSimbrief(XElement ofp)
        {
            if (this.SimbriefOfpLoaded)
            {
                return;
            }

            Debug.WriteLine("SimConnect is importing simbrief flight plan");

            // Departure airport is not part of the navlog so add a position for the polyline for it
            var originLat = double.Parse((string)ofp.Element("origin").Element("pos_lat"));
            var originLon = double.Parse((string)ofp.Element("origin").Element("pos_long"));
            UpdateGUIDelegate addOriginLocation = () => this.SimbriefRouteLocations.Add(new Location(originLat, originLon));
            Application.Current.Dispatcher.Invoke(addOriginLocation);

            var fixes = ofp.Element("navlog").Elements("fix");
            foreach (var fix in fixes)
            {
                var ident = (string)fix.Element("ident");
                var latitude = double.Parse((string)fix.Element("pos_lat"));
                var longitude = double.Parse((string)fix.Element("pos_long"));
                var type = (string)fix.Element("type");

                UpdateGUIDelegate addLocation = () =>
                {
                    this.SimbriefRouteLocations.Add(new Location(latitude, longitude));
                    if (type != "apt")
                    {
                        Debug.WriteLine($"SimConnect creating simbrief waypoint marker {ident}");
                        var newMarker = new SimbriefWaypointMarker(latitude, longitude, ident, type);
                        this.simbriefWaypointMarkers.Add(newMarker);
                        this.SimbriefWaypointMarkerAdded?.Invoke(this, newMarker);
                    }
                };
                Application.Current.Dispatcher.BeginInvoke(addLocation);
            }

            this.SimbriefOfpLoaded = true;
        }
    }
}