using DecaTec.Toolkit.Tools;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Wsapm.Core;

namespace Wsapm.Service
{
    /// <summary>
    /// Class for setting a timer to wake up the computer. Only one wake timer is allowed at a time. If a timer was set and a new timer gets set, the already running timer is cancelled.
    /// In order to set multiple timers, multiple instances of WakeManager have to be created.
    /// </summary>
    internal sealed class WakeManager
    {
        private SafeWaitHandle currentWaitHandle;
        private Thread waitThread;
        private WsapmLog log;
        private DateTime? lastDueTime;

        private delegate void TimerCompleteDelegate();

        /// <summary>
        /// Creates a new instance of WakeManager.
        /// </summary>
        /// <param name="log"></param>
        internal WakeManager(WsapmLog log)
        {
            this.log = log;
            this.lastDueTime = null;
            this.WakeTimerSet = false;
        }

        #region Events

        /// <summary>
        /// Occurs when the timer is stated. The DataEventArgs contain the DateTime the timer was started.
        /// </summary>
        public event EventHandler<DataEventArgs<DateTime>> TimerStarted;

        /// <summary>
        /// Occurs when the timer is cancelled. The DataEventArgs contain the DateTime the timer was cancelled.
        /// </summary>
        public event EventHandler<DataEventArgs<DateTime>> TimerCancelled;

        /// <summary>
        /// Occurs when the timer is completed. The DataEventArgs contain the DateTime when the event was fired.
        /// </summary>
        public event EventHandler<DataEventArgs<DateTime>> TimerCompleted;


        #endregion Events

        /// <summary>
        /// Sets a timer with the due time specified.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <returns>True if the timer was started successfully, otherwise false.</returns>
        internal bool SetTimer(DateTime dueTime)
        {
            try
            {
                // Cancel waitable timer if there is any.
                CancelTimer();

                long interval = dueTime.ToFileTime();
                this.currentWaitHandle = CreateWaitableTimer(IntPtr.Zero, true, null);
                var success = SetWaitableTimer(this.currentWaitHandle, ref interval, 0, null, IntPtr.Zero, true);

                if (success)
                {
                    var tmp = TimerStarted;

                    if (tmp != null)
                        tmp(this, new DataEventArgs<DateTime>(DateTime.Now));

                    // Start a thread which waits for timer to complete.
                    // Otherwise the callback procedure would not be called.
                    ThreadStart ts = new ThreadStart(WaitForTimer);
                    this.waitThread = new Thread(ts);
                    this.waitThread.IsBackground = true;
                    this.waitThread.Start();
                    this.WakeTimerSet = true;

                    if (!this.lastDueTime.HasValue || this.lastDueTime.Value != dueTime)
                        this.log.WriteLine(String.Format(Resources.Wsapm_Service.WakeManager_WakeScheduled, dueTime.ToShortDateString() + " " + dueTime.ToLongTimeString()), LogMode.Normal);

                    this.lastDueTime = dueTime;
                }
                else
                {
                    var ex = GetWin32Exception();
                    throw new WsapmException(Resources.Wsapm_Service.WakeManager_SetWakeTimerFailed, ex);
                }

                return success;
            }
            catch (Exception ex)
            {
                this.log.WriteError(ex);
            }

            return false;
        }

        /// <summary>
        /// Cancels a set timer.
        /// </summary>
        /// <returns>True, if the timer was cancelled successfully. Otherwise false (e.g. the timer was not started before).</returns>
        internal bool CancelTimer()
        {
            try
            {
                if (this.currentWaitHandle != null && !this.currentWaitHandle.IsClosed)
                {
                    var success = CancelWaitableTimer(this.currentWaitHandle);
                    this.currentWaitHandle.Close();

                    if (success)
                    {
                        if (this.waitThread != null && this.waitThread.IsAlive)
                        {
                            // If the wait thread is not alive anymore, the timer has completed, avoid firing the cancelled event.
                            var tmp = this.TimerCancelled;

                            if (tmp != null)
                                tmp(this, new DataEventArgs<DateTime>(DateTime.Now));
                        }

                        this.currentWaitHandle = null;
                        this.WakeTimerSet = false;
                        this.lastDueTime = null;

                        if (this.waitThread != null && this.waitThread.IsAlive && this.waitThread.ThreadState == (ThreadState.WaitSleepJoin | ThreadState.Background))
                        {
                            this.waitThread.Abort();
                            this.waitThread.Join();
                        }
                    }
                    else
                    {
                        var ex = GetWin32Exception();
                        throw new WsapmException(Resources.Wsapm_Service.WakeManager_CancelWakeTimerFailed, ex);
                    }

                    this.log.WriteLine(Resources.Wsapm_Service.WakeManager_WakeTimerCancelled, LogMode.Verbose);
                    return success;
                }

                return false;
            }
            catch (Exception ex)
            {             
                this.log.WriteError(ex);
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating if a wake timer is currently set.
        /// </summary>
        internal bool WakeTimerSet
        {
            get;
            private set;
        }

        private void WaitForTimer()
        {
            using (var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                waitHandle.SafeWaitHandle = this.currentWaitHandle;
                waitHandle.WaitOne();
            }

            this.currentWaitHandle.Close();
            this.WakeTimerSet = false;

            var tmp = TimerCompleted;

            if (tmp != null)
                tmp(this, new DataEventArgs<DateTime>(DateTime.Now));
        }

        /// <summary>
        /// Gets the last thrown Win32Exception.
        /// </summary>
        /// <returns>The last thrown Win32Exception.</returns>
        private Win32Exception GetWin32Exception()
        {
            var error = Marshal.GetLastWin32Error();

            try
            {
                if (error != 0)
                    throw new Win32Exception(error);
            }
            catch (Win32Exception ex)
            {
                return ex;
            }

            return null;
        }

        #region Interop

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeWaitHandle OpenWaitableTimer(uint dwDesiredAccess, bool bInheritHandle, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetWaitableTimer(SafeWaitHandle hTimer, [In] ref long pDueTime, int lPeriod, TimerCompleteDelegate pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CancelWaitableTimer(SafeWaitHandle hTimer);

        #endregion Interop
    }
}
