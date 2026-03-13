using BlockSense.Backend.Models.ActivityLog;
using BlockSense.Contracts.Definitions;
using System.Text.Json;

namespace BlockSense.Backend.Utilities
{
    /// <summary>
    /// Maps activity log action codes and their optional context into human-readable messages.
    /// </summary>
    public static class ActivityMessageMapper
    {
        /// <summary>
        /// Produces a human-readable message for the given activity action and its JSON context.
        /// Returns the raw action string if no mapping is found.
        /// </summary>
        /// <param name="action">The activity action code (see <see cref="ActivityActions"/>).</param>
        /// <param name="contextJson">Optional JSON string representing the activity context.</param>
        /// <returns>A human-readable description of the activity.</returns>
        public static string Map(string action, string? contextJson)
        {
            var context = DeserializeContext(contextJson);

            return action switch
            {
                ActivityActions.Device.Authenticated => FormatDeviceAuthenticated(context),
                ActivityActions.Device.Revoked => FormatDeviceRevoked(context),

                ActivityActions.TwoFactorAuthentication.Enabled => FormatTwoFaEnabled(context),
                ActivityActions.TwoFactorAuthentication.Disabled => FormatTwoFaDisabled(context),
                ActivityActions.TwoFactorAuthentication.BackupCodesGenerated => "Two-factor backup codes were regenerated.",

                ActivityActions.Profile.UsernameChanged => FormatUsernameChanged(context),
                ActivityActions.Profile.EmailChanged => FormatEmailChanged(context),
                ActivityActions.Profile.PasswordChanged => "Account password was changed.",
                ActivityActions.Profile.PictureChanged => "Profile picture was updated.",

                ActivityActions.User.Registered => "Account was created.",
                ActivityActions.User.RoleUpdated => FormatRoleUpdated(context),
                ActivityActions.User.Deleted => FormatUserDeleted(context),
                ActivityActions.User.Restored => FormatUserRestored(context),

                ActivityActions.Invitation.CodeGenerated => FormatInvitationGenerated(context),
                ActivityActions.Invitation.CodeRedeemed => FormatInvitationRedeemed(context),

                _ => action
            };
        }

        private static ActivityLogContext? DeserializeContext(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ActivityLogContext>(json, JsonOptions.Default);
            }
            catch
            {
                return null;
            }
        }

        private static string FormatDeviceAuthenticated(ActivityLogContext? context)
        {
            if (context?.IpAddress is not string ipString)
                return "A new device was authenticated.";

            var maskedIp = IpAddressMasker.Mask(ipString);
            return $"New device with IP {maskedIp} authenticated.";
        }

        private static string FormatDeviceRevoked(ActivityLogContext? context)
        {
            var reason = context?.Reason;
            return reason is not null
                ? $"Device session was revoked. Reason: {reason}."
                : "A device session was revoked.";
        }

        private static string FormatTwoFaEnabled(ActivityLogContext? context)
        {
            var method = context?.TwoFactorMethod;
            return method is not null
                ? $"Two-factor authentication was enabled via {method}."
                : "Two-factor authentication was enabled.";
        }

        private static string FormatTwoFaDisabled(ActivityLogContext? context)
        {
            var method = context?.TwoFactorMethod;
            return method is not null
                ? $"Two-factor authentication ({method}) was disabled."
                : "Two-factor authentication was disabled.";
        }

        private static string FormatUsernameChanged(ActivityLogContext? context)
        {
            var oldValue = context?.OldValue;
            var newValue = context?.NewValue;

            return (oldValue, newValue) switch
            {
                (not null, not null) => $"Username changed from \"{oldValue}\" to \"{newValue}\".",
                (null, not null) => $"Username changed to \"{newValue}\".",
                _ => "Username was changed."
            };
        }

        private static string FormatEmailChanged(ActivityLogContext? context)
        {
            var oldValue = context?.OldValue;
            var newValue = context?.NewValue;

            return (oldValue, newValue) switch
            {
                (not null, not null) => $"Email changed from \"{oldValue}\" to \"{newValue}\".",
                (null, not null) => $"Email changed to \"{newValue}\".",
                _ => "Email address was changed."
            };
        }

        private static string FormatRoleUpdated(ActivityLogContext? context)
        {
            var oldValue = context?.OldValue;
            var newValue = context?.NewValue;
            var subject = context?.TargetUserId is uint id ? $"User #{id}" : "A user";

            return (oldValue, newValue) switch
            {
                (not null, not null) => $"{subject}'s role was changed from {oldValue} to {newValue}.",
                (null, not null) => $"{subject}'s role was set to {newValue}.",
                _ => $"{subject}'s role was updated."
            };
        }

        private static string FormatUserDeleted(ActivityLogContext? context)
        {
            var subject = context?.TargetUserId is uint id ? $"User #{id}" : "A user account";
            var suffix = context?.Reason is string reason ? $" Reason: {reason}." : ".";
            return $"{subject} was deleted{suffix}";
        }

        private static string FormatUserRestored(ActivityLogContext? context)
        {
            return context?.TargetUserId is uint id
                ? $"User #{id} was restored."
                : "A user account was restored.";
        }

        private static string FormatInvitationGenerated(ActivityLogContext? context)
        {
            return context?.InvitationCode is string code
                ? $"Invitation code \"{code}\" was generated."
                : "An invitation code was generated.";
        }

        private static string FormatInvitationRedeemed(ActivityLogContext? context)
        {
            return context?.InvitationCode is string code
                ? $"Invitation code \"{code}\" was redeemed."
                : "An invitation code was redeemed.";
        }
    }
}