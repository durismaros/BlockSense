using BlockSense.Contracts.Cryptography.Utilities;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSense.Contracts.Cryptography.Hashing
{
    /// <summary>
    /// Implements secure password hashing and key derivation using the Argon2id algorithm.
    /// Provides configurable parameters for memory cost, iterations, parallelism, output length, and salt length.
    /// </summary>
    public sealed class Argon2idHasher
    {
        /// <summary>
        /// Memory cost in kibibytes (KiB). Determines the RAM usage of the algorithm.
        /// Secure defaults are 65,536 KiB (64 MB) or higher.
        /// </summary>
        public int MemoryCostKb
        {
            get;
        }

        /// <summary>
        /// Number of iterations (time cost). Higher values increase resistance to brute-force attacks
        /// at the cost of computation time. Defaults to 3.
        /// </summary>
        public int Iterations
        {
            get;
        }

        /// <summary>
        /// Degree of parallelism (number of threads) used during hashing.
        /// Defaults to 1, or can be set to match the number of available CPU cores.
        /// </summary>
        public int Parallelism
        {
            get;
        }

        /// <summary>
        /// Length of the derived key in bytes. 32 bytes (256 bits) is recommended for password hashing.
        /// </summary>
        public int OutputLength
        {
            get;
        }

        /// <summary>
        /// Length of the salt in bytes used when generating a new salt. Defaults to 16 bytes.
        /// </summary>
        public int SaltLength
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Argon2idHasher"/> with the specified parameters.
        /// </summary>
        /// <param name="memoryCostKb">Memory cost in KiB. Must be at least 8 KiB.</param>
        /// <param name="iterations">Number of iterations. Must be greater than zero.</param>
        /// <param name="parallelism">Degree of parallelism. Must be greater than zero.</param>
        /// <param name="outputLength">Length of the derived key in bytes. Must be greater than zero.</param>
        /// <param name="saltLength">Length of the salt in bytes. Must be greater than zero.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any argument is outside its valid range.</exception>
        public Argon2idHasher(
            int memoryCostKb = 65536,
            int iterations = 3,
            int parallelism = 1,
            int outputLength = 32,
            int saltLength = 16)
        {
            if (memoryCostKb < 8) throw new ArgumentOutOfRangeException(nameof(memoryCostKb), "Memory cost must be at least 8 KiB.");
            if (iterations <= 0) throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be greater than zero.");
            if (parallelism <= 0) throw new ArgumentOutOfRangeException(nameof(parallelism), "Parallelism must be greater than zero.");
            if (outputLength <= 0) throw new ArgumentOutOfRangeException(nameof(outputLength), "Output length must be greater than zero.");
            if (saltLength <= 0) throw new ArgumentOutOfRangeException(nameof(saltLength), "Salt length must be greater than zero.");

            MemoryCostKb = memoryCostKb;
            Iterations = iterations;
            Parallelism = parallelism;
            OutputLength = outputLength;
            SaltLength = saltLength;
        }

        /// <summary>
        /// Derives a cryptographically secure hash from the provided password using the Argon2id algorithm.
        /// </summary>
        /// <param name="password">The input password or key material as a byte array. Cannot be null.</param>
        /// <param name="salt">
        /// Output parameter returning the salt used during hashing.
        /// A new cryptographically secure salt is generated when <paramref name="providedSalt"/> is null.
        /// </param>
        /// <param name="providedSalt">Optional salt to use instead of generating one. Must be at least 8 bytes if provided.</param>
        /// <returns>A byte array of length <see cref="OutputLength"/> containing the derived key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="providedSalt"/> is shorter than 8 bytes.</exception>
        public byte[] Derive(byte[] password, out byte[] salt, byte[]? providedSalt = null)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));

            salt = ResolveSalt(providedSalt);

            var parameters = BuildArgon2Parameters(salt);
            return DeriveKey(password, parameters);
        }

        /// <summary>
        /// Resolves the salt to use for hashing, either validating the provided salt or generating a new one.
        /// </summary>
        /// <param name="providedSalt">The caller-supplied salt, or null to generate a new one.</param>
        /// <returns>The salt bytes to use for the Argon2id operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="providedSalt"/> is shorter than 8 bytes.</exception>
        private byte[] ResolveSalt(byte[]? providedSalt)
        {
            if (providedSalt is null)
                return CryptographyUtilities.GenerateSecureRandomBytes(SaltLength);

            if (providedSalt.Length < 8)
                throw new ArgumentOutOfRangeException(nameof(providedSalt), "Salt must be at least 8 bytes.");

            return providedSalt;
        }

        /// <summary>
        /// Constructs the Argon2id parameter set from the current hasher configuration.
        /// </summary>
        /// <param name="salt">The salt to embed in the parameter set.</param>
        /// <returns>A configured <see cref="Argon2Parameters"/> instance for key derivation.</returns>
        private Argon2Parameters BuildArgon2Parameters(byte[] salt)
            => new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                .WithSalt(salt)
                .WithMemoryAsKB(MemoryCostKb)
                .WithIterations(Iterations)
                .WithParallelism(Parallelism)
                .Build();

        /// <summary>
        /// Performs the Argon2id key derivation using the provided password and parameters.
        /// </summary>
        /// <param name="password">The password bytes to derive from.</param>
        /// <param name="parameters">The configured Argon2id parameters.</param>
        /// <returns>A byte array of length <see cref="OutputLength"/> containing the derived key.</returns>
        private byte[] DeriveKey(byte[] password, Argon2Parameters parameters)
        {
            var generator = new Argon2BytesGenerator();
            generator.Init(parameters);

            byte[] output = new byte[OutputLength];
            generator.GenerateBytes(password, output, 0, output.Length);
            return output;
        }
    }
}