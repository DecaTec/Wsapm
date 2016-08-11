using System;
using System.Threading;

namespace Wsapm.Core
{
    public sealed class CpuLoadCurrent : CpuLoadBase
    {
        private readonly Timer timer;

        /// <summary>
        /// Initializes a new instance of CpuLoadCurrent with a default interval of 1 second.
        /// </summary>
        public CpuLoadCurrent() : this(TimeSpan.FromSeconds(1))
        {

        }

        /// <summary>
        /// Initializes a new instance of CpuLoadCurrent with the given interval.
        /// </summary>
        public CpuLoadCurrent(TimeSpan timerInterval) : base()
        {
            this.timer = new Timer(TimerCallback, null, new TimeSpan(), timerInterval);
        }

        /// <summary>
        /// Gets the current CPU load.
        /// </summary>
        public float CurrentCpuLoad
        {
            get;
            private set;
        }

        private void TimerCallback(object stateInfo)
        {
            this.CurrentCpuLoad = this.performanceCounterCpu.NextValue();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.timer != null)
                {
                    this.timer.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
