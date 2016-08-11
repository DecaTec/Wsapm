using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for checking the memory usage.
    /// </summary>
    public sealed class MemoryLoadCheck : IStandbyCheck, IDisposable
    {
        private MemoryLoadAverage memoryLoad;

        private const int NumberCheckProbes = 5;

        /// <summary>
        /// Creates a new instance of MemoryLoadCheck.
        /// </summary>
        private MemoryLoadCheck()
        {
            this.memoryLoad = new MemoryLoadAverage();
        }

        /// <summary>
        /// Gets the instance of MemoryLoadCheck.
        /// </summary>
        public static MemoryLoadCheck GetInstance()
        {
            return new MemoryLoadCheck();
        }

        /// <summary>
        /// Checks if the current memory usage is over the menory load value in the settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The result as CheckSuspendResult.</returns>
        public CheckSuspendResult CheckMemoryLoad(WsapmSettings settings)
        {
            if (settings == null)
                return new CheckSuspendResult(false, String.Empty);

            try
            {
                if (!settings.EnableCheckMemoryLoad || settings.MemoryLoad == 0.0f)
                    return new CheckSuspendResult(false, String.Empty);

                var averageLoad = this.memoryLoad.GetAverageMemoryLoad(NumberCheckProbes, TimeSpan.FromMilliseconds(WsapmConstants.PerformanceCounterBreakIntervallMs));
                var result = averageLoad > settings.MemoryLoad;

                if (result)
                    return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.MemoryLoadCheck_MemoryLoadCheckReason, settings.MemoryLoad, averageLoad.ToString("0")));
                else
                    return new CheckSuspendResult(false, String.Empty);
            }
            catch (Exception ex)
            {
                // Might happen if the performance counters get broken.
                WsapmLog.Log.WriteError(ex);
            }

            return new CheckSuspendResult(false, String.Empty);
        }

        #region IStandbyCheck members

        /// <summary>
        /// Checks if the standby should be suspended.
        /// </summary>
        /// <param name="settings">The WsapmSettings to use.</param>
        /// <returns>The result as CheckSuspendResult.</returns>
        public CheckSuspendResult CheckStandby(WsapmSettings settings)
        {
            return this.CheckMemoryLoad(settings);
        }

        #endregion IStandbyCheck members

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.memoryLoad != null)
                    this.memoryLoad.Dispose();
            }
        }

        #region IDisposable members

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable members
    }
}
