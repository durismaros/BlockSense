using BlockSense.Backend.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlockSense.Backend.Models.ActivityLog
{
    /// <summary>
    /// Represents the contextual metadata associated with an activity log entry.
    /// Supports a fluent builder pattern for constructing context objects.
    /// </summary>
    public sealed class ActivityLogContext
    {
        /// <summary>Gets the target user ID involved in the activity, if applicable.</summary>
        [JsonPropertyName("target_user_id")]
        public uint? TargetUserId { get; set; }

        /// <summary>Gets the IP address associated with the activity, if applicable.</summary>
        [JsonPropertyName("ip_address")]
        public string? IpAddress { get; set; }

        /// <summary>Gets the previous value before the activity change, if applicable.</summary>
        [JsonPropertyName("old_value")]
        public string? OldValue { get; set; }

        /// <summary>Gets the new value after the activity change, if applicable.</summary>
        [JsonPropertyName("new_value")]
        public string? NewValue { get; set; }

        /// <summary>Gets the invitation code associated with the activity, if applicable.</summary>
        [JsonPropertyName("invitation_code")]
        public string? InvitationCode { get; set; }

        /// <summary>Gets the two-factor authentication method used, if applicable.</summary>
        [JsonPropertyName("two_factor_method")]
        public string? TwoFactorMethod { get; set; }

        /// <summary>Gets the reason associated with the activity, if applicable.</summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        /// <summary>Gets the error message associated with the activity, if applicable.</summary>
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        /// <summary>Sets the target user ID and returns the updated context.</summary>
        /// <param name="id">The target user ID.</param>
        /// <returns>The current <see cref="ActivityLogContext"/> instance.</returns>
        public ActivityLogContext WithTargetUserId(uint id)
            => Apply(x => x.TargetUserId = id);

        /// <summary>Sets the IP address and returns the updated context.</summary>
        /// <param name="ip">The IP address string.</param>
        /// <returns>The current <see cref="ActivityLogContext"/> instance.</returns>
        public ActivityLogContext WithIpAddress(string ip)
            => Apply(x => x.IpAddress = ip);

        /// <summary>Sets the old value and returns the updated context.</summary>
        /// <param name="oldValue">The previous value before the change.</param>
        /// <returns>The current <see cref="ActivityLogContext"/> instance.</returns>
        public ActivityLogContext WithOldValue(string oldValue)
            => Apply(x => x.OldValue = oldValue);

        /// <summary>Sets the new value and returns the updated context.</summary>
        /// <param name="newValue">The new value after the change.</param>
        /// <returns>The current <see cref="ActivityLogContext"/> instance.</returns>
        public ActivityLogContext WithNewValue(string newValue)
            => Apply(x => x.NewValue = newValue);

        /// <summary>Sets the invitation code and returns the updated context.</summary>
        /// <param name="code">The invitation code.</param>
        /// <returns>The current <see cref="ActivityLogContext"/> instance.</returns>
        public ActivityLogContext WithInvitationCode(string code)
            => Apply(x => x.InvitationCode = code);

        /// <summary>Sets the two-factor authentication method and returns the updated context.</summary>
        /// <param name="method">The two-factor method name (e.g., "TOTP").</param>
        /// <returns>The current <see cref="ActivityLogContext"/> instance.</returns>
        public ActivityLogContext WithTwoFactorMethod(string method)
            => Apply(x => x.TwoFactorMethod = method);

        /// <summary>Sets the reason and returns the updated context.</summary>
        /// <param name="reason">A description of the reason for the activity.</param>
        /// <returns>The current <see cref="ActivityLogContext"/> instance.</returns>
        public ActivityLogContext WithReason(string reason)
            => Apply(x => x.Reason = reason);

        /// <summary>Sets the error message and returns the updated context.</summary>
        /// <param name="error">The error message associated with the activity.</param>
        /// <returns>The current <see cref="ActivityLogContext"/> instance.</returns>
        public ActivityLogContext WithError(string error)
            => Apply(x => x.Error = error);

        /// <summary>
        /// Serializes this context to a JSON string.
        /// </summary>
        /// <returns>A JSON representation of the activity log context.</returns>
        public string ToJson()
            => JsonSerializer.Serialize(this, JsonOptions.Default);

        private ActivityLogContext Apply(Action<ActivityLogContext> setter)
        {
            setter(this);
            return this;
        }
    }
}