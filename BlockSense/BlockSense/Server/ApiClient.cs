using BlockSense.Client;
using BlockSense.Models.Responses;
using BlockSenseAPI.Models;
using BlockSenseAPI.Models.Requests;
using NBitcoin.RPC;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiClient()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://localhost:7058/")
            };

        }

        public async Task<UserInfo> LoadUserInfo(int uid)
        {
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"api/users/{uid}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserInfo>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<LoginResponse> Login(string login, string password, SystemIdentifiers identifiers)
        {
            var request = new LoginRequest { Login = login, Password = password, Identifiers = identifiers};
            var response = await _httpClient.PostAsJsonAsync("api/users/login", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }

        public async Task<RegisterResponse> Register(string username, string email, string password, string invitationCode)
        {
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password,
                InvitationCode = invitationCode
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/users/register", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RegisterResponse>();
            }

            throw new ApiException(await response.Content.ReadAsStringAsync());
        }
    }

    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
    }
}
