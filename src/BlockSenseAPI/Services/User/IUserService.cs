using BlockSenseAPI.Models.Login;
using BlockSenseAPI.Models.Register;
using BlockSenseAPI.Models.User;

namespace BlockSenseAPI.Services.User
{
    public interface IUserService
    {
        Task<UserInfo?> FetchUserInfo(int userId);
        Task<AdditionalUserInfo?> FetchAddUserInfo(int userId);
        Task<LoginResponse?> Login(LoginRequest request);
        Task<RegisterResponse?> Register(Models.Requests.RegisterRequest request);
        Task Logout(Guid tokenId);
    }
}
