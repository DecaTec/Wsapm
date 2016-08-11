using Ionic.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Wsapm.Core;
using Wsapm.Extensions;
using System.Linq;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, IDisposable
    {
        private SettingsManager settingsManager;
        private WsapmSettings currentSettings;
        private bool closedByButton;
        private bool settingsChanged;
        private bool closedByPluginInstallation;
        // Flag indicating if UI initialization is running.
        // Use this flag in order to suppress validation in changed events, etc.
        private bool uiInitializing;
        private InitTabSettings initTab;
        private string oldPasswordHash;
        private const string dummyPassword = "****";
        private bool remotePasswordChanged = false;

        // Stuff for showing current system loads (CPU/RAM).
        private CpuLoadCurrent cpuLoad;
        private MemoryLoadCurrent memoryLoad;
        private Timer loadTimer;

        private readonly TimeSpan displayRefreshIntervalForLoads = TimeSpan.FromSeconds(1);

        private static Action EmptyDelegate = delegate () { };

        public SettingsWindow()
        {
            this.uiInitializing = true;
            this.settingsManager = new SettingsManager();
            this.currentSettings = this.settingsManager.LoadSettings();

            InitializeComponent();

            this.labelCurrentCpuLoad.Content = Wsapm.Resources.Wsapm.SettingsWindow_LabelCurrentCpuLoadDefault;            
            this.busyIndicator.BusyContent = Wsapm.Resources.Wsapm.General_BusyIndicatorBusyContent;
            this.busyIndicator.IsBusy = true;
            this.settingsManager.SettingsChanged += settingsManager_SettingsChanged;
            this.closedByButton = false;
            this.closedByPluginInstallation = false;
            this.uiInitializing = false;
            this.initTab = InitTabSettings.General;
        }

        public SettingsWindow(InitTabSettings initTab) : this()
        {
            this.initTab = initTab;
        }

        #region General

        private void InitUI()
        {
            this.uiInitializing = true;
            this.busyIndicator.IsBusy = true;
            this.busyIndicator.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Render); // Force render refresh so that the busy indicator is shown.
            InitTabGeneral();
            InitTabMonitoringSystem();
            InitTabMonitoringGeneral();
            InitTabAfterPolicyCheck();
            InitTabWake();
            InitTabUptime();
            InitTabPlugins();
            InitTabRemoteShutdown();
            this.settingsChanged = false; // Reset settings changed flag.
            this.tabControl.SelectedIndex = (int)this.initTab;
            this.busyIndicator.IsBusy = false;
            this.uiInitializing = false;
        }

        private void settingsManager_SettingsChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke((Action)(() => this.InitUI()));
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.closedByButton = true;
            var newSettings = BuildSettings();

            var valid = ValidateSettings(newSettings);

            if (valid)
            {
                // Only save and close after successful validation.
                // Avoid reloading settings in UI after successful validation.
                this.settingsManager.SettingsChanged -= settingsManager_SettingsChanged;
                this.settingsManager.SaveSettings(newSettings);
                this.DialogResult = true;
                this.Close();
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.closedByButton = true;
            this.DialogResult = false;
            this.Close();
        }

        private void menuItemLoadDefaultSettings_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxLoadDefaultSettings, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.currentSettings = this.settingsManager.GetDefaultSettings();
                InitUI();
            }
        }

        private void menuItemExportSettings_Click(object sender, RoutedEventArgs e)
        {
            if (this.settingsChanged)
            {
                var resultMsg = MessageBox.Show(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxSaveSettings, Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultMsg == MessageBoxResult.Yes)
                {
                    var newSettings = BuildSettings();
                    this.settingsManager.SaveSettings(newSettings);
                    this.settingsChanged = false;
                }
                else
                    return;
            }

            var sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.CheckFileExists = false;
            sfd.DefaultExt = "zip";
            sfd.Filter = Wsapm.Resources.Wsapm.SettingsWindow_ImportExportSettingsOpenFileDialogFilter;
            var result = sfd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                this.busyIndicator.IsBusy = true;
                var exportFile = sfd.FileName;
                this.settingsManager.ExportSettings(exportFile);
                this.busyIndicator.IsBusy = false;
            }
        }

        private void menuItemImportSettings_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = Wsapm.Resources.Wsapm.SettingsWindow_ImportExportSettingsOpenFileDialogFilter;
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                this.busyIndicator.IsBusy = true;

                try
                {
                    var importFile = ofd.FileName;
                    this.settingsManager.ImportSettings(importFile);
                    this.settingsChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format(string.Format(Wsapm.Resources.Wsapm.SettingsWindow_ErrorImportSettings, Environment.NewLine, ex.Message), Environment.NewLine, ex.Message), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Get new values in UI.
                this.currentSettings = this.settingsManager.LoadSettings();
                this.busyIndicator.IsBusy = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.uiInitializing)
                return; // No message should be shown if the UI is still initialized.

            if (!this.closedByButton && this.settingsChanged)
            {
                if (!this.closedByPluginInstallation)
                {
                    // 'Regular' close.
                    var result = MessageBox.Show(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxSaveSettings, Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var newSettings = BuildSettings();
                        this.settingsManager.SaveSettings(newSettings);
                    }
                    else if (result == MessageBoxResult.Cancel)
                        e.Cancel = true;
                }
                else
                {
                    // Closed because of plugin installation/uninstallation.
                    var result = MessageBox.Show(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxSaveSettings, Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var newSettings = BuildSettings();
                        this.settingsManager.SaveSettings(newSettings);
                    }
                }
            }
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.busyIndicator.IsBusy = false;
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke((Action)(() => this.busyIndicator.IsBusy = true));
            Dispatcher.Invoke((Action)(() => this.InitUI()));
        }

        private bool ValidateSettings(WsapmSettings settings)
        {
            // Validate check interval.
            var winIdleTimeoutAcMinutes = WsapmTools.GetCurrentWindowsIdleTimeoutAcMinutes();

            if (settings.MonitoringTimerInterval >= winIdleTimeoutAcMinutes)
            {
                var result = MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_ValidationErrorCheckInterval, Environment.NewLine, winIdleTimeoutAcMinutes, settings.MonitoringTimerInterval), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    // Open affected tab.
                    this.tabGeneral.IsSelected = true;
                    return false;
                }
            }

            //Validate if wake timers are allowed.
            bool atLeastOneWakeSchedulerActive = false;

            if (settings.WakeSchedulers != null)
            {
                foreach (var wakeScheduler in settings.WakeSchedulers)
                {
                    if (wakeScheduler.EnableWakeScheduler)
                    {
                        atLeastOneWakeSchedulerActive = true;
                        break;
                    }
                }
            }

            if (atLeastOneWakeSchedulerActive)
            {
                if (!WsapmTools.GetWakeTimersAllowed())
                {
                    var result = MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_ValidationErrorWakeTimers, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        // Open affected tab.
                        this.tabItemWake.IsSelected = true;
                        return false;
                    }
                }
            }

            if (settings.EnableRemoteShutdown)
            {
                var password1 = this.passwordBoxRemoteShutdown.Password;
                var password2 = this.passwordBoxRemoteShutdownRepetition.Password;

                if (password1 != password2)
                {
                    MessageBox.Show(Wsapm.Resources.Wsapm.SettingsWindow_ValidationErrorRemoteShutdownPassword, Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    // Open affected tab.
                    this.tabItemRemoteShutdown.IsSelected = true;
                    return false;
                }
            }

            return true;
        }

        private WsapmSettings BuildSettings()
        {
            var newSettings = new WsapmSettings();

            // Tab General.
            newSettings.MonitoringTimerInterval = (uint)this.upDownCheckInterval.Value;

            newSettings.ResetNetworkConnectionsAfterWake = this.checkBoxResetNetworkConnectionAfterWake.IsChecked.Value;

            newSettings.EnableRestartServicesAfterEveryWake = this.checkBoxRestartServicesAfterEveryWake.IsChecked.Value;
            List<Service> servicesToRestart = new List<Service>(this.dataGridRestartServicesAfterEveryWake.Items.Count);

            foreach (var item in this.dataGridRestartServicesAfterEveryWake.Items)
            {
                var serviceToRestart = item as Service;

                if (serviceToRestart == null || String.IsNullOrEmpty(serviceToRestart.ServiceName))
                    continue;

                servicesToRestart.Add(serviceToRestart);
            }

            newSettings.RestartServicesAfterEveryWake = servicesToRestart;

            newSettings.EnableStartProgramsAfterEveryWake = this.checkBoxStartProgramsAfterEveryWake.IsChecked.Value;
            List<ProgramStart> programsToStart = new List<ProgramStart>(this.dataGridStartProgramsAfterEveryWake.Items.Count);

            foreach (var item in this.dataGridStartProgramsAfterEveryWake.Items)
            {
                var programToStart = item as ProgramStart;

                if (programToStart == null)
                    continue;

                programsToStart.Add(programToStart);
            }

            newSettings.StartProgramsAfterEveryWake = programsToStart;

            if (this.comboBoxLogMode.SelectedIndex == 0)
            {
                // None.
                newSettings.LogMode = LogMode.None;
            }
            else if (this.comboBoxLogMode.SelectedIndex == 1)
            {
                // OnlyErrors.
                newSettings.LogMode = LogMode.OnlyErrors;
            }
            else if (this.comboBoxLogMode.SelectedIndex == 2)
            {
                // Normal.
                newSettings.LogMode = LogMode.Normal;
            }
            else if (this.comboBoxLogMode.SelectedIndex == 3)
            {
                // Verbose.
                newSettings.LogMode = LogMode.Verbose;
            }

            newSettings.MaxLogFileSize = (uint)WsapmConvert.ConvertKBToByte((float)this.upDownMaxLoggingFileSize.Value);

            // Tab Monitoring (system)
            newSettings.EnableNetworkInterfacesToMonitor = this.checkBoxEnableNetworkInterfaces.IsChecked.Value;

            List<NetworkInterfaceToMonitor> nicsToMonitor = new List<NetworkInterfaceToMonitor>(this.dataGridNetworkInterfaces.Items.Count);

            foreach (var item in this.dataGridNetworkInterfaces.Items)
            {
                var nic = item as NetworkInterfaceToMonitor;

                if (nic == null)
                    continue;

                if (nic.NetworkInterface == WsapmTools.GetCommonDiaplayNameAllNetworkInterfaces())
                    nic.NetworkInterface = WsapmConstants.AllNetworkInterfacesName;

                nicsToMonitor.Add(nic);
            }

            newSettings.NetworkInterfacesToMonitor = nicsToMonitor;

            newSettings.EnableMonitorProcesses = this.checkBoxEnableProcesses.IsChecked.Value;
            newSettings.EnableCheckNetworkResourceAccess = this.checkBoxEnableNetworkResourcesAccessCheck.IsChecked.Value;

            if (this.comboBoxNetworkResourceAccessType.SelectedIndex == 0)
                newSettings.CheckNetworkResourcesType = NetworkShareAccessType.Files;
            else if (this.comboBoxNetworkResourceAccessType.SelectedIndex == 1)
                newSettings.CheckNetworkResourcesType = NetworkShareAccessType.Directories;
            else if (this.comboBoxNetworkResourceAccessType.SelectedIndex == 2)
                newSettings.CheckNetworkResourcesType = NetworkShareAccessType.Directories | NetworkShareAccessType.Files;

            newSettings.EnableCheckCpuLoad = this.checkBoxEnableCpuLoad.IsChecked.Value;
            newSettings.CpuLoad = (float)this.upDownCpuLoad.Value;

            newSettings.EnableCheckMemoryLoad = this.checkBoxEnableMemoryLoad.IsChecked.Value;
            newSettings.MemoryLoad = (float)this.upDownMemoryLoad.Value;

            newSettings.EnableHddsToMonitor = this.checkBoxEnableHdd.IsChecked.Value;

            List<HddToMonitor> hddsToMonitor = new List<HddToMonitor>(this.dataGridHdd.Items.Count);

            foreach (var item in this.dataGridHdd.Items)
            {
                var hdd = item as HddToMonitor;

                if (hdd == null)
                    continue;

                if (hdd.Drive == WsapmTools.GetCommonDiaplayNameAllDrives())
                    hdd.Drive = WsapmConstants.AllHddsName;

                hddsToMonitor.Add(hdd);
            }

            newSettings.HddsToMonitor = hddsToMonitor;

            // Tab Monitoring (general).
            newSettings.EnableMonitorNetworkMachines = this.checkBoxEnableComputers.IsChecked.Value;

            List<NetworkMachine> networkMachines = new List<NetworkMachine>(this.dataGridComputers.Items.Count);

            foreach (var item in this.dataGridComputers.Items)
            {
                var networkMachine = item as NetworkMachine;

                if (networkMachine == null)
                    continue;

                networkMachines.Add(networkMachine);
            }

            newSettings.NetworkMachinesToMonitor = networkMachines;

            newSettings.EnableMonitorProcesses = this.checkBoxEnableProcesses.IsChecked.Value;

            List<ProcessToMonitor> processes = new List<ProcessToMonitor>(this.dataGridProcesses.Items.Count);

            foreach (var item in this.dataGridProcesses.Items)
            {
                var process = item as ProcessToMonitor;

                if (process == null)
                    continue;

                processes.Add(process);
            }

            newSettings.ProcessesToMonitor = processes;          

            // Tab After policy check
            newSettings.EnableActionsAfterPolicyCheck = this.checkBoxEnableActionsAfterPolicyCheck.IsChecked.Value;
            var actionsAfterPolicyCheck = BuildActionsAfterPolicyCheck();
            newSettings.ActionsAfterPolicyCheck = actionsAfterPolicyCheck;

            // Tab Wake.
            newSettings.EnableWakeTimers = this.checkBoxEnableWakeSchedulers.IsChecked.Value;
            var wakeSchedulers = BuildWakeSchedulers();
            newSettings.WakeSchedulers = wakeSchedulers;

            // Tab uptime
            newSettings.EnableUptimes = this.checkBoxEnableUptimeSchedulers.IsChecked.Value;
            var uptimeSchedulders = BuildUptimeSchedulers();
            newSettings.UptimeSchedulers = uptimeSchedulders;

            // Tab Plugins.
            List<Guid> activePlugins = new List<Guid>();
            List<Guid> disabledPlugins = new List<Guid>();
            var plugins = this.dataGridPlugins.Items.SourceCollection;

            foreach (var plugin in plugins)
            {
                WsapmPluginBase pluginBase = plugin as WsapmPluginBase;

                if (pluginBase != null && pluginBase.IsActivated)
                    activePlugins.Add(pluginBase.PluginAttribute.PluginGuid);
                else
                    disabledPlugins.Add(pluginBase.PluginAttribute.PluginGuid);
            }           

            newSettings.ActivePlugins = activePlugins;
            newSettings.DisabledPlugins = disabledPlugins;

            // Tab Remote-Shutdown
            newSettings.EnableRemoteShutdown = this.checkBoxEnableRemoteShutdown.IsChecked.Value;
            newSettings.RemoteShutdownPort = (ushort)this.upDownRemoteShutdownPort.Value;

            if (this.remotePasswordChanged)
            {
                var password = this.passwordBoxRemoteShutdown.Password;

                if (string.IsNullOrEmpty(password))
                    newSettings.RemoteShutdownPasswordHash = null;
                else
                {
                    // Save only password hash.
                    var hash = SHA256Managed.Create();
                    var bArr = Encoding.UTF8.GetBytes(password);
                    var hashBytes = hash.ComputeHash(bArr);
                    newSettings.RemoteShutdownPasswordHash = System.Convert.ToBase64String(hashBytes);
                }
            }
            else
                newSettings.RemoteShutdownPasswordHash = this.oldPasswordHash;

            return newSettings;
        }

        #endregion General

        #region Tab General

        private void InitTabGeneral()
        {
            FillComboBoxLogMode();

            // Check interval.
            this.upDownCheckInterval.Value = (int)this.currentSettings.MonitoringTimerInterval;

            // Reset network connections after wake.
            this.checkBoxResetNetworkConnectionAfterWake.IsChecked = this.currentSettings.ResetNetworkConnectionsAfterWake;

            // Restart services after wake.
            this.checkBoxRestartServicesAfterEveryWake.IsChecked = this.currentSettings.EnableRestartServicesAfterEveryWake;
            FilldataGridRestartServicesAfterEveryWakeFromSettings();
            EnableDisableRestartServicesAfterEveryWakeOptions();

            // Start programs after every wake.
            this.checkBoxStartProgramsAfterEveryWake.IsChecked = this.currentSettings.EnableStartProgramsAfterEveryWake;
            FilldataGridStartProgramsAfterEveryWake();
            EnableDisableStartProgramsAfterEveryWakeOptions();

            // Logging.
            switch (this.currentSettings.LogMode)
            {
                case LogMode.None:
                    this.comboBoxLogMode.SelectedIndex = 0;
                    break;
                case LogMode.OnlyErrors:
                    this.comboBoxLogMode.SelectedIndex = 1;
                    break;
                case LogMode.Normal:
                    this.comboBoxLogMode.SelectedIndex = 2;
                    break;
                case LogMode.Verbose:
                    this.comboBoxLogMode.SelectedIndex = 3;
                    break;
                default:
                    break;
            }

            // Max. log file size.
            this.upDownMaxLoggingFileSize.Value = (int)WsapmConvert.ConvertByteToKB(this.currentSettings.MaxLogFileSize);
        }

        private void FillComboBoxLogMode()
        {
            this.comboBoxLogMode.Items.Clear();
            this.comboBoxLogMode.Items.Add(Wsapm.Resources.Wsapm.SettingsWindow_ComboBoxLogModeNone);
            this.comboBoxLogMode.Items.Add(Wsapm.Resources.Wsapm.SettingsWindow_ComboBoxLogModeOnlyErrors);
            this.comboBoxLogMode.Items.Add(Wsapm.Resources.Wsapm.SettingsWindow_ComboBoxLogModeNormal);
            this.comboBoxLogMode.Items.Add(Wsapm.Resources.Wsapm.SettingsWindow_ComboBoxLogModeVerbose);
            this.comboBoxLogMode.SelectedIndex = 2;
        }

        private void upDownCheckInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownCheckInterval.Value == null)
                this.upDownCheckInterval.Value = 1;

            this.settingsChanged = true;
        }

        private void buttonOptimalCheckInterval_Click(object sender, RoutedEventArgs e)
        {
            var optimalCheckInterval = WsapmTools.GetOptimalCheckIntervalInMinutes();
            this.upDownCheckInterval.Value = (int)optimalCheckInterval;
        }

        private void checkBoxResetNetworkConnectionAfterWake_Checked(object sender, RoutedEventArgs e)
        {
            this.settingsChanged = true;
        }

        private void checkBoxResetNetworkConnectionAfterWake_Unchecked(object sender, RoutedEventArgs e)
        {
            this.settingsChanged = true;
        }

        private void checkBoxRestartServicesAfterEveryWake_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableRestartServicesAfterEveryWakeOptions();
            this.settingsChanged = true;
        }

        private void checkBoxRestartServicesAfterEveryWake_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableRestartServicesAfterEveryWakeOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableRestartServicesAfterEveryWakeOptions()
        {
            var enabled = this.checkBoxRestartServicesAfterEveryWake.IsChecked.Value;
            this.dataGridRestartServicesAfterEveryWake.IsEnabled = enabled;
            this.buttonAddRestartServiceAfterEveryWake.IsEnabled = enabled;
            this.buttonRemoveRestartServiceAfterEveryWake.IsEnabled = enabled;
            this.buttonEditRestartServiceAfterEveryWake.IsEnabled = enabled;
        }

        private void FilldataGridRestartServicesAfterEveryWakeFromSettings()
        {
            this.dataGridRestartServicesAfterEveryWake.Items.Clear();

            for (int i = 0; i < this.currentSettings.RestartServicesAfterEveryWake.Count; i++)
            {
                this.dataGridRestartServicesAfterEveryWake.Items.Add(this.currentSettings.RestartServicesAfterEveryWake[i]);
            }
        }

        private void buttonAddRestartServiceAfterEveryWake_Click(object sender, RoutedEventArgs e)
        {
            var strList = new List<Service>(this.dataGridRestartServicesAfterEveryWake.Items.Count);

            for (int i = 0; i < this.dataGridRestartServicesAfterEveryWake.Items.Count; i++)
            {
                strList.Add((Service)this.dataGridRestartServicesAfterEveryWake.Items[i]);
            }

            var addForm = new AddServiceToRestartWindow(strList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var service = addForm.GetService();

            if (service == null || String.IsNullOrEmpty(service.ServiceName))
                return;

            this.dataGridRestartServicesAfterEveryWake.Items.Add(service);
            this.settingsChanged = true;
        }

        private void buttonRemoveRestartServiceAfterEveryWake_Click(object sender, RoutedEventArgs e)
        {
            var service = this.dataGridRestartServicesAfterEveryWake.SelectedItem as Service;

            if (service == null)
                return;

            this.dataGridRestartServicesAfterEveryWake.Items.Remove(service);
            this.settingsChanged = true;
        }

        private void EnableDisableStartProgramsAfterEveryWakeOptions()
        {
            var enabled = this.checkBoxStartProgramsAfterEveryWake.IsChecked.Value;
            this.dataGridStartProgramsAfterEveryWake.IsEnabled = enabled;
            this.buttonAddStartProgramAfterEveryWake.IsEnabled = enabled;
            this.buttonRemoveStartProgramAfterEveryWake.IsEnabled = enabled;
            this.buttonEditStartProgramAfterEveryWake.IsEnabled = enabled;
        }

        private void checkBoxStartProgramsAfterEveryWake_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableStartProgramsAfterEveryWakeOptions();
            this.settingsChanged = true;
        }

        private void checkBoxStartProgramsAfterEveryWake_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableStartProgramsAfterEveryWakeOptions();
            this.settingsChanged = true;
        }

        private void FilldataGridStartProgramsAfterEveryWake()
        {
            this.dataGridStartProgramsAfterEveryWake.Items.Clear();

            for (int i = 0; i < this.currentSettings.StartProgramsAfterEveryWake.Count; i++)
            {
                this.dataGridStartProgramsAfterEveryWake.Items.Add(this.currentSettings.StartProgramsAfterEveryWake[i]);
            }
        }

        private void buttonAddStartProgramAfterEveryWake_Click(object sender, RoutedEventArgs e)
        {
            var psList = new List<ProgramStart>(this.dataGridStartProgramsAfterEveryWake.Items.Count);

            for (int i = 0; i < this.dataGridStartProgramsAfterEveryWake.Items.Count; i++)
            {
                psList.Add((ProgramStart)this.dataGridStartProgramsAfterEveryWake.Items[i]);
            }

            var addForm = new AddProcessToStartWindow(psList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var p = addForm.GetProgramStart();

            if (p == null)
                return;

            this.dataGridStartProgramsAfterEveryWake.Items.Add(p);
            this.settingsChanged = true;
        }

        private void buttonRemoveStartProgramAfterEveryWake_Click(object sender, RoutedEventArgs e)
        {
            var p = this.dataGridStartProgramsAfterEveryWake.SelectedItem as ProgramStart;

            if (p == null)
                return;

            this.dataGridStartProgramsAfterEveryWake.Items.Remove(p);
            this.settingsChanged = true;
        }

        private void buttonEditStartProgramAfterEveryWake_Click(object sender, RoutedEventArgs e)
        {
            EditStartProgramsAfterEveryWake();
        }

        private void dataGridStartProgramsAfterEveryWake_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditStartProgramsAfterEveryWake();
        }

        private void EditStartProgramsAfterEveryWake()
        {
            var selectedItem = this.dataGridStartProgramsAfterEveryWake.SelectedItem as ProgramStart;
            var selectedIndex = this.dataGridStartProgramsAfterEveryWake.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditStartProgramsAfterEveryWake(selectedItem);

            if (editItem != null)
            {
                this.dataGridStartProgramsAfterEveryWake.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private ProgramStart EditStartProgramsAfterEveryWake(ProgramStart selectedItem)
        {
            var psList = new List<ProgramStart>(this.dataGridStartProgramsAfterEveryWake.Items.Count);

            for (int i = 0; i < this.dataGridStartProgramsAfterEveryWake.Items.Count; i++)
            {
                psList.Add((ProgramStart)this.dataGridStartProgramsAfterEveryWake.Items[i]);
            }

            var addForm = new AddProcessToStartWindow(selectedItem, psList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var p = addForm.GetProgramStart();

            if (p == null)
                return null;

            return p;
        }

        private void dataGridRestartServicesAfterEveryWake_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditRestartServiceAfterEveryWake();
        }

        private void buttonEditRestartServiceAfterEveryWake_Click(object sender, RoutedEventArgs e)
        {
            EditRestartServiceAfterEveryWake();
        }

        private void EditRestartServiceAfterEveryWake()
        {
            var selectedItem = this.dataGridRestartServicesAfterEveryWake.SelectedItem as Service;
            var selectedIndex = this.dataGridRestartServicesAfterEveryWake.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditRestartServiceAfterEveryWake(selectedItem);

            if (editItem != null)
            {
                this.dataGridRestartServicesAfterEveryWake.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private Service EditRestartServiceAfterEveryWake(Service selectedItem)
        {
            var serviceList = new List<Service>(this.dataGridRestartServicesAfterEveryWake.Items.Count);

            for (int i = 0; i < this.dataGridRestartServicesAfterEveryWake.Items.Count; i++)
            {
                serviceList.Add((Service)this.dataGridRestartServicesAfterEveryWake.Items[i]);
            }

            var addForm = new AddServiceToRestartWindow(selectedItem, serviceList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var service = addForm.GetService();

            if (service == null)
                return null;

            return service;
        }

        private void comboBoxLogMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.settingsChanged = true;
        }

        private void upDownMaxLoggingFileSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownMaxLoggingFileSize.Value == null)
                this.upDownMaxLoggingFileSize.Value = 0;

            this.settingsChanged = true;
        }

        private void HyperlinkMagicPacket_Click(object sender, RoutedEventArgs e)
        {
            var link = Wsapm.Resources.Wsapm.SettingsWindow_HyperLinkMagicPacket;
            ProcessStartInfo psi = new ProcessStartInfo(link);
            Process.Start(psi);
        }

        #endregion Tab General

        #region Tab Monitoring (system)

        private void InitTabMonitoringSystem()
        {
            this.checkBoxEnableNetworkInterfaces.IsChecked = this.currentSettings.EnableNetworkInterfacesToMonitor;
            FillDataGridNetworkInterfacesFromSettings();

            EnableDisableNetworkInterfaceOptions();

            // Network access.
            this.checkBoxEnableNetworkResourcesAccessCheck.IsChecked = this.currentSettings.EnableCheckNetworkResourceAccess;

            if (this.comboBoxNetworkResourceAccessType.Items == null || this.comboBoxNetworkResourceAccessType.Items.Count == 0)
            {
                this.comboBoxNetworkResourceAccessType.Items.Add(Wsapm.Resources.Wsapm.SettingsWindow_ComboBoxNetworkResourceAccessTypeFiles);
                this.comboBoxNetworkResourceAccessType.Items.Add(Wsapm.Resources.Wsapm.SettingsWindow_ComboBoxNetworkResourceAccessTypeDirectories);
                this.comboBoxNetworkResourceAccessType.Items.Add(Wsapm.Resources.Wsapm.SettingsWindow_ComboBoxNetworkResourceAccessTypeFilesAndDirectories);

                if ((this.currentSettings.CheckNetworkResourcesType & NetworkShareAccessType.Files) == NetworkShareAccessType.Files)
                    this.comboBoxNetworkResourceAccessType.SelectedIndex = 0;
                else if ((this.currentSettings.CheckNetworkResourcesType & NetworkShareAccessType.Directories) == NetworkShareAccessType.Directories)
                    this.comboBoxNetworkResourceAccessType.SelectedIndex = 1;
                else
                    this.comboBoxNetworkResourceAccessType.SelectedIndex = 2;
            }

            EnableDisableNetworkFileAccessOptions();

            // CPU load.
            this.checkBoxEnableCpuLoad.IsChecked = this.currentSettings.EnableCheckCpuLoad;
            this.upDownCpuLoad.Value = (int)this.currentSettings.CpuLoad;
            EnableDisableCpuLoadOptions();

            // Memory load
            this.checkBoxEnableMemoryLoad.IsChecked = this.currentSettings.EnableCheckMemoryLoad;
            this.upDownMemoryLoad.Value = (int)this.currentSettings.MemoryLoad;
            EnableDisableMemoryLoadOptions();

            // HDD load
            this.checkBoxEnableHdd.IsChecked = this.currentSettings.EnableHddsToMonitor;
            FillDataGridHddFromSettings();

            EnableDisableHddOptions();

            // Live views of system loads
            this.cpuLoad = new CpuLoadCurrent(this.displayRefreshIntervalForLoads);
            this.memoryLoad = new MemoryLoadCurrent(this.displayRefreshIntervalForLoads);

            this.loadTimer = null;
            this.loadTimer = new Timer(OnShowSystemLoadsCallback, null, new TimeSpan(), this.displayRefreshIntervalForLoads);
        }

        private void FillDataGridNetworkInterfacesFromSettings()
        {
            this.dataGridNetworkInterfaces.Items.Clear();
            var availableNics = WsapmTools.GetAvailableNetworkInterfaces();

            for (int i = 0; i < this.currentSettings.NetworkInterfacesToMonitor.Count; i++)
            {
                var nic = this.currentSettings.NetworkInterfacesToMonitor[i];

                if (nic.NetworkInterface == WsapmConstants.AllNetworkInterfacesName)
                    nic.NetworkInterface = WsapmTools.GetCommonDiaplayNameAllNetworkInterfaces();

                if (nic.NetworkInterface != WsapmTools.GetCommonDiaplayNameAllNetworkInterfaces() && !availableNics.Contains(nic.NetworkInterface))
                {
                    MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxNicNotAvailable, Environment.NewLine, nic.NetworkInterface), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                this.dataGridNetworkInterfaces.Items.Add(nic);
            }
        }

        private void FillDataGridHddFromSettings()
        {
            this.dataGridHdd.Items.Clear();
            var availableDrives = WsapmTools.GetAvailableLogicalVolumeNames();

            for (int i = 0; i < this.currentSettings.HddsToMonitor.Count; i++)
            {
                var hdd = this.currentSettings.HddsToMonitor[i];

                if (hdd.Drive == WsapmConstants.AllHddsName)
                    hdd.Drive = WsapmTools.GetCommonDiaplayNameAllDrives();

                if (hdd.Drive != WsapmTools.GetCommonDiaplayNameAllDrives() && !availableDrives.Contains(hdd.Drive))
                {
                    MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxHddNotAvailable, Environment.NewLine, hdd.Drive), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                this.dataGridHdd.Items.Add(hdd);
            }
        }

        private void OnShowSystemLoadsCallback(object state)
        {
            var cpuLoad = string.Empty;
            var memoryLoad = string.Empty;

            try
            {
                cpuLoad = this.cpuLoad.CurrentCpuLoad.ToString("0.00") + " %";
                memoryLoad = this.memoryLoad.CurrenMemoryLoad.ToString("0.0.0") + " %";
            }
            catch (Exception)
            {
            }

            Dispatcher.Invoke((Action)(() =>
            {
                this.labelCurrentCpuLoad.Content = cpuLoad;
                this.labelCurrentMemoryLoad.Content = memoryLoad +  " (" + WsapmConvert.ConvertByteToGB(this.memoryLoad.CurrentPhysicalMemory).ToString("0.0") + "/" + WsapmConvert.ConvertByteToGB(this.memoryLoad.TotalPhysicalMemory).ToString("0.0") + " GB)";
            }));
        }

        private void checkBoxEnableNetworkFileAccessCheck_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkFileAccessOptions();
            this.settingsChanged = true;
        }

        private void checkBoxEnableNetworkFileAccessCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkFileAccessOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableNetworkFileAccessOptions()
        {
            var enabled = this.checkBoxEnableNetworkResourcesAccessCheck.IsChecked.Value;
            this.comboBoxNetworkResourceAccessType.IsEnabled = enabled;
        }

        private void comboBoxNetworkResourceAccessType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Suppress validation if UI is initialized.
            if (!this.uiInitializing)
            {
                if (this.comboBoxNetworkResourceAccessType.SelectedIndex != 0)
                {
                    var result = MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_QuestionMonitoringNetworkFolders, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                        this.comboBoxNetworkResourceAccessType.SelectedIndex = 0;
                }
            }
        }

        private void upDownCpuLoad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownCpuLoad.Value == null)
                this.upDownCpuLoad.Value = 0;

            this.settingsChanged = true;
        }

        private void upDownMemoryLoad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownMemoryLoad.Value == null)
                this.upDownMemoryLoad.Value = 0;

            this.settingsChanged = true;
        }

        private void buttonAddNetworkInterface_Click(object sender, RoutedEventArgs e)
        {
            var nicList = new List<NetworkInterfaceToMonitor>(this.dataGridNetworkInterfaces.Items.Count);

            for (int i = 0; i < this.dataGridNetworkInterfaces.Items.Count; i++)
            {
                nicList.Add((NetworkInterfaceToMonitor)this.dataGridNetworkInterfaces.Items[i]);
            }

            var addForm = new AddNetworkInterfaceWindow(nicList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var nic = addForm.GetNetworkInterface();

            if (nic == null)
                return;

            this.dataGridNetworkInterfaces.Items.Add(nic);
            this.settingsChanged = true;
        }

        private void buttonRemoveNetworkInterface_Click(object sender, RoutedEventArgs e)
        {
            var nic = this.dataGridNetworkInterfaces.SelectedItem as NetworkInterfaceToMonitor;

            if (nic == null)
                return;

            this.dataGridNetworkInterfaces.Items.Remove(nic);
            this.settingsChanged = true;
        }

        private void buttonEditNetworkInterface_Click(object sender, RoutedEventArgs e)
        {
            EditNetworkInterfaceToMonitor();
        }

        private void dataGridNetworkInterfaces_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditNetworkInterfaceToMonitor();
        }

        private void EditNetworkInterfaceToMonitor()
        {
            var selectedItem = this.dataGridNetworkInterfaces.SelectedItem as NetworkInterfaceToMonitor;
            var selectedIndex = this.dataGridNetworkInterfaces.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditNetworkInterfaceToMonitor(selectedItem);

            if (editItem != null)
            {
                this.dataGridNetworkInterfaces.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private NetworkInterfaceToMonitor EditNetworkInterfaceToMonitor(NetworkInterfaceToMonitor selectedItem)
        {
            var nicList = new List<NetworkInterfaceToMonitor>(this.dataGridNetworkInterfaces.Items.Count);

            for (int i = 0; i < this.dataGridNetworkInterfaces.Items.Count; i++)
            {
                nicList.Add((NetworkInterfaceToMonitor)this.dataGridNetworkInterfaces.Items[i]);
            }

            var addForm = new AddNetworkInterfaceWindow(selectedItem, nicList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var nic = addForm.GetNetworkInterface();

            if (nic == null)
                return null;

            return nic;
        }

        private void checkBoxEnableNetworkInterfaces_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkInterfaceOptions();
            this.settingsChanged = true;
        }

        private void checkBoxEnableNetworkInterfaces_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkInterfaceOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableNetworkInterfaceOptions()
        {
            var enabled = this.checkBoxEnableNetworkInterfaces.IsChecked.Value;
            this.dataGridNetworkInterfaces.IsEnabled = enabled;
            this.buttonAddNetworkInterface.IsEnabled = enabled;
            this.buttonRemoveNetworkInterface.IsEnabled = enabled;
            this.buttonEditNetworkInterface.IsEnabled = enabled;
        }

        private void checkBoxEnableHdd_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableHddOptions();
            this.settingsChanged = true;
        }

        private void checkBoxEnableHdd_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableHddOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableHddOptions()
        {
            var enabled = this.checkBoxEnableHdd.IsChecked.Value;
            this.dataGridHdd.IsEnabled = enabled;
            this.buttonAddHdd.IsEnabled = enabled;
            this.buttonRemoveHdd.IsEnabled = enabled;
            this.buttonEditHdd.IsEnabled = enabled;
        }

        private void checkBoxEnableCpuLoad_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableCpuLoadOptions();
            this.settingsChanged = true;
        }

        private void checkBoxEnableCpuLoad_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableCpuLoadOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableCpuLoadOptions()
        {
            var enabled = this.checkBoxEnableCpuLoad.IsChecked.Value;
            this.upDownCpuLoad.IsEnabled = enabled;
        }

        private void checkBoxEnableMemoryLoad_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableMemoryLoadOptions();
            this.settingsChanged = true;
        }

        private void checkBoxEnableMemoryLoad_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableMemoryLoadOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableMemoryLoadOptions()
        {
            var enabled = this.checkBoxEnableMemoryLoad.IsChecked.Value;
            this.upDownMemoryLoad.IsEnabled = enabled;
        }

        private void buttonAddHdd_Click(object sender, RoutedEventArgs e)
        {
            var hddList = new List<HddToMonitor>(this.dataGridHdd.Items.Count);

            for (int i = 0; i < this.dataGridHdd.Items.Count; i++)
            {
                hddList.Add((HddToMonitor)this.dataGridHdd.Items[i]);
            }

            var addForm = new AddHddWindow(hddList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var hdd = addForm.GetHdd();

            if (hdd == null)
                return;

            this.dataGridHdd.Items.Add(hdd);
            this.settingsChanged = true;
        }

        private void buttonRemoveHdd_Click(object sender, RoutedEventArgs e)
        {
            var hdd = this.dataGridHdd.SelectedItem as HddToMonitor;

            if (hdd == null)
                return;

            this.dataGridHdd.Items.Remove(hdd);
            this.settingsChanged = true;
        }

        private void buttonEditHdd_Click(object sender, RoutedEventArgs e)
        {
            EditHddToMonitor();
        }

        private void dataGridHdd_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditHddToMonitor();
        }

        private void EditHddToMonitor()
        {
            var selectedItem = this.dataGridHdd.SelectedItem as HddToMonitor;
            var selectedIndex = this.dataGridHdd.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditHddToMonitor(selectedItem);

            if (editItem != null)
            {
                this.dataGridHdd.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private HddToMonitor EditHddToMonitor(HddToMonitor selectedItem)
        {
            var hddList = new List<HddToMonitor>(this.dataGridHdd.Items.Count);

            for (int i = 0; i < this.dataGridHdd.Items.Count; i++)
            {
                hddList.Add((HddToMonitor)this.dataGridHdd.Items[i]);
            }

            var addForm = new AddHddWindow(selectedItem, hddList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var nic = addForm.GetHdd();

            if (nic == null)
                return null;

            return nic;
        }

        #endregion Tab Monitoring (system)

        #region Tab Monitoring (general)

        private void InitTabMonitoringGeneral()
        {
            // Network machines.
            this.checkBoxEnableComputers.IsChecked = this.currentSettings.EnableMonitorNetworkMachines;
            FillListBoxNetworkMachinesFromSettings();
            EnableDisableNetworkMachineOptions();

            // Processes.
            this.checkBoxEnableProcesses.IsChecked = this.currentSettings.EnableMonitorProcesses;
            FillListBoxProcessFromSettings();
            EnableDisableProcessesOptions();            
        }  

        private void checkBoxEnableNetworkMachines_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkMachineOptions();
            this.settingsChanged = true;
        }

        private void checkBoxEnableNetworkMachines_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkMachineOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableNetworkMachineOptions()
        {
            var enabled = this.checkBoxEnableComputers.IsChecked.Value;
            this.dataGridComputers.IsEnabled = enabled;
            this.buttonAddComputer.IsEnabled = enabled;
            this.buttonRemoveComputer.IsEnabled = enabled;
            this.buttonEditComputer.IsEnabled = enabled;
        }

        private void FillListBoxNetworkMachinesFromSettings()
        {
            this.dataGridComputers.Items.Clear();

            for (int i = 0; i < this.currentSettings.NetworkMachinesToMonitor.Count; i++)
            {
                this.dataGridComputers.Items.Add(this.currentSettings.NetworkMachinesToMonitor[i]);
            }
        }

        private void buttonAddComputer_Click(object sender, RoutedEventArgs e)
        {
            var nmList = new List<NetworkMachine>(this.dataGridComputers.Items.Count);

            for (int i = 0; i < this.dataGridComputers.Items.Count; i++)
            {
                nmList.Add((NetworkMachine)this.dataGridComputers.Items[i]);
            }

            var addForm = new AddNetworkMachineWindow(nmList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var machine = addForm.GetNetworkMachine();

            if (machine == null)
                return;

            this.dataGridComputers.Items.Add(machine);
            this.settingsChanged = true;
        }

        private void buttonRemoveComputer_Click(object sender, RoutedEventArgs e)
        {
            var machine = this.dataGridComputers.SelectedItem as NetworkMachine;

            if (machine == null)
                return;

            this.dataGridComputers.Items.Remove(machine);
            this.settingsChanged = true;
        }

        private void dataGridComputers_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditComputer();
        }

        private void buttonEditComputer_Click(object sender, RoutedEventArgs e)
        {
            EditComputer();
        }

        private void EditComputer()
        {
            var selectedItem = this.dataGridComputers.SelectedItem as NetworkMachine;
            var selectedIndex = this.dataGridComputers.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditComputer(selectedItem);

            if (editItem != null)
            {
                this.dataGridComputers.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private NetworkMachine EditComputer(NetworkMachine selectedItem)
        {
            var nmList = new List<NetworkMachine>(this.dataGridComputers.Items.Count);

            for (int i = 0; i < this.dataGridComputers.Items.Count; i++)
            {
                nmList.Add((NetworkMachine)this.dataGridComputers.Items[i]);
            }

            var addForm = new AddNetworkMachineWindow(selectedItem, nmList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var networkMachine = addForm.GetNetworkMachine();

            if (networkMachine == null)
                return null;

            return networkMachine;
        }

        private void checkBoxEnableProcesses_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableProcessesOptions();
            this.settingsChanged = true;
        }

        private void checkBoxEnableProcesses_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableProcessesOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableProcessesOptions()
        {
            var enabled = this.checkBoxEnableProcesses.IsChecked.Value;

            this.dataGridProcesses.IsEnabled = enabled;
            this.buttonAddProcess.IsEnabled = enabled;
            this.buttonRemoveProcess.IsEnabled = enabled;
            this.buttonEditProcess.IsEnabled = enabled;
        }

        private void FillListBoxProcessFromSettings()
        {
            this.dataGridProcesses.Items.Clear();

            for (int i = 0; i < this.currentSettings.ProcessesToMonitor.Count; i++)
            {
                this.dataGridProcesses.Items.Add(this.currentSettings.ProcessesToMonitor[i]);
            }
        }

        private void buttonAddProcess_Click(object sender, RoutedEventArgs e)
        {
            var processList = new List<ProcessToMonitor>(this.dataGridProcesses.Items.Count);

            for (int i = 0; i < this.dataGridProcesses.Items.Count; i++)
            {
                processList.Add((ProcessToMonitor)this.dataGridProcesses.Items[i]);
            }

            var addForm = new AddProcessWindow(processList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var p = addForm.GetProcess();

            if (p == null)
                return;

            this.dataGridProcesses.Items.Add(p);
            this.settingsChanged = true;
        }

        private void buttonRemoveProcess_Click(object sender, RoutedEventArgs e)
        {
            var p = this.dataGridProcesses.SelectedItem as ProcessToMonitor;

            if (p == null)
                return;

            this.dataGridProcesses.Items.Remove(p);
            this.settingsChanged = true;
        }

        private void buttonEditProcess_Click(object sender, RoutedEventArgs e)
        {
            EditProcess();
        }

        private void dataGridProcesses_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditProcess();
        }

        private void EditProcess()
        {
            var selectedItem = this.dataGridProcesses.SelectedItem as ProcessToMonitor;
            var selectedIndex = this.dataGridProcesses.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditProcess(selectedItem);

            if (editItem != null)
            {
                this.dataGridProcesses.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private ProcessToMonitor EditProcess(ProcessToMonitor selectedItem)
        {
            var processList = new List<ProcessToMonitor>(this.dataGridProcesses.Items.Count);

            for (int i = 0; i < this.dataGridProcesses.Items.Count; i++)
            {
                processList.Add((ProcessToMonitor)this.dataGridProcesses.Items[i]);
            }

            var addForm = new AddProcessWindow(selectedItem, processList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var proc = addForm.GetProcess();

            if (proc == null)
                return null;

            return proc;
        }                 

        #endregion Tab Monitoring (general)

        #region Tab After policy check

        private void InitTabAfterPolicyCheck()
        {
            this.checkBoxEnableActionsAfterPolicyCheck.IsChecked = this.currentSettings.EnableActionsAfterPolicyCheck;
            FillDataGridActionsAfterPolicyCheck();
            EnableDisableActionsAfterPolicyCheckOptions();
        }

        private void FillDataGridActionsAfterPolicyCheck()
        {
            this.dataGridActionsAfterPolicyCheck.Items.Clear();

            for (int i = 0; i < this.currentSettings.ActionsAfterPolicyCheck.Count; i++)
            {
                this.dataGridActionsAfterPolicyCheck.Items.Add(this.currentSettings.ActionsAfterPolicyCheck[i]);
            }
        }

        private void EnableDisableActionsAfterPolicyCheckOptions()
        {
            var enabled = this.checkBoxEnableActionsAfterPolicyCheck.IsChecked.Value;
            this.dataGridActionsAfterPolicyCheck.IsEnabled = enabled;
            this.buttonAddActionAfterPolicyCheck.IsEnabled = enabled;
            this.buttonRemoveActionAfterPolicyCheck.IsEnabled = enabled;
            this.buttonEditActionAfterPolicyCheck.IsEnabled = enabled;
        }

        private void checkBoxEnableActionsAfterPolicyCheck_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableActionsAfterPolicyCheckOptions();
        }

        private void checkBoxEnableActionsAfterPolicyCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableActionsAfterPolicyCheckOptions();
        }

        private List<ActionAfterPolicyCheck> BuildActionsAfterPolicyCheck()
        {
            var list = new List<ActionAfterPolicyCheck>();

            foreach (var item in this.dataGridActionsAfterPolicyCheck.Items)
            {
                list.Add((ActionAfterPolicyCheck)item);
            }

            return list;
        }

        private void buttonAddActionAfterPolicyCheck_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<ActionAfterPolicyCheck>(this.dataGridActionsAfterPolicyCheck.Items.Count);

            for (int i = 0; i < this.dataGridActionsAfterPolicyCheck.Items.Count; i++)
            {
                list.Add((ActionAfterPolicyCheck)this.dataGridActionsAfterPolicyCheck.Items[i]);
            }

            var addForm = new AddActionAfterPolicyCheckWindow(list.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var action = addForm.GetActionAfterPolicyCheck();

            if (action == null)
                return;

            this.dataGridActionsAfterPolicyCheck.Items.Add(action);
            this.settingsChanged = true;
        }

        private void buttonRemoveActionAfterPolicyCheck_Click(object sender, RoutedEventArgs e)
        {
            var action = this.dataGridActionsAfterPolicyCheck.SelectedItem as ActionAfterPolicyCheck;

            if (action == null)
                return;

            this.dataGridActionsAfterPolicyCheck.Items.Remove(action);
            this.settingsChanged = true;
        }

        private void buttonEditActionAfterPolicyCheck_Click(object sender, RoutedEventArgs e)
        {
            EditActionAfterPolicyCheck();
        }

        private void EditActionAfterPolicyCheck()
        {
            var selectedItem = this.dataGridActionsAfterPolicyCheck.SelectedItem as ActionAfterPolicyCheck;
            var selectedIndex = this.dataGridActionsAfterPolicyCheck.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditActionAfterPolicyCheck(selectedItem);

            if (editItem != null)
            {
                this.dataGridActionsAfterPolicyCheck.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private object EditActionAfterPolicyCheck(ActionAfterPolicyCheck selectedItem)
        {
            var list = new List<ActionAfterPolicyCheck>(this.dataGridActionsAfterPolicyCheck.Items.Count);

            for (int i = 0; i < this.dataGridActionsAfterPolicyCheck.Items.Count; i++)
            {
                list.Add((ActionAfterPolicyCheck)this.dataGridActionsAfterPolicyCheck.Items[i]);
            }

            var addForm = new AddActionAfterPolicyCheckWindow(selectedItem, list.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var wakeScheduler = addForm.GetActionAfterPolicyCheck();

            if (wakeScheduler == null)
                return null;

            return wakeScheduler;
        }

        private void dataGridActionsAfterPolicyCheck_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditActionAfterPolicyCheck();
        }

        #endregion Tab After policy check

        #region Tab Wake

        private void InitTabWake()
        {
            this.checkBoxEnableWakeSchedulers.IsChecked = this.currentSettings.EnableWakeTimers;
            FillDataGridWakeSchedulers();
            EnableDisableWakeOptions();
        }

        private void EnableDisableWakeOptions()
        {
            var enabled = this.checkBoxEnableWakeSchedulers.IsChecked.Value;
            this.dataGridWakeSchedulers.IsEnabled = enabled;
            this.buttonAddWakeScheduler.IsEnabled = enabled;
            this.buttonRemoveWakeScheduler.IsEnabled = enabled;
            this.buttonEditWakeScheduler.IsEnabled = enabled;
        }

        private void FillDataGridWakeSchedulers()
        {
            this.dataGridWakeSchedulers.Items.Clear();

            for (int i = 0; i < this.currentSettings.WakeSchedulers.Count; i++)
            {
                this.dataGridWakeSchedulers.Items.Add(this.currentSettings.WakeSchedulers[i]);
            }
        }

        private List<WakeScheduler> BuildWakeSchedulers()
        {
            var list = new List<WakeScheduler>();

            foreach (var item in this.dataGridWakeSchedulers.Items)
            {
                list.Add((WakeScheduler)item);
            }

            return list;
        }

        private void buttonAddWakeScheduler_Click(object sender, RoutedEventArgs e)
        {
            var wsList = new List<WakeScheduler>(this.dataGridWakeSchedulers.Items.Count);

            for (int i = 0; i < this.dataGridWakeSchedulers.Items.Count; i++)
            {
                wsList.Add((WakeScheduler)this.dataGridWakeSchedulers.Items[i]);
            }

            var addForm = new AddWakeScheduleWindow(wsList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var wakeScheduler = addForm.GetWakeScheduler();

            if (wakeScheduler == null)
                return;

            this.dataGridWakeSchedulers.Items.Add(wakeScheduler);
            this.settingsChanged = true;
        }

        private void buttonRemoveWakeScheduler_Click(object sender, RoutedEventArgs e)
        {
            var wakeScheduler = this.dataGridWakeSchedulers.SelectedItem as WakeScheduler;

            if (wakeScheduler == null)
                return;

            this.dataGridWakeSchedulers.Items.Remove(wakeScheduler);
            this.settingsChanged = true;
        }

        private void buttonEditWakeScheduler_Click(object sender, RoutedEventArgs e)
        {
            EditWakeScheduler();
        }

        private void EditWakeScheduler()
        {
            var selectedItem = this.dataGridWakeSchedulers.SelectedItem as WakeScheduler;
            var selectedIndex = this.dataGridWakeSchedulers.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditWakeScheduler(selectedItem);

            if (editItem != null)
            {
                this.dataGridWakeSchedulers.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private WakeScheduler EditWakeScheduler(WakeScheduler selectedItem)
        {
            var wsList = new List<WakeScheduler>(this.dataGridWakeSchedulers.Items.Count);

            for (int i = 0; i < this.dataGridWakeSchedulers.Items.Count; i++)
            {
                wsList.Add((WakeScheduler)this.dataGridWakeSchedulers.Items[i]);
            }

            var addForm = new AddWakeScheduleWindow(selectedItem, wsList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var wakeScheduler = addForm.GetWakeScheduler();

            if (wakeScheduler == null)
                return null;

            return wakeScheduler;
        }

        private void checkBoxEnableWakeSchedulers_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableWakeOptions();
        }

        private void checkBoxEnableWakeSchedulers_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableWakeOptions();
        }

        private void buttonRemoveExpiredWakeSchedulers_Click(object sender, RoutedEventArgs e)
        {
            for (int i = this.dataGridWakeSchedulers.Items.Count - 1; i >= 0; i--)
            {
                var wakeScheduler = this.dataGridWakeSchedulers.Items[i] as WakeScheduler;

                if (wakeScheduler.WakeSchedulerExpired)
                {
                    this.dataGridWakeSchedulers.Items.Remove(wakeScheduler);
                    this.settingsChanged = true;
                }
            }
        }

        private void dataGridWakeSchedulers_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditWakeScheduler();
        }

        #endregion Tab Wake

        #region Tab uptime

        private void InitTabUptime()
        {
            this.checkBoxEnableUptimeSchedulers.IsChecked = this.currentSettings.EnableUptimes;
            FillDataGridUptimeSchedulers();
            EnableDisableUptimeOptions();
        }

        private void checkBoxEnableUptimeSchedulers_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableUptimeOptions();
        }

        private void checkBoxEnableUptimeSchedulers_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableUptimeOptions();
        }

        private void EnableDisableUptimeOptions()
        {
            var enabled = this.checkBoxEnableUptimeSchedulers.IsChecked.Value;
            this.dataGridUptimeSchedulers.IsEnabled = enabled;
            this.buttonAddUptimeScheduler.IsEnabled = enabled;
            this.buttonRemoveUptimeScheduler.IsEnabled = enabled;
            this.buttonEditUptimeScheduler.IsEnabled = enabled;
        }

        private void FillDataGridUptimeSchedulers()
        {
            this.dataGridUptimeSchedulers.Items.Clear();

            for (int i = 0; i < this.currentSettings.UptimeSchedulers.Count; i++)
            {
                this.dataGridUptimeSchedulers.Items.Add(this.currentSettings.UptimeSchedulers[i]);
            }
        }

        private List<UptimeScheduler> BuildUptimeSchedulers()
        {
            var list = new List<UptimeScheduler>();

            foreach (var item in this.dataGridUptimeSchedulers.Items)
            {
                list.Add((UptimeScheduler)item);
            }

            return list;
        }

        private void dataGridUptimeSchedulers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditUptimeScheduler();
        }


        private void buttonAddUptimeScheduler_Click(object sender, RoutedEventArgs e)
        {
            var usList = new List<UptimeScheduler>(this.dataGridUptimeSchedulers.Items.Count);

            for (int i = 0; i < this.dataGridUptimeSchedulers.Items.Count; i++)
            {
                usList.Add((UptimeScheduler)this.dataGridUptimeSchedulers.Items[i]);
            }

            var addForm = new AddUptimeScheduleWindow(usList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var uptimeScheduler = addForm.GetUptimeScheduler();

            if (uptimeScheduler == null)
                return;

            this.dataGridUptimeSchedulers.Items.Add(uptimeScheduler);
            this.settingsChanged = true;
        }

        private void buttonRemoveUptimeScheduler_Click(object sender, RoutedEventArgs e)
        {
            var uptimeScheduler = this.dataGridUptimeSchedulers.SelectedItem as UptimeScheduler;

            if (uptimeScheduler == null)
                return;

            this.dataGridUptimeSchedulers.Items.Remove(uptimeScheduler);
            this.settingsChanged = true;
        }

        private void buttonEditUptimeScheduler_Click(object sender, RoutedEventArgs e)
        {
            EditUptimeScheduler();
        }

        private void EditUptimeScheduler()
        {
            var selectedItem = this.dataGridUptimeSchedulers.SelectedItem as UptimeScheduler;
            var selectedIndex = this.dataGridUptimeSchedulers.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditUptimeScheduler(selectedItem);

            if (editItem != null)
            {
                this.dataGridUptimeSchedulers.Items[selectedIndex] = editItem;
                this.settingsChanged = true;
            }
        }

        private UptimeScheduler EditUptimeScheduler(UptimeScheduler selectedItem)
        {
            var usList = new List<UptimeScheduler>(this.dataGridUptimeSchedulers.Items.Count);

            for (int i = 0; i < this.dataGridUptimeSchedulers.Items.Count; i++)
            {
                usList.Add((UptimeScheduler)this.dataGridUptimeSchedulers.Items[i]);
            }

            var addForm = new AddUptimeScheduleWindow(selectedItem, usList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var uptimeScheduler = addForm.GetUptimeScheduler();

            if (uptimeScheduler == null)
                return null;

            return uptimeScheduler;
        }

        private void buttonRemoveExpiredUptimeSchedulers_Click(object sender, RoutedEventArgs e)
        {
            for (int i = this.dataGridUptimeSchedulers.Items.Count - 1; i >= 0; i--)
            {
                var uptimeScheduler = this.dataGridUptimeSchedulers.Items[i] as UptimeScheduler;

                if (uptimeScheduler.UptimeSchedulerExpired)
                {
                    this.dataGridUptimeSchedulers.Items.Remove(uptimeScheduler);
                    this.settingsChanged = true;
                }
            }
        }

        #endregion Tab uptime

        #region Tab Plugins

        private void InitTabPlugins()
        {
            PluginManager pim = new PluginManager();
            pim.LoadAndInitialzePlugins();

            if (pim.Plugins != null && pim.Plugins.Length > 0)
            {
                foreach (var plugin in pim.Plugins)
                {
                    if (this.currentSettings.ActivePlugins != null && this.currentSettings.ActivePlugins.Count > 0 && this.currentSettings.ActivePlugins.Contains(plugin.PluginAttribute.PluginGuid))
                        plugin.IsActivated = true;
                }

                this.dataGridPlugins.ItemsSource = pim.Plugins;
            }
        }

        private void dataGridPlugins_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            this.settingsChanged = true;
        }

        private void buttonInstallPlugin_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = Wsapm.Resources.Wsapm.SettingsWindow_LoadPluginOpenFileDialogFilter;
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                // Message that the program will shut down while installation of plugin.
                var msgResult = MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxInstallQuestion, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (msgResult == MessageBoxResult.No)
                    return;

                var tmpFolder = Path.GetTempPath() + Guid.NewGuid().ToString();
                var tmpUnzipFolder = tmpFolder + @"\tmp";

                try
                {
                    // Create tmp directory.
                    Directory.CreateDirectory(tmpUnzipFolder);

                    // Unzip in temp folder.
                    ZipFile zipFile = ZipFile.Read(ofd.FileName);

                    foreach (ZipEntry entry in zipFile)
                    {
                        entry.Extract(tmpUnzipFolder, ExtractExistingFileAction.OverwriteSilently);
                    }

                    WsapmPluginAttribute pluginAttribute = null;
                    var pil = new PluginLoader<WsapmPluginBase>(tmpFolder);
                    pil.LoadPlugins();

                    if (pil.Plugins == null || pil.Plugins.Length == 0)
                    {
                        MessageBox.Show(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxNoWsapmPlugin, Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        WsapmTools.MarkTempResourcesForDeletion(tmpFolder);
                        //Directory.Delete(tmpFolder, true);
                        return;
                    }

                    pluginAttribute = pil.Plugins[0].PluginAttribute; // Only take first. One plugin in one zip file!
                    pil = null;

                    // Find directory to install the plugin to.
                    var installedPlugins = this.dataGridPlugins.ItemsSource as WsapmPluginBase[];
                    string installFolder = String.Empty;

                    if (installedPlugins != null)
                    {
                        foreach (var installedPlugin in installedPlugins)
                        {
                            if (installedPlugin.PluginAttribute.PluginGuid == pluginAttribute.PluginGuid)
                            {
                                // Override?
                                var mboxResult = MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxOverridePlugin, pluginAttribute.Name, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                                if (mboxResult == MessageBoxResult.No)
                                    return;

                                installFolder = installedPlugin.GetInstallDir();
                                break;
                            }
                        }
                    }

                    if (String.IsNullOrEmpty(installFolder))
                    {
                        var basePathWsapm = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        installFolder = basePathWsapm + @"\" + WsapmConstants.WsapmPluginFolder + @"\" + pluginAttribute.Name;

                        if (Directory.Exists(installFolder))
                        {
                            // Another plugin is installed in this folder.
                            installFolder += " (" + pluginAttribute.PluginGuid + ")";
                        }
                        else
                            Directory.CreateDirectory(installFolder);
                    }

                    this.closedByPluginInstallation = true;

                    // Start plugin installer.
                    var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    ProcessStartInfo psi = new ProcessStartInfo(basePath + @"\" + "Wsapm.PluginInstaller.exe");
                    psi.WorkingDirectory = basePath;
                    psi.Arguments = "INSTALL " + "\"" + ofd.FileName + "\"" + " " + "\"" + installFolder + "\"" + " " + "\"" + tmpFolder + "\"";
                    Process.Start(psi);

                    // Shutdown app.
                    App.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxInstallPluginFailed, Environment.NewLine, ex.Message), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void buttonUninstallPlugin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItem = this.dataGridPlugins.SelectedItem;
                WsapmPluginBase pluginBase = selectedItem as WsapmPluginBase;

                if (pluginBase == null)
                    return;

                var msgResult = MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxUninstallQuestion, pluginBase.PluginAttribute.Name, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (msgResult == MessageBoxResult.No)
                    return;

                pluginBase.IsActivated = false;

                // Save settings.
                var newSettings = BuildSettings();
                this.settingsManager.SaveSettings(newSettings);

                var installDir = pluginBase.GetInstallDir();

                WsapmPluginAdvancedBase pluginAdvanced = pluginBase as WsapmPluginAdvancedBase;

                this.closedByPluginInstallation = true;

                // Start plugin uninstaller.
                ProcessStartInfo psi = new ProcessStartInfo("Wsapm.PluginInstaller.exe");

                if (pluginAdvanced == null)
                    psi.Arguments = "UNINSTALL " + "\"" + installDir + "\"" + " " + "\"" + String.Empty + "\"";
                else
                    psi.Arguments = "UNINSTALL " + "\"" + installDir + "\"" + " " + "\"" + pluginAdvanced.SettingsFolder + "\"";

                psi.UseShellExecute = false;
                Process.Start(psi);

                // Shutdown app.
                App.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxUninstallPluginFailed, Environment.NewLine, ex.Message), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonPluginReadMe_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.dataGridPlugins.SelectedItem;
            WsapmPluginBase pluginBase = selectedItem as WsapmPluginBase;

            if (pluginBase == null)
                return;

            string pluginInfo = string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(pluginBase.Manifest.PluginName);
            sb.Append(" (");
            sb.Append(pluginBase.PluginAttribute.Version);
            sb.Append(")");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(pluginBase.Manifest.Description);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Wsapm.Resources.Wsapm.SettingsWindow_PluginAuthor);
            sb.Append(" ");
            sb.Append(pluginBase.Manifest.AuthorName);
            pluginInfo = sb.ToString();

            CultureInfo ci = CultureInfo.CurrentUICulture;
            var languageCode = ci.TwoLetterISOLanguageName.ToLower();

            string readMeContent = string.Empty;
            var fileName = "ReadMe_" + languageCode + ".txt";

            if (File.Exists(Path.Combine(pluginBase.GetInstallDir(), fileName)))
            {
                readMeContent = File.ReadAllText(Path.Combine(pluginBase.GetInstallDir(), fileName));
            }
            else if (File.Exists(Path.Combine(pluginBase.GetInstallDir(), "ReadMe.txt")))
            {
                // Culture specific file not found -> try ReadMe.txt        
                readMeContent = File.ReadAllText(Path.Combine(pluginBase.GetInstallDir(), "ReadMe.txt"));
            }

            if (!string.IsNullOrEmpty(readMeContent))
                pluginInfo = readMeContent;

            PluginInfoWindow piWindow = new PluginInfoWindow(pluginInfo);
            piWindow.ShowDialog();
        }

        private void buttonPluginSettings_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.dataGridPlugins.SelectedItem;
            WsapmPluginAdvancedBase advancedPluginBase = selectedItem as WsapmPluginAdvancedBase;

            if (advancedPluginBase == null)
                return;

            try
            {
                if (advancedPluginBase.SettingsControl != null)
                {
                    var pluginSettingsWindow = new PluginSettingsWindow(advancedPluginBase);

                    var result = pluginSettingsWindow.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        var settings = ((IWsapmPluginSettingsControl)advancedPluginBase.SettingsControl).GetSettingsBeforeSave();
                        advancedPluginBase.SaveSettings(settings);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.SettingsWindow_MessageBoxPluginSettingsError, advancedPluginBase.PluginAttribute.Name, Environment.NewLine, ex.Message), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGridPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle enabled state of plugin settings button.
            if (this.dataGridPlugins.SelectedItem is WsapmPluginAdvancedBase)
                this.buttonPluginSettings.IsEnabled = true;
            else
                this.buttonPluginSettings.IsEnabled = false;
        }

        #endregion Tab Plugins

        #region Tab Remote-Shutdown

        private void InitTabRemoteShutdown()
        {
            // Remote shutdown
            this.checkBoxEnableRemoteShutdown.IsChecked = this.currentSettings.EnableRemoteShutdown;

            this.upDownRemoteShutdownPort.Value = this.currentSettings.RemoteShutdownPort == 0 ? 9 : this.currentSettings.RemoteShutdownPort;

            // Save old password hash value (needed later for saving)
            this.oldPasswordHash = this.currentSettings.RemoteShutdownPasswordHash;

            var password = this.currentSettings.RemoteShutdownPasswordHash;

            if (string.IsNullOrEmpty(password))
            {
                this.passwordBoxRemoteShutdown.Password = string.Empty;
                this.passwordBoxRemoteShutdownRepetition.Password = string.Empty;
            }
            else
            {
                this.passwordBoxRemoteShutdown.Password = dummyPassword;
                this.passwordBoxRemoteShutdownRepetition.Password = dummyPassword;
            }

            EnableDisableRemoteShutdownOptions();
        }

        private void checkBoxEnableRemoteShutdown_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableRemoteShutdownOptions();
            this.settingsChanged = true;
        }

        private void checkBoxEnableRemoteShutdown_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableRemoteShutdownOptions();
            this.settingsChanged = true;
        }

        private void EnableDisableRemoteShutdownOptions()
        {
            var enabled = this.checkBoxEnableRemoteShutdown.IsChecked.Value;
            this.upDownRemoteShutdownPort.IsEnabled = enabled;
            this.labelRemoteShutdownPort.IsEnabled = enabled;
            this.labelRemoteShutdownPassword.IsEnabled = enabled;
            this.passwordBoxRemoteShutdown.IsEnabled = enabled;
            this.labelRemoteShutdownPasswordRepetition.IsEnabled = enabled;
            this.passwordBoxRemoteShutdownRepetition.IsEnabled = enabled;
        }

        private void upDownRemoteShutdownPort_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownRemoteShutdownPort.Value == null)
                this.upDownRemoteShutdownPort.Value = 1;

            this.settingsChanged = true;
        }

        private void passwordBoxRemoteShutdown_GotFocus(object sender, RoutedEventArgs e)
        {
            this.passwordBoxRemoteShutdown.SelectAll();
        }

        private void passwordBoxRemoteShutdownRepetition_GotFocus(object sender, RoutedEventArgs e)
        {
            this.passwordBoxRemoteShutdownRepetition.SelectAll();
        }

        private void passwordBoxRemoteShutdown_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!this.uiInitializing)
                this.remotePasswordChanged = true;
        }

        private void passwordBoxRemoteShutdownRepetition_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!this.uiInitializing)
                this.remotePasswordChanged = true;
        }

        #endregion Tab Remote-Shutdown

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.settingsManager != null)
                {
                    this.settingsManager.Dispose();
                    this.settingsManager = null;
                }

                if (this.cpuLoad != null)
                {
                    this.cpuLoad.Dispose();
                    this.cpuLoad = null;
                }

                if (this.loadTimer != null)
                {
                    this.loadTimer.Dispose();
                    this.loadTimer = null;
                }
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

    /// <summary>
    /// Enum defining the tab which should be shown initially when the settings window is opened.
    /// </summary>
    public enum InitTabSettings
    {
        /// <summary>
        /// General tab.
        /// </summary>
        General,
        /// <summary>
        /// Monitoring tab (system).
        /// </summary>
        MonitoringSystem,
        /// <summary>
        /// Monitoring tab (advanced).
        /// </summary>
        MonitoringAdvanced,
        /// <summary>
        /// After policy check tab
        /// </summary>
        AfterPloicyCheck,
        /// <summary>
        /// Wake tab.
        /// </summary>
        Wake,
        /// <summary>
        /// Uptime tab.
        /// </summary>
        Uptime,
        /// <summary>
        /// Plugins tab.
        /// </summary>
        Plugins,
        /// <summary>
        /// Remote shut down tab.
        /// </summary>
        RemoteShutdown
    }
}
