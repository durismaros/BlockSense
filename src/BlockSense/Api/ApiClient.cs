using BlockSense.Client.TokenAuthentication;
using BlockSense.Models;
using BlockSense.Models.Invite;
using BlockSense.Models.Login;
using BlockSense.Models.Register;
using BlockSense.Models.Token.DTO;
using BlockSense.Models.TwoFactorAuth.BackupCode;
using BlockSense.Models.TwoFactorAuth.Setup;
using BlockSense.Models.TwoFactorAuth.Verification;
using BlockSense.Models.User;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlockSense.Api
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AccessTokenManager _accessTokenManager;

        public ApiClient(HttpClient httpClient, AccessTokenManager accessTokenManager)
        {
            _httpClient = httpClient;
            _accessTokenManager = accessTokenManager;
        }

        public async Task<ServerStatus?> CheckStatus()
        {
            var response = await _httpClient.GetAsync($"api/status/check");

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return await response.Content.ReadFromJsonAsync<ServerStatus>();
            }

            return new ServerStatus
            {
                Status = "not accessible",
                DbStatus = "unknown",
                TimeStamp = DateTime.UtcNow.ToString("o")
            };
        }

        public async Task<LoginResponse?> Login(LoginRequest loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<RegisterResponse?> Register(RegisterRequest registerRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", registerRequest);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return await response.Content.ReadFromJsonAsync<RegisterResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TokenRefreshResponse?> TokenRefresh(TokenRefreshRequest comparisonRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/token-refresh", comparisonRequest);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TokenRefreshResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<UserInfo?> GetUserInfo()
        {
            var response = await _httpClient.GetAsync("api/users/get");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserInfo>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<AdditionalUserInfo?> GetAddUserInfo()
        {
            var response = await _httpClient.GetAsync("api/users/get-additional");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AdditionalUserInfo>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<UserInvites?> FetchInviteInfo()
        {
            var response = await _httpClient.GetAsync("api/invites/fetch-all");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserInvites>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TwoFactorSetupResponse?> BeginSetup()
        {
            var response = await _httpClient.GetAsync("api/2fa/setup");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TwoFactorSetupResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TwoFactorVerificationResponse?> CompleteSetup(TwoFactorSetupRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/2fa/enable", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TwoFactorVerificationResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TwoFactorVerificationResponse?> VerifyOtp(TwoFactorVerificationRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/2fa/verify", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TwoFactorVerificationResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TwoFactorVerificationResponse?> DisableTwoFa(TwoFactorVerificationRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/2fa/disable", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TwoFactorVerificationResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TwoFactorBackupResponse?> GenerateBackupCodes()
        {
            var response = await _httpClient.GetAsync("api/2fa/backup-generation");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TwoFactorBackupResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }
    }

    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
    }
}
