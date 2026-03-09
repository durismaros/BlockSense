using BlockSense.Backend.Models.ActivityLog;
using BlockSense.Contracts.Definitions;
using System.Text.Json;

namespace BlockSense.Backend.Utilities
{
    public static class ActivityMessageMapper
    {
        public static string Map(string action, string? contextJson)
        {
            var context = Deserialize(contextJson);

            return action switch
            {
                ActivityActions.Device.Authenticated => DeviceAuthenticated(context),
                ActivityActions.Device.Revoked => DeviceRevoked(context),

                ActivityActions.TwoFactorAuthentication.Enabled => TwoFaEnabled(context),
                ActivityActions.TwoFactorAuthentication.Disabled => TwoFaDisabled(context),
                ActivityActions.TwoFactorAuthentication.BackupCodesGenerated => "Two-factor backup codes were regenerated.",

                ActivityActions.Profile.UsernameChanged => ProfileUsernameChanged(context),
                ActivityActions.Profile.EmailChanged => ProfileEmailChanged(context),
                ActivityActions.Profile.PasswordChanged => "Account password was changed.",
                ActivityActions.Profile.PictureChanged => "Profile picture was updated.",

                ActivityActions.User.Registered => "Account was created.",
                ActivityActions.User.RoleUpdated => UserRoleUpdated(context),
                ActivityActions.User.Deleted => UserDeleted(context),
                ActivityActions.User.Restored => UserRestored(context),

                ActivityActions.Invitation.CodeGenerated => InvitationGenerated(context),
                ActivityActions.Invitation.CodeRedeemed => InvitationRedeemed(context),

                _ => action
            };
        }

        private static ActivityLogContext? Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<ActivityLogContext>(json);
            }
            catch
            {
                return null;
            }
        }

        private static string DeviceAuthenticated(ActivityLogContext? context)
        {
            if (context?.IpAddress is not string ipString)
            {
                return "A new device was authenticated.";
            }

            var ip = IpAddressMasker.Mask(ipString);
            return $"New device with IP {ip} authenticated.";
        }

        private static string DeviceRevoked(ActivityLogContext? context)
        {
            var reason = context?.Reason;
            return reason is not null
                ? $"Device session was revoked. Reason: {reason}."
                : "A device session was revoked.";
        }

        private static string TwoFaEnabled(ActivityLogContext? context)
        {
            var method = context?.TwoFactorMethod;
            return method is not null
                ? $"Two-factor authentication was enabled via {method}."
                : "Two-factor authentication was enabled.";
        }

        private static string TwoFaDisabled(ActivityLogContext? context)
        {
            var method = context?.TwoFactorMethod;
            return method is not null
                ? $"Two-factor authentication ({method}) was disabled."
                : "Two-factor authentication was disabled.";
        }

        private static string ProfileUsernameChanged(ActivityLogContext? context)
        {
            var oldVal = context?.OldValue;
            var newVal = context?.NewValue;

            return (oldVal, newVal) switch
            {
                (not null, not null) => $"Username changed from \"{oldVal}\" to \"{newVal}\".",
                (null, not null) => $"Username changed to \"{newVal}\".",
                _ => "Username was changed."
            };
        }

        private static string ProfileEmailChanged(ActivityLogContext? context)
        {
            var oldVal = context?.OldValue;
            var newVal = context?.NewValue;

            return (oldVal, newVal) switch
            {
                (not null, not null) => $"Email changed from \"{oldVal}\" to \"{newVal}\".",
                (null, not null) => $"Email changed to \"{newVal}\".",
                _ => "Email address was changed."
            };
        }

        private static string UserRoleUpdated(ActivityLogContext? context)
        {
            var oldVal = context?.OldValue;
            var newVal = context?.NewValue;
            var target = context?.TargetUserId;

            var who = target is not null ? $"User #{target}" : "A user";

            return (oldVal, newVal) switch
            {
                (not null, not null) => $"{who}'s role was changed from {oldVal} to {newVal}.",
                (null, not null) => $"{who}'s role was set to {newVal}.",
                _ => $"{who}'s role was updated."
            };
        }

        private static string UserDeleted(ActivityLogContext? context)
        {
            var target = context?.TargetUserId;
            var reason = context?.Reason;

            var who = target is not null ? $"User #{target}" : "A user account";
            var tail = reason is not null ? $" Reason: {reason}." : ".";
            return $"{who} was deleted{tail}";
        }

        private static string UserRestored(ActivityLogContext? context)
        {
            var target = context?.TargetUserId;
            return target is not null
                ? $"User #{target} was restored."
                : "A user account was restored.";
        }

        private static string InvitationGenerated(ActivityLogContext? context)
        {
            var code = context?.InvitationCode;
            return code is not null
                ? $"Invitation code \"{code}\" was generated."
                : "An invitation code was generated.";
        }

        private static string InvitationRedeemed(ActivityLogContext? context)
        {
            var code = context?.InvitationCode;
            return code is not null
                ? $"Invitation code \"{code}\" was redeemed."
                : "An invitation code was redeemed.";
        }
    }
}
