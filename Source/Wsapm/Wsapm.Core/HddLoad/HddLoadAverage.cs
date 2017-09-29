using System;
using System.Collections.Generic;

namespace Wsapm.Core
{
    public sealed class HddLoadAverage : IDisposable
    {
        private readonly Dictionary<string, HddLoadSingleVolumeAverage> performanceCounters;

        public HddLoadAverage() : base()
        {
            this.performanceCounters = new Dictionary<string, HddLoadSingleVolumeAverage>();

            var availableLogicalVolumes = HddTools.GetAvailableLogicalVolumeNames();

            foreach (var volume in availableLogicalVolumes)
            {
                var hddLoadSingleVolumeAverage = new HddLoadSingleVolumeAverage(volume);
                this.performanceCounters.Add(volume, hddLoadSingleVolumeAverage);
            }
        }

        #region Public methods

        public float GetAverageHddLoadAllVolumesInBytesPerSecond()
        {
            float buffer = 0.0f;

            foreach (var pc in this.performanceCounters.Values)
            {
                buffer += pc.GetAverageHddLoadInBytesPerSecond();
            }

            return buffer;
        }

        public float GetAverageHddLoadInBytesPerSecond(string volumeName)
        {
            return this.performanceCounters[volumeName].GetAverageHddLoadInBytesPerSecond();
        }        

        #endregion Public methdos

        #region IDisposable members

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.performanceCounters != null && this.performanceCounters.Count > 0)
                {
                    foreach (var performanceCounter in this.performanceCounters.Values)
                    {
                        if (performanceCounter != null)
                        {
                            performanceCounter.Dispose();
                        }
                    }
                }
            }
        }

        #endregion IDisposable members
    }
}
