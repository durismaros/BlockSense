using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using BlockSense.Client.DataProtection;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.DatabaseUtils;
using BlockSense.Server;
using BlockSense.Server.Cryptography.TokenAuthentication;
using K4os.Compression.LZ4.Encoders;
using Org.BouncyCastle.Crypto.Prng;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client.Utilities
{
    class SystemUtils
    {
        private static DispatcherTimer? timer;

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        /// <summary>
        /// Validates session by comparing refresh tokens and their atributes
        /// </summary>
        /// <returns>status of session</returns>
        public static async Task<bool> IsSessionActive()
        {
            byte[] entropy = EntropyManager.RetrieveEntropy();
            var (plainToken, tokenId) = SecureTokenManager.RetrieveToken(entropy);
            try
            {
                if (await TokenUtils.Comparison(plainToken, tokenId))
                {
                    int uid = await TokenUtils.FetchUidFromToken(tokenId);
                    await User.LoadUserInfo(uid);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
                return false;
            }
        }

        public static bool CheckTimeOut()
        {
            void Timer_event(object? sender, EventArgs e)
            {
                User.Attempts = 0;
                timer?.Stop(); // Stop the timer after execution
            }

            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(10);

                timer.Tick += Timer_event; // Subscribes to the event
            }

            timer.Start();

            return User.Attempts >= 5 ? false : true; 

        }

        public static string DateTransform(DateTime date)
        {
            int day = date.Day;
            string suffix = string.Empty;
            switch (day % 10)
            {
                case 1 when day != 11:
                    suffix = "st";
                    break;

                case 2 when day != 12:
                    suffix = "nd";
                    break;

                case 3 when day != 13:
                    suffix = "rd";
                    break;

                default:
                    suffix = "th";
                    break;
            }

            return $"{date.ToString("MMMM dd", CultureInfo.GetCultureInfo("en-US"))}{suffix}, {date:yyyy}";
        }
    }
}
