using BlockSense.Client;
using BlockSense.Client.Token_authentication;
using BlockSense.Models;
using BlockSense.Models.Requests;
using BlockSense.Models.Responses;
using BlockSense.Models.Token;
using BlockSense.Models.User;
using BlockSenseAPI.Models.TwoFactorAuth;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

        public async Task<LoginResponseModel> Login(LoginRequestModel loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponseModel>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<RegisterResponseModel> Register(RegisterRequestModel registerRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", registerRequest);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return await response.Content.ReadFromJsonAsync<RegisterResponseModel>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TokenRefreshResponseModel> TokenRefresh(TokenRefreshRequestModel comparisonRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/token-refresh", comparisonRequest);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TokenRefreshResponseModel>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<UserInfoModel> GetUserInfo()
        {
            var response = await _httpClient.GetAsync($"api/users/get");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserInfoModel>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<AdditionalUserInfoModel> GetAddUserInfo()
        {
            var response = await _httpClient.GetAsync("api/users/get-additional");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AdditionalUserInfoModel>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<List<InviteInfoModel>> FetchInviteInfo()
        {
            var response = await _httpClient.GetAsync("api/invites/fetch-all");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<InviteInfoModel>>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TwoFactorSetupResponseModel> OtpSetup()
        {
            var response = await _httpClient.GetAsync($"api/users/otp-setup");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TwoFactorSetupResponseModel>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TwoFactorVerificationResponse> CompleteOtpSetup(TwoFactorSetupRequestModel request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/otp-enable", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TwoFactorVerificationResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<TwoFactorVerificationResponse> VerifyOtp(TwoFactorVerificationRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/otp-verify", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TwoFactorVerificationResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }
    }

    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
    }
}
