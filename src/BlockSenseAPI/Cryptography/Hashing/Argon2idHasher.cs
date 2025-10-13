using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSenseAPI.Cryptography.Hashing
{
    /// <summary>
    /// Provides secure password hashing and key derivation using the Argon2id algorithm.
    /// </summary>
    public sealed class Argon2idHasher
    {
        /// <summary>
        /// Gets the memory cost parameter in kibibytes (KiB).
        /// Typical secure defaults are 65,536 (64 MB) or higher, depending on the environment and use case.
        /// </summary>
        public int MemoryCostKb { get; }

        /// <summary>
        /// Gets the number of iterations (time cost) to perform.
        /// Higher values increase computational effort and resistance to brute-force attacks.
        /// Typical default is <c>3</c>.
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Gets the degree of parallelism (threads) used during hashing.
        /// Typical default is <c>1</c> or the number of available CPU cores.
        /// </summary>
        public int Parallelism { get; }

        /// <summary>
        /// Gets the length of the derived key in bytes.
        /// For password hashing, 32 bytes (256 bits) is common.
        /// </summary>
        public int OutputLength { get; }

        /// <summary>
        /// Gets the salt length in bytes used when <see cref="Derive(byte[], out byte[], byte[])"/> generates a new salt.
        /// The default value is 16 bytes.
        /// </summary>
        public int SaltLength { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="Argon2idHasher"/> class with the specified parameters.
        /// </summary>
        /// <param name="memoryCostKb"></param>
        /// <param name="iterations"></param>
        /// <param name="parallelism"></param>
        /// <param name="outputLength"></param>
        /// <param name="saltLength"></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any numeric argument is less than or equal to zero, or when memory cost is below 8 KiB.</exception>
        public Argon2idHasher(
            int memoryCostKb = 65536,   // 64 MB by default
            int iterations = 3,
            int parallelism = 1,
            int outputLength = 32,
            int saltLength = 16)
        {
            if (memoryCostKb < 8) throw new ArgumentOutOfRangeException(nameof(memoryCostKb));
            if (iterations <= 0) throw new ArgumentOutOfRangeException(nameof(iterations));
            if (parallelism <= 0) throw new ArgumentOutOfRangeException(nameof(parallelism));
            if (outputLength <= 0) throw new ArgumentOutOfRangeException(nameof(outputLength));
            if (saltLength <= 0) throw new ArgumentOutOfRangeException(nameof(saltLength));

            MemoryCostKb = memoryCostKb;
            Iterations = iterations;
            Parallelism = parallelism;
            OutputLength = outputLength;
            SaltLength = saltLength;
        }

        /// <summary>
        /// Derives a cryptographically secure hash using the Argon2id algorithm.
        /// </summary>
        /// <param name="password">The input password or key material as a byte array. Must not be null.</param>
        /// <param name="salt">If <paramref name="providedSalt"/> is null, a new cryptographically secure random salt is generated. Defaults to <paramref name="providedSalt"/></param>
        /// <param name="providedSalt">An optional salt to use. If provided, must be at least 8 bytes long.</param>
        /// <returns>A derived key (hash) as a byte array of length <see cref="OutputLength"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="providedSalt"/> is shorter than 8 bytes.</exception>
        public byte[] Derive(byte[] password, out byte[] salt, byte[]? providedSalt = null)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (providedSalt != null)
            {
                if (providedSalt.Length < 8) throw new ArgumentException("Salt too short", nameof(providedSalt));
                salt = providedSalt;
            }
            else
            {
                salt = CryptographyUtilities.GenerateSecureRandomBytes(SaltLength);
            }

            // Build Argon2 parameters (ARGON2_id)
            // Argon2Parameters in BouncyCastle accepts memory in KB, iterations, parallelism and the salt.
            var builder = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                .WithSalt(salt)
                .WithMemoryAsKB(MemoryCostKb)
                .WithIterations(Iterations)
                .WithParallelism(Parallelism);

            var parameters = builder.Build();

            // Initialize the Argon2id generator with configured parameters.
            var generator = new Argon2BytesGenerator();
            generator.Init(parameters);

            // Allocate output buffer and perform key derivation.
            byte[] output = new byte[OutputLength];
            generator.GenerateBytes(password, output, 0, output.Length);
            return output;
        }
    }
}
