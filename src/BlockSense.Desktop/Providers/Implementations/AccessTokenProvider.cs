using BlockSense.Contracts.DTOs.Token;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling.Exceptions;
using Microsoft.Extensions.Logging;
using System;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly ILogger<AccessTokenProvider> _logger;

        private string _accessToken;
        private DateTime _expiresAt;

        public AccessTokenProvider(ILogger<AccessTokenProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accessToken = string.Empty;
            _expiresAt = DateTime.MinValue;
        }

        /// <inheritdoc/>
        public string Get()
        {
            if (string.IsNullOrWhiteSpace(_accessToken) && _expiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Access token requested but none is valid");
                throw new AuthenticationRequiredException();
            }

            return _accessToken;
        }

        /// <inheritdoc/>
        public void Set(AccessTokenDto accessToken)
        {
            if (accessToken is null)
                throw new ArgumentNullException(nameof(accessToken));

            if (string.IsNullOrWhiteSpace(accessToken.Token))
                throw new ArgumentException("Token value must not be empty.", nameof(accessToken));

            if (accessToken.ExpiresAt < DateTime.UtcNow)
                throw new ArgumentException("Cannot store an already-expired access token.", nameof(accessToken));

            _accessToken = accessToken.Token;
            _expiresAt = accessToken.ExpiresAt;

            _logger.LogDebug("Access token stored (expires {ExpiresAt:O})", _expiresAt);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _accessToken = string.Empty;
            _expiresAt = DateTime.MinValue;

            _logger.LogDebug("Access token cleared");

        }
    }
}
