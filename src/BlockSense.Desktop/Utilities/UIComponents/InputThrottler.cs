using System;

namespace BlockSense.Desktop.Utilities.UIComponents
{
    public static class InputThrottler
    {
        private static DateTime _lastInputTime = DateTime.MinValue;
        private static readonly TimeSpan Cooldown = TimeSpan.FromMilliseconds(500);

        public static bool ShouldProcess()
        {
            var now = DateTime.UtcNow;
            if (now - _lastInputTime < Cooldown)
                return false;

            _lastInputTime = now;
            return true;
        }
    }
}
