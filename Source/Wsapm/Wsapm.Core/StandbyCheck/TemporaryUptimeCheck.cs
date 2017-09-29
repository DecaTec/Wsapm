using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for checking for a temporary uptime.
    /// </summary>
    public sealed class TemporaryUptimeCheck : IStandbyCheck
    {
        private static TemporaryUptimeCheck instance;
        private TemporaryUptimeManager temporaryUptimeManager;

        /// <summary>
        /// Initializes a new instance of TemporaryUptimeCheck.
        /// </summary>
        /// <param name="temporaryUptimeManager"></param>
        private TemporaryUptimeCheck(TemporaryUptimeManager temporaryUptimeManager)
        {
            this.temporaryUptimeManager = temporaryUptimeManager;
        }

        /// <summary>
        /// Gets the instance of TemporaryUptimeCheck.
        /// </summary>
        public static TemporaryUptimeCheck Instance
        {
            get
            {
                if (instance == null)
                    instance = new TemporaryUptimeCheck(new TemporaryUptimeManager());

                return instance;
            }
        }

        #region IStandbyCheck members

        /// <summary>
        /// Checks if the standby should be suspended.
        /// </summary>
        /// <param name="settings">The WsapmSettings to use.</param>
        /// <returns>The result as CheckSuspendResult.</returns>
        public CheckSuspendResult CheckStandby(WsapmSettings settings)
        {
            var temporaryUptime = this.temporaryUptimeManager.TemporaryUptimeDefinedAndActive;

            if (!temporaryUptime.HasValue)
            {
                // No temporary uptime defined.
                return new CheckSuspendResult(false, String.Empty);
            }
            else
            {
                // Temporary uptime defined and active.
                return new CheckSuspendResult(true, string.Format(Wsapm.Core.Resources.Wsapm_Core.TemporaryUptimeCheck_UptimeDefinedReason, temporaryUptime.Value.ToShortTimeString()));
            }
        }

        #endregion IStandbyCheck members
    }
}
