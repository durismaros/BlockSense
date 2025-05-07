using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server.Cryptography.TokenAuthentication
{
    public class TokenCache
    {
        public Guid TokenId { get; set; }
        public int UserId { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public byte[] Data { get; set; }
        public byte[] Hmac { get; set; }

        public byte[] GetBinaryData()
        {
            // Calculate size to avoid resizing
            int size = sizeof(ushort) + 32 + (2 * sizeof(long)) + Data.Length;

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
