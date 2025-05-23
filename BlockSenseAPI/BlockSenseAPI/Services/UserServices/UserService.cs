using BlockSense.Cryptography.Hashing;
using BlockSense.Models.Responses;
using BlockSenseAPI.Cryptography;
using BlockSenseAPI.Models.Requests;
using BlockSenseAPI.Models.Responses;
using BlockSenseAPI.Models.User;
using BlockSenseAPI.Services.TokenServices;
using MaxMind.Db;
using Microsoft.AspNetCore.Identity.Data;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Security.Certificates;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace BlockSenseAPI.Services.UserServices
{
    public interface IUserService
    {
        Task<UserInfoModel?> FetchUserInfo(int userId);
        Task<AdditionalUserInfoModel?> FetchAddUserInfo(int userId);
        Task<LoginResponseModel?> Login(LoginRequestModel request);
        Task<RegisterResponseModel?> Register(RegisterRequestModel request);
        Task Logout(Guid tokenId);
    }

    public class UserService : IUserService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAccessTokenService _accessTokenService;

        public UserService(DatabaseContext dbContext, IRefreshTokenService refreshTokenService, IAccessTokenService accessTokenService)
        {
            _dbContext = dbContext;
            _refreshTokenService = refreshTokenService;
            _accessTokenService = accessTokenService;
        }

        public async Task<UserInfoModel?> FetchUserInfo(int userId)
        {
            // Similar to your existing LoadUserInfo but returns a DTO
            // Implementation would query database and return UserInfo object
            string query = "select users.user_id, users.username, users.email, users.user_type, users.password_hash, users.password_salt, users.created_at, users.updated_at, invitation_user.username as generated_by from users " +
                "join invitation_codes on users.invitation_code_id = invitation_codes.invitation_code_id left join users as invitation_user on invitation_codes.generated_by = invitation_user.user_id where users.user_id = @user_id";

            Dictionary<string, object> parameters = new()
            {
                {"@user_id", userId}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return null;

                Enum.TryParse(reader.GetString("user_type"), true, out UserType type);

                return new UserInfoModel
                {
                    UserId = userId,
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Type = type,
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at"),
                    InvitingUser = reader.GetString("generated_by")
                };
            }
        }

        public async Task<AdditionalUserInfoModel?> FetchAddUserInfo(int userId)
        {
            string query = "select " +
                "(select count(invitation_code_id) from invitation_codes where generated_by = @user_id and is_used = true group by generated_by) as invited_users, " +
                "(select count(distinct hardware_fingerprint) from refresh_tokens where user_id = @user_id and is_revoked = false and expires_at > now()) as active_devices, " +
                "(select is_2fa_enabled from user_authentication where user_id = @user_id) as 2fa_enabled";
            Dictionary<string, object> parameters = new()
            {
                {"@user_id", userId}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return null;

                return new AdditionalUserInfoModel
                {
                    InvitedUsers = reader.IsDBNull(reader.GetOrdinal("invited_users")) ? 0 : reader.GetInt32("invited_users"),
                    ActiveDevices = reader.GetInt32("active_devices"),
                    TwoFaEnabled = reader.IsDBNull(reader.GetOrdinal("2fa_enabled"))
                    ? false : reader.GetBoolean("2fa_enabled")
                };
            }

        }

        public async Task<LoginResponseModel?> Login(LoginRequestModel request)
        {
            if (string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password) || request.Identifiers is null)
                return null;

            string query = "select user_id, password_hash, password_salt from users where (username = @login or email = @login) and user_type != 'banned'";
            Dictionary<string, object> parameters = new()
            {
                {"@login", request.Login}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return new LoginResponseModel
                    {
                        Success = false,
                        Message = "Hmm, we couldn’t find your account"
                    };

                byte[] hashedPassword = new byte[32];
                reader.GetBytes("password_hash", 0, hashedPassword, 0, 32);

                byte[] salt = new byte[16];
                reader.GetBytes("password_salt", 0, salt, 0, 16);

                byte[] passwordBytes = Encoding.UTF8.GetBytes(request.Password);

                byte[] hashedPasswordRequest = HashingFunctions.ComputeSha256(passwordBytes, salt);

                if (CryptographicOperations.FixedTimeEquals(hashedPassword, hashedPasswordRequest))
                {
                    int userId = reader.GetInt32("user_id");
                    var refreshToken = _refreshTokenService.GenerateRefreshToken(userId);
                    var accessToken = _accessTokenService.GenerateAccessToken(userId);

                    await reader.DisposeAsync();

                    await _refreshTokenService.StoreRefreshToken(new TokenRefreshRequestModel
                    {
                        RefreshToken = refreshToken,
                        Identifiers = request.Identifiers,
                    });

                    return new LoginResponseModel
                    {
                        Success = true,
                        Message = "Welcome back! You’re all set",
                        RefreshToken = refreshToken,
                        AccessToken = accessToken
                    };
                }

                return new LoginResponseModel
                {
                    Success = false,
                    Message = "Oops! Wrong password entered"
                };
            }
        }

        public async Task<RegisterResponseModel?> Register(RegisterRequestModel request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.InvitationCode))
                return null;

            // Check if the invitation code is valid and unused
            if (!await InvitationCheck(request.InvitationCode))
                return new RegisterResponseModel
                {
                    Success = false,
                    Message = "Invitation code doesn’t seem right"
                };

            if (Zxcvbn.Core.EvaluatePassword(request.Password).Score < 3)
                return new RegisterResponseModel
                {
                    Success = false,
                    Message = "Too weak! Try a stronger password"
                };

            // Generate salt and hash the password
            byte[] salt = CryptographyUtils.SecureRandomGenerator();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(request.Password);
            byte[] hashedPassword = HashingFunctions.ComputeSha256(passwordBytes, salt);

            string query = "insert into users (username, email, password_hash, password_salt, invitation_code_id) values (@username, @email, @password_hash, @password_salt, (select invitation_code_id from invitation_codes where code = @code))";
            Dictionary<string, object> parameters = new()
            {
                {"@username", request.Username},
                {"@email", request.Email},
                {"@password_hash", hashedPassword},
                {"@password_salt", salt},
                {"@code", request.InvitationCode}
            };

            if (await _dbContext.ExecuteNonQueryAsync(query, parameters) != 1 || // Insert user into the database
                await _dbContext.ExecuteNonQueryAsync("update invitation_codes set is_used = true where code = @code", parameters) != 1) // Set the invitation code as used
                return null; // Request not handled correctly

            return new RegisterResponseModel
            {
                Success = true,
                Message = "Registration complete! Welcome"
            };
        }

        public async Task Logout(Guid tokenId)
        {
            await _refreshTokenService.RevokeRefreshToken(tokenId);
        }


        private async Task<bool> InvitationCheck(string invitationCode)
        {
            string query = "select is_used from invitation_codes where (code = @code and is_used = false and is_revoked = false and expires_at > now())";
            Dictionary<string, object> parameters = new()
            {
                {"@code", invitationCode}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (reader.Read())
                    return true;

                return false; // If no record is found for the invitation code
            }
        }
    }
}
