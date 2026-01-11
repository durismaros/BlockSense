using System;

namespace BlockSense.Desktop.Utilities.UIComponents
{
    /// <summary>
    /// Provides a simple utility to throttle user input and prevent rapid consecutive actions.
    /// Useful for UI buttons or controls to avoid spamming event handlers.
    /// </summary>
    public static class InputThrottler
    {
        // Timestamp of the last processed input
        private static DateTime _lastInputTime = DateTime.MinValue;

        // Minimum interval between successive inputs
        private static readonly TimeSpan Cooldown = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Determines whether the current input should be processed based on the defined cooldown.
        /// </summary>
        /// <returns><c>true</c> if the input is allowed to proceed; otherwise, <c>false</c>.</returns>
        public static bool ShouldProcess()
        {
            var now = DateTime.UtcNow;

            // Reject input if cooldown period has not elapsed
            if (now - _lastInputTime < Cooldown)
            {
                return false;
            }

            // Accept input and update last processed timestamp
            _lastInputTime = now;
            return true;
        }
    }
}
