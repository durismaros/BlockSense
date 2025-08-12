using BlockSense.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Token
{
    public class RefreshTokenModel
    {
        public Guid TokenId { get; set; }
        public int UserId { get; set; }
        public byte[]? Data { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public byte[]? Hmac { get; set; }

        public byte[] GetBinaryData()
        {
            if (Data is null)
            {
                ConsoleLogger.Log("Token data is empty");
                return Array.Empty<byte>();
            }

            // Calculate size to avoid resizing
            int size = 16 + sizeof(int) + Data.Length + 2 * sizeof(long);

            using var ms = new MemoryStream(size);
            using var writer = new BinaryWriter(ms);

            writer.Write(TokenId.ToByteArray());
            writer.Write(UserId);
            writer.Write(Data);
            writer.Write(IssuedAt.ToBinary());
            writer.Write(ExpiresAt.ToBinary());

            return ms.ToArray();
        }
    }
}
