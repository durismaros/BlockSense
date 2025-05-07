using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client.Utilities
{
    class SecureWiper
    {
        public static void Wipe(ref byte[] data)
        {
            if (data == null || data.Length == 0) return;

            try
            {
                var secureRandom = new SecureRandom();

                // Overwrite 3 times (NIST standard for sensitive data)
                for (int i = 0; i < 3; i++)
                {
                    secureRandom.NextBytes(data);
                }

                // Final clear (C# intrinsic)
                Array.Clear(data, 0, data.Length);

                // Optional: Null the reference to prevent reuse
                data = null;
            }
            catch (Exception ex)
            {
                // Fail secure - if wiping fails, at least clear the array
                Array.Clear(data, 0, data.Length);
                throw new SecurityException("Secure wipe failed", ex);
            }
        }

        public static void Wipe(ref string str)
        {
            if (string.IsNullOrEmpty(str)) return;

            // Convert to char[] since strings are immutable
            char[] chars = str.ToCharArray();
            Wipe(ref chars);
            str = string.Empty; // Ensure original reference is cleared
        }

        public static void Wipe(ref char[] chars)
        {
            if (chars == null || chars.Length == 0) return;

            // Convert to bytes for cryptographic wiping
            byte[] bytes = new byte[chars.Length * sizeof(char)];
            Buffer.BlockCopy(chars, 0, bytes, 0, bytes.Length);

            // Wipe the byte version
            Wipe(ref bytes);

            // Clear the original chars
            Array.Clear(chars, 0, chars.Length);
            chars = null;
        }
    }
}
