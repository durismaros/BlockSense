using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Utilities.Logging
{
    /// <summary>
    /// Provides simple logging functionality to output timestamped messages to the console.
    /// </summary>
    public static class ConsoleLogger
    {
        /// <summary>
        /// Writes a message to the console prefixed with the current timestamp.
        /// </summary>
        /// <param name="input">The message to log. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
        /// <remarks>The timestamp format used is "yyyy-MM-dd HH:mm:ss".</remarks>
        public static void Log(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[{currentTime}] {input}");
        }
    }
}
