using System;
using System.Diagnostics;

namespace Wsapm.Core
{
    /// <summary>
    /// Base class for network load stuff of single NIC.
    /// </summary>
    public abstract class NetworkLoadSingleNicBase : IDisposable
    {
        protected readonly PerformanceCounter performanceCounterNetworkTotal;
        protected readonly PerformanceCounter performanceCounterNetworkSent;
        protected readonly PerformanceCounter performanceCounterNetworkReceived;

        private const string CategoryNameNic = "Network Interface";
        private const string CounterTotalName = "Bytes Total/sec";
        private const string CounterSentName = "Bytes Sent/sec";
        private const string CounterReceivedName = "Bytes Received/sec";

        protected NetworkLoadSingleNicBase(string nic)
        {
            if (string.IsNullOrEmpty(nic))
                throw new ArgumentException("A network interface has to be specified.");

            this.performanceCounterNetworkTotal = new PerformanceCounter(CategoryNameNic, CounterTotalName, nic);
            this.performanceCounterNetworkReceived = new PerformanceCounter(CategoryNameNic, CounterReceivedName, nic);
            this.performanceCounterNetworkSent = new PerformanceCounter(CategoryNameNic, CounterSentName, nic);
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
                if (this.performanceCounterNetworkTotal != null)
                    this.performanceCounterNetworkTotal.Dispose();

                if (this.performanceCounterNetworkSent != null)
                    this.performanceCounterNetworkSent.Dispose();

                if (this.performanceCounterNetworkReceived != null)
                    this.performanceCounterNetworkReceived.Dispose();
            }
        }

        #endregion IDisposable members
    }
}
