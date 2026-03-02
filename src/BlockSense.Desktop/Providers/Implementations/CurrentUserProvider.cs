using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Providers.Interfaces;
using System;
using System.Collections.Generic;
using static BlockSense.Contracts.Definitions.ActivityActions;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class CurrentUserProvider : ICurrentUserProvider
    {
        public UserSummaryDto Profile
        {
            get;
            private set;
        }

        public IList<SessionDto> ActiveDevices
        {
            get;
            private set; 
        }

        public IReadOnlyList<InvitationDto> Invitations
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
            ActiveDevices = Array.Empty<SessionDto>();
            Invitations = Array.Empty<InvitationDto>();
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

        public void SetActiveDevices(IList<SessionDto> activeDevices)
        {
            ActiveDevices = activeDevices;

            _onCurrentUserChanged?.Invoke();
        }

        public void SetInvitations(IReadOnlyList<InvitationDto> invitations)
        {
            Invitations = invitations;

            _onCurrentUserChanged?.Invoke();
        }

        public void SetTwoFactorBackupCodes(IReadOnlyList<string>? backupCodes)
        {
            TwoFactorBackupCodes = backupCodes;

            _onCurrentUserChanged?.Invoke();
        }

        public void Clear()
        {
            Profile = default!;
            ActiveDevices = Array.Empty<SessionDto>();
            Invitations = Array.Empty<InvitationDto>();

            _onCurrentUserChanged?.Invoke();
        }
    }
}
