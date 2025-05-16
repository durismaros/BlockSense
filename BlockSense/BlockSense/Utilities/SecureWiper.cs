using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Utilities
{
    class SecureWiper
    {
        public static void Wipe(ref byte[] data)
        {
            if (data == null || data.Length == 0) return;

            try
            {

                // Overwrite 3 times (NIST standard for sensitive data)
                for (int i = 0; i < 3; i++)
                {
                    new SecureRandom().NextBytes(data);
                }

                // Clear the array as final step
                Array.Clear(data, 0, data.Length);

                // Null the reference
                data = null;
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
                Array.Clear(data, 0, data.Length);
                throw;
            }
        }

        public static void Wipe(ref string str)
        {
            if (string.IsNullOrEmpty(str)) return;

            // Convert to char[] since strings are immutable
            char[] chars = str.ToCharArray();
            Wipe(ref chars);
            str = null;
        }

        public static void Wipe(ref char[] chars)
        {
            if (chars == null || chars.Length == 0) return;

            // Convert to bytes for secure wiping
            byte[] bytes = new byte[chars.Length * sizeof(char)];
            Buffer.BlockCopy(chars, 0, bytes, 0, bytes.Length);


            Wipe(ref bytes);
            // Clear the original chars
            Array.Clear(chars, 0, chars.Length);
            chars = null;
        }
    }
}
