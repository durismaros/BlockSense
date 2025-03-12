using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense
{
    public static class ConsoleHelper
    {
        public static void WriteLine(string input)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[{currentTime}] {input}");
        }
    }
}
