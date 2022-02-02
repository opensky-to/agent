// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimaryTracking.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.UdpXPlane11.Models
{
    using XPlaneConnector;
    using XPlaneConnector.DataRefs;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// XPlane 11 dataref enabled version of primary tracking model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/02/2022.
    /// </remarks>
    /// <seealso cref="OpenSky.Agent.Simulator.Models.PrimaryTracking"/>
    /// -------------------------------------------------------------------------------------------------
    public class PrimaryTrackingDataRef : Simulator.Models.PrimaryTracking
    {
        public void RegisterWithConnector(XPlaneConnector connector, int sampleRate)
        {
            //connector.Subscribe(DataRefs.FlightmodelPositionLatitude, 1000 / sampleRate, this.DataRefUpdated);
            connector.Subscribe(DataRefs.FlightmodelPositionLatitude, -1, this.DataRefUpdated);
        }

        private void DataRefUpdated(DataRefElement element, float value)
        {
            if (element.DataRef == DataRefs.FlightmodelPositionLatitude.DataRef)
            {
                this.Latitude = value;
            }
        }
    }
}