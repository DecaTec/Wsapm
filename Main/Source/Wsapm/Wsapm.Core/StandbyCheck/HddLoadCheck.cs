using System;
using System.Linq;

namespace Wsapm.Core
{
    public sealed class HddLoadCheck : IStandbyCheck, IDisposable
    {
        private HddLoadAverage hddLoad;

        /// <summary>
        /// Creates a new instance of HddLoadCheck.
        /// </summary>
        private HddLoadCheck()
        {
            this.hddLoad = new HddLoadAverage();
        }

        /// <summary>
        /// Gets the instance of HddLoadCheck.
        /// </summary>
        public static HddLoadCheck GetInstance()
        {
            return new HddLoadCheck();
        }

        /// <summary>
        /// Checks if the HDD load is over the HDD load value in the settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>True, if the current HDD load is greater than the HDD load value in the settings, otherwise false.</returns>
        public CheckSuspendResult CheckHddLoad(WsapmSettings settings)
        {
            if (settings == null || !settings.EnableHddsToMonitor)
                return new CheckSuspendResult(false, String.Empty);

            var availableHdds = WsapmTools.GetAvailableLogicalVolumeNames();

            foreach (var hddToMonitor in settings.HddsToMonitor)
            {
                if (hddToMonitor.Drive != WsapmConstants.AllHddsName && !availableHdds.Contains(hddToMonitor.Drive))
                {
                    // HDD not available -> don't check, just write entry in log.
                    WsapmLog.Log.WriteLine(string.Format(Wsapm.Core.Resources.Wsapm_Core.HddLoadCheck_HddNotAvailable, hddToMonitor.Drive), LogMode.Normal);
                    continue;
                }

                bool checkAllHdds = false;
                string hddName = hddToMonitor.Drive;

                if (hddToMonitor.Drive == WsapmConstants.AllHddsName)
                {
                    checkAllHdds = true;
                    hddName = WsapmTools.GetCommonDiaplayNameAllDrives();
                }

                // Check load.
                try
                {
                    if (hddToMonitor.EnableCheckHddLoad && hddToMonitor.HddLoad != 0.0f)
                    {
                        var averageHddLoad = 0.0f;

                        if (checkAllHdds)
                            averageHddLoad = this.hddLoad.GetAverageHddLoadAllVolumesInBytesPerSecond(); // Byte/s
                        else
                            averageHddLoad = this.hddLoad.GetAverageHddLoadInBytesPerSecond(hddToMonitor.Drive); // Byte/s

                        var result = averageHddLoad > WsapmConvert.ConvertKBToByte(hddToMonitor.HddLoad); // Settings are saved as KBit/s.

                        if (result)
                            return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.HddLoadCheck_HddLoadReason, hddName, hddToMonitor.HddLoad, WsapmConvert.ConvertByteToKB(averageHddLoad).ToString("0")));
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
            return this.CheckHddLoad(settings);
        }

        #endregion IStandbyCheck members

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.hddLoad != null)
                    this.hddLoad.Dispose();
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
