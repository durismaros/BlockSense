using System;
using System.Collections.Generic;
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
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        public static void OpenConsole()
        {
            AllocConsole();
        }
    }
}
