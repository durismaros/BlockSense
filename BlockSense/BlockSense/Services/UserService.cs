using Avalonia.Metadata;
using BlockSense.Api;
using BlockSense.auth.DataProtection;
using BlockSense.Client;
using BlockSense.Client.Token_authentication;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.Models.Requests;
using BlockSense.Models.Responses;
using BlockSense.Models.User;
using BlockSense.Utilities;
using NBitcoin.RPC;
using Org.BouncyCastle.Math.EC.Endo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace BlockSense.Services
{
    public class UserService
    {
        private readonly ApiClient _apiClient;
        private readonly UserInfoModel _userInfo;
        private readonly AdditionalUserInfoModel _additionalUserInfo;
        private readonly RefreshTokenManager _refreshTokenManager;
        private readonly AccessTokenManager _accessTokenManager;

        public UserService(ApiClient apiClient, UserInfoModel userInfoModel, AdditionalUserInfoModel additionalUserInfoModel, RefreshTokenManager refreshTokenManager, AccessTokenManager accessTokenManager)
        {
            _apiClient = apiClient;
            _userInfo = userInfoModel;
            _additionalUserInfo = additionalUserInfoModel;
            _refreshTokenManager = refreshTokenManager;
            _accessTokenManager = accessTokenManager;
        }

        //public class AdditionalInformation
        //{
        //    public int InvitedUsers {  get; private set; }
        //    public int ActiveDevices { get; private set; }

        //    public async Task LoadAdditionalUserInfo()
        //    {
        //        await GetInvitedUsers();
        //        await GetActiveDevices();
        //    }
        //    private async Task GetInvitedUsers()
        //    {
        //        string query = "select count(invitation_id) as invitedUsers from invitationcodes where generated_by = @uid and is_used = 1 group by generated_by";
        //        Dictionary<string, object> parameters = new()
        //        {
        //            {"@uid", _userInfo.UserId}
        //        };

        //        using (var reader = await Database.FetchData(query, parameters))
        //        {
        //            if (reader.Read())
        //                InvitedUsers = reader.GetInt32("invitedUsers");
        //        }
        //    }

        //    private async Task GetActiveDevices()
        //    {
        //        string query = "select count(distinct hardware_identifier) as active_devices from refreshtokens where user_id = @uid and revoked = 0 and expires_at > now()";
        //        Dictionary<string, object> parameters = new()
        //        {
        //            {"@uid", _userInfo.UserId}
        //        };
        //        using (var reader = await Database.FetchData(query, parameters))
        //        {
        //            if (reader.Read())
        //                ActiveDevices = reader.GetInt32("active_devices");

        //        }
        //    }

        //}

        /// <summary>
        /// Loads a basic user information into memory
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<bool> LoadUserInfo()
        {
            var userInfo = await _apiClient.GetUserInfo();
            if (userInfo is null || userInfo.UserId == 0)
                return false;

            _userInfo.UserId = userInfo.UserId;
            _userInfo.Username = userInfo.Username;
            _userInfo.Email = userInfo.Email;
            _userInfo.Type = userInfo.Type;
            _userInfo.CreatedAt = userInfo.CreatedAt;
            _userInfo.UpdatedAt = userInfo.UpdatedAt;
            _userInfo.InvitingUser = userInfo.InvitingUser;

            ConsoleHelper.Log("User data fetched successfully");
            return true;
        }

        public async Task<bool> LoadAddUserInfo()
        {
            var addUserInfo = await _apiClient.GetAddUserInfo();
            if (addUserInfo is null)
                return false;

            _additionalUserInfo.InvitedUsers = addUserInfo.InvitedUsers;
            _additionalUserInfo.ActiveDevices = addUserInfo.ActiveDevices;
            _additionalUserInfo.TwoFaEnabled = addUserInfo.TwoFaEnabled;

            ConsoleHelper.Log("Additional User data fetched successfully");
            return true;
        }


        public async Task<LoginResponseModel?> Login(LoginRequestModel loginRequest)
        {
            var loginResponse = await _apiClient.Login(loginRequest);

            if (loginResponse is null)
                return null;

            if (!loginResponse.Success || loginResponse.RefreshToken is null || loginResponse.AccessToken is null)
            {
                loginResponse.RefreshToken = null;
                return loginResponse;
            }


            ConsoleHelper.Log("User logged in successfully");
            EntropyManager.StoreEntropy();

            // Store the token securely
            _refreshTokenManager.StoreToken(loginResponse.RefreshToken);
            _accessTokenManager.StoreToken(loginResponse.AccessToken);


            // Load user info
            await LoadUserInfo();
            
            return loginResponse;
            // Update your UI with user info
        }


        public async Task<(bool correctRegister, string registerMessage)> Register(RegisterRequestModel registerRequest)
        {
            var registerResponse = await _apiClient.Register(registerRequest);

            if (registerResponse.Success)
            {
                ConsoleHelper.Log("User registered in successfully");
            }

            return (registerResponse.Success, registerResponse.Message);
        }

        //public static async Task Logout()
        //{
        //    SecureTokenStorage.Delete();
        //    await TokenUtils.Revoke(RemoteRefreshToken.TokenId);
        //    EraseUserData();
        //}

        public enum UserType
        {
            User,
            Admin,
            Banned
        }
    }
}