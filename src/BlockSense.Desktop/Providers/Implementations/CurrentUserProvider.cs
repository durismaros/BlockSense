using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public IReadOnlyList<ActivityLogDto> RecentActivity
        {
            get;
            private set;
        }

        public IEnumerable<InvitationDto> Invitations
        {
            get;
            private set;
        }

        public IEnumerable<string>? TwoFactorBackupCodes
        {
            get;
            private set;
        }

        public event Action? OnCurrentUserChanged
        {
            add
            {
                _onCurrentUserChanged += value; value?.Invoke();
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
            RecentActivity = Array.Empty<ActivityLogDto>();
            Invitations = Array.Empty<InvitationDto>();
        }

        public void Set(UserDashboardDto dashboard)
        {
            Profile = dashboard.Profile;
            ActiveDevices = dashboard.ActiveTokens.ToList();
            RecentActivity = dashboard.RecentActivity.ToList();
            Invitations = dashboard.UserInvitations;
            _onCurrentUserChanged?.Invoke();
        }

        public void SetProfile(UserSummaryDto profile)
        {
            Profile = profile;
            _onCurrentUserChanged?.Invoke();
        }

        public void SetActiveDevices(IList<SessionDto> activeDevices)
        {
            ActiveDevices = activeDevices;
            _onCurrentUserChanged?.Invoke();
        }

        public void SetInvitations(IEnumerable<InvitationDto> invitations)
        {
            Invitations = invitations;
            _onCurrentUserChanged?.Invoke();
        }

        public void SetRecentActivity(IReadOnlyList<ActivityLogDto> entries)
        {
            RecentActivity = entries;
            _onCurrentUserChanged?.Invoke();
        }

        public void SetTwoFactorBackupCodes(IEnumerable<string>? backupCodes)
        {
            TwoFactorBackupCodes = backupCodes;
        }

        public void Clear()
        {
            Profile = default!;
            ActiveDevices = Array.Empty<SessionDto>();
            RecentActivity = Array.Empty<ActivityLogDto>();
            Invitations = Array.Empty<InvitationDto>();
            _onCurrentUserChanged?.Invoke();
        }
    }
}