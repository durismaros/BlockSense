using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Utilities.UI
{
    public class AsyncDebouncer
    {
        private readonly Dictionary<string, DateTime> _lastExecutionTimes = new();
        private readonly Dictionary<string, bool> _executionFlags = new();
        private readonly int _debounceMilliseconds;

        public AsyncDebouncer(int debounceMilliseconds = 1000)
        {
            _debounceMilliseconds = debounceMilliseconds;
        }

        /// <summary>
        /// Executes an async action with debouncing
        /// </summary>
        /// <param name="key">Unique key for the operation</param>
        /// <param name="action">The async action to execute</param>
        /// <returns>True if executed, false if debounced</returns>
        public async Task<bool> TryExecuteAsync(string key, Func<Task> action)
        {
            // Check if already executing
            if (_executionFlags.ContainsKey(key) && _executionFlags[key])
                return false;

            // Check debounce timing
            var now = DateTime.Now;
            if (_lastExecutionTimes.ContainsKey(key))
            {
                var timeSinceLastExecution = now - _lastExecutionTimes[key];
                if (timeSinceLastExecution.TotalMilliseconds < _debounceMilliseconds)
                    return false;
            }

            // Update tracking
            _lastExecutionTimes[key] = now;
            _executionFlags[key] = true;

            try
            {
                await action();
                return true;
            }
            finally
            {
                _executionFlags[key] = false;
            }
        }

        /// <summary>
        /// Executes an async function with debouncing
        /// </summary>
        /// <param name="key">Unique key for the operation</param>
        /// <param name="func">The async function to execute</param>
        /// <returns>The result if executed, default(T) if debounced</returns>
        public async Task<(bool executed, T result)> TryExecuteAsync<T>(string key, Func<Task<T>> func)
        {
            // Check if already executing
            if (_executionFlags.ContainsKey(key) && _executionFlags[key])
                return (false, default(T)!);

            // Check debounce timing
            var now = DateTime.Now;
            if (_lastExecutionTimes.ContainsKey(key))
            {
                var timeSinceLastExecution = now - _lastExecutionTimes[key];
                if (timeSinceLastExecution.TotalMilliseconds < _debounceMilliseconds)
                    return (false, default(T)!);
            }

            // Update tracking
            _lastExecutionTimes[key] = now;
            _executionFlags[key] = true;

            try
            {
                var result = await func();
                return (true, result);
            }
            finally
            {
                _executionFlags[key] = false;
            }
        }
    }
}