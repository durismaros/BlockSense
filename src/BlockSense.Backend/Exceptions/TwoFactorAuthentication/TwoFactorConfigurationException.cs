using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.TwoFactorAuthentication
{
    /// <summary>
    /// Thrown when a requested operation conflicts with the current two-factor authentication configuration.
    /// </summary>
    public sealed class TwoFactorConfigurationException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.TwoFactorAuthentication.ConfigurationConflict;

        /// <inheritdoc/>
        public override string Title => "Two-Factor Authentication Configuration Conflict";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status409Conflict;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFactorConfigurationException"/> class.
        /// </summary>
        public TwoFactorConfigurationException()
            : base("The requested operation conflicts with the current two-factor authentication configuration.") { }
    }
}