// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserSessionService.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    using OpenSky.AgentMSFS.Properties;

    using OpenSkyApi;

    using XDMessaging;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// User session service for OpenSky API.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class UserSessionService : INotifyPropertyChanged
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The username (for display purposes only)
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string username;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is a user currently logged in?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private bool isUserLoggedIn;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether there is a user currently logged in.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsUserLoggedIn
        {
            get => this.isUserLoggedIn;
        
            set
            {
                if(Equals(this.isUserLoggedIn, value))
                {
                   return;
                }
        
                this.isUserLoggedIn = value;
                this.OnPropertyChanged();
            }
        }

        

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current OpenSky API token, null if no token is available.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OpenSkyApiToken { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the username (for display purposes only).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Username
        {
            get => this.username;

            set
            {
                if (Equals(this.username, value))
                {
                    return;
                }

                this.username = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string RefreshToken { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Date/Time of the expiration of the main JWT OpenSky API token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Expiration { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Date/Time of the refresh token expiration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? RefreshExpiration { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="UserSessionService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static UserSessionService()
        {
            Instance = new UserSessionService();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UserSessionService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private UserSessionService()
        {
            this.LoadOpenSkyTokens();

            var client = new XDMessagingClient();
            var listener = client.Listeners.GetListenerForMode(XDTransportMode.Compatibility);
            listener.RegisterChannel("OPENSKY-AGENT-MSFS");
            listener.MessageReceived += (_, e) =>
            {
                if (e.DataGram.Channel == "OPENSKY-AGENT-MSFS")
                {
                    switch (e.DataGram.Message)
                    {
                        case "TokensUpdated":
                            this.LoadOpenSkyTokens();
                            break;
                    }
                }
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Load OpenSky tokens from settings.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void LoadOpenSkyTokens()
        {
            Settings.Default.Reload();
            this.OpenSkyApiToken = Settings.Default.OpenSkyApiToken;
            this.Expiration = Settings.Default.OpenSkyTokenExpiration;
            this.RefreshToken = Settings.Default.OpenSkyRefreshToken;
            this.RefreshExpiration = Settings.Default.OpenSkyRefreshTokenExpiration;
            this.Username = Settings.Default.OpenSkyUsername;

            this.CheckExpiration();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check token expiration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <returns>
        /// True if the main JWT token needs to be refreshed, false if it is current or the refresh token
        /// is also expired (will trigger logout!).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public bool CheckExpiration()
        {
            if (!string.IsNullOrEmpty(this.OpenSkyApiToken) && this.Expiration.HasValue && this.Expiration.Value > DateTime.UtcNow)
            {
                this.IsUserLoggedIn = true;
                return false;
            }

            if (!string.IsNullOrEmpty(this.RefreshToken) && this.RefreshExpiration.HasValue && this.RefreshExpiration.Value > DateTime.UtcNow)
            {
                this.IsUserLoggedIn = true;
                return true;
            }

            this.Logout();
            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tokens were refreshed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="refreshToken">
        /// The refresh token response model (contains new tokens).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void RefreshedToken(RefreshTokenResponse refreshToken)
        {
            this.OpenSkyApiToken = refreshToken.Token;
            this.Expiration = refreshToken.Expiration.UtcDateTime;
            this.RefreshToken = refreshToken.RefreshToken;
            this.RefreshExpiration = refreshToken.RefreshTokenExpiration.UtcDateTime;

            Settings.Default.OpenSkyApiToken = refreshToken.Token;
            Settings.Default.OpenSkyTokenExpiration = refreshToken.Expiration.UtcDateTime;
            Settings.Default.OpenSkyRefreshToken = refreshToken.RefreshToken;
            Settings.Default.OpenSkyRefreshTokenExpiration = refreshToken.RefreshTokenExpiration.UtcDateTime;
            Settings.Default.Save();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the single static instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static UserSessionService Instance { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// User wants to log out.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public void Logout()
        {
            this.IsUserLoggedIn = false;
            this.OpenSkyApiToken = null;
            this.Expiration = null;
            this.Username = null;
            this.RefreshToken = null;
            this.RefreshExpiration = null;

            Settings.Default.OpenSkyApiToken = null;
            Settings.Default.OpenSkyTokenExpiration = DateTime.MinValue;
            Settings.Default.OpenSkyRefreshToken = null;
            Settings.Default.OpenSkyRefreshTokenExpiration = DateTime.MinValue;
            Settings.Default.Save();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Executes the property changed action.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="propertyName">
        /// (Optional) Name of the property.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}