using System;
using System.Threading;

namespace Wsapm.Core
{
    public sealed class HddLoadSingleVolumeCurrent : HddLoadSingleVolumeBase
    {
        private Timer timer;

        public HddLoadSingleVolumeCurrent(string logicalVolume) : this(logicalVolume, TimeSpan.FromSeconds(1))
        {

        }

        public HddLoadSingleVolumeCurrent(string logicalVolume, TimeSpan timerInterval) : base(logicalVolume)
        {
            this.timer = new Timer(TimerCallback, null, new TimeSpan(), timerInterval);
        }        

        public float CurrentHddLoad
        {
            get;
            private set;
        }

        private void TimerCallback(object stateInfo)
        {
            this.CurrentHddLoad = this.performanceCounterDiskBytesPerSecond.NextValue();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
