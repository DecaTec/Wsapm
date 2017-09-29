using System;
using System.Collections.Generic;

namespace Wsapm.Core
{
    public sealed class NetworkLoadCurrent : IDisposable
    {
        private readonly Dictionary<string, NetworkLoadSingleNicCurrent> performanceCounters;

        /// <summary>
        /// Initializes a new instance of NetworkLoadCurrent with a default interval of 1 second.
        /// </summary>
        public NetworkLoadCurrent() : this(TimeSpan.FromSeconds(1))
        {
        }

        /// <summary>
        /// Initializes a new instance of NetworkLoadCurrent with the given interval.
        /// </summary>
        public NetworkLoadCurrent(TimeSpan timerInterval) : base()
        {
            this.performanceCounters = new Dictionary<string, NetworkLoadSingleNicCurrent>();
            var availableNics = NetworkTools.GetAvailableNetworkInterfaces();

            foreach (var nic in availableNics)
            {
                var networkLoadSingleNicCcurrent = new NetworkLoadSingleNicCurrent(nic);
                this.performanceCounters.Add(nic, networkLoadSingleNicCcurrent);
            }
        }

        /// <summary>
        /// Gets the current network load (total) of the given NIC.
        /// </summary>
        /// <param name="nic"></param>
        /// <returns></returns>
        public float GetCurrentNetworkLoadTotal(string nic)
        {
            return this.performanceCounters[nic].CurrentNetworkLoadTotal;
        }

        /// <summary>
        /// Gets the current network load (download) of the given NIC.
        /// </summary>
        /// <param name="nic"></param>
        /// <returns></returns>
        public float GetCurrentNetworkLoadDownload(string nic)
        {
            return this.performanceCounters[nic].CurrentNetworkLoadDownload;
        }

        /// <summary>
        /// Gets the current network load (upload) of the given NIC.
        /// </summary>
        /// <param name="nic"></param>
        /// <returns></returns>
        public float GetCurrentNetworkLoadUpload(string nic)
        {
            return this.performanceCounters[nic].CurrentNetworkLoadUpload;
        }

        /// <summary>
        /// Gets the current network load (total) of all NICs combined.
        /// </summary>
        /// <returns></returns>
        public float GetCurrentNetworkLoadTotalAllNics()
        {
            float buffer = 0.0f;

            foreach (var pc in this.performanceCounters)
            {
                buffer += pc.Value.CurrentNetworkLoadTotal;
            }

            return buffer;
        }

        /// <summary>
        /// Gets the current network load (download) of all NICs combined.
        /// </summary>
        /// <returns></returns>
        public float GetCurrentNetworkLoadDownloadAllNics()
        {
            float buffer = 0.0f;

            foreach (var pc in this.performanceCounters)
            {
                buffer += pc.Value.CurrentNetworkLoadDownload;
            }

            return buffer;
        }

        /// <summary>
        /// Gets the current network load (upload) of all NICs combined.
        /// </summary>
        /// <returns></returns>
        public float GetCurrentNetworkLoadUploadAllNics()
        {
            float buffer = 0.0f;

            foreach (var pc in this.performanceCounters)
            {
                buffer += pc.Value.CurrentNetworkLoadUpload;
            }

            return buffer;
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
