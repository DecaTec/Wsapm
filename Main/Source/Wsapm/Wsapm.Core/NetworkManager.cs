using System;
using System.Management;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for network management.
    /// </summary>
    internal sealed class NetworkManager
    {
        private WsapmLog log;

        /// <summary>
        /// Creates a new instance of NetworkManager.
        /// </summary>
        /// <param name="log"></param>
        internal NetworkManager(WsapmLog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Resets all network connections (LAN/WLAN).
        /// </summary>
        internal void ResetNetworkConnections()
        {
            try
            {
                this.log.WriteLine(Resources.Wsapm_Core.NetworkManager_ResetNetworkConnections, LogMode.Normal);
                SelectQuery query = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus='2' AND NetEnabled='TRUE' AND (AdapterType = 'Ethernet 802.3' OR AdapterType = 'Wireless')");
                ManagementObjectSearcher search = new ManagementObjectSearcher(query);

                foreach (ManagementObject nic in search.Get())
                {
                    string nicName = String.Empty;

                    try
                    {
                        nicName = nic.GetPropertyValue("Description") as string;
                        nic.InvokeMethod("Disable", null);
                        nic.InvokeMethod("Enable", null);
                        this.log.WriteLine(String.Format(Resources.Wsapm_Core.NetworkManager_ResetNetworkInterface, nicName), LogMode.Verbose);
                    }
                    catch (Exception ex)
                    {
                        this.log.WriteError(String.Format(Resources.Wsapm_Core.NetworkManager_ResetNetworkInterfaceFailed, nicName), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                this.log.WriteError(Resources.Wsapm_Core.NetworkManager_ResetNetworkInterfaceGeneralError, ex);
            }
        }
    }
}
