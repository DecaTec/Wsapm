using System;
using System.Collections.Generic;

namespace Wsapm.Core
{
    /// <summary>
    /// Class to determine average network load.
    /// </summary>
    public sealed class NetworkLoadAverage : IDisposable
    {
        private readonly Dictionary<string, NetworkLoadSingleNicAverage> performanceCounters;

        public NetworkLoadAverage() : base()
        {
            this.performanceCounters = new Dictionary<string, NetworkLoadSingleNicAverage>();
            var availableNics = NetworkTools.GetAvailableNetworkInterfaces();

            foreach (var nic in availableNics)
            {
                var networkLoadSingleNic = new NetworkLoadSingleNicAverage(nic);
                this.performanceCounters.Add(nic, networkLoadSingleNic);
            }
        }

        #region Public methods

        public float GetAverageNetworkLoadTotalAllNicsInBytesPerSecond()
        {
            float buffer = 0.0f;

            foreach (var pc in this.performanceCounters.Values)
            {
                buffer += pc.GetAverageNetworkLoadTotalInBytesPerSecond();
            }

            return buffer;
        }

        public float GeAverageNetworkLoadTotalInBytesPerSecond(string nic)
        {
            return this.performanceCounters[nic].GetAverageNetworkLoadTotalInBytesPerSecond();
        }

        public float GetAverageNetworkLoadDownloaAllNicsInBytesPerSecond()
        {
            float buffer = 0.0f;

            foreach (var pc in this.performanceCounters.Values)
            {
                buffer += pc.GetAverageNetworkLoadDownloadInBytesPerSecond();
            }

            return buffer;
        }

        public float GetAverageNetworkLoadDownloadInBytesPerSecond(string nic)
        {
            return this.performanceCounters[nic].GetAverageNetworkLoadDownloadInBytesPerSecond();
        }

        public float GetAverageNetworkLoadUploadAllNicsInBytesPerSecond()
        {
            float buffer = 0.0f;

            foreach (var pc in this.performanceCounters.Values)
            {
                buffer += pc.GetAverageNetworkLoadUploadInBytesPerSecond();
            }

            return buffer;
        }

        public float GetAverageNetworkLoadUploadInBytesPerSecond(string nic)
        {
            return this.performanceCounters[nic].GetAverageNetworkLoadUploadInBytesPerSecond();
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
