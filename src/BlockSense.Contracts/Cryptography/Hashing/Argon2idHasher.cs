using BlockSense.Contracts.Cryptography.Utilities;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSense.Contracts.Cryptography.Hashing
{
    /// <summary>
    /// Implements secure password hashing and key derivation using the Argon2id algorithm.
    /// Provides configurable parameters for memory, iterations, parallelism, output length, and salt length.
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
        /// Number of iterations (time cost). Increasing this value strengthens resistance to brute-force attacks at the cost of computation time.
        /// Defaults to 3.
        /// </summary>
        public int Iterations
        {
            get; 
        }

        /// <summary>
        /// Degree of parallelism (threads) used during hashing.
        /// Default is 1, or it can match the number of available CPU cores.
        /// </summary>
        public int Parallelism
        {
            get;
        }

        /// <summary>
        /// Length of the derived key in bytes.
        /// 32 bytes (256 bits) is a common choice for password hashing.
        /// </summary>
        public int OutputLength
        {
            get;
        }

        /// <summary>
        /// Length of the salt in bytes. Used when generating a new salt for hashing.
        /// Defaults to 16 bytes.
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any numeric argument is out of the valid range.
        /// </exception>
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
        /// Derives a cryptographically secure hash using the Argon2id algorithm.
        /// </summary>
        /// <param name="password">The input password or key material as a byte array. Cannot be null.</param>
        /// <param name="salt">Output parameter returning the salt used for hashing. If <paramref name="providedSalt"/> is null, a new cryptographically secure salt is generated.</param>
        /// <param name="providedSalt">Optional salt to use. Must be at least 8 bytes if provided.</param>
        /// <returns>A byte array containing the derived key (hash) with length equal to <see cref="OutputLength"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="providedSalt"/> is shorter than 8 bytes.</exception>
        public byte[] Derive(byte[] password, out byte[] salt, byte[]? providedSalt = null)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            if (providedSalt is not null)
            {
                if (providedSalt.Length < 8) throw new ArgumentOutOfRangeException(nameof(providedSalt), "Salt too short");
                salt = providedSalt;
            }

            else
            {
                salt = CryptographyUtilities.GenerateSecureRandomBytes(SaltLength);
            }

            // Configure Argon2id parameters
            var builder = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                .WithSalt(salt)
                .WithMemoryAsKB(MemoryCostKb)
                .WithIterations(Iterations)
                .WithParallelism(Parallelism);

            var parameters = builder.Build();

            // Initialize generator and derive key
            var generator = new Argon2BytesGenerator();
            generator.Init(parameters);

            // Allocate output buffer and perform key derivation.
            byte[] output = new byte[OutputLength];
            generator.GenerateBytes(password, output, 0, output.Length);
            return output;
        }
    }
}
