// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AgentOpenSkyService.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace OpenSkyApi
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using OpenSky.Agent;
    using OpenSky.Agent.Properties;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight tracking agent OpenSky service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// <seealso cref="T:OpenSkyApi.OpenSkyService"/>
    /// -------------------------------------------------------------------------------------------------
    public class AgentOpenSkyService : OpenSkyService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The refresh token mutex.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Mutex refreshTokenMutex = new(false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="OpenSkyService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static AgentOpenSkyService()
        {
            Instance = new AgentOpenSkyService(new HttpClient());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentOpenSkyService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="httpClient">
        /// The HTTP client.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AgentOpenSkyService(HttpClient httpClient) : base(httpClient)
        {
            this.BaseUrl = Settings.Default.OpenSkyAPIUrl;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the single static instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static OpenSkyService Instance { get; }

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
        protected override async Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
        {
            var msg = new HttpRequestMessage();

            // Check if the token needs to be refreshed
            if (UserSessionService.Instance.CheckTokenNeedsRefresh())
            {
                try
                {
                    await this.RefreshToken(cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    if (ex.InnerException is WebException webEx)
                    {
                        if (webEx.Status is WebExceptionStatus.ConnectFailure or WebExceptionStatus.NameResolutionFailure or WebExceptionStatus.SendFailure or WebExceptionStatus.ReceiveFailure)
                        {
                            // Server not available? Try again later
                            Debug.WriteLine($"Error refreshing tokens: {ex}");
                            throw;
                        }

                        Debug.WriteLine($"Error refreshing tokens: {ex}");
                        UserSessionService.Instance.Logout();
                        throw;
                    }

                    Debug.WriteLine($"Error refreshing tokens: {ex}");
                    UserSessionService.Instance.Logout();
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error refreshing tokens: {ex}");
                    UserSessionService.Instance.Logout();
                    throw;
                }
            }

            // Add the JWT token to the authorization header
            if (UserSessionService.Instance.IsUserLoggedIn)
            {
                msg.Headers.Add("Authorization", $"Bearer {UserSessionService.Instance.OpenSkyApiToken}");
            }

            return msg;
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
        protected override async Task RefreshToken(CancellationToken cancellationToken)
        {
            try
            {
                if (!this.refreshTokenMutex.WaitOne(30 * 1000))
                {
                    // Timeout refreshing token
                    Debug.WriteLine("Timeout waiting for refresh token mutex.");
                    return;
                }

                // Now that we have the mutex, check if another refresh was successful in the meantime
                if (!UserSessionService.Instance.CheckTokenNeedsRefresh())
                {
                    return;
                }

                var requestBody = new RefreshToken
                {
                    Token = UserSessionService.Instance.OpenSkyApiToken,
                    Refresh = UserSessionService.Instance.RefreshToken
                };

                var urlBuilder = new StringBuilder();
                var baseUrl = Settings.Default.OpenSkyAPIUrl;
                urlBuilder.Append(baseUrl != null ? baseUrl.TrimEnd('/') : "").Append("/Authentication/refreshToken");

                using var request = new HttpRequestMessage();
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, this.settings.Value));
                content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                request.Content = content;
                request.Method = new HttpMethod("POST");
                request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                var url = urlBuilder.ToString();
                request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

                var response = await this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                try
                {
                    var headers = response.Headers.ToDictionary(h => h.Key, h => h.Value);
                    if (response.Content is { Headers: { } })
                    {
                        foreach (var header in response.Content.Headers)
                        {
                            headers[header.Key] = header.Value;
                        }
                    }

                    var status = (int)response.StatusCode;
                    if (status == 200)
                    {
                        var objectResponse = await this.ReadObjectResponseAsync<RefreshTokenResponseApiResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                        if (objectResponse.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);
                        }

                        if (!objectResponse.Object.IsError)
                        {
                            UserSessionService.Instance.TokensWereRefreshed(objectResponse.Object.Data);
                        }
                        else
                        {
                            // Check if another refresh caused a server side concurrency issue but the other request completed the refresh successfully
                            if (!UserSessionService.Instance.CheckTokenNeedsRefresh())
                            {
                                return;
                            }

                            throw new ApiException($"Unable to refresh OpenSky token: {objectResponse.Object.Message}", 401, objectResponse.Text, headers, null);
                        }
                    }
                    else
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status + ").", status, responseData, headers, null);
                    }
                }
                finally
                {
                    response.Dispose();
                }
            }
            catch (AbandonedMutexException)
            {
                //Ignore and retry
                await this.RefreshToken(cancellationToken);
            }
            finally
            {
                try
                {
                    this.refreshTokenMutex.ReleaseMutex();
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}