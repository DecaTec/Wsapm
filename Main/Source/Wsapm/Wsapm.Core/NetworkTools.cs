using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for networking tools.
    /// </summary>
    public static class NetworkTools
    {
        private const string CategoryNameNic = "Network Interface";

        /// <summary>
        /// Gets the available network interfaces on the machine.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAvailableNetworkInterfaces()
        {
            try
            {
                var performanceCounterCategoryNic = new PerformanceCounterCategory(CategoryNameNic);
                return performanceCounterCategoryNic.GetInstanceNames();
            }
            catch (InvalidOperationException)
            {
                // Category does not exist!
                return new string[] { };
            }
        }

        /// <summary>
        /// Determines if the given string is a valid IP address.
        /// </summary>
        /// <param name="ipStr">The IP address as string.</param>
        /// <returns>True, if the string is a valid IP address, otherwise false.</returns>
        public static bool IsStringIPAddress(string ipStr)
        {
            IPAddress ip;

            if (IPAddress.TryParse(ipStr, out ip))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the IP address of the local computer.
        /// </summary>
        /// <param name="addressFamily">The AddressFamily to use.</param>
        /// <returns>The IPAddress of the local computer or IPAddress.None if the local computer does not have an IP address.</returns>
        public static IPAddress GetLocalIPAddress(AddressFamily addressFamily)
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == addressFamily)
                    return ip;
            }

            return IPAddress.None;
        }

        /// <summary>
        /// Gets the host name from an IP address.
        /// </summary>
        /// <param name="ip">The IP address as string.</param>
        /// <returns>The host name for the given IP address or an empty string if the name could not be found (e.g. if the given string was no real IP address).</returns>
        public static string GetHostnameByIPAddress(string ip)
        {
            if (String.IsNullOrEmpty(ip) || !NetworkTools.IsStringIPAddress(ip))
                return String.Empty;

            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                return hostEntry.HostName;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the host name from an IP address.
        /// </summary>
        /// <param name="ip">The IP address as string.</param>
        /// <returns>The host name for the given IP address or an empty string if the name could not be found.</returns>
        public static string GetHostnameByIPAddress(IPAddress ip)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                return hostEntry.HostName;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the IP address from a host name.
        /// </summary>
        /// <param name="hostName">The host name.</param>
        /// <returns>The IP address for the given host name or null if the IP dress could not be found.</returns>
        public static IPAddress GetIPAddressByHostname(string hostName)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                return hostEntry.AddressList[0];
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the IP (v4) address from a host name.
        /// </summary>
        /// <param name="hostName">The host name.</param>
        /// <returns>The IP (v4) address for the given host name or null if the IP dress could not be found.</returns>
        public static IPAddress GetIP4AddressByHostname(string hostName)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                IPAddress[] ip4Adresses = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

                if (ip4Adresses == null || ip4Adresses.Length == 0)
                    return null;
                else
                    return ip4Adresses[0];
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the computers in the local network.
        /// </summary>
        /// <returns>The computers in the local network.</returns>
        /// <remarks>To find computers in the local network, this method uses the Active Directory Domain Services.</remarks>
        public static NetworkMachine[] GetComputersInLocalNetwork()
        {
            List<NetworkMachine> networkMachines = new List<NetworkMachine>();
            DirectoryEntry root = new DirectoryEntry("WinNT:");

            foreach (DirectoryEntry computers in root.Children)
            {
                foreach (DirectoryEntry computer in computers.Children)
                {
                    if (computer.Name != "Schema")
                    {
                        var networkMachine = new NetworkMachine();
                        networkMachine.Name = computer.Name;
                        networkMachine.IPAddress = GetIP4AddressByHostname(computer.Name);
                        networkMachines.Add(networkMachine);
                    }
                }
            }

            return networkMachines.ToArray();
        }

        /// <summary>
        /// Gets all network devices in a local network by IP dress range.
        /// </summary>
        /// <param name="ipAddressRange">The IP address range to scan.</param>
        /// <returns>A NetworkMachine array containing all network devices active in the local network.</returns>
        public static NetworkMachine[] GetNetworkMachinesInLocalNetworkFromIPAddressRange(IPAddress[] ipAddressRange, CancellationToken cancellationToken)
        {
            // Find active machines first, because Dns.GetHostEntry will last longer.
            ConcurrentBag<IPAddress> alive = new ConcurrentBag<IPAddress>();
            List<Task> tasks = new List<Task>();

            foreach (var ip in ipAddressRange)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var ipCopy = ip; // Work with a copy.
                var task = Task.Factory.StartNew(() =>
                {
                    var ping = new Ping();

                    if (ping.Send(ipCopy, 200).Status != IPStatus.TimedOut)
                        alive.Add(ip);
                });

                tasks.Add(task);
            }

            try
            {
                Task.WaitAll(tasks.ToArray(), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            ipAddressRange = alive.ToArray();
            ConcurrentBag<NetworkMachine> networkMachines = new ConcurrentBag<NetworkMachine>();
            tasks = new List<Task>();

            foreach (var ip in ipAddressRange)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var ipCopy = ip; // Work with a copy.
                var task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var hostEntry = Dns.GetHostEntry(ip);
                        var networkMachine = new NetworkMachine();
                        networkMachine.Name = hostEntry.HostName;
                        IPAddress[] ip4Adresses = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

                        if (ip4Adresses != null && ip4Adresses.Length > 0)
                            networkMachine.IPAddress = ip4Adresses[0];

                        networkMachines.Add(networkMachine);
                    }
                    catch (SocketException)
                    {
                    }
                });

                tasks.Add(task);
            }

            try
            {
                Task.WaitAll(tasks.ToArray(), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            return networkMachines.ToArray();
        }

        /// <summary>
        /// Gets a rage of IP addresses.
        /// </summary>
        /// <param name="ipFrom">The lowest IP address.</param>
        /// <param name="ipTo">The highest IP address.</param>
        /// <returns>An array of IPAddress containing the range of IP addresses.</returns>
        public static IPAddress[] GetIPAddressRange(IPAddress ipFrom, IPAddress ipTo)
        {
            List<IPAddress> ipAddressRange = new List<IPAddress>();
            var byteIPFrom = ipFrom.GetAddressBytes();
            var byteIPTo = ipTo.GetAddressBytes();
            int counter;

            int startIP = (
               byteIPFrom[0] << 24 |
               byteIPFrom[1] << 16 |
               byteIPFrom[2] << 8 |
               byteIPFrom[3]);

            int endIP = (
               byteIPTo[0] << 24 |
               byteIPTo[1] << 16 |
               byteIPTo[2] << 8 |
               byteIPTo[3]);

            for (counter = startIP; counter <= endIP; counter++)
            {
                var byteArr = new byte[4];
                byteArr[0] = System.Convert.ToByte((counter & 0xFF000000) >> 24);
                byteArr[1] = System.Convert.ToByte((counter & 0x00FF0000) >> 16);
                byteArr[2] = System.Convert.ToByte((counter & 0x0000FF00) >> 8);
                byteArr[3] = System.Convert.ToByte(counter & 0x000000FF);
                IPAddress ip = new IPAddress(byteArr);
                ipAddressRange.Add(ip);
            }

            return ipAddressRange.ToArray();
        }

        /// <summary>
        /// Gets a rage of IP addresses.
        /// </summary>
        /// <param name="ipFrom">The lowest IP address.</param>
        /// <param name="ipTo">The highest IP address.</param>
        /// <returns>An array of IPAddress containing the range of IP addresses.</returns>
        public static IPAddress[] GetIPAddressRange(string ipFrom, string ipTo)
        {
            return GetIPAddressRange(IPAddress.Parse(ipFrom), IPAddress.Parse(ipTo));
        }

        /// <summary>
        /// Resets all active network connections.
        /// </summary>
        public static void ResetActiveNetworkConnections()
        {
            SelectQuery query = null;

            // Property 'NetEnabled' is not available until Windows Vista.
            if (Environment.OSVersion.Version.Major >= 6)
                query = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus='2' AND NetEnabled='TRUE' AND (AdapterType = 'Ethernet 802.3' OR AdapterType = 'Wireless')");
            else
                query = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus='2' AND (AdapterType = 'Ethernet 802.3' OR AdapterType = 'Wireless')");

            ManagementObjectSearcher search = new ManagementObjectSearcher(query);
            Exception ex = null;

            foreach (ManagementObject nic in search.Get())
            {
                try
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        nic.InvokeMethod("Disable", null);
                        nic.InvokeMethod("Enable", null);
                    }
                    else
                    {
                        string connectionName = ((string)nic["NetConnectionId"]);
                        ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + connectionName + "\" disable");
                        psi.RedirectStandardOutput = true;
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        var process = Process.Start(psi);
                        process.WaitForExit();

                        psi = new ProcessStartInfo("netsh", "interface set interface \"" + connectionName + "\" enable");
                        psi.RedirectStandardOutput = true;
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        process = Process.Start(psi);
                        process.WaitForExit();
                    }
                }
                catch (Exception e)
                {
                    ex = e;
                }
            }

            if (ex != null)
                throw ex;
        }
    }
}
