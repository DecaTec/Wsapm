using System;
using System.Threading;

namespace Wsapm.Core
{
    public sealed class NetworkLoadSingleNicCurrent : NetworkLoadSingleNicBase
    {
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of NetworkLoadSingleNicCurrent with the default timer interval of 1 second.
        /// </summary>
        /// <param name="nic"></param>
        public NetworkLoadSingleNicCurrent(string nic) : this(nic, TimeSpan.FromSeconds(1))
        {

        }

        /// <summary>
        /// Initialized a new instance of NetworkLoadSingleNicCurrent with the given timer interval.
        /// </summary>
        /// <param name="nic"></param>
        /// <param name="timerInterval"></param>
        public NetworkLoadSingleNicCurrent(string nic, TimeSpan timerInterval) : base(nic)
        {
            this.timer = new Timer(TimerCallback, null, new TimeSpan(), timerInterval);
        }

        public float CurrentNetworkLoadTotal
        {
            get;
            private set;
        }

        public float CurrentNetworkLoadDownload
        {
            get;
            private set;
        }

        public float CurrentNetworkLoadUpload
        {
            get;
            private set;
        }

        private void TimerCallback(object stateInfo)
        {
            this.CurrentNetworkLoadTotal = this.performanceCounterNetworkTotal.NextValue();
            this.CurrentNetworkLoadDownload = this.performanceCounterNetworkReceived.NextValue();
            this.CurrentNetworkLoadUpload = this.performanceCounterNetworkSent.NextValue();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
