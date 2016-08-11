using System;
using System.Diagnostics;

namespace Wsapm.Core
{
    public class HddLoadSingleVolumeBase : IDisposable
    {
        protected PerformanceCounter performanceCounterDiskBytesPerSecond;

        private const string CategoryName = "LogicalDisk";
        private const string CounterName = "Disk Bytes/sec";
        private string logicalVolumeInstancenName;

        public const string AllLogicalVolumes = "_Total";

        public HddLoadSingleVolumeBase(string logicalVolume)
        {
            this.logicalVolumeInstancenName = HddTools.GetPerformanceCounterInstanceName(logicalVolume);

            if (string.IsNullOrEmpty(this.logicalVolumeInstancenName))
                throw new ArgumentException(string.Format(Wsapm.Core.Resources.Wsapm_Core.HddLoadSingleVolumeBase_LogicalVolumeCannotBeFound, this.logicalVolumeInstancenName));

            this.performanceCounterDiskBytesPerSecond = new PerformanceCounter(CategoryName, CounterName, logicalVolumeInstancenName);
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.performanceCounterDiskBytesPerSecond != null)
                    this.performanceCounterDiskBytesPerSecond.Dispose();
            }
        }

        #endregion IDisposable members   
    }
}
