using System;
using System.Collections.Generic;

namespace Wsapm.Core
{
    public sealed class HddLoadCurrent : IDisposable
    {
        private Dictionary<string, HddLoadSingleVolumeCurrent> performanceCounters;

        public HddLoadCurrent() : this(TimeSpan.FromSeconds(1))
        {
        }

        public HddLoadCurrent(TimeSpan timerInterval) : base()
        {
            this.performanceCounters = new Dictionary<string, HddLoadSingleVolumeCurrent>();

            var availableLogicalVolumes = HddTools.GetAvailableLogicalVolumeNames();

            foreach (var volume in availableLogicalVolumes)
            {
                var hddLoadSingleVolumeCcurrent = new HddLoadSingleVolumeCurrent(volume);
                this.performanceCounters.Add(volume, hddLoadSingleVolumeCcurrent);
            }
        }

        #region Public methods

        public float GetCurrentHddLoadInBytesPerSecond(string logicalVolume)
        {
            return this.performanceCounters[logicalVolume].CurrentHddLoad;
        }

        /// <summary>
        /// Gets the current HDD load of all HDDs combined.
        /// </summary>
        /// <returns></returns>
        public float GetCurrentHddLoadTotalAllHdds()
        {
            float buffer = 0.0f;

            foreach (var pc in this.performanceCounters)
            {
                buffer += pc.Value.CurrentHddLoad;
            }

            return buffer;
        }

        #endregion Public methods

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
