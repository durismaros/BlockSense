using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.User;
using System;
using System.Collections.Generic;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface ICurrentUserProvider
    {
        UserSummaryDto Profile
        {
            get; 
        }

        IList<SessionDto> ActiveDevices
        {
            get; 
        }

        IReadOnlyList<ActivityLogDto> RecentActivity
        {
            get; 
        }

        IEnumerable<InvitationDto> Invitations
        {
            get; 
        }

        IEnumerable<string>? TwoFactorBackupCodes
        {
            get;
        }

        event Action? OnCurrentUserChanged;

        void Set(UserDashboardDto dashboard);
        void SetProfile(UserSummaryDto profile);
        void SetActiveDevices(IList<SessionDto> activeDevices);
        void SetInvitations(IEnumerable<InvitationDto> invitations);
        void SetRecentActivity(IReadOnlyList<ActivityLogDto> entries);
        void SetTwoFactorBackupCodes(IEnumerable<string>? backupCodes);
        void Clear();
    }
}