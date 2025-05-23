using BlockSenseAPI.Models;
using System.Data;
using System.Globalization;

namespace BlockSenseAPI.Services
{
    public interface IInviteCodeService
    {
        Task<List<InviteInfoModel>> FetchAllInvites(int userId);
    }
    public class InviteCodeService : IInviteCodeService
    {
        private readonly DatabaseContext _dbContext;

        public InviteCodeService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<InviteInfoModel>> FetchAllInvites(int userId)
        {
            string query = "select code, is_used, invitation_codes.created_at, expires_at, is_revoked, users.username from invitation_codes left join users on invitation_codes.invitation_code_id = users.invitation_code_id where generated_by = @user_id";
            Dictionary<string, object> parameters = new()
            {
                {"@user_id", userId}
            };
            var invites = new List<InviteInfoModel>();
            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                while (await reader.ReadAsync())
                {
                    // Extract data from db reader
                    string invitationCode = reader.GetString("code");
                    string creationDate = reader.GetDateTime("created_at").ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"));
                    string expirationDate = reader.GetDateTime("expires_at").ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);
                    bool isUsed = reader.GetBoolean("is_used");
                    string invitedUser = isUsed ? reader.GetString("username") : string.Empty;

                    string status = isUsed ? "used" : "active";
                    if (reader.GetBoolean("is_revoked"))
                        status = "revoked";
                    else if (DateTime.UtcNow > reader.GetDateTime("expires_at"))
                        status = "expired";

                    invites.Add(new InviteInfoModel
                    {
                        InvitationCode = invitationCode,
                        CreationDate = creationDate,
                        ExpirationDate = expirationDate,
                        InvitedUser = invitedUser,
                        IsUsed = isUsed,
                        Status = status
                    });
                }
            }

            return invites;
        }
    }
}
