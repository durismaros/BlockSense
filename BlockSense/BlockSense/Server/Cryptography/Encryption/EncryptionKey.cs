using BlockSense.Client.Cryptography;
using BlockSense.Client.Cryptography.Hashing;
using BlockSense.DatabaseUtils;
using BlockSense.Server;
using NBitcoin.Crypto;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server_Based.Cryptography.Token_authentication
{
    class EncryptionKey
    {
        private static string _initializationVector = Convert.ToBase64String(CryptoUtils.SecureRandomGenerator());

        /// <summary>
        /// Derives 256bits enrcyption key from user password and salt
        /// </summary>
        /// <param name="password">plain user password</param>
        /// <param name="salt"></param>
        /// <param name="iterations">default - 100000</param>
        /// <param name="keyLength">default - 256 bits</param>
        /// <returns>encryption key in Base64</returns>
        public static string Generate(byte[] password, byte[] salt, int iterations = 100000, int keyLength = 256)
        {
            // Create an instance of PBKDF2 using HMAC-SHA256
            Pkcs5S2ParametersGenerator generator = new Pkcs5S2ParametersGenerator(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());

            // Initialize the generator with password, salt, and iteration count
            generator.Init(password, salt, iterations);

            // Generate the AES key (AES-256 requires a 256-bit key)
            KeyParameter keyParameter = (KeyParameter)generator.GenerateDerivedMacParameters(keyLength);

            string encryptionKey = Convert.ToBase64String(keyParameter.GetKey());

            // Returnes the derived key
            return encryptionKey;
        }

        public async static Task DatabaseStore(string uid, string encryptionKey)
        {
            Dictionary<string, object> parameters = new()
            {
                {"@user_id", uid},
                {"@encryption_key", encryptionKey},
                {"@initialization_vector", _initializationVector},
                {"@issued_at", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}
            };
            string query = "INSERT INTO encryptionkeys (user_id, encryption_key, initialization_vector, issued_at) VALUES (@user_id, @encryption_key, @initialization_vector, @issued_at)";
            if (await Database.StoreData(query, parameters))
            {
                ConsoleHelper.Log($"Encryption Key & IV stored successfully");
            }
        }

        public async static Task<(byte[] encryptionKey, byte[] initializationVector)> Fetch()
        {
            Dictionary<string, object> parameters = new()
            {
                {"@user_id", User.Uid}
            };
            string query = "SELECT encryption_key, initialization_vector, revoked FROM encryptionkeys WHERE user_id = @user_id";
            using (var reader = await Database.FetchData(query, parameters))
            {
                if (reader.Read() && !reader.GetBoolean("revoked"))
                {
                    byte[] encryptionKeyBytes = Convert.FromBase64String(reader.GetString("encryption_key"));
                    byte[] ivBytes = Convert.FromBase64String(reader.GetString("initialization_vector"));


                    return (encryptionKeyBytes, ivBytes);
                }

                ConsoleHelper.Log("No valid encryption key found");
                //throw new Exception("No valid encryption key found");
                return (Array.Empty<byte>(), Array.Empty<byte>());
            }
        }
    }
}
