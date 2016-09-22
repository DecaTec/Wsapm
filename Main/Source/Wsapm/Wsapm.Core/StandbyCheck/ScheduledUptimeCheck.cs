using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for checking for a scheduled uptime.
    /// </summary>
    public sealed class ScheduledUptimeCheck : IStandbyCheck
    {
        private static ScheduledUptimeCheck instance;

        /// <summary>
        /// Initializes a new instance of ScheduledUptimeCheck.
        /// </summary>
        private ScheduledUptimeCheck()
        {
        }

        /// <summary>
        /// Gets the instance of ScheduledUptimeCheck.
        /// </summary>
        public static ScheduledUptimeCheck Instance
        {
            get
            {
                if (instance == null)
                    instance = new ScheduledUptimeCheck();

                return instance;
            }
        }

        /// <summary>
        /// Checks for a scheduled uptime is running.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The result as CheckSuspendResult.</returns>
        private CheckSuspendResult CheckScheduledUptime(WsapmSettings settings)
        {
            if (!settings.EnableUptimes || settings.UptimeSchedulers == null || settings.UptimeSchedulers.Count == 0)
                return new CheckSuspendResult(false, String.Empty);

            for (int i = 0; i < settings.UptimeSchedulers.Count; i++)
            {
                var result = CheckUptime(settings.UptimeSchedulers[i]);

                if (result.SuspendStandby)
                    return result;
            }

            return new CheckSuspendResult(false, String.Empty);
        }

        private CheckSuspendResult CheckUptime(UptimeScheduler uptimeScheduler)
        {
            var now = DateTime.Now;

            if (uptimeScheduler.NextDueTimeWithDuration.HasValue && uptimeScheduler.NextDueTime.HasValue
                && uptimeScheduler.NextDueTime < now && now < uptimeScheduler.NextDueTimeWithDuration.Value)
            {
                return new CheckSuspendResult(true, string.Format(Wsapm.Core.Resources.Wsapm_Core.ScheduledUptimeCheck_UptimeDefinedReason, uptimeScheduler.NextDueTimeWithDuration.Value.ToShortTimeString()));
            }
            else
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
            return CheckScheduledUptime(settings);
        }

        #endregion IStandbyCheck members
    }
}
