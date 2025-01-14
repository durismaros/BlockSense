using BlockSense.DB;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server_Based.Cryptography.Token_authentication
{
    class EncryptionKey
    {
        private static string _initializationVector = RemoteTokenUtils.GenerateIV();
        public static string Generate(string password, string salt, int iterations = 100000, int keyLength = 256)
        {
            // Convert password to bytes
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            // Create an instance of PBKDF2 using HMAC-SHA256
            Pkcs5S2ParametersGenerator generator = new Pkcs5S2ParametersGenerator(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());

            // Initialize the generator with password, salt, and iteration count
            generator.Init(passwordBytes, saltBytes, iterations);

            // Generate the AES key (AES-256 requires a 256-bit key)
            KeyParameter keyParameter = (KeyParameter)generator.GenerateDerivedMacParameters(keyLength);

            string encryptionKey = Convert.ToBase64String(keyParameter.GetKey());

            // Returnes the derived key
            return encryptionKey;
        }

        public async static Task DatabaseStore(string uid, string encryptionKey)
        {
            Dictionary<string, string> parameters = new()
            {
                {"@user_id", uid},
                {"@encryption_key", encryptionKey},
                {"@initialization_vector", _initializationVector},
                {"@issued_at", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}
            };
            string query = "INSERT INTO encryptionkeys (user_id, encryption_key, initialization_vector, issued_at) VALUES (@user_id, @encryption_key, @initialization_vector, @issued_at)";
            if (await Database.StoreData(query, parameters))
            {
                ConsoleHelper.WriteLine($"Encryption Key & IV stored successfully");
            }
        }

        public async static Task<(string encryptionKey, string initializationVector)> Fetch(string uid)
        {
            Dictionary<string, string> parameters = new()
            {
                {"@user_id", uid}
            };
            string query = "SELECT encryption_key, initialization_vector, revoked FROM encryptionkeys WHERE user_id = @user_id";
            using (var reader = await Database.FetchData(query, parameters))
            {
                if (reader.Read() && !reader.GetBoolean("revoked"))
                {
                    return (reader.GetString("encryption_key"), reader.GetString("initialization_vector"));
                }
                return (null, null);
            }
        }
    }
}
