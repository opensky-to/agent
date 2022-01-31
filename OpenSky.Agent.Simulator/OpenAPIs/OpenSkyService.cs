// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyService.cs" company="OpenSky">
// OpenSky project 2021-2022
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
        /// <param name="baseUrl">
        /// The base URL of the service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected OpenSkyService(HttpClient httpClient, string baseUrl) : base(httpClient)
        {
            this.BaseUrl = baseUrl;
            this._httpClient = httpClient;
            this._settings = new System.Lazy<Newtonsoft.Json.JsonSerializerSettings>(this.CreateSerializerSettings);
        }
    }
}