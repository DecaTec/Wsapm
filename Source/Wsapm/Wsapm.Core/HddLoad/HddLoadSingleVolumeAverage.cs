using System;
using System.Collections.Generic;
using System.Threading;

namespace Wsapm.Core
{
    public sealed class HddLoadSingleVolumeAverage : HddLoadSingleVolumeBase
    {
        public HddLoadSingleVolumeAverage(string volumeName) : base(volumeName)
        {
        }

        #region Public methods

        /// <summary>
        /// Get the average Disk Bytes/sec.
        /// </summary>
        /// <returns>The average Disk Bytes/sec.</returns>
        public float GetAverageHddLoadInBytesPerSecond()
        {
            return GetAverageHddLoadInBytesPerSecond(WsapmConstants.PerformanceCounterDefaultNumberCheckProbes, TimeSpan.FromMilliseconds(WsapmConstants.PerformanceCounterBreakIntervallMs));
        }

        /// <summary>
        /// Get the average Disk Bytes/sec.
        /// </summary>
        /// <returns>The average Disk Bytes/sec.</returns>
        public float GetAverageHddLoadInBytesPerSecond(int numberOfProbes, TimeSpan breakBetweenProbes)
        {
            try
            {
                IList<float> buffer = new List<float>();

                for (int i = 0; i < numberOfProbes; i++)
                {
                    buffer.Add(this.performanceCounterDiskBytesPerSecond.NextValue());
                    Thread.Sleep(breakBetweenProbes);
                }

                float tmp = 0;

                for (int i = 0; i < buffer.Count; i++)
                {
                    tmp += buffer[i];
                }

                tmp = tmp / buffer.Count;
                return tmp;
            }
            catch (Exception ex)
            {
                throw new WsapmException(Wsapm.Core.Resources.Wsapm_Core.HddLoadSingleVolumeAverage_RetrieveLoadError, ex);
            }
        }

        #endregion Public methods
    }
}
