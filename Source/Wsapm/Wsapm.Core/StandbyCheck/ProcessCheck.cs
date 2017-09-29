using System;
using System.Diagnostics;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for checking processes.
    /// </summary>
    public sealed class ProcessCheck : IStandbyCheck
    {
        private static ProcessCheck instance;

        /// <summary>
        /// Initializes a new instance of ProcessCheck.
        /// </summary>
        private ProcessCheck()
        {
        }

        /// <summary>
        /// Gets the instance of ProcessCheck.
        /// </summary>
        public static ProcessCheck Instance
        {
            get
            {
                if (instance == null)
                    instance = new ProcessCheck();

                return instance;
            }
        }

        /// <summary>
        /// Checks if at least one of the processes in the server configuration is running.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The result as CheckSuspendResult.</returns>
        private CheckSuspendResult CheckProcesses(WsapmSettings settings)
        {
            if (!settings.EnableMonitorProcesses || settings.ProcessesToMonitor == null || settings.ProcessesToMonitor.Count == 0)
                return new CheckSuspendResult(false, String.Empty);

            for (int i = 0; i < settings.ProcessesToMonitor.Count; i++)
            {
                var result = CheckProcess(settings.ProcessesToMonitor[i]);

                if (result.SuspendStandby)
                    return result;
            }

            return new CheckSuspendResult(false, String.Empty);
        }
        
        private CheckSuspendResult CheckProcess(ProcessToMonitor process)
        {
            var p = Process.GetProcesses();

            for (int i = 0; i < p.Length; i++)
            {
                if (p[i].ProcessName.ToLower() == process.ProcessName.ToLower())
                    return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.ProcessCheck_ProcessFoundReason, p[i].ProcessName));
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
            return this.CheckProcesses(settings);
        }

        #endregion IStandbyCheck members
    }
}
