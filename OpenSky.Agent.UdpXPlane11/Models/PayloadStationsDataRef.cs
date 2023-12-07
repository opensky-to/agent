// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayloadStationsDataRef.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane 11 dataref enabled version of payload stations model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 07/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.Simulator.Models.PayloadStations"/>
    /// -------------------------------------------------------------------------------------------------
    public class PayloadStationsDataRef : Simulator.Models.PayloadStations
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadStationsDataRef"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/02/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public PayloadStationsDataRef()
        {
            this.Count = 1;
            this.Name1 = "Payload";
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
        public Simulator.Models.PayloadStations Clone()
        {
            return new Simulator.Models.PayloadStations
            {
                // Omitted the other 19 stations as they will never be populated
                Count = this.Count,
                Name1 = this.Name1,
                Weight1 = this.Weight1
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
            connector.Subscribe(DataRefs.FlightmodelWeightMFixed, 1000 / sampleRate, this.DataRefUpdated);
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
            if (element.DataRef == DataRefs.FlightmodelWeightMFixed.DataRef)
            {
                this.Weight1 = value * 2.205;
            }
        }
    }
}