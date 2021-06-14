// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.Process.ChangeOverTime.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Structs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Simconnect client - change over time tracking code.
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/03/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Contains the last 10 headings (recorded every 500ms) = 5s of recording.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<int> headingOverTimes = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Contains the last 30 vertical speed readings (recorded every 500ms) = 15s of recording.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly List<int> verticalSpeedOverTimes = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// True if the plane is turning (more then 3 degrees total in the last 5 seconds).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isTurning;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// When was the last set of change over time values recorded?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private DateTime lastChangeOverTimeRecording = DateTime.MinValue;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical profile.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private VerticalProfile verticalProfile = VerticalProfile.Level;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the plane is turning (more then 3 degrees total in the last 5 seconds).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsTurning
        {
            get => this.isTurning;

            private set
            {
                if (Equals(this.isTurning, value))
                {
                    return;
                }

                this.isTurning = value;
                this.OnPropertyChanged();
                Debug.WriteLine($"Is turning changed to {value}");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the vertical profile.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public VerticalProfile VerticalProfile
        {
            get => this.verticalProfile;

            private set
            {
                if (Equals(this.verticalProfile, value))
                {
                    return;
                }

                this.verticalProfile = value;
                this.OnPropertyChanged();
                Debug.WriteLine($"Vertical profile changed to {value}");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check for changes over time (like turning, climbing, etc.).
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/03/2021.
        /// </remarks>
        /// <param name="primary">
        /// The primary Simconnect tracking data.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void CheckChangesOverTime(PrimaryTracking primary)
        {
            // Is it time to check changes over time?
            if ((DateTime.UtcNow - this.lastChangeOverTimeRecording).TotalMilliseconds > 500)
            {
                this.headingOverTimes.Add((int)Math.Round(primary.Heading, 0));
                if (this.headingOverTimes.Count > 10)
                {
                    this.headingOverTimes.RemoveAt(0);
                }

                this.verticalSpeedOverTimes.Add((int)primary.VerticalSpeed);
                if (this.verticalSpeedOverTimes.Count > 30)
                {
                    this.verticalSpeedOverTimes.RemoveAt(0);
                }

                this.lastChangeOverTimeRecording = DateTime.UtcNow;

                // Check if the plane is turning (heading change > 3 degrees in the last 5 seconds total)
                var totalHeadingChange = 0;
                var lastHeadingValue = this.headingOverTimes[0];
                for (var i = 1; i < this.headingOverTimes.Count; i++)
                {
                    totalHeadingChange += lastHeadingValue - this.headingOverTimes[i];
                    lastHeadingValue = this.headingOverTimes[i];
                }

                this.IsTurning = Math.Abs(totalHeadingChange) > 3;

                // Check if the plane is turning off the runway
                if (this.taxiInStarted && Math.Abs(totalHeadingChange) > 30)
                {
                    Debug.WriteLine("Detected large enough turn to set taxiInTurned=true");
                    this.taxiInTurned = true;
                }

                // Check the vertical profile (vertical speed average above 300 feet/min change in the last 10 seconds)
                var verticalSpeedsAverage = (int)this.verticalSpeedOverTimes.Average();
                if (verticalSpeedsAverage > 300)
                {
                    this.VerticalProfile = VerticalProfile.Climbing;
                }
                else if (verticalSpeedsAverage < -300)
                {
                    this.VerticalProfile = VerticalProfile.Descending;
                }
                else
                {
                    this.VerticalProfile = VerticalProfile.Level;
                }
            }
        }
    }
}