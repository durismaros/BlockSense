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
            string query = "select invitationcodes.invitation_code, is_used, invitationcodes.created_at, expires_at, revoked, users.username as invited_user from invitationcodes left join users on invitation_id = users.invitation_code where generated_by = @uid";
            Dictionary<string, object> parameters = new()
            {
                {"@uid", userId}
            };
            var invites = new List<InviteInfoModel>();
            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                while (await reader.ReadAsync())
                {
                    // Extract data from db reader
                    string invitationCode = (reader.GetString("invitation_code"));
                    string creationDate = reader.GetDateTime("created_at").ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"));
                    string expirationDate = reader.GetDateTime("expires_at").ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"));
                    bool isUsed = reader.GetBoolean("is_used");
                    string invitedUser = string.Empty;
                    if (isUsed)
                        invitedUser = reader.GetString("invited_user");

                    string status = "active";
                    if (reader.GetBoolean("revoked"))
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
