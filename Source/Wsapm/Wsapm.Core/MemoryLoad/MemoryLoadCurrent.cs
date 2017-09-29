using System;
using System.Threading;

namespace Wsapm.Core
{
    public sealed class MemoryLoadCurrent : MemoryLoadBase
    {
        private readonly Timer timer;
        private ulong lastAvailablePhysicalMemory;

        /// <summary>
        /// Initializes a new instance of MemoryLoadCurrent with a default interval of 1 second.
        /// </summary>
        public MemoryLoadCurrent() : this(TimeSpan.FromSeconds(1))
        {

        }

        /// <summary>
        /// Initializes a new instance of MemoryLoadCurrent with the given interval.
        /// </summary>
        public MemoryLoadCurrent(TimeSpan timerInterval) : base()
        {
            this.timer = new Timer(TimerCallback, null, new TimeSpan(), timerInterval);
        }

        /// <summary>
        /// Gets the current memory load.
        /// </summary>
        public float CurrenMemoryLoad
        {
            get;
            private set;
        }

        private void TimerCallback(object stateInfo)
        {
            try
            {
                this.lastAvailablePhysicalMemory = this.computerInfo.AvailablePhysicalMemory;
            }
            catch (Exception)
            {
            }

            this.CurrenMemoryLoad = (((float)TotalPhysicalMemory - (float)lastAvailablePhysicalMemory) / (float)TotalPhysicalMemory) * 100f;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
