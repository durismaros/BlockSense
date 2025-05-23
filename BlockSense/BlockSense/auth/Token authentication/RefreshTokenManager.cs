using Avalonia.Controls.Platform;
using Avalonia.Remote.Protocol;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using Avalonia.Media.TextFormatting.Unicode;
using BlockSense.Models.Token;
using BlockSense.Cryptography.Hashing;
using BlockSense.Utilities;
using BlockSense.auth.DataProtection;

namespace BlockSense.Client_Side.Token_authentication
{
    public class RefreshTokenManager
    {
        private static string AuthPath { get; }
        private static string TokenFilePath { get; }

        static RefreshTokenManager()
        {
            AuthPath = DirectoryStructure.AuthPath;
            TokenFilePath = Path.Combine(AuthPath, "token.bin");
        }

        public void StoreToken(RefreshTokenModel token)
        {
            byte[]? entropy = EntropyManager.RetrieveEntropy();

            if (token is null || token.Data is null || entropy is null)
            {
                ConsoleLogger.Log("Either token or entropy is empty");
                return;
            }

            // Create and Mark auth directory as hidden
            Directory.CreateDirectory(AuthPath);
            File.SetAttributes(AuthPath, FileAttributes.Hidden | FileAttributes.NotContentIndexed);

            // Encrypt plain token data before saving
            token.Data = WinDataProtection.Encrypt(token.Data, entropy);

            // Add HMAC before saving
            token.Hmac = HashingFunctions.ComputeHmacSha256(token.GetBinaryData(), entropy);

            WriteDataToFile(token);
        }

        public RefreshTokenModel? RetrieveToken()
        {
            var existingToken = ReadDataFromFile();

            var entropy = EntropyManager.RetrieveEntropy();

            if (existingToken is null || existingToken.Data is null || existingToken.Hmac is null || entropy is null)
                return null;

            // Verify HMAC
            byte[] computedHmac = HashingFunctions.ComputeHmacSha256(existingToken.GetBinaryData(), entropy);
            if (!CryptographicOperations.FixedTimeEquals(computedHmac, existingToken.Hmac))
            {
                ConsoleLogger.Log("HMAC validation failed");
                return null;
            }

            if (existingToken.ExpiresAt < DateTime.UtcNow)
            {
                ConsoleLogger.Log("Token expired");
                return null;
            }

            existingToken.Data = WinDataProtection.Decrypt(existingToken.Data, entropy);
            ConsoleLogger.Log("Token retrieved successfully");
            return existingToken;
        }

        private static void WriteDataToFile(RefreshTokenModel token)
        {
            if (token is null || token.Data is null || token.Hmac is null)
                return;

            using var fs = new FileStream(TokenFilePath, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            writer.Write(token.TokenId.ToByteArray());
            writer.Write(token.UserId);

            writer.Write(token.Data.Length);
            writer.Write(token.Data);

            writer.Write(token.IssuedAt.ToBinary());
            writer.Write(token.ExpiresAt.ToBinary());

            writer.Write(token.Hmac.Length);
            writer.Write(token.Hmac);
        }

        private static RefreshTokenModel? ReadDataFromFile()
        {
            if (!File.Exists(TokenFilePath))
            {
                ConsoleLogger.Log("Token file not found");
                return null;
            }

            using var fs = new FileStream(TokenFilePath, FileMode.Open);
            using var reader = new BinaryReader(fs);

            return new RefreshTokenModel
            {
                TokenId = new Guid(reader.ReadBytes(16)),
                UserId = reader.ReadInt32(),
                Data = reader.ReadBytes(reader.ReadInt32()),
                IssuedAt = DateTime.FromBinary(reader.ReadInt64()),
                ExpiresAt = DateTime.FromBinary(reader.ReadInt64()),
                Hmac = reader.ReadBytes(reader.ReadInt32())
            };
        }
    }
}
