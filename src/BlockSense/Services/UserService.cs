using BlockSense.Api;
using BlockSense.auth.DataProtection;
using BlockSense.Client.TokenAuthentication;
using BlockSense.Client_Side.TokenAuthentication;
using BlockSense.Models.Login;
using BlockSense.Models.Register;
using BlockSense.Models.User;
using BlockSense.Utilities.Logging;
using System.Threading.Tasks;

namespace BlockSense.Services
{
    public class UserService
    {
        private readonly ApiClient _apiClient;
        private readonly UserInfo _userInfo;
        private readonly AdditionalUserInfo _additionalUserInfo;
        private readonly RefreshTokenManager _refreshTokenManager;
        private readonly AccessTokenManager _accessTokenManager;

        public UserService(ApiClient apiClient, UserInfo userInfoModel, AdditionalUserInfo additionalUserInfoModel, RefreshTokenManager refreshTokenManager, AccessTokenManager accessTokenManager)
        {
            _apiClient = apiClient;
            _userInfo = userInfoModel;
            _additionalUserInfo = additionalUserInfoModel;
            _refreshTokenManager = refreshTokenManager;
            _accessTokenManager = accessTokenManager;
        }

        /// <summary>
        /// Loads a basic user information into memory
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<bool> LoadUserInfo()
        {
            var userInfo = await _apiClient.GetUserInfo();

            if (userInfo is null)
                return false;

            _userInfo.UserId = userInfo.UserId;
            _userInfo.Username = userInfo.Username;
            _userInfo.Email = userInfo.Email;
            _userInfo.Type = userInfo.Type;
            _userInfo.CreatedAt = userInfo.CreatedAt;
            _userInfo.UpdatedAt = userInfo.UpdatedAt;
            _userInfo.InvitingUser = userInfo.InvitingUser;

            ConsoleLogger.Log("User data fetched successfully");
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

            ConsoleLogger.Log("Additional User data fetched successfully");
            return true;
        }


        public async Task<LoginResponse?> Login(LoginRequest loginRequest)
        {
            var loginResponse = await _apiClient.Login(loginRequest);

            if (loginResponse is null)
            {
                ConsoleLogger.Log("Error occurred");
                return null;
            }

            if (!loginResponse.Success || loginResponse.RefreshToken is null || loginResponse.AccessToken is null || loginResponse.TwoFactorRequired)
                return loginResponse;

            EntropyManager.StoreEntropy();

            // Store the token securely
            _refreshTokenManager.StoreToken(loginResponse.RefreshToken);
            _accessTokenManager.StoreToken(loginResponse.AccessToken);


            // Load user info
            await LoadUserInfo();
            await LoadAddUserInfo();

            ConsoleLogger.Log("User logged in successfully");
            return loginResponse;
            // Update your UI with user info
        }


        public async Task<RegisterResponse?> Register(RegisterRequest registerRequest)
        {
            var registerResponse = await _apiClient.Register(registerRequest);

            if (registerResponse is null)
            {
                ConsoleLogger.Log("Error occurred");
                return null;
            }

            if (registerResponse.Success)
                ConsoleLogger.Log("User registered in successfully");

            return registerResponse;
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