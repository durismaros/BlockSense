using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.DB;
using BlockSense.Server.User;
using BlockSense.Server_Based.Cryptography.Token_authentication.Refresh_Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client
{
    class SystemUtils
    {
        private static DispatcherTimer? timer;

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        public static async Task GetIPAddress()
        {
            using (var httpClient = new HttpClient())
            {
                User.ipAddress = await httpClient.GetStringAsync("https://api64.ipify.org");
            }
        }

        /// <summary>
        /// Validates session by comparing refresh tokens and their atributes
        /// </summary>
        /// <returns>status of session</returns>
        public static async Task<bool> IsSessionActive()
        {
            var (userId, refreshToken) = LocalRefreshToken.Retrieve();
            await User.LoadUserInfo(userId);
            if (InputHelper.Check(userId, refreshToken))
            {
                if (await RemoteRefreshToken.Comparison(refreshToken))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public static void StartCheckTimer()
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
        }
    }
}
