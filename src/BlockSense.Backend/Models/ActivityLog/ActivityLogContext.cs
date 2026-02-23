using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlockSense.Backend.Models.ActivityLog
{
    public sealed class ActivityLogContext
    {
        [JsonPropertyName("target_user_id")]
        public uint? TargetUserId
        {
            get;
            private set;
        }

        [JsonPropertyName("ip_address")]
        public string? IpAddress
        {
            get;
            private set;
        }

        [JsonPropertyName("old_value")]
        public string? OldValue
        {
            get;
            private set;
        }

        [JsonPropertyName("new_value")]
        public string? NewValue
        {
            get;
            private set;
        }

        [JsonPropertyName("invitation_code")]
        public string? InvitationCode
        {
            get;
            private set;
        }
        
        [JsonPropertyName("two_factor_method")]
        public string? TwoFactorMethod
        {
            get;
            private set;
        }

        [JsonPropertyName("reason")]
        public string? Reason
        {
            get;
            private set;
        }

        [JsonPropertyName("error")]
        public string? Error
        {
            get;
            private set;
        }

        public ActivityLogContext WithTargetUserId(uint id)
            => this.Apply(x => x.TargetUserId = id);

        public ActivityLogContext WithIpAddress(string ip)
            => this.Apply(x => x.IpAddress = ip);

        public ActivityLogContext WithOldValue(string oldValue)
            => this.Apply(x => x.OldValue = oldValue);

        public ActivityLogContext WithNewValue(string newValue)
            => this.Apply(x => x.NewValue = newValue);

        public ActivityLogContext WithInvitationCode(string code)
            => this.Apply(x => x.InvitationCode = code);

        public ActivityLogContext WithTwoFactorMethod(string method)
            => this.Apply(x => x.TwoFactorMethod = method);

        public ActivityLogContext WithReason(string reason)
            => this.Apply(x => x.Reason = reason);

        public ActivityLogContext WithError(string error)
            => this.Apply(x => x.Error = error);

        public string ToJson()
            => JsonSerializer.Serialize(this);

        private ActivityLogContext Apply(Action<ActivityLogContext> setter)
        {
            setter.Invoke(this);
            return this;
        }
    }
}
