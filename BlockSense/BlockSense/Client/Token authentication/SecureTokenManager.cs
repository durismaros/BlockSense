using Avalonia.Controls.Platform;
using Avalonia.Remote.Protocol;
using BlockSense.Client.DataProtection;
using BlockSense.Server.Cryptography.TokenAuthentication;
using CredentialManagement;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using BlockSense.Client;
using MaxMind.Db;
using BlockSense.Client.Cryptography.Hashing;
using Avalonia.Media.TextFormatting.Unicode;

namespace BlockSense.Client_Side.Token_authentication
{
    class SecureTokenManager
    {
        private static readonly string _filePath = Path.Combine(DirStructure.authPath, "token.bin");

        public static void StoreToken(TokenCache token, byte[] entropy)
        {
            token.Data = WinDPAPI.Encrypt(token.Data, entropy);

            // Add HMAC before saving
            token.Hmac = HashUtils.ComputeHmacSha256(token.GetBinaryData(), entropy);

            WriteDataToFile(token);
        }

        public static (byte[] plainToken, Guid tokenId) RetrieveToken(byte[] entropy)
        {
            if (!File.Exists(_filePath))
            {
                ConsoleHelper.Log("Token file not found");
                return (Array.Empty<byte>(), Guid.Empty);
            }

            var token = ReadDataFromFile();

            // Verify HMAC
            byte[] computedHmac = HashUtils.ComputeHmacSha256(token.GetBinaryData(), entropy);
            if (!CryptographicOperations.FixedTimeEquals(computedHmac, token.Hmac))
            {
                ConsoleHelper.Log("HMAC validation failed");
                return (Array.Empty<byte>(), Guid.Empty);
            }

            //if (token.ExpiresAt < DateTime.UtcNow)
            //    ConsoleHelper.Log("Token expired");

            ConsoleHelper.Log("Token retrieved successfully");
            return (WinDPAPI.Decrypt(token.Data, entropy), token.TokenId);
        }

        private static void WriteDataToFile(TokenCache token)
        {
            using var fs = new FileStream(_filePath, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            writer.Write(token.TokenId.ToByteArray());
            writer.Write(token.IssuedAt.ToBinary());
            writer.Write(token.ExpiresAt.ToBinary());

            writer.Write(token.Data.Length);
            writer.Write(token.Data);

            writer.Write(token.Hmac.Length);
            writer.Write(token.Hmac);
        }

        private static TokenCache ReadDataFromFile()
        {
            if (File.Exists(_filePath))
            {
                using var fs = new FileStream(_filePath, FileMode.Open);
                using var reader = new BinaryReader(fs);

                return new TokenCache
                {
                    TokenId = new Guid(reader.ReadBytes(16)),
                    IssuedAt = DateTime.FromBinary(reader.ReadInt64()),
                    ExpiresAt = DateTime.FromBinary(reader.ReadInt64()),
                    Data = reader.ReadBytes(reader.ReadInt32()),
                    Hmac = reader.ReadBytes(reader.ReadInt32())
                };
            }
            return new TokenCache();
        }
    }
}
