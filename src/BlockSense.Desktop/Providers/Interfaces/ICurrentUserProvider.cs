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

        IReadOnlyList<UserTokenSessionDto> ActiveDevices
        {
            get;
        }

        IReadOnlyList<InvitationCodeDto> Invitations
        {
            get;
        }

        /// <summary>
        /// Raised whenever any dashboard data changes.
        /// </summary>
        event Action? Changed;

        /// <summary>
        /// Sets the dashboard data from backend response.
        /// </summary>
        /// <param name="dto"></param>
        void Set(UserDashboardDto dto);

        /// <summary>
        /// Clears all stored user data (e.g. on logout).
        /// </summary>
        void Clear();
    }
}
