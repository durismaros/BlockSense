using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using BlockSense.Api;
using BlockSense.Client.Token_authentication;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.Models;
using BlockSense.Models.Requests;
using BlockSense.Models.User;
using BlockSense.Services;
using K4os.Compression.LZ4.Encoders;
using Org.BouncyCastle.Crypto.Prng;
using Splat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Utilities
{
    class SystemUtils
    {
        private readonly ApiClient _apiClient;
        private readonly UserService _userService;
        private readonly RefreshTokenManager _refreshTokenManager;
        private readonly AccessTokenManager _accessTokenManager;
        private readonly SystemIdentifierModel _systemIdents;

        public SystemUtils(ApiClient apiClient, UserService userService, RefreshTokenManager refreshTokenManager, AccessTokenManager accessTokenManager, SystemIdentifierModel systemIdents)
        {
            _apiClient = apiClient;
            _userService = userService;
            _refreshTokenManager = refreshTokenManager;
            _accessTokenManager = accessTokenManager;
            _systemIdents = systemIdents;
        }

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        /// <summary>
        /// Validates session by comparing refresh tokens and their atributes
        /// </summary>
        /// <returns>status of session</returns>
        public async Task<bool> IsSessionActive()
        {
            var cachedToken = _refreshTokenManager.RetrieveToken();

            if (cachedToken is null)
                return false;

            try
            {
                var request = new TokenRefreshRequestModel(cachedToken, _systemIdents);
                var response = await _apiClient.TokenRefresh(request);

                if (response is null)
                    return false;

                if (!response.Success || response.AccessToken is null)
                    return false;

                _accessTokenManager.StoreToken(response.AccessToken);
                var userInfo = await _userService.LoadUserInfo();
                var addUserInfo = await _userService.LoadAddUserInfo();

                if (userInfo && addUserInfo)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("Error: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> CheckServerStatus()
        {
            var response = await _apiClient.CheckStatus();
            if (response is null)
                return false;

            ConsoleLogger.Log($"Server status: " + response.Status);
            ConsoleLogger.Log("Database status: " + response.DbStatus);
            ConsoleLogger.Log("Timestamp: " + response.TimeStamp);

            if (response.Status == "Online" && response.DbStatus == "Healthy")
                return true;

            return false;
        }



        //public bool CheckTimeOut()
        //{
        //    void Timer_event(object? sender, EventArgs e)
        //    {
        //        _addUserInfo.Attempts = 0;
        //        timer?.Stop(); // Stop the timer after execution
        //    }

        //    if (timer == null)
        //    {
        //        timer = new DispatcherTimer();
        //        timer.Interval = TimeSpan.FromSeconds(10);

        //        timer.Tick += Timer_event; // Subscribes to the event
        //    }

        //    timer.Start();

        //    return _addUserInfo.Attempts >= 5 ? false : true; 

        //}

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
