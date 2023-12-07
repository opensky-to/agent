// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftIdentityDataRef.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane 11 dataref enabled version of aircraft identity model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.Simulator.Models.AircraftIdentity"/>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftIdentityDataRef : Agent.Simulator.Models.AircraftIdentity
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) The aircraft ICAO character array.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly char[] acfICAO = new char[40];

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// (Immutable) The aircraft description character array.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly char[] acfDescription = new char[50];

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftIdentityDataRef"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftIdentityDataRef()
        {
            // todo Not sure how to determine otherwise
            this.FlapsAvailable = true;
            this.AtcType = "n/a";
        }

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
        public Agent.Simulator.Models.AircraftIdentity Clone()
        {
            return new Agent.Simulator.Models.AircraftIdentity
            {
                Type = this.Type,
                EngineType = this.EngineType,
                EngineCount = this.EngineCount,
                AtcType = this.AtcType,
                AtcModel = this.AtcModel,
                FlapsAvailable = this.FlapsAvailable,
                GearRetractable = this.GearRetractable
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
            for (var i = 0; i < 40; i++)
            {
                var icao = DataRefs.AircraftViewAcfICAO;
                icao.DataRef += $"[{i}]";
                connector.Subscribe(icao, 1000 / sampleRate, this.DataRefUpdated);
            }

            for (var i = 0; i < 50; i++)
            {
                var description = DataRefs.AircraftViewAcfDescrip;
                description.DataRef += $"[{i}]";
                connector.Subscribe(description, 1000 / sampleRate, this.DataRefUpdated);
            }

            connector.Subscribe(DataRefs.AircraftEngineAcfNumEngines, 1000 / sampleRate, this.DataRefUpdated);
            var engineType = DataRefs.AircraftPropAcfEnType;
            engineType.DataRef += "[0]";
            connector.Subscribe(engineType, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.AircraftGearAcfGearRetract, 1000 / sampleRate, this.DataRefUpdated);
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
            if (element.DataRef.StartsWith(DataRefs.AircraftViewAcfICAO.DataRef))
            {
                if (element.DataRef.Contains("[") && element.DataRef.EndsWith("]"))
                {
                    var indexString = element.DataRef.Split('[')[1].Replace("]", string.Empty);
                    if (int.TryParse(indexString, out var index) && index is >= 0 and < 40)
                    {
                        this.acfICAO[index] = (char)value;
                        this.AtcModel = new string(this.acfICAO).Replace("\0", string.Empty);
                    }
                }
            }

            if (element.DataRef.StartsWith(DataRefs.AircraftViewAcfDescrip.DataRef))
            {
                if (element.DataRef.Contains("[") && element.DataRef.EndsWith("]"))
                {
                    var indexString = element.DataRef.Split('[')[1].Replace("]", string.Empty);
                    if (int.TryParse(indexString, out var index) && index is >= 0 and < 50)
                    {
                        this.acfDescription[index] = (char)value;
                        this.Type = new string(this.acfDescription).Replace("\0", string.Empty);
                    }
                }
            }

            if (element.DataRef == DataRefs.AircraftEngineAcfNumEngines.DataRef)
            {
                this.EngineCount = (int)value;
            }

            if (element.DataRef.StartsWith(DataRefs.AircraftPropAcfEnType.DataRef))
            {
                this.EngineType = Agent.UdpXPlane11.EngineType.ConvertEngineType((int)value);
            }

            if (element.DataRef == DataRefs.AircraftGearAcfGearRetract.DataRef)
            {
                this.GearRetractable = (int)value == 1;
            }
        }
    }
}