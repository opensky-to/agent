// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionExtensions.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Tools
{
    using System;
    using System.Diagnostics;
    using System.Windows;

    using Newtonsoft.Json;

    using OpenSky.AgentMSFS.MVVM;
    using OpenSky.AgentMSFS.OpenAPIs;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Exception extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class ExceptionExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Handle exception being thrown as a result of an OpenSky API call.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// <param name="ex">
        /// The exception to act on.
        /// </param>
        /// <param name="command">
        /// The asynchronous command executing the API call.
        /// </param>
        /// <param name="friendlyErrorMessage">
        /// Friendly error messages describing what we were trying to do.
        /// </param>
        /// <param name="alert401">
        /// (Optional) True to alert about HTTP 401 (unauthorized) errors - letting the user know to login again.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void HandleApiCallException(this Exception ex, AsynchronousCommand command, string friendlyErrorMessage, bool alert401 = true)
        {
            if (ex is ApiException apiException)
            {
                if (apiException.StatusCode == 401)
                {
                    // todo
                }
                else if (!string.IsNullOrEmpty(apiException.Response))
                {
                    var problemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(apiException.Response);
                    if (problemDetails != null)
                    {
                        foreach (var problemDetailsError in problemDetails.Errors)
                        {
                            foreach (var errorMessage in problemDetailsError.Value)
                            {
                                command.ReportProgress(
                                    () =>
                                    {
                                        Debug.WriteLine($"{friendlyErrorMessage}: " + errorMessage);
                                        ModernWpf.MessageBox.Show(errorMessage, friendlyErrorMessage, MessageBoxButton.OK, MessageBoxImage.Error);
                                    });
                            }
                        }
                    }
                    else
                    {
                        command.ReportProgress(
                            () =>
                            {
                                Debug.WriteLine($"{friendlyErrorMessage}: " + apiException.Message);
                                ModernWpf.MessageBox.Show(apiException.Message, friendlyErrorMessage, MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                    }
                }
                else
                {
                    command.ReportProgress(
                        () =>
                        {
                            Debug.WriteLine($"{friendlyErrorMessage}: " + apiException.Message);
                            ModernWpf.MessageBox.Show(apiException.Message, friendlyErrorMessage, MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                }
            }
            else
            {
                command.ReportProgress(
                    () =>
                    {
                        Debug.WriteLine($"{friendlyErrorMessage}: " + ex.Message);
                        ModernWpf.MessageBox.Show(ex.Message, friendlyErrorMessage, MessageBoxButton.OK, MessageBoxImage.Error);
                    });
            }
        }
    }
}