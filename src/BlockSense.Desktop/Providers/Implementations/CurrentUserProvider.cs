using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.DTOs.TokenSession;
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

        public IReadOnlyList<UserSessionDto> ActiveDevices
        {
            get;
            private set; 
        }

        public IReadOnlyList<InvitationCodeDto> Invitations
        {
            get;
            private set;
        }

        public IReadOnlyList<string>? TwoFactorBackupCodes
        {
            get;
            private set;
        }

        public event Action? OnCurrentUserChanged
        {
            add
            {
                _onCurrentUserChanged += value;
                value?.Invoke();
            }
            remove
            {
                _onCurrentUserChanged -= value;
            }
        }

        private Action? _onCurrentUserChanged;

        public CurrentUserProvider()
        {
            Profile = default!;
            ActiveDevices = Array.Empty<UserSessionDto>();
            Invitations = Array.Empty<InvitationCodeDto>();
            TwoFactorBackupCodes = null;
        }

        public void Set(UserDashboardDto userDashboardDto)
        {
            Profile = userDashboardDto.Profile;
            ActiveDevices = userDashboardDto.ActiveTokens;
            Invitations = userDashboardDto.UserInvitations;

            _onCurrentUserChanged?.Invoke();
        }

        public void SetProfile(UserSummaryDto userSummaryDto)
        {
            Profile = userSummaryDto;

            _onCurrentUserChanged?.Invoke();
        }

        public void SetTwoFactorBackupCodes(IReadOnlyList<string>? backupCodes)
        {
            TwoFactorBackupCodes = backupCodes;
        }

        public void Clear()
        {
            Profile = default!;
            ActiveDevices = Array.Empty<UserSessionDto>();
            Invitations = Array.Empty<InvitationCodeDto>();

            _onCurrentUserChanged?.Invoke();
        }
    }
}
