using BlockSenseAPI.Models.Invite;

namespace BlockSenseAPI.Services.Invite
{
    public interface IInviteCodeService
    {
        Task<UserInvites> FetchAllInvites(int userId);
    }
}
