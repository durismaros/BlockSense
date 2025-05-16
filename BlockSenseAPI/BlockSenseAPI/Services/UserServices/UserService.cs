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
            string query = "select users.uid, users.username, users.email, users.type, users.password, users.salt, users.created_at, users.updated_at, users1.username as generated_by from users " +
                           "join invitationcodes ON users.invitation_code = invitationcodes.invitation_id left join users AS users1 ON invitationcodes.generated_by = users1.uid where users.uid = @uid";

            Dictionary<string, object> parameters = new()
            {
                {"@uid", userId}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return null;

                Enum.TryParse(reader.GetString("type"), true, out UserType type);

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
                "(select count(invitation_id) from invitationcodes where generated_by = @uid and is_used = 1 group by generated_by) as invitedusers, " +
                "(select count(distinct hardware_identifier) from refreshtokens where user_id = @uid and revoked = 0 and expires_at > now()) as active_devices, " +
                "(select 2fa_enabled from users where uid = @uid) as 2fa_enabled";
            Dictionary<string, object> parameters = new()
            {
                {"@uid", userId}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return null;

                return new AdditionalUserInfoModel
                {
                    InvitedUsers = reader.GetInt32("invitedusers"),
                    ActiveDevices = reader.GetInt32("active_devices"),
                    TwoFaEnabled = reader.GetBoolean("2fa_enabled")
                };
            }

        }

        public async Task<LoginResponseModel?> Login(LoginRequestModel request)
        {
            if (string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password) || request.Identifiers is null)
                return null;

            string query = "select uid, username, email, password, salt from users where (username = @login or email = @login) and type != 'banned'";
            Dictionary<string, object> parameters = new()
            {
                {"@login", request.Login}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                {
                    return new LoginResponseModel
                    {
                        Success = false,
                        Message = "Hmm, we couldn’t find your account",
                        RefreshToken = null,
                        AccessToken = null
                    };
                }

                byte[] hashedPassword = new byte[32];
                reader.GetBytes("password", 0, hashedPassword, 0, 32);

                byte[] salt = new byte[16];
                reader.GetBytes("salt", 0, salt, 0, 16);

                byte[] passwordBytes = Encoding.UTF8.GetBytes(request.Password);

                byte[] hashedPasswordRequest = HashingFunctions.ComputeSha256(passwordBytes, salt);

                if (CryptographicOperations.FixedTimeEquals(hashedPassword, hashedPasswordRequest))
                {
                    int userId = reader.GetInt32("uid");
                    var refreshToken = _refreshTokenService.GenerateRefreshToken(userId);
                    var accessToken = _accessTokenService.GenerateAccessToken(userId);

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
                    Message = "Oops! Wrong password entered",
                    RefreshToken = null,
                    AccessToken = null
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

            // Generate salt and hash the password
            byte[] salt = CryptographyUtils.SecureRandomGenerator();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(request.Password);
            byte[] hashedPassword = HashingFunctions.ComputeSha256(passwordBytes, salt);

            string query = "insert into users (username, email, password, salt, invitation_code) values (@username, @email, @password, @salt, (select invitation_id from invitationcodes where invitation_code = @invitation_code))";
            Dictionary<string, object> parameters = new()
            {
                {"@username", request.Username},
                {"@email", request.Email},
                {"@password", hashedPassword},
                {"@salt", salt},
                {"@invitation_code", request.InvitationCode}
            };

            if (await _dbContext.ExecuteNonQueryAsync(query, parameters) != 1 || // Insert user into the database
                await _dbContext.ExecuteNonQueryAsync("update InvitationCodes set is_used = TRUE where invitation_code = @invitation_code", parameters) != 1) // Set the invitation code as used
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
            string query = "select is_used from invitationcodes where invitation_code = @invitation_code and is_used = false and revoked = false and expires_at > now()";
            Dictionary<string, object> parameters = new()
            {
                {"@invitation_code", invitationCode}
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
