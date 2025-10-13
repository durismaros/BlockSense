using BlockSenseAPI.Models.Invite;
using System.Data;

namespace BlockSenseAPI.Services.Invite
{
    public class InviteCodeService : IInviteCodeService
    {
        private readonly DatabaseContext _dbContext;

        public InviteCodeService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserInvites> FetchAllInvites(int userId)
        {
            string query = "select code, is_used, invitation_codes.created_at, expires_at, is_revoked, users.username from invitation_codes left join users on invitation_codes.invitation_code_id = users.invitation_code_id where generated_by = @user_id";
            Dictionary<string, object> parameters = new()
            {
                {"@user_id", userId}
            };

            var invites = new UserInvites();

            using var reader = await _dbContext.ExecuteReaderAsync(query, parameters);

            while (await reader.ReadAsync())
            {
                // Extract data from db reader
                var invitationCode = reader.GetString("code");
                var creationDate = reader.GetDateTime("created_at");
                var expirationDate = reader.GetDateTime("expires_at");
                var isUsed = reader.GetBoolean("is_used");
                var invitedUser = isUsed ? reader.GetString("username") : string.Empty;

                var status = isUsed ? InvitationStatus.Used : InvitationStatus.Active;
                if (reader.GetBoolean("is_revoked"))
                    status = InvitationStatus.Revoked;
                else if (DateTime.UtcNow > reader.GetDateTime("expires_at"))
                    status = InvitationStatus.Expired;

                invites.Invites.Add(new InvitationDto
                {
                    InvitationCode = invitationCode,
                    CreationDate = creationDate,
                    ExpirationDate = expirationDate,
                    InvitedUser = invitedUser,
                    IsUsed = isUsed,
                    Status = status
                });
            }

            return invites;
        }
    }
}
