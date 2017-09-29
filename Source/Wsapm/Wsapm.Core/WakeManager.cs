using System;
using System.Collections.Generic;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for handling of WakeTimers.
    /// </summary>
    public sealed class WakeManager
    {
        private IDictionary<WakeTimer, WakeScheduler> wakeTimers;

        #region Events

        /// <summary>
        /// Occurs when the timer is stated. The DataEventArgs contain the DateTime the timer was started.
        /// </summary>
        internal event EventHandler<DataEventArgs<Tuple<WakeScheduler, DateTime>>> WakeTimerStarted;

        /// <summary>
        /// Occurs when the timer is canceled. The DataEventArgs contain the DateTime the timer was canceled.
        /// </summary>
        internal event EventHandler<DataEventArgs<Tuple<WakeScheduler, DateTime>>> WakeTimerCancelled;

        /// <summary>
        /// Occurs when the timer is completed. The DataEventArgs contain the DateTime when the event was fired.
        /// </summary>
        internal event EventHandler<DataEventArgs<Tuple<WakeScheduler, DateTime>>> WakeTimerCompleted;

        #endregion Events

        /// <summary>
        /// Creates a new instance of WakeManager.
        /// </summary>
        public WakeManager()
        {
        }

        /// <summary>
        /// Receives a list of WakeSchedulers and starts WakeTimers for these WakeSchedulers.
        /// All active WakeTimers are canceled before the new WakeTimers are created.
        /// </summary>
        /// <param name="wakeSchedulers">The list of WakeSchedulers.</param>
        public void StartWakeTimersFromWakeSchedulers(WakeScheduler[] wakeSchedulers)
        {
            CancelAllWakeTimers();
            this.wakeTimers = new Dictionary<WakeTimer, WakeScheduler>();

            foreach (var wakeScheduler in wakeSchedulers)
            {
                // Only set wake timer when next due time is not in the past and the wake is enabled.
                var nextDueTime = wakeScheduler.NextDueTime;

                if (wakeScheduler.EnableWakeScheduler && nextDueTime.HasValue)
                {
                    var wakeTimer = new WakeTimer();

                    wakeTimer.WakeTimerCancelled += wakeTimer_WakeTimerCancelled;
                    wakeTimer.WakeTimerCompleted += wakeTimer_WakeTimerCompleted;
                    wakeTimer.WakeTimerStarted += wakeTimer_WakeTimerStarted;

                    wakeTimer.SetTimer(nextDueTime.Value);
                    this.wakeTimers.Add(wakeTimer, wakeScheduler);
                }
            }
        }

        public void CancelAllWakeTimers()
        {
            if (this.wakeTimers == null)
                return;

            foreach (var item in this.wakeTimers)
            {
                item.Key.CancelTimer();

                item.Key.WakeTimerCancelled -= wakeTimer_WakeTimerCancelled;
                item.Key.WakeTimerCompleted -= wakeTimer_WakeTimerCompleted;
                item.Key.WakeTimerStarted -= wakeTimer_WakeTimerStarted;
            }

            this.wakeTimers.Clear();
        }      

        private void wakeTimer_WakeTimerCompleted(object sender, DataEventArgs<DateTime> e)
        {
           var wakeTimer = (WakeTimer)sender;
            WakeScheduler wakeScheduler = null;

            if (!this.wakeTimers.TryGetValue(wakeTimer, out wakeScheduler))
                return;

            var tmp = WakeTimerCompleted;

            if (tmp != null)
                tmp(this, new DataEventArgs<Tuple<WakeScheduler, DateTime>>(new Tuple<WakeScheduler, DateTime>(wakeScheduler, DateTime.Now)));

            if (wakeScheduler != null)
            {
                var nextDueTime = wakeScheduler.NextDueTime;

                if(nextDueTime.HasValue)
                    wakeTimer.SetTimer(nextDueTime.Value);
            }
        }

        private void wakeTimer_WakeTimerStarted(object sender, DataEventArgs<DateTime> e)
        {
            var tmp = WakeTimerStarted;

            if (tmp != null)
                tmp(this, new DataEventArgs<Tuple<WakeScheduler, DateTime>>(new Tuple<WakeScheduler, DateTime>(null, DateTime.Now)));
        }

        private void wakeTimer_WakeTimerCancelled(object sender, DataEventArgs<DateTime> e)
        {
            var tmp = this.WakeTimerCancelled;

            if (tmp != null)
                tmp(this, new DataEventArgs<Tuple<WakeScheduler, DateTime>>(new Tuple<WakeScheduler, DateTime>(null, DateTime.Now)));
        }
    }
}
