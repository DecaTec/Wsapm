using System;
using System.Collections.Generic;
using System.Threading;

namespace Wsapm.Core
{
    /// <summary>
    /// Class to determine average network load of a single NIC (used for checking network load when it comes to standby checks).
    /// </summary>
    public sealed class NetworkLoadSingleNicAverage : NetworkLoadSingleNicBase
    {
        public NetworkLoadSingleNicAverage(string nic) : base(nic)
        {            
        }      

        #region Public methods

        /// <summary>
        /// Get the average total network load (upload + download) in bytes/s.
        /// </summary>
        /// <returns>The average total network load (upload + download) in bytes/s.</returns>
        public float GetAverageNetworkLoadTotalInBytesPerSecond()
        {
            return GetAverageNetworkLoadTotalInBytesPerSecond(WsapmConstants.PerformanceCounterDefaultNumberCheckProbes, TimeSpan.FromMilliseconds(WsapmConstants.PerformanceCounterBreakIntervallMs));
        }

        /// <summary>
        /// Get the average total network load (upload + download) in bytes/s.
        /// </summary>
        /// <returns>The average total network load (upload + download) in bytes/s.</returns>
        public float GetAverageNetworkLoadTotalInBytesPerSecond(int numberOfProbes, TimeSpan breakBetweenProbes)
        {
            try
            {
                IList<float> buffer = new List<float>();

                for (int i = 0; i < numberOfProbes; i++)
                {
                    buffer.Add(this.performanceCounterNetworkTotal.NextValue());
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
                throw new WsapmException(Wsapm.Core.Resources.Wsapm_Core.NetworkLoadSingleNicAverage_RetrieveLoadError, ex);
            }
        }

        public float GetAverageNetworkLoadUploadInBytesPerSecond()
        {
            return GetAverageNetworkLoadUploadInBytesPerSecond(WsapmConstants.PerformanceCounterDefaultNumberCheckProbes, TimeSpan.FromMilliseconds(WsapmConstants.PerformanceCounterBreakIntervallMs));
        }

        /// <summary>
        /// Get the average current network load (upload) in bytes/s.
        /// </summary>
        /// <returns>The average current network load (upload) in bytes/s.</returns>
        public float GetAverageNetworkLoadUploadInBytesPerSecond(int numberOfProbes, TimeSpan breakBetweenProbes)
        {
            try
            {
                IList<float> buffer = new List<float>();

                for (int i = 0; i < numberOfProbes; i++)
                {
                    buffer.Add(this.performanceCounterNetworkSent.NextValue());
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
                throw new WsapmException(Wsapm.Core.Resources.Wsapm_Core.NetworkLoadSingleNicAverage_RetrieveLoadError, ex);
            }
        }

        public float GetAverageNetworkLoadDownloadInBytesPerSecond()
        {
            return GetAverageNetworkLoadDownloadInBytesPerSecond(5, TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Get the average current network load (download) in bytes/s.
        /// </summary>
        /// <returns>The average current network load (download) in bytes/s.</returns>
        public float GetAverageNetworkLoadDownloadInBytesPerSecond(int numberOfProbes, TimeSpan breakBetweenProbes)
        {
            try
            {
                IList<float> buffer = new List<float>();

                for (int i = 0; i < numberOfProbes; i++)
                {
                    buffer.Add(this.performanceCounterNetworkReceived.NextValue());
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
                throw new WsapmException(Wsapm.Core.Resources.Wsapm_Core.NetworkLoadSingleNicAverage_RetrieveLoadError, ex);
            }
        }

        #endregion Public methods

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
