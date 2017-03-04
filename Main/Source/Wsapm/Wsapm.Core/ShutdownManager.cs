using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Wsapm.Core
{
    /// <summary>
    /// Class handling shutdown/hibernate/standby on Magic Packet.
    /// </summary>
    public sealed class ShutdownManager : IDisposable
    {
        private UdpClient updClient;
        private ushort port;
        private bool disposed = false;
        private string expectedPassword;

        public void Start(ushort port, string expectedPassword)
        {
            Stop();
            this.port = port;
            this.expectedPassword = expectedPassword;
            this.updClient = new UdpClient(port);
            this.updClient.BeginReceive(DataReceived, this.updClient);
            WsapmLog.Log.WriteLine(String.Format(Resources.Wsapm_Core.ShutdownManager_StartListening, port), LogMode.Verbose);
        }

        public void Stop()
        {
            if (this.updClient != null)
            {               
                this.updClient.Close();
                this.updClient = null;
                WsapmLog.Log.WriteLine(String.Format(Resources.Wsapm_Core.ShutdownManager_StopListening, this.port), LogMode.Verbose);
            }
        }

        private void DataReceived(IAsyncResult ar)
        {
            try
            {
                if (this.updClient == null)
                    return;

                bool actionTaken = false;
                UdpClient c = (UdpClient)ar.AsyncState;
                IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);

                if (IsByteArrayMagicShutdownPacket(receivedBytes, this.expectedPassword))
                {
                    var macByte = GetMacBytesFromMagicPacket(receivedBytes);

                    if (macByte != null && IsShutDownPacketAdressedToThisComputer(macByte))
                    {
                        var shutdownBehavior = GetShutdownBehaviorFromMagicPacket(receivedBytes);

                        switch (shutdownBehavior)
                        {
                            case ShutDownBehavior.None:
                                break;
                            case ShutDownBehavior.Standby:
                                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.ShutdownManager_Standby, LogMode.Normal);
                                actionTaken = true;
                                SetStandbyState();
                                break;
                            case ShutDownBehavior.Hibernate:
                                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.ShutdownManager_Hibernate, LogMode.Normal);
                                actionTaken = true;
                                SetHibernateState();
                                break;
                            case ShutDownBehavior.Restart:
                                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.ShutdownManager_Restart, LogMode.Normal);
                                actionTaken = true;
                                Restart();
                                break;
                            case ShutDownBehavior.Shutdown:
                                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.ShutdownManager_Shutdown, LogMode.Normal);
                                actionTaken = true;
                                ShutDown();
                                break;
                            default:
                                break;
                        }
                    }
                }

                // Wait so that subsequent shutdown requests are not taken into account which are received in a very short time.
                if (!actionTaken)
                    Thread.Sleep(TimeSpan.FromSeconds(15));

                if (this.updClient != null)
                    this.updClient.BeginReceive(DataReceived, this.updClient);
            }
            catch (ObjectDisposedException)
            {
                // This is normal behavior if the listening is stopped...do nothing as the UdpClient is already disposed.
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
            }
        }

        private static bool IsByteArrayMagicShutdownPacket(byte[] packet, string expectedPassword)
        {
            try
            {
                if (GetShutdownBehaviorFromMagicPacket(packet) == ShutDownBehavior.None)
                    return false;

                // Get the MAC address.
                var macArr = new byte[6];

                for (int i = 0; i < macArr.Length; i++)
                {
                    macArr[i] = packet[i + 6];
                }

                // This MAC address has to be the same 16 times in a row.
                for (int i = 1; i < 17; i++)
                {
                    var checkArr = new byte[6];
                    Array.Copy(packet, 6 * i, checkArr, 0, 6);

                    for (int j = 0; j < checkArr.Length; j++)
                    {
                        if (checkArr[j] != macArr[j])
                            return false;
                    }
                }

                // Check password
                if (!string.IsNullOrEmpty(expectedPassword))
                {
                    // Get all contents which are after 102 bytes
                    var passwordBytes = packet.Skip(102).ToArray();
                    var receivedPassword = Encoding.UTF8.GetString(passwordBytes);

                    if (!expectedPassword.Equals(receivedPassword, StringComparison.Ordinal))
                    {
                        WsapmLog.Log.WriteWarning(Resources.Wsapm_Core.ShutdownManager_WarningWrongPassword, LogMode.Normal);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static ShutDownBehavior GetShutdownBehaviorFromMagicPacket(byte[] packet)
        {
            if (packet == null || packet.Length < 6)
                return ShutDownBehavior.None;

            // Header:
            // 6x 0xAA -> Standby
            // 6x 0XBB -> Hibernate
            // 6x 0xCC -> Restart
            // 6x 0xDD -> Shutdown

            bool validHeaderFound = true;

            for (int i = 0; i < 6; i++)
            {
                if (packet[i] != 0xAA)
                {
                    validHeaderFound = false;
                    break;
                }
            }

            if (validHeaderFound)
                return ShutDownBehavior.Standby;

            validHeaderFound = true;

            for (int i = 0; i < 6; i++)
            {
                if (packet[i] != 0xBB)
                {
                    validHeaderFound = false;
                    break;
                }
            }

            if (validHeaderFound)
                return ShutDownBehavior.Hibernate;

            validHeaderFound = true;

            for (int i = 0; i < 6; i++)
            {
                if (packet[i] != 0xCC)
                {
                    validHeaderFound = false;
                    break;
                }
            }

            if (validHeaderFound)
                return ShutDownBehavior.Restart;

            validHeaderFound = true;

            for (int i = 0; i < 6; i++)
            {
                if (packet[i] != 0xDD)
                {
                    validHeaderFound = false;
                    break;
                }
            }

            if (validHeaderFound)
                return ShutDownBehavior.Shutdown;

            return ShutDownBehavior.None;
        }

        private static bool IsShutDownPacketAdressedToThisComputer(byte[] macByte)
        {
            var macAdresses = GetMacAddresses();

            if (macAdresses == null || macAdresses.Length == 0 || macByte == null || macByte.Length != 6)
                return false;

            foreach (var physicalAddress in macAdresses)
            {
                var paMacByte = physicalAddress.GetAddressBytes();

                for (int i = 0; i < macByte.Length; i++)
                {
                    if (macByte.SequenceEqual(paMacByte))
                        return true;
                }
            }

            return false;
        }

        private static byte[] GetMacBytesFromMagicPacket(byte[] magicPacket)
        {
            if (magicPacket == null || magicPacket.Length < 6)
                return null;

            // Get the MAC address.
            var macArr = new byte[6];

            for (int i = 0; i < macArr.Length; i++)
            {
                macArr[i] = magicPacket[i + 6];
            }

            return macArr;
        }

        private static PhysicalAddress[] GetMacAddresses()
        {
            string macAddresses = string.Empty;

            var macList = new List<PhysicalAddress>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macList.Add(nic.GetPhysicalAddress());
                }
            }

            return macList.ToArray();
        }

        #region Dispose

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (this.updClient != null)
                {
                    this.updClient.Close();
                    this.updClient = null;
                }

                disposed = true;
            }
        }

        ~ShutdownManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Dispose

        private enum ShutDownBehavior
        {
            None,
            Standby,
            Hibernate,
            Restart,
            Shutdown
        }

        internal static void SetStandbyState()
        {
            Thread.Sleep(WsapmCoreConstants.DefaultTimeSpanBeforeTakingAction);
            NativeMethods.SetSuspendState(false, true, false);
        }

        internal static void SetHibernateState()
        {
            Thread.Sleep(WsapmCoreConstants.DefaultTimeSpanBeforeTakingAction);
            NativeMethods.SetSuspendState(true, true, false);
        }

        internal static void ShutDown()
        {
            Thread.Sleep(WsapmCoreConstants.DefaultTimeSpanBeforeTakingAction);
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        internal static void Restart()
        {
            Thread.Sleep(WsapmCoreConstants.DefaultTimeSpanBeforeTakingAction);
            var psi = new ProcessStartInfo("shutdown", "/r /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        /// <summary>
        /// Class containing native methods.
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
        }        
    }
}
