namespace BlockSense.Contracts.DTOs.TwoFactorAuth.Setup
{
    /// <summary>
    /// Represents the data returned when initiating a two-factor authentication (2FA) setup.
    /// </summary>
    public sealed record TwoFactorSetupInit
    {
        /// <summary>
        /// The 20-byte (160-bit) Base32-encoded secret key used to configure 2FA in the authenticator app.
        /// </summary>
        public required string SetupKey
        {
            get;
            init;
        }

        /// <summary>
        /// The QR code image data corresponding to the setup key.
        /// </summary>
        public required byte[] QrCodeData
        {
            get;
            init;
        }
    }
}