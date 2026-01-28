using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Providers.Interfaces;
using System;
using System.Collections.Generic;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class CurrentUserProvider : ICurrentUserProvider
    {
        public UserSummaryDto Profile
        {
            get;
            private set;
        }

        public IReadOnlyList<UserTokenSessionDto> ActiveDevices
        {
            get;
            private set; 
        }

        public IReadOnlyList<InvitationCodeDto> Invitations
        {
            get;
            private set;
        }

        public event Action? Changed;

        public CurrentUserProvider()
        {
            Profile = default!;
            ActiveDevices = Array.Empty<UserTokenSessionDto>();
            Invitations = Array.Empty<InvitationCodeDto>();
        }

        public void Set(UserDashboardDto dto)
        {
            Profile = dto.Profile;
            ActiveDevices = dto.ActiveTokens;
            Invitations = dto.UserInvitations;

            Changed?.Invoke();
        }

        public void Clear()
        {
            Profile = default!;
            ActiveDevices = Array.Empty<UserTokenSessionDto>();
            Invitations = Array.Empty<InvitationCodeDto>();

            Changed?.Invoke();
        }
    }
}
