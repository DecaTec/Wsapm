using System;
using System.Linq;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for network load check.
    /// </summary>
    public sealed class NetworkLoadCheck : IStandbyCheck, IDisposable
    {
        private NetworkLoadAverage networkLoad;

        /// <summary>
        /// Creates a new instance of NetworkLoadCheck.
        /// </summary>
        private NetworkLoadCheck()
        {
            this.networkLoad = new NetworkLoadAverage();
        }

        /// <summary>
        /// Gets the instance of NetworkLoadCheck.
        /// </summary>
        public static NetworkLoadCheck GetInstance()
        {
            return new NetworkLoadCheck();
        }

        /// <summary>
        /// Checks if the network load is over the network load value in the settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>True, if the current network load is greater than the network load value in the settings, otherwise false.</returns>
        public CheckSuspendResult CheckNetworkLoad(WsapmSettings settings)
        {
            if (settings == null || !settings.EnableNetworkInterfacesToMonitor)
                return new CheckSuspendResult(false, String.Empty);

            var availableNics = WsapmTools.GetAvailableNetworkInterfaces();

            foreach (var networkInterfaceToMonitor in settings.NetworkInterfacesToMonitor)
            {
                if(networkInterfaceToMonitor.NetworkInterface != WsapmConstants.AllNetworkInterfacesName && !availableNics.Contains(networkInterfaceToMonitor.NetworkInterface))
                {
                    // NIC not available -> don't check, just write entry in log.
                    WsapmLog.Log.WriteLine(string.Format(Wsapm.Core.Resources.Wsapm_Core.NetworkLoadCheck_NicNotAvailable, networkInterfaceToMonitor.NetworkInterface), LogMode.Normal);
                    continue;
                }

                bool checkAllNics = false;
                string nicName = networkInterfaceToMonitor.NetworkInterface;

                if(networkInterfaceToMonitor.NetworkInterface == WsapmConstants.AllNetworkInterfacesName)
                {
                    checkAllNics = true;
                    nicName = WsapmTools.GetCommonDiaplayNameAllNetworkInterfaces();
                }

                // Check total network load.
                try
                {
                    if (networkInterfaceToMonitor.EnableCheckNetworkLoadTotal && networkInterfaceToMonitor.NetworkLoadTotal != 0.0f)
                    {
                        var averageNetworkLoad = 0.0f;

                        if(checkAllNics)
                            averageNetworkLoad = this.networkLoad.GetAverageNetworkLoadTotalAllNicsInBytesPerSecond(); // Byte/s
                        else
                            averageNetworkLoad = this.networkLoad.GeAverageNetworkLoadTotalInBytesPerSecond(networkInterfaceToMonitor.NetworkInterface); // Byte/s

                        var result = averageNetworkLoad > WsapmConvert.ConvertKBitToByte(networkInterfaceToMonitor.NetworkLoadTotal); // Settings are saved as KBit/s.

                        if (result)
                            return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.NetworkLoadCheck_CombinedNetworkLoadReason, nicName, networkInterfaceToMonitor.NetworkLoadTotal, WsapmConvert.ConvertByteToKBit(averageNetworkLoad).ToString("0")));
                        else
                            return new CheckSuspendResult(false, String.Empty);
                    }
                }
                catch (Exception ex)
                {
                    // Might happen if the performance counters get broken.
                    WsapmLog.Log.WriteError(ex);
                }

                // Check download network load.
                try
                {
                    if (networkInterfaceToMonitor.EnableCheckNetworkLoadDownload && networkInterfaceToMonitor.NetworkLoadDownload != 0.0f)
                    {
                        var averageNetworkReceived = 0.0f;

                        if (checkAllNics)
                            averageNetworkReceived = this.networkLoad.GetAverageNetworkLoadDownloaAllNicsInBytesPerSecond(); //Byte/s
                        else
                            averageNetworkReceived = this.networkLoad.GetAverageNetworkLoadDownloadInBytesPerSecond(networkInterfaceToMonitor.NetworkInterface); //Byte/s

                        var result = averageNetworkReceived > WsapmConvert.ConvertKBitToByte(networkInterfaceToMonitor.NetworkLoadDownload); // Settings are saved as KBit/s.

                        if (result)
                            return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.NetworkLoadCheck_DownloadNetworkLoadReason, nicName, networkInterfaceToMonitor.NetworkLoadDownload, WsapmConvert.ConvertByteToKBit(averageNetworkReceived).ToString("0")));
                        else
                            return new CheckSuspendResult(false, String.Empty);
                    }
                }
                catch (Exception ex)
                {
                    // Might happen if the performance counters get broken.
                    WsapmLog.Log.WriteError(ex);
                }

                // Check upload network load.
                try
                {
                    if (networkInterfaceToMonitor.EnableCheckNetworkLoadUpload && networkInterfaceToMonitor.NetworkLoadUpload != 0.0f)
                    {
                        var averageNetworkSent = 0.0f;

                        if (checkAllNics)
                            averageNetworkSent = this.networkLoad.GetAverageNetworkLoadUploadAllNicsInBytesPerSecond(); // Byte/s
                        else
                            averageNetworkSent = this.networkLoad.GetAverageNetworkLoadUploadInBytesPerSecond(networkInterfaceToMonitor.NetworkInterface); // Byte/s

                        var result = averageNetworkSent > WsapmConvert.ConvertKBitToByte(networkInterfaceToMonitor.NetworkLoadUpload); // Settings are saved as KBit/s.

                        if (result)
                            return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.NetworkLoadCheck_UploadNetworkLoadReason, nicName, networkInterfaceToMonitor.NetworkLoadUpload, WsapmConvert.ConvertByteToKBit(averageNetworkSent).ToString("0")));
                        else
                            return new CheckSuspendResult(false, String.Empty);
                    }
                }
                catch (Exception ex)
                {
                    // Might happen if the performance counters get broken.
                    WsapmLog.Log.WriteError(ex);
                }
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
            return this.CheckNetworkLoad(settings);
        }

        #endregion IStandbyCheck members

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.networkLoad != null)
                    this.networkLoad.Dispose();
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
