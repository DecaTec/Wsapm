using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private ServiceManager serviceManager;
        private SettingsManager settingsManager;
        private TemporaryUptimeManager temporaryUptimeManager;
        private LogManager logManager;
        private WsapmSettings currentSettings;
        private bool startupPluginConfig;

        private object settingsLock;

        // Controls inside the StatusPanels.
        // TODO: Add a reference when content controls in StausPanels change.
        private TextBox textBoxLog;
        private Hyperlink linkStartService;
        private Hyperlink linkStopService;
        private Hyperlink linkSettings;
        private TextBlock textBlockStatus;
        private TextBlock textBlockWake;
        private TextBlock textBlockUptime;
        private TextBlock textBlockPlugins;
        private TextBlock textBlockRemoteShutdown;
        private ScrollViewer scrollViewerLog;
        private TextBlock hyperlinkTextBlockTemporaryUptime;

        public MainWindow()
        {            
            if (AlreadyRunning())
            {
                MessageBox.Show(Wsapm.Resources.Wsapm.MainWindow_AppAlreadyRunning, Wsapm.Resources.Wsapm.MainWindow_Title, MessageBoxButton.OK, MessageBoxImage.Information);
                App.Current.Shutdown();
                return;
            }

            ParseArgs();

            try
            {
                this.settingsLock = new object();
                this.logManager = new LogManager(WsapmTools.GetCommonApplicationDataFolder(), WsapmConstants.WsapmApplicationLogFile);
                this.logManager.LogChanged += logManager_LogChanged;

                InitializeComponent();

                this.serviceManager = new ServiceManager();
                this.settingsManager = new SettingsManager();

                lock (this.settingsLock)
                {
                    this.currentSettings = this.settingsManager.LoadSettings();
                }

                this.logManager.Start();

                if (this.textBoxLog != null && this.scrollViewerLog != null)
                {
                    this.textBoxLog.Text = this.logManager.Log; // Load current log manually for the first time.
                    this.scrollViewerLog.ScrollToEnd();
                }

                this.temporaryUptimeManager = new TemporaryUptimeManager();
            }
            catch (Exception ex)
            {
                UnhandledExceptionManager.HandleException(ex);
            }
        }

        private void ParseArgs()
        {
            this.startupPluginConfig = false;
            var args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
            {
                if (arg.ToLower() == "-startpluginconfig")
                    this.startupPluginConfig = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start watching the service after window is loaded in order to have access to the status panels inner controls.
            // See workaround.
            CheckStatusPanelStatusAsync();
            CheckStatusPanelWakeAsync();
            CheckStatusPanelUptimeAsync();
            CheckStatusPanelPluginsAsync();
            // This is pretty static, so we don't need a BackgroundWorker, etc.
            FillStatusPanelRemoteShutdown();

            if(this.startupPluginConfig)
            {
                var settingsWindow = new SettingsWindow(InitTabSettings.Plugins);
                settingsWindow.ShowDialog();

                FillAllStatusPanels();
            }
        }

        void logManager_LogChanged(object sender, EventArgs e)
        {
            if (this.textBoxLog != null && this.scrollViewerLog != null)
            {
                Dispatcher.Invoke((Action)(() => this.textBoxLog.Text = this.logManager.Log));
                Dispatcher.Invoke((Action)(() => this.scrollViewerLog.ScrollToEnd()));
            }
        }

        private void CheckStatusPanelStatusAsync()
        {
            var workerStatusPanelStatus = new BackgroundWorker();
            workerStatusPanelStatus.DoWork += workerStatusPanelStatus_DoWork;
            workerStatusPanelStatus.RunWorkerCompleted += workerStatusPanelStatus_RunWorkerCompleted;
            workerStatusPanelStatus.RunWorkerAsync();
        }

        private void workerStatusPanelStatus_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CheckStatusPanelStatusAsync();
        }

        /// <summary>
        /// Handles the contents of the status panel (depending if service is running, validation errors, etc.).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void workerStatusPanelStatus_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!this.serviceManager.ServiceInstalled)
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelService.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Error));
                Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelStatusServiceNotInstalled));
                Dispatcher.Invoke((Action)(() => this.linkStartService.IsEnabled = false));
                Dispatcher.Invoke((Action)(() => this.linkStopService.IsEnabled = false));
            }
            else
            {
                var running = this.serviceManager.ServiceRunning;

                if (!this.serviceManager.ServiceRunning)
                {
                    // Service not running.
                    Dispatcher.Invoke((Action)(() => this.statusPanelService.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Warning));
                    Dispatcher.Invoke((Action)(() => this.linkStartService.IsEnabled = true));
                    Dispatcher.Invoke((Action)(() => this.linkStopService.IsEnabled = false));

                    if (!String.IsNullOrEmpty(this.serviceManager.StatusMessage))
                        Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text = this.serviceManager.StatusMessage));
                    else
                        Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelStatusServiceNotMonitoring));
                }
                else
                {
                    // Service running.
                    // Check for temporary uptime
                    DateTime? temporaryUptimeUntil = this.temporaryUptimeManager.TemporaryUptimeDefinedAndActive;

                    if(temporaryUptimeUntil.HasValue)
                    {
                        // Temporary Uptime defined.
                        Dispatcher.Invoke((Action)(() => this.statusPanelService.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Warning));
                        Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text = string.Format(Wsapm.Resources.Wsapm.MainWindow_StatusPanelStatusTemporaryUptimeDefinedUntil, temporaryUptimeUntil.Value.ToShortTimeString())));
                        Dispatcher.Invoke((Action)(() => this.hyperlinkTextBlockTemporaryUptime.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelStatusDeleteTemporaryUptime));
                        return;
                    }
                    else
                    {
                        // No temporary uptime defined.
                        Dispatcher.Invoke((Action)(() => this.hyperlinkTextBlockTemporaryUptime.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelStatusTemporaryUptime));
                    }

                    // Now check for validation errors.
                    bool validationErrors = false;

                    if (currentSettings != null)
                    {
                        bool checkIntervalValid = true;
                        bool wakeTimersValid = true;
                        uint currentCheckInterval = 0;

                        lock (this.settingsLock)
                        {
                            // Validate settings and print warning when needed.
                            checkIntervalValid = WsapmTools.ValidateCheckInterval(currentSettings);

                            // Validate if wake timers are allowed.
                            wakeTimersValid = WsapmTools.ValidateWakeTimers(currentSettings);
                            currentCheckInterval = currentSettings.MonitoringTimerInterval;
                        }

#if DEBUG
                        checkIntervalValid = true;
                        wakeTimersValid = true;
#endif

                        if (!checkIntervalValid || !wakeTimersValid)
                        {
                            validationErrors = true;
                            Dispatcher.Invoke((Action)(() => this.statusPanelService.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Warning));
                        }

                        Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text = String.Empty));

                        if (!checkIntervalValid)
                        {
                            var winIdleTimeoutAcMinutes = WsapmTools.GetCurrentWindowsIdleTimeoutAcMinutes();
                            Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text = String.Format(Wsapm.Resources.Wsapm.MainWindow_ValidationErrorCheckInterval, winIdleTimeoutAcMinutes, currentCheckInterval)));
                        }

                        if (!wakeTimersValid)
                            Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text += Environment.NewLine + Wsapm.Resources.Wsapm.MainWindow_ValidationErrorWakeTimers));
                    }

                    if (!validationErrors)
                    {
                        // Everything OK.
                        Dispatcher.Invoke((Action)(() => this.statusPanelService.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.OK));
                        Dispatcher.Invoke((Action)(() => this.linkStartService.IsEnabled = false));
                        Dispatcher.Invoke((Action)(() => this.linkStopService.IsEnabled = true));

                        if (!String.IsNullOrEmpty(this.serviceManager.StatusMessage))
                            Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text = this.serviceManager.StatusMessage));
                        else
                            Dispatcher.Invoke((Action)(() => this.textBlockStatus.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelStatusServiceMonitoring));
                    }
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        private void CheckStatusPanelWakeAsync()
        {
            var workerStatusPanelWake = new BackgroundWorker();
            workerStatusPanelWake.DoWork += workerStatusPanelWake_DoWork;
            workerStatusPanelWake.RunWorkerCompleted += workerStatusPanelWake_RunWorkerCompleted;
            workerStatusPanelWake.RunWorkerAsync();
        }

        private void workerStatusPanelWake_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CheckStatusPanelWakeAsync();
        }

        private void workerStatusPanelWake_DoWork(object sender, DoWorkEventArgs e)
        {
            FillStatusPanelWake();
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        private void FillStatusPanelWake()
        {
            var nextWakeTimes = GetNextWakeActiveWakeScheulers();

            if (nextWakeTimes == null || nextWakeTimes.Length == 0 || !this.serviceManager.ServiceInstalled || !this.serviceManager.ServiceRunning)
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelWake.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Undefined));
                Dispatcher.Invoke((Action)(() => this.textBlockWake.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelWakeNoWakeScheduled));
            }
            else
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelWake.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.OK));

                var sb = new StringBuilder();

                for (int i = 0; i < nextWakeTimes.Length; i++)
                {
                    if (!nextWakeTimes[i].NextDueTime.HasValue)
                        continue;

                    sb.Append(nextWakeTimes[i].NextDueTime.Value.ToLongDateString());
                    sb.Append(" ");
                    sb.Append(nextWakeTimes[i].NextDueTime.Value.ToLongTimeString());

                    if (nextWakeTimes[i].EnableStartProgramsAfterWake && nextWakeTimes[i].StartProgramsAfterWakeActive && nextWakeTimes[i].StartProgramsAfterWake != null && nextWakeTimes[i].StartProgramsAfterWake.Count() > 0)
                    {
                        sb.Append(" (");
                        sb.Append(Wsapm.Resources.Wsapm.MainWindow_StatusPanelWakeStartProgramsAfterWake);
                        sb.Append(")");
                    }

                    if (i != nextWakeTimes.Length - 1)
                        sb.Append(Environment.NewLine);
                }

                Dispatcher.Invoke((Action)(() => this.textBlockWake.Text = sb.ToString()));
            }
        }

        private void FillStatusPanelUptime()
        {
            var nextUptimes = GetNextUptimeActiveUptimeScheulers();

            if (nextUptimes == null || nextUptimes.Length == 0 || !this.serviceManager.ServiceInstalled || !this.serviceManager.ServiceRunning)
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelUptime.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Undefined));
                Dispatcher.Invoke((Action)(() => this.textBlockUptime.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelUptimeNoUptimeScheduled));
            }
            else
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelUptime.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.OK));

                var sb = new StringBuilder();

                for (int i = 0; i < nextUptimes.Length; i++)
                {
                    if (!nextUptimes[i].NextDueTime.HasValue)
                        continue;

                    sb.Append(nextUptimes[i].NextDueTime.Value.ToLongDateString());
                    sb.Append(" ");
                    sb.Append(nextUptimes[i].NextDueTime.Value.ToLongTimeString());
                    sb.Append(" (");
                    sb.Append(nextUptimes[i].Duration.ToString());
                    sb.Append(")");

                    if (i != nextUptimes.Length - 1)
                        sb.Append(Environment.NewLine);
                }

                Dispatcher.Invoke((Action)(() => this.textBlockUptime.Text = sb.ToString()));
            }
        }

        private void CheckStatusPanelUptimeAsync()
        {
            var workerStatusPanelUptime = new BackgroundWorker();
            workerStatusPanelUptime.DoWork += workerStatusPanelUptime_DoWork;
            workerStatusPanelUptime.RunWorkerCompleted += workerStatusPanelUptime_RunWorkerCompleted;
            workerStatusPanelUptime.RunWorkerAsync();
        }

        private void workerStatusPanelUptime_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CheckStatusPanelUptimeAsync();
        }

        private void workerStatusPanelUptime_DoWork(object sender, DoWorkEventArgs e)
        {
            FillStatusPanelUptime();
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        private UptimeScheduler[] GetNextUptimeActiveUptimeScheulers()
        {
            if (this.currentSettings == null || this.currentSettings.UptimeSchedulers == null || !this.currentSettings.EnableUptimes)
                return null;

            Dictionary<DateTime, UptimeScheduler> dictionaryUptimeSchedulers = new Dictionary<DateTime, UptimeScheduler>();

            foreach (var item in this.currentSettings.UptimeSchedulers)
            {
                if (item.EnableUptimeScheduler && !item.UptimeSchedulerExpired && item.NextDueTime.HasValue)
                    dictionaryUptimeSchedulers.Add(item.NextDueTime.Value, item);
            }

            return (from entry in dictionaryUptimeSchedulers
                    orderby entry.Key ascending
                    select entry.Value).Take(3).ToArray();
        }

        private void FillStatusPanelPlugins()
        {
            var installed = 0;
            var active = 0;
            var disabled = 0;

            if(this.currentSettings == null)
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelPlugins.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Undefined));
                Dispatcher.Invoke((Action)(() => this.textBlockPlugins.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelPluginsNoPluginsInstalled));
                return;
            }

            if (this.currentSettings.ActivePlugins != null)
                active = this.currentSettings.ActivePlugins.Count;

            if (this.currentSettings.DisabledPlugins != null)
                disabled = this.currentSettings.DisabledPlugins.Count;

            if (active == 0 && disabled == 0)
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelPlugins.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Undefined));
                Dispatcher.Invoke((Action)(() => this.textBlockPlugins.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelPluginsNoPluginsInstalled));
            }
            else
            {
                installed = active + disabled;

                Dispatcher.Invoke((Action)(() => this.statusPanelPlugins.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.OK));
                Dispatcher.Invoke((Action)(() => this.textBlockPlugins.Text = string.Format(Wsapm.Resources.Wsapm.MainWindow_StatusPanelPluginsPluginsInstalled, Environment.NewLine, installed, active, disabled)));
            }
        }

        private void CheckStatusPanelPluginsAsync()
        {
            var workerStatusPanelPlugins = new BackgroundWorker();
            workerStatusPanelPlugins.DoWork += workerStatusPanelPlugins_DoWork;
            workerStatusPanelPlugins.RunWorkerCompleted += workerStatusPanelPlugins_RunWorkerCompleted;
            workerStatusPanelPlugins.RunWorkerAsync();
        }

        private void workerStatusPanelPlugins_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CheckStatusPanelPluginsAsync();
        }

        private void workerStatusPanelPlugins_DoWork(object sender, DoWorkEventArgs e)
        {
            FillStatusPanelPlugins();
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        private void HyperlinkStartService_Click(object sender, RoutedEventArgs e)
        {
            if (this.serviceManager != null)
            {
                BackgroundWorker worker = new BackgroundWorker();

                worker.DoWork += (o, ea) =>
                {
                    this.serviceManager.StartService();
                    CheckStatusPanelStatusAsync();
                };

                worker.RunWorkerCompleted += (o, ea) =>
                {
                    this.busyIndicator.IsBusy = false;
                };

                this.busyIndicator.BusyContent = Wsapm.Resources.Wsapm.MainWindow_BusyIndicatorBusyContentStartService;
                this.busyIndicator.IsBusy = true;
                worker.RunWorkerAsync();
            }
        }

        private void HyperlinkStopService_Click(object sender, RoutedEventArgs e)
        {
            if (this.serviceManager != null)
            {
                BackgroundWorker worker = new BackgroundWorker();

                worker.DoWork += (o, ea) =>
                {
                    this.serviceManager.StopService();
                    CheckStatusPanelStatusAsync();
                };

                worker.RunWorkerCompleted += (o, ea) =>
                {
                    this.busyIndicator.IsBusy = false;
                };

                this.busyIndicator.BusyContent = Wsapm.Resources.Wsapm.MainWindow_BusyIndicatorBusyContentStopService;
                this.busyIndicator.IsBusy = true;
                worker.RunWorkerAsync();
            }
        }

        private void HyperlinkTemporaryUptime_Click(object sender, RoutedEventArgs e)
        {
            DateTime? temporaryUptimeUntil = this.temporaryUptimeManager.TemporaryUptimeDefinedAndActive;

            if(!temporaryUptimeUntil.HasValue)
            {
                // No temporary uptime defined.
                var temporaryUptimeWindow = new TemporaryUptimeWindow();
                temporaryUptimeWindow.ShowDialog();
            }
            else
            {
                try
                {
                    this.temporaryUptimeManager.DeleteTemporaryUptime();
                }
                catch (WsapmException ex)
                {
                    MessageBox.Show(string.Format("{1}:{0}{0}{2}", Environment.NewLine, ex.Message, ex.InnerException.Message), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void HyperlinkSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();

            lock (this.settingsLock)
            {
                this.currentSettings = this.settingsManager.LoadSettings();
            }

            FillAllStatusPanels();
        }

        private void FillAllStatusPanels()
        {
            FillStatusPanelRemoteShutdown();
            FillStatusPanelWake();
            FillStatusPanelUptime();
            FillStatusPanelPlugins();
        }

        private void HyperlinkWindowsEnergySettings_Click(object sender, RoutedEventArgs e)
        {
            StartWindowsEnergySettings();
        }

        private void StartWindowsEnergySettings()
        {
            var psi = new ProcessStartInfo();
            psi.Arguments = "powercfg.cpl";
            psi.FileName = "control.exe";
            Process.Start(psi);
        }

        private static bool AlreadyRunning()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processesByName = Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (Process process in processesByName)
            {
                if (process.Id != currentProcess.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == currentProcess.MainModule.FileName)
                        return true;
                }
            }

            return false;
        }

        private void HyperlinkWakeSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(InitTabSettings.Wake);
            settingsWindow.ShowDialog();

            lock (this.settingsLock)
            {
                this.currentSettings = this.settingsManager.LoadSettings();
            }

            FillAllStatusPanels();
        }

        private void HyperlinkPluginSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(InitTabSettings.Plugins);
            settingsWindow.ShowDialog();

            lock (this.settingsLock)
            {
                this.currentSettings = this.settingsManager.LoadSettings();
            }

            FillAllStatusPanels();
        }

        private void HyperlinkUptimeSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(InitTabSettings.Uptime);
            settingsWindow.ShowDialog();

            lock (this.settingsLock)
            {
                this.currentSettings = this.settingsManager.LoadSettings();
            }

            FillAllStatusPanels();
        }

        private void HyperlinkRemoteShutdownSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(InitTabSettings.RemoteShutdown);
            settingsWindow.ShowDialog();

            lock (this.settingsLock)
            {
                this.currentSettings = this.settingsManager.LoadSettings();
            }

            FillAllStatusPanels();
        }

        private void FillStatusPanelRemoteShutdown()
        {
            if (this.currentSettings == null || !this.currentSettings.EnableRemoteShutdown)
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelRemoteShutdown.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.Undefined));
                Dispatcher.Invoke((Action)(() => this.textBlockRemoteShutdown.Text = Wsapm.Resources.Wsapm.MainWindow_StatusPanelRemoteShutdownRemoteShutdownDisabled));
            }
            else
            {
                Dispatcher.Invoke((Action)(() => this.statusPanelRemoteShutdown.Status = Wsapm.Wpf.Controls.StatusPanel.StatusPanelStatus.OK));

                if(string.IsNullOrEmpty(this.currentSettings.RemoteShutdownPasswordHash))
                    Dispatcher.Invoke((Action)(() => this.textBlockRemoteShutdown.Text = string.Format(Wsapm.Resources.Wsapm.MainWindow_StatusPanelRemoteShutdownRemoteShutdownEnabled, this.currentSettings.RemoteShutdownPort)));
                else
                    Dispatcher.Invoke((Action)(() => this.textBlockRemoteShutdown.Text = string.Format(Wsapm.Resources.Wsapm.MainWindow_StatusPanelRemoteShutdownRemoteShutdownEnabledWithPassword, this.currentSettings.RemoteShutdownPort)));
            }
        }

        private void HyperlinkCopyLogToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var txt = this.textBoxLog.Text;
            Clipboard.SetText(txt);
        }

        #region Get contents from StatusPanel

        // TODO:
        // When adding new content to the StatusPanel controls, this code has to be modified!
        //
        // Because the content items of custom user control can't have a name attribute,
        // all content have to be accessed in another way.
        // So, after these contents where loaded, references to these controls are hold in the Window class.

        private void TextBoxLog_Initialized(object sender, EventArgs e)
        {
            this.textBoxLog = (TextBox)sender;
        }

        private void HyperlinkStart_Initialized(object sender, EventArgs e)
        {
            this.linkStartService = (Hyperlink)sender;
        }

        private void HyperlinkStop_Initialized(object sender, EventArgs e)
        {
            this.linkStopService = (Hyperlink)sender;
        }

        private void HyperlinkSettings_Initialized(object sender, EventArgs e)
        {
            this.linkSettings = (Hyperlink)sender;
        }

        private void TextBlockStatus_Initialized(object sender, EventArgs e)
        {
            this.textBlockStatus = (TextBlock)sender;
        }

        private void ScrollViewer_Initialized(object sender, EventArgs e)
        {
            this.scrollViewerLog = (ScrollViewer)sender;
        }

        private void TextBlockWake_Initialized(object sender, EventArgs e)
        {
            this.textBlockWake = (TextBlock)sender;
        }

        private void TextBlockPlugins_Initialized(object sender, EventArgs e)
        {
            this.textBlockPlugins = (TextBlock)sender;
        }

        private void TextBlockUptime_Initialized(object sender, EventArgs e)
        {
            this.textBlockUptime = (TextBlock)sender;
        }

        private void TextBlockRemoteShutdown_Initialized(object sender, EventArgs e)
        {
            this.textBlockRemoteShutdown = (TextBlock)sender;
        }

        private void HyperLinkTemporaryUptime_Initialized(object sender, EventArgs e)
        {
            this.hyperlinkTextBlockTemporaryUptime = (TextBlock)sender;
        }

        #endregion Get conents from StatusPanel

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void ButtonCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var updateManager = new WsapmUpdateManager();
            bool? updateAvailable = null;
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, ea) =>
            {
                updateAvailable = updateManager.CheckForUpdates();
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                this.busyIndicator.IsBusy = false;

                if (!updateAvailable.HasValue)
                {
                    // We're offline.
                    var result = MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.MainWindow_MessageBoxNoUpdateAvailableOffline, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if (result == MessageBoxResult.Yes)
                    {
                        ProcessStartInfo psi = new ProcessStartInfo("https://decatec.de/software/windows-server-advanced-power-management/");
                        Process.Start(psi);
                    }
                }
                else if (updateAvailable.HasValue && updateAvailable.Value)
                {
                    // Update available.
                    var updateInfo = updateManager.UpdateInformation;

                    var messageResult = MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.MainWindow_MessageBoxUpdateAvailable, Environment.NewLine, VersionInformation.Version.ToString(3), updateInfo.CurrentVersion), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (messageResult == MessageBoxResult.Yes)
                    {
                        if (!updateManager.UpdateWsapm())
                        {
                            // Something went wrong while downloading the update.
                            var result = MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.MainWindow_MessageBoxUpdateFailed, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);

                            if (result == MessageBoxResult.Yes)
                            {
                                ProcessStartInfo psi = new ProcessStartInfo("https://decatec.de/software/windows-server-advanced-power-management/");
                                Process.Start(psi);
                            }
                        }
                    }
                }
                else
                {
                    // No update available.
                    MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.MainWindow_MessageBoxNoUpdateAvailable, Environment.NewLine, VersionInformation.Version.ToString(3)), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            this.busyIndicator.BusyContent = Wsapm.Resources.Wsapm.MainWindow_BusyIndicatorBusyContentCheckForUpdates;
            this.busyIndicator.IsBusy = true;
            worker.RunWorkerAsync();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown(1);
        }

        private WakeScheduler[] GetNextWakeActiveWakeScheulers()
        {
            if (this.currentSettings == null || this.currentSettings.WakeSchedulers == null || !this.currentSettings.EnableWakeTimers)
                return null;

            Dictionary<DateTime, WakeScheduler> dictionaryWakeSchedulers = new Dictionary<DateTime, WakeScheduler>();

            foreach (var item in this.currentSettings.WakeSchedulers)
            {
                if (item.EnableWakeScheduler && !item.WakeSchedulerExpired && item.NextDueTime.HasValue)
                    dictionaryWakeSchedulers.Add(item.NextDueTime.Value, item);
            }

            return (from entry in dictionaryWakeSchedulers
                    orderby entry.Key ascending
                    select entry.Value).Take(3).ToArray();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.logManager != null)
                    this.logManager.Dispose();

                if (this.serviceManager != null)
                    this.serviceManager.Dispose();

                if (this.settingsManager != null)
                    settingsManager.Dispose();
            }
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

        #endregion IDisposable members
    }
}
