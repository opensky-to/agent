// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyService.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System.Net.Http;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky API service client.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSkyApi.OpenSkyServiceBase"/>
    /// -------------------------------------------------------------------------------------------------
    public abstract partial class OpenSkyService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="httpClient">
        /// The HTTP client.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected OpenSkyService(HttpClient httpClient) : base(httpClient)
        {
            this._httpClient = httpClient;
            this._settings = new System.Lazy<Newtonsoft.Json.JsonSerializerSettings>(this.CreateSerializerSettings);
        }
    }
}