using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for ping check,
    /// </summary>
    public sealed class PingCheck : IStandbyCheck
    {
        private static PingCheck instance;

        /// <summary>
        /// Initializes a new instance of PingCheck.
        /// </summary>
        private PingCheck()
        {
        }

        /// <summary>
        /// Gets the instance of PingCheck.
        /// </summary>
        public static PingCheck Instance
        {
            get
            {
                if (instance == null)
                    instance = new PingCheck();

                return instance;
            }
        }

        /// <summary>
        /// Checks if at least one if the machines in the server configuration reacts to ping.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The result as CheckSuspendResult.</returns>
        public CheckSuspendResult CheckPing(WsapmSettings settings)
        {
            if (!settings.EnableMonitorNetworkMachines || settings.NetworkMachinesToMonitor == null || settings.NetworkMachinesToMonitor.Count == 0)
                return new CheckSuspendResult(false, String.Empty);

            for (int i = 0; i < settings.NetworkMachinesToMonitor.Count; i++)
            {
                // Check IP address based.
                if ((settings.NetworkMachinesToMonitor[i].IPAddress) != null)
                {
                    IPAddress ip = settings.NetworkMachinesToMonitor[i].IPAddress;
                    var result = Ping(ip);

                    if (result.SuspendStandby)
                        return result;
                }

                // Check machine name based.
                if (!String.IsNullOrEmpty(settings.NetworkMachinesToMonitor[i].Name))
                {
                    var result = Ping(settings.NetworkMachinesToMonitor[i].Name);

                    if (result.SuspendStandby)
                        return result;
                }
            }

            return new CheckSuspendResult(false, String.Empty);
        }

        private CheckSuspendResult Ping(IPAddress ip)
        {
            Ping p = new Ping();
            var result = p.Send(ip);

            if (result.Status == IPStatus.Success)
                return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.PingCheck_MachineOnlineReason, ip.ToString()));
            else
                return new CheckSuspendResult(false, String.Empty);
        }

        private CheckSuspendResult Ping(string computername)
        {
            var ipAddress = NetworkTools.GetIPAddressByHostname(computername);

            if (ipAddress == null)
                return new CheckSuspendResult(false, String.Empty);

            Ping p = new Ping();
            var result = p.Send(ipAddress);

            if (result.Status == IPStatus.Success)
                return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.PingCheck_MachineOnlineReason, computername));
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
            return this.CheckPing(settings);
        }

        #endregion IStandbyCheck members
    }
}
