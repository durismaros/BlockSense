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

        IReadOnlyList<UserSessionDto> ActiveDevices
        {
            get;
        }

        IReadOnlyList<InvitationDto> Invitations
        {
            get;
        }

        IReadOnlyList<string>? TwoFactorBackupCodes
        {
            get;
        }

        /// <summary>
        /// Raised whenever any dashboard data changes.
        /// </summary>
        event Action? OnCurrentUserChanged;

        /// <summary>
        /// Sets the dashboard data from backend response.
        /// </summary>
        /// <param name="userDashboardDto"></param>
        void Set(UserDashboardDto userDashboardDto);

        /// <summary>
        /// Sets the user summary data from backend response.
        /// </summary>
        /// <param name="userSummaryDto"></param>
        void SetProfile(UserSummaryDto userSummaryDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backupCodes"></param>
        void SetTwoFactorBackupCodes(IReadOnlyList<string>? backupCodes);

        /// <summary>
        /// Clears all stored user data (e.g. on logout).
        /// </summary>
        void Clear();
    }
}
