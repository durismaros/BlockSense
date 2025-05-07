using BlockSenseAPI.Models;
using BlockSenseAPI.Services.Cryptography;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace BlockSenseAPI.Services
{
    public interface IUserService
    {
        Task<UserInfo> LoadUserInfo(int uid);
        Task<(bool correctLogin, string loginMessage, TokenCache tokenData)> Login(string login, string password, SystemIdentifiers identifiers);
        Task<(bool correctRegister, string registerMessage)> Register(string username, string email, string password, string invitationCode);
        Task Logout(Guid tokenId);
    }

    public class UserService : IUserService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IRefreshTokenService _tokenService;

        public UserService(DatabaseContext dbContext, IRefreshTokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<UserInfo> LoadUserInfo(int uid)
        {
            // Similar to your existing LoadUserInfo but returns a DTO
            // Implementation would query database and return UserInfo object
            string query = "select users.uid, users.username, users.email, users.type, users.password, users.salt, users.created_at, users.updated_at, users1.username as generated_by from users " +
                           "join invitationcodes ON users.invitation_code = invitationcodes.invitation_id left join users AS users1 ON invitationcodes.generated_by = users1.uid where users.uid = @uid";

            Dictionary<string, object> parameters = new()
            {
                {"@uid", uid}
            };

            var reader = await _dbContext.ExecuteReaderAsync(query, parameters);
            var userInfo = new UserInfo();
            if (await reader.ReadAsync())
            {
                if (Enum.TryParse(reader.GetString("type"), true, out UserType type))
                    userInfo = new UserInfo()
                    {
                        Uid = uid,
                        Username = reader.GetString("username"),
                        Email = reader.GetString("email"),
                        Type = type,
                        CreatedAt = reader.GetDateTime("created_at"),
                        UpdatedAt = reader.GetDateTime("updated_at"),
                        InvitingUser = reader.GetString("generated_by")
                    };

            }
            _dbContext.Dispose();

            return userInfo;
        }

        public async Task<(bool correctLogin, string loginMessage, TokenCache tokenData)> Login(string login, string password, SystemIdentifiers identifiers)
        {
            string query = "select uid, username, email, password, salt from users where (username = @login or email = @login) and type != 'banned'";
            Dictionary<string, object> parameters = new()
            {
                {"@login", login}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (await reader.ReadAsync())
                {
                    byte[] hashedPassword = new byte[32];
                    reader.GetBytes("password", 0, hashedPassword, 0, 32);

                    byte[] salt = new byte[16];
                    reader.GetBytes("salt", 0, salt, 0, 16);

                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                        
                    byte[] hashedPasswordRequest = CryptoUtils.ComputeSha256(passwordBytes, salt);

                    if (CryptographicOperations.FixedTimeEquals(hashedPassword, hashedPasswordRequest))
                    {
                        TokenCache token = _tokenService.GenerateRefreshToken(reader.GetInt32("uid"));
                        await _tokenService.StoreRefreshToken(token, identifiers);
                        return (true, "Welcome back! You’re all set", token);
                    }

                    else
                        return (false, "Oops! Wrong password entered", new TokenCache());
                }

                else
                    return (false, "Hmm, we couldn’t find your account", new TokenCache());
            }
        }

        public async Task<(bool correctRegister, string registerMessage)> Register(string username, string email, string password, string invitationCode)
        {
            if (!await InvitationCheck(invitationCode))
                return (false, "Invitation code doesn’t seem right");

            byte[] salt = CryptoUtils.SecureRandomGenerator();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashedPassword = CryptoUtils.ComputeSha256(passwordBytes, salt);

            string query = "insert into users (username, email, password, salt, invitation_code) values (@username, @email, @password, @salt, (select invitation_id from invitationcodes where invitation_code = @invitation_code))";
            Dictionary<string, object> parameters = new()
            {
                {"@username", username},
                {"@email", email},
                {"@password", hashedPassword},
                {"@salt", salt},
                {"@invitation_code", invitationCode}
            };

            if (await _dbContext.ExecuteNonQueryAsync(query, parameters) != 1)
                return (false, "We couldn’t register your account");


            query = "update InvitationCodes set is_used = TRUE where invitation_code = @invitation_code";
            if (await _dbContext.ExecuteNonQueryAsync(query, parameters) != 1)
                return (false, "We couldn’t register your account");


            return (true, "Registration complete! Welcome");
        }

        public async Task Logout(Guid tokenId)
        {
            await _tokenService.RevokeRefreshToken(tokenId);
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
                {
                    return true;
                }

                return false; // If no record is found for the invitation code
            }
        }
    }
}
