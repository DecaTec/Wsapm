using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for checking the CPU usage.
    /// </summary>
    public sealed class CpuLoadCheck : IStandbyCheck, IDisposable
    {
        private CpuLoadAverage cpuLoad;

        private const int NumberCheckProbes = 5;

        /// <summary>
        /// Creates a new instance of ProcessorUsageCheck.
        /// </summary>
        private CpuLoadCheck()
        {
            this.cpuLoad = new CpuLoadAverage();
        }

        /// <summary>
        /// Gets the instance of CpuLoadCheck.
        /// </summary>
        public static CpuLoadCheck GetInstance()
        {
            return new CpuLoadCheck();
        }

        /// <summary>
        /// Checks if the current CPU usage is over the CPU load value in the settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The result as CheckSuspendResult.</returns>
        public CheckSuspendResult CheckCpuLoad(WsapmSettings settings)
        {
            if (settings == null)
                return new CheckSuspendResult(false, String.Empty);

            try
            {
                if (!settings.EnableCheckCpuLoad || settings.CpuLoad == 0.0f)
                    return new CheckSuspendResult(false, String.Empty);

                var averageLoad = this.cpuLoad.GetAverageCpuLoad(NumberCheckProbes, TimeSpan.FromMilliseconds(WsapmConstants.PerformanceCounterBreakIntervallMs));
                var result = averageLoad > settings.CpuLoad;

                if (result)
                    return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.CpuLoadCheck_CpuLoadCheckReason, settings.CpuLoad, averageLoad.ToString("0")));
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
            return this.CheckCpuLoad(settings);
        }

        #endregion IStandbyCheck members

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.cpuLoad != null)
                    this.cpuLoad.Dispose();
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