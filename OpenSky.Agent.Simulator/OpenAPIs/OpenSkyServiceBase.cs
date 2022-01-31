// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyServiceBase.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky service client base class (implements auto-token refresh and JWT bearer.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public abstract class OpenSkyServiceBase
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The HTTP client.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        protected readonly HttpClient httpClient;

       

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// JSON serializer settings.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        protected readonly Lazy<Newtonsoft.Json.JsonSerializerSettings> settings;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyServiceBase"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        protected OpenSkyServiceBase()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyServiceBase"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="httpClient">
        /// The HTTP client.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        protected OpenSkyServiceBase(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            this.settings = new Lazy<Newtonsoft.Json.JsonSerializerSettings>(CreateSerializerSettings);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the response as string should be read.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ReadResponseAsString { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the JSON serializer settings.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        protected Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings => this.settings.Value;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates OpenSky HTTP request message asynchronous - automatically adds authorization header and refreshes tokens.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/05/2021.
        /// </remarks>
        /// <param name="cancellationToken">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the create HTTP request message.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected abstract Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Reads object response asynchronous.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/05/2021.
        /// </remarks>
        /// <exception cref="ApiException">
        /// Thrown when an API error condition occurs.
        /// </exception>
        /// <param name="response">
        /// The HTTP response.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the object response asynchronous.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected virtual async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
        {
            if (response?.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (this.ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText, this.JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    using var streamReader = new System.IO.StreamReader(responseStream);
                    using var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader);
                    var serializer = Newtonsoft.Json.JsonSerializer.Create(this.JsonSerializerSettings);
                    var typedBody = serializer.Deserialize<T>(jsonTextReader);
                    return new ObjectResponseResult<T>(typedBody, string.Empty);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Create json serializer settings.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/05/2021.
        /// </remarks>
        /// <returns>
        /// The new serializer settings.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static Newtonsoft.Json.JsonSerializerSettings CreateSerializerSettings()
        {
            return new();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh current main JWT token using the refresh token.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/05/2021.
        /// </remarks>
        /// <exception cref="ApiException">
        /// Thrown when an API error condition occurs.
        /// </exception>
        /// <param name="cancellationToken">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected abstract Task RefreshToken(CancellationToken cancellationToken);

        // -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Encapsulates the result of an object response.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        protected readonly struct ObjectResponseResult<T>
        {
            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="OpenSkyServiceBase"/> class.
            /// </summary>
            /// <remarks>
            /// sushi.at, 01/06/2021.
            /// </remarks>
            /// <param name="responseObject">
            /// The response object.
            /// </param>
            /// <param name="responseText">
            /// The response text.
            /// </param>
            /// -------------------------------------------------------------------------------------------------
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Gets the object.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public T Object { get; }

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Gets the text.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public string Text { get; }
        }
    }
}