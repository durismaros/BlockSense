using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client.Cryptography.KeyDerivation
{
    class Argon2IdDerivation
    {
        private const int Iterations = 3;
        private const int MemoryCost = 65536; // 64MB in KB
        private const int Parallelism = 1;
        private const int OutputLength = 32; // 256-bit key

        public static byte[] DeriveKeyFromPin(string pin, byte[] salt)
        {
            // Set parameters for derivation
            var argon2Params = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                .WithSalt(salt)
                .WithIterations(Iterations)
                .WithMemoryAsKB(MemoryCost)
                .WithParallelism(Parallelism)
                .Build();

            // Creates generator
            var argon2Gen = new Argon2BytesGenerator();
            argon2Gen.Init(argon2Params);

            byte[] derivedKey = new byte[OutputLength];
            argon2Gen.GenerateBytes(pin.ToCharArray(), derivedKey);

            return derivedKey;
        }
    }
}
