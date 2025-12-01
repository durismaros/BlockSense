using BlockSenseAPI.Models.Login;
using BlockSenseAPI.Models.Register;
using BlockSenseAPI.Models.Requests;
using BlockSenseAPI.Models.User;

namespace BlockSenseAPI.Services.User
{
    public interface IUserService
    {
        Task<UserInfo?> FetchUserInfoAsync(int userId);
        Task<AdditionalUserInfo?> FetchAddUserInfoAsync(int userId);
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
        Task LogoutAsync(Guid tokenId);
    }
}
