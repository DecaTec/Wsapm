using System;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Wsapm.Core
{
    /// <summary>
    /// Class that manages the standby of the server. All check methods are called here.
    /// </summary>
    public sealed class WsapmManager : IDisposable
    {
        // Timer for monitoring.
        private Timer monitoringTimer;

        // The current application's settings.
        private WsapmSettings currentSettings;

        // Managers.
        private SettingsManager settingsManager;
        private StandbyManager standbyManager;
        private WakeManager wakeManager;
        private PluginManager pluginManager;
        private ShutdownManager shutdownManager;
        private TemporaryUptimeManager temporaryUptimeManager;

        // Timer to keep the system alive after ResumeAutomatic event without any program suppressing standby.
        private Timer resumeAutomaticTimer;
        private DateTime? lastWakeTime;

        // Interval to wait until starting programs/restart services.
        private static readonly TimeSpan timeSpanBeforeStartProgramsAndServices = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Creates a new instance of WsapmManager.
        /// </summary>
        public WsapmManager()
        {
            this.settingsManager = new SettingsManager();
            LoadSettings();

            // Create managers.
            this.standbyManager = new StandbyManager();
            this.wakeManager = new WakeManager();
            this.pluginManager = new PluginManager();
            this.shutdownManager = new ShutdownManager();

            this.temporaryUptimeManager = new TemporaryUptimeManager();

            this.lastWakeTime = null;

            this.settingsManager.SettingsChanged += settingsManager_SettingsChanged;
            this.temporaryUptimeManager.TemporaryUptimeChanged += TemporaryUptimeManager_TemporaryUptimeChanged;        
        }

        /// <summary>
        /// Starts monitoring.
        /// </summary>
        public void StartMonitoring()
        {
            // Scheduled wake will not take place if WsapmManager is not running.
            this.wakeManager.WakeTimerCompleted -= wakeManager_WakeTimerCompleted;
            this.wakeManager.WakeTimerCompleted += wakeManager_WakeTimerCompleted;

            SetWakeTimers();
            StartMonitoringTimer();
            StartShutdownListening();

            WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_Started, LogMode.Normal);

            // Check for updates every time the monitoring gets started.
            CheckForUpdates();
        }

        /// <summary>
        /// Stops monitoring.
        /// </summary>
        public void StopMonitoring()
        {
            CleanUp();
        }

        /// <summary>
        /// Pauses monitoring.
        /// </summary>
        public void PauseMonitoring()
        {
            try
            {
                this.wakeManager.CancelAllWakeTimers();
                StopShutdownListening();
                StopMonitoringTimer();
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_Paused, LogMode.Normal);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
            }
        }        

        /// <summary>
        /// Continues monitoring.
        /// </summary>
        public void ContinueMonitoring()
        {
            try
            {
                LoadSettings();
                StartMonitoringTimer();
                SetWakeTimers();
                StartShutdownListening();
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_Continued, LogMode.Normal);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// Cleanup of the class.
        /// </summary>
        private void CleanUp()
        {
            try
            {
                StopResumeAutomaticTimer();

                EnableStandby();

                this.standbyManager = null;

                StopShutdownListening();
                this.shutdownManager = null;

                StopMonitoringTimer();
                this.monitoringTimer = null;

                if (this.wakeManager != null)
                {
                    this.wakeManager.WakeTimerCompleted -= wakeManager_WakeTimerCompleted;
                    this.wakeManager.CancelAllWakeTimers();
                }

                this.wakeManager = null;
                this.pluginManager = null;

                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_StopMonitoringTimer, LogMode.Normal);

                if (this.settingsManager != null)
                    this.settingsManager.SettingsChanged -= settingsManager_SettingsChanged;

                this.currentSettings = null;
                this.settingsManager = null;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// Handles power events published by WsapmService.
        /// </summary>
        /// <param name="powerStatus">The power status to handle.</param>
        public void OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            try
            {
                switch (powerStatus)
                {
                    case PowerBroadcastStatus.ResumeAutomatic:
                        // System woken up (by wake timer, on user input, WoL, etc.).
                        // ResumeAutomatic will always appear when the computer wakes up from standby. When these is any user input (i.e. the user is present), a ResumeSuspend will appear afterwards.
                        // Reset windows idle timer, a ResumeSuspend should be raised afterwards.
                        // Start the WSAPM timers not generally when a ResumeAutomatic appears, just start them when WSAPM scheduled automatic wake!
                        WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_ResumeAutomatic, LogMode.Normal);
                        this.lastWakeTime = DateTime.Now;
                        StartMonitoringTimer();
                        HandleResumeActions();
                        break;
                    case PowerBroadcastStatus.Suspend:
                        // System gets suspended.
                        this.lastWakeTime = null;
                        StopMonitoringTimer();
                        WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_Suspend, LogMode.Normal);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// Handles all resume actions which should executed every time the computer was woken from standby (like starting programs, restarting services and resetting the network connections).
        /// </summary>
        private void HandleResumeActions()
        {
            ResetActiveNetworkConnections();
            RestartServices();
            StartProgramsAfterEveryWake();
        }

        #region Monitoring timer

        /// <summary>
        /// Starts the monitoring timer.
        /// </summary>
        private void StartMonitoringTimer()
        {
            try
            {
                StopMonitoringTimer();
                this.monitoringTimer = new Timer();
                this.monitoringTimer.AutoReset = false; // The timer gets re-started manually.
                double monitoringTimerIntervalDefault = WsapmTools.GetTimerIntervalInMilliseconds(1);
                double monitoringIntervalSettings = WsapmTools.GetTimerIntervalInMilliseconds(this.currentSettings.MonitoringTimerInterval);
                this.monitoringTimer.Interval = monitoringIntervalSettings <= 0 ? monitoringTimerIntervalDefault : monitoringIntervalSettings;
                this.monitoringTimer.Elapsed += monitoringTimer_Elapsed;
                this.monitoringTimer.Start();
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_StartMonitoringTimer, LogMode.Verbose);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// Stops the monitoring timer.
        /// </summary>
        private void StopMonitoringTimer()
        {
            try
            {
                if (this.monitoringTimer != null)
                {
                    this.monitoringTimer.Elapsed -= monitoringTimer_Elapsed;
                    this.monitoringTimer.Stop();
                    this.monitoringTimer.Close();
                    this.monitoringTimer = null;
                    WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_StopMonitoringTimer, LogMode.Verbose);
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// Monitoring timer elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Suspend standby temporarily: When checking policies last a longer time, the system should not switch to standby in between.
            this.standbyManager.SuspendStandby();
            var result = CheckPolicies();
            this.standbyManager.EnableStandby();

            if (result.SuspendStandby)
            {
                // At least one policy satisfied, suspend standby.
                StopResumeAutomaticTimer();
                this.standbyManager.SuspendStandby(result);
            }
            else
                EnableStandby(); // No policy satisfied, enable standby.

            StartMonitoringTimer();
        }

        #endregion Monitoring timer

        #region Monitoring check

        /// <summary>
        /// Checks all available monitoring policies.
        /// </summary>
        /// <returns>The check result as CheckSuspendResult.</returns>
        private CheckSuspendResult CheckPolicies()
        {
            try
            {
                System.Diagnostics.Debugger.Break();
                // Check all sources.
                // From these checks that last a shorter time to these that take a longer timer.
                // When standby is suspended, cancel the ResumeAutomaticTimer, so that it can't enable standby when it elapses.
                CheckSuspendResult result = CheckTemporaryUptime();

                if (result.SuspendStandby)
                {
                    // Maybe not needed
                    //ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckScheduledUptime();

                if(result.SuspendStandby)
                {
                    // Maybe not needed
                    //ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckProcesses();

                if (result.SuspendStandby)
                {
                    ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckNetworkAccess();

                if (result.SuspendStandby)
                {
                    ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckCpuLoad();

                if (result.SuspendStandby)
                {
                    ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckMemoryLoad();

                if (result.SuspendStandby)
                {
                    ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckPing();

                if (result.SuspendStandby)
                {
                    ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckNetworkLoad();

                if (result.SuspendStandby)
                {
                    ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckHddLoad();

                if (result.SuspendStandby)
                {
                    ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }

                result = CheckPluginPloicies();

                if (result.SuspendStandby)
                {
                    ExecuteActionsWhenAtLeastOnePolicySatisfied();
                    return result;
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
            }

            ExecuteActionsWhenNoPolicySatisfied();
            return new CheckSuspendResult(false, String.Empty);
        }

        private void ExecuteActionsWhenNoPolicySatisfied()
        {
            if (this.currentSettings.EnableActionsAfterPolicyCheck)
            {
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_ExecuteActionWhenNoPolicySatisfied, LogMode.Verbose);
                ActionAfterPolicyCheckManager.ExecuteActionsAfterPolicyCheckNoPolicySatisfied(this.currentSettings);
            }
        }

        private void ExecuteActionsWhenAtLeastOnePolicySatisfied()
        {
            if (this.currentSettings.EnableActionsAfterPolicyCheck)
            {
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_ExecuteActionWhenAtLeastOnePolicySatisfied, LogMode.Verbose);
                ActionAfterPolicyCheckManager.ExecuteActionsAfterPolicyCheckAtLeastPolicySatisfied(this.currentSettings);
            }
        }

        private CheckSuspendResult CheckTemporaryUptime()
        {
            try
            {
                var temporaryUptimeCheck = TemporaryUptimeCheck.Instance;
                return temporaryUptimeCheck.CheckStandby(this.currentSettings);
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
        }

        private CheckSuspendResult CheckScheduledUptime()
        {
            try
            {
                var scheduledUptimeCheck = ScheduledUptimeCheck.Instance;
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_CheckScheduledUptimes, LogMode.Verbose);

                if (this.currentSettings == null)
                    return new CheckSuspendResult(false, String.Empty);

                var result = scheduledUptimeCheck.CheckStandby(this.currentSettings);
                scheduledUptimeCheck = null;

                return result;
            }
            catch(Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
        }

        private CheckSuspendResult CheckProcesses()
        {
            try
            {
                var processCheck = ProcessCheck.Instance;
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_CheckProcesses, LogMode.Verbose);

                if (this.currentSettings == null)
                    return new CheckSuspendResult(false, String.Empty);

                var result = processCheck.CheckStandby(this.currentSettings);
                processCheck = null;

                return result;
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
        }

        private CheckSuspendResult CheckNetworkAccess()
        {
            try
            {
                var networkResourcesCheck = NetworkResourcesCheck.Instance;
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_CheckNetworkFileAccess, LogMode.Verbose);

                if (this.currentSettings == null)
                    return new CheckSuspendResult(false, String.Empty);

                var result = networkResourcesCheck.CheckStandby(this.currentSettings);
                networkResourcesCheck = null;
                return result;
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
        }

        private CheckSuspendResult CheckPing()
        {
            try
            {
                var pingCheck = PingCheck.Instance;
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_CheckNetworkMachines, LogMode.Verbose);

                if (this.currentSettings == null)
                    return new CheckSuspendResult(false, String.Empty);

                var result = pingCheck.CheckPing(this.currentSettings);
                pingCheck = null;
                return result;
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
        }

        private CheckSuspendResult CheckCpuLoad()
        {
            // Avoid holing an instance of CpuLoadCheck.
            // Dispose after use to free resources.
            CpuLoadCheck cpuLoadCheck = null;

            try
            {
                cpuLoadCheck = CpuLoadCheck.GetInstance();
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_CheckCpuLoad, LogMode.Verbose);

                if (this.currentSettings == null)
                    return new CheckSuspendResult(false, String.Empty);

                var result = cpuLoadCheck.CheckCpuLoad(this.currentSettings);
                return result;
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
            finally
            {
                if (cpuLoadCheck != null)
                {
                    cpuLoadCheck.Dispose();
                    cpuLoadCheck = null;
                }
            }
        }

        private CheckSuspendResult CheckMemoryLoad()
        {
            // Avoid holing an instance of MemoryLoadCheck.
            // Dispose after use to free resources.
            MemoryLoadCheck memoryLoadCheck = null;

            try
            {
                memoryLoadCheck = MemoryLoadCheck.GetInstance();
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_CheckMemoryLoad, LogMode.Verbose);

                if (this.currentSettings == null)
                    return new CheckSuspendResult(false, String.Empty);

                var result = memoryLoadCheck.CheckMemoryLoad(this.currentSettings);
                return result;
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
            finally
            {
                if (memoryLoadCheck != null)
                {
                    memoryLoadCheck.Dispose();
                    memoryLoadCheck = null;
                }
            }
        }

        private CheckSuspendResult CheckNetworkLoad()
        {
            // Avoid holing an instance of CpuLoadCheck.
            // Dispose after use to free resources.
            NetworkLoadCheck networkLoadCheck = null;

            try
            {
                networkLoadCheck = NetworkLoadCheck.GetInstance();
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_CheckNetworkLoad, LogMode.Verbose);

                if (this.currentSettings == null)
                    return new CheckSuspendResult(false, String.Empty);

                var result = networkLoadCheck.CheckNetworkLoad(this.currentSettings);
                networkLoadCheck = null;
                return result;
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
            finally
            {
                if (networkLoadCheck != null)
                {
                    networkLoadCheck.Dispose();
                    networkLoadCheck = null;
                }
            }
        }

        private CheckSuspendResult CheckHddLoad()
        {
            // Avoid holing an instance of HddLoadCheck.
            // Dispose after use to free resources.
            HddLoadCheck hddLoadCheck = null;

            try
            {
                hddLoadCheck = HddLoadCheck.GetInstance();
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_CheckHddLoad, LogMode.Verbose);

                if (this.currentSettings == null)
                    return new CheckSuspendResult(false, String.Empty);

                var result = hddLoadCheck.CheckHddLoad(this.currentSettings);
                hddLoadCheck = null;
                return result;
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
                return new CheckSuspendResult(false, String.Empty);
            }
            finally
            {
                if (hddLoadCheck != null)
                {
                    hddLoadCheck.Dispose();
                    hddLoadCheck = null;
                }
            }
        }

        private CheckSuspendResult CheckPluginPloicies()
        {
            // No try-catch here because this is handled in the PluginManager.
            if (this.currentSettings == null)
                return new CheckSuspendResult(false, String.Empty);

            // (Re-) load plugins in case that new plugins available.
            this.pluginManager.LoadAndInitialzePlugins();
            var ret = this.pluginManager.CheckPluginPolicies(this.currentSettings.ActivePlugins);
            return ret;
        }

        #endregion Monitoring check

        #region Standby handling

        /// <summary>
        /// Suspends standby according to the given CheckSuspendResult.
        /// </summary>
        /// <param name="result"></param>
        private void SuspendStandby(CheckSuspendResult result)
        {
            if (this.standbyManager == null)
                return;

            this.standbyManager.SuspendStandby(result);
        }

        /// <summary>
        /// Enables standby.
        /// </summary>
        private void EnableStandby()
        {
            if (this.standbyManager == null)
                return;

            this.standbyManager.EnableStandby();
        }

        #endregion Standby handling

        #region Wake

        /// <summary>
        /// Starts the wake timer.
        /// </summary>
        private void SetWakeTimers()
        {
            if (this.currentSettings != null && this.currentSettings.WakeSchedulers != null && this.currentSettings.EnableWakeTimers)
            {
                this.wakeManager.StartWakeTimersFromWakeSchedulers(this.currentSettings.WakeSchedulers.ToArray());
            }
        }

        /// <summary>
        /// Wake timer completed, i.e. the computer was woken from standby by WASPM timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wakeManager_WakeTimerCompleted(object sender, DataEventArgs<Tuple<WakeScheduler, DateTime>> e)
        {
            // The monitoring timer is already started by the ResumeAutomaic event handler, so do no start the monitoring timer another time.
            WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_WakeTimerCompleted, LogMode.Normal);

            StartResumeAutomaticTimer();

            // Start programs specified to start after system was woken by WSAPM.
            StartProgramsAfterScheduledWake(e.Data.Item1);
        }

        /// <summary>
        /// Start the resume automatic timer.
        /// </summary>
        /// <remarks>This has to be done in order to prevent system entering standby after a few minutes after the ResumeAutomatic event when no ResumeSuspend event follows, 
        /// i.e. system is woken, but user is not present.</remarks>
        private void StartResumeAutomaticTimer()
        {
            // Only start resume automatic timer if system was really woken by WSAPM timer.
            // Otherwise the system is already running when the wake timer elapses.
            if (!SystemWokenByWakeTimer())
                return;

            StopResumeAutomaticTimer();
            this.resumeAutomaticTimer = new Timer();

            // The resumeAutomaticTimer's interval has to be shorter than the Wsapm's timer!
            var interval = TimeSpan.FromMinutes(WsapmTools.GetCurrentWindowsIdleTimeoutAcMinutes()).TotalMilliseconds;

            if (this.monitoringTimer != null)
            {
                var delay = TimeSpan.FromSeconds(10).TotalMilliseconds;
                interval = this.monitoringTimer.Interval - delay;
            }

            SuspendStandby(new CheckSuspendResult(true, string.Format(Resources.Wsapm_Core.WsapmManager_TimerStartedAfterResumeAutomatic, TimeSpan.FromMilliseconds(interval).TotalMinutes.ToString("00"))));
            this.resumeAutomaticTimer.Interval = interval;
            this.resumeAutomaticTimer.Elapsed += resumeAutomaticTimer_Elapsed;
            this.resumeAutomaticTimer.AutoReset = false;
            this.resumeAutomaticTimer.Start();
        }

        /// <summary>
        /// Stops the resume automatic timer.
        /// </summary>
        private void StopResumeAutomaticTimer()
        {
            if (this.resumeAutomaticTimer != null)
            {
                this.resumeAutomaticTimer.Stop();
                this.resumeAutomaticTimer.Elapsed -= resumeAutomaticTimer_Elapsed;
                this.resumeAutomaticTimer.Close();
                this.resumeAutomaticTimer = null;
            }
        }

        /// <summary>
        /// Resume automatic timer elapses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resumeAutomaticTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EnableStandby();
        }

        private bool SystemWokenByWakeTimer()
        {
            if (!this.lastWakeTime.HasValue)
                return false;

            if (DateTime.Now - this.lastWakeTime.Value <= TimeSpan.FromMinutes(1))
                return true;
            else
                return false;
        }

        #endregion Wake

        #region Handle network connections

        /// <summary>
        /// Resets all active network connections when the appropriate option is senabled.
        /// </summary>
        private void ResetActiveNetworkConnections()
        {
            try
            {
                if (this.currentSettings != null && this.currentSettings.ResetNetworkConnectionsAfterWake)
                {
                    WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_ResetNetworkConnectionsAfterWake, LogMode.Normal);
                    NetworkTools.ResetActiveNetworkConnections();
                }
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
            }
        }

        #endregion Handle network connections

        #region Handle restart of services

        private void RestartServices()
        {
            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        System.Threading.Thread.Sleep(timeSpanBeforeStartProgramsAndServices);

                        if (this.currentSettings != null && this.currentSettings.RestartServicesAfterEveryWake != null && this.currentSettings.RestartServicesAfterEveryWake.Count > 0)
                        {
                            WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_RestartServicesAfterWake, LogMode.Normal);
                            ServiceManager.RestartServices(this.currentSettings.RestartServicesAfterEveryWake.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        WsapmLog.Log.WriteError(ex);
                    }
                });
        }

        #endregion Handle restart of services

        #region Handle Start of programs

        private void StartProgramsAfterEveryWake()
        {
            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        System.Threading.Thread.Sleep(timeSpanBeforeStartProgramsAndServices);

                        if (this.currentSettings != null && this.currentSettings.EnableStartProgramsAfterEveryWake && this.currentSettings.StartProgramsAfterEveryWake != null && this.currentSettings.StartProgramsAfterEveryWake.Count > 0)
                        {
                            WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_StartProgramsAfterEveryWake, LogMode.Normal);
                            ProgramManager.StartPrograms(this.currentSettings.StartProgramsAfterEveryWake.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        WsapmLog.Log.WriteError(ex);
                    }
                });
        }

        private void StartProgramsAfterScheduledWake(WakeScheduler wakeScheduler)
        {
            if (wakeScheduler == null)
                return;

            if (this.currentSettings != null && this.currentSettings.WakeSchedulers != null && this.currentSettings.EnableWakeTimers)
            {
                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(timeSpanBeforeStartProgramsAndServices);

                    try
                    {
                        if (wakeScheduler.StartProgramsAfterWake != null && wakeScheduler.StartProgramsAfterWake.Count > 0 && wakeScheduler.EnableStartProgramsAfterWake
                            && (SystemWokenByWakeTimer() || wakeScheduler.StartProgramsWhenSystemIsAlreadyRunning))
                        {
                            WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_StartProgramsAfterWake, LogMode.Normal);
                            ProgramManager.StartPrograms(wakeScheduler.StartProgramsAfterWake.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        WsapmLog.Log.WriteError(ex);
                    }
                });
            }
        }

        #endregion Handle Start of programs

        #region Handle remote shutdown

        /// <summary>
        /// Starts listening for remote shut down commands.
        /// </summary>
        private void StartShutdownListening()
        {
            if (this.shutdownManager != null)
            {
                if (this.currentSettings.EnableRemoteShutdown)
                    this.shutdownManager.Start(this.currentSettings.RemoteShutdownPort, this.currentSettings.RemoteShutdownPasswordHash);
            }
        }

        /// <summary>
        /// Stops listening for remote shutdown commands.
        /// </summary>
        private void StopShutdownListening()
        {
            if (this.shutdownManager != null)
                this.shutdownManager.Stop();
        }

        #endregion Handle remote shutdown

        #region Updates

        private static void CheckForUpdates()
        {
            var updateManager = new WsapmUpdateManager();
            var updateStatus = updateManager.CheckForUpdates();

            if (updateStatus.HasValue && updateStatus.Value)
                WsapmLog.Log.WriteLine(string.Format(Resources.Wsapm_Core.WsapmManager_UpdateAvailable, updateManager.UpdateInformation.CurrentVersion), LogMode.Normal);
        }

        #endregion Updates

        #region Settings

        /// <summary>
        /// Loads the settings and updates the current settings.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                this.currentSettings = this.settingsManager.LoadSettings();
                WsapmLog.Log.MaxLogFileSize = this.currentSettings.MaxLogFileSize;
                WsapmLog.Log.LogMode = this.currentSettings.LogMode;

                ValidateSettings(this.currentSettings);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// Settings changed event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settingsManager_SettingsChanged(object sender, EventArgs e)
        {
            LoadSettings();
            WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_SettingsChanged, LogMode.Verbose);
            StartMonitoringTimer();
            SetWakeTimers();

            if (currentSettings.EnableRemoteShutdown)
                this.shutdownManager.Start(this.currentSettings.RemoteShutdownPort, this.currentSettings.RemoteShutdownPasswordHash);
            else
                this.shutdownManager.Stop();
        }
        
        private void TemporaryUptimeManager_TemporaryUptimeChanged(object sender, EventArgs e)
        {
            if (this.temporaryUptimeManager == null)
                return;

            var temporaryUptime = this.temporaryUptimeManager.TemporaryUptimeDefinedAndActive;

            if(temporaryUptime.HasValue)
            {
                // Temporary uptime defined
                WsapmLog.Log.WriteLine(string.Format(Resources.Wsapm_Core.WsapmManager_TemporaryUptimeDefined, temporaryUptime.Value.ToShortTimeString()), LogMode.Normal);
            }
            else
            {
                // No temporary uptime defined (exceeded or canceled).
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.WsapmManager_TemporaryUptimeUnDefined, LogMode.Verbose);
            }
        }

        /// <summary>
        /// Validates the given settings.
        /// </summary>
        /// <param name="settings"></param>
        private void ValidateSettings(WsapmSettings settings)
        {
            // Validate settings and print warning when needed.
            if (!WsapmTools.ValidateCheckInterval(settings))
            {
                var winIdleTimeoutAcMinutes = WsapmTools.GetCurrentWindowsIdleTimeoutAcMinutes();
                WsapmLog.Log.WriteWarning(String.Format(Resources.Wsapm_Core.WsapmManager_ValidationErrorCheckInterval, winIdleTimeoutAcMinutes, settings.MonitoringTimerInterval), LogMode.Verbose);
            }

            // Validate if wake timers are allowed.
            if (!WsapmTools.ValidateWakeTimers(settings))
                WsapmLog.Log.WriteWarning(Resources.Wsapm_Core.WsapmManager_ValidationErrorWakeTimers, LogMode.Verbose);
        }

        #endregion Settings

        #region Logging

        private void WriteErrorLog(Exception ex)
        {
            var sb = new StringBuilder();
            sb.Append(Resources.Wsapm_Core.WsapmManager_ErrorLogFileCaption);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(ex.Message);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Resources.Wsapm_Core.WsapmManager_ErrorLogStacktraceCaption);
            sb.Append(Environment.NewLine);
            sb.Append(ex.StackTrace);
            var errorString = sb.ToString();
            var fileName = WsapmTools.GetNewErrorLogFileName();

            try
            {
                File.WriteAllText(fileName, errorString);
            }
            catch (Exception)
            {
            }
        }

        #endregion Logging

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
                if (this.monitoringTimer != null)
                    this.monitoringTimer.Dispose();

                if (this.resumeAutomaticTimer != null)
                    this.resumeAutomaticTimer.Dispose();

                if (this.standbyManager != null)
                    this.standbyManager.Dispose();

                if (this.temporaryUptimeManager != null)
                    this.temporaryUptimeManager.Dispose();

                if (this.settingsManager != null)
                    this.settingsManager.Dispose();
            }
        }

        #endregion IDisposable members
    }
}
