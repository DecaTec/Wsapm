using System;
using System.Collections.Generic;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for settings.
    /// </summary>
    [Serializable]
    public sealed class WsapmSettings
    {
        /// <summary>
        /// Initializes a new instance of WolUtilityServerSettings.
        /// </summary>
        public WsapmSettings()
        {
            this.NetworkMachinesToMonitor = new List<NetworkMachine>();
            this.ProcessesToMonitor = new List<ProcessToMonitor>();
            this.RestartServicesAfterEveryWake = new List<Service>();
            this.StartProgramsAfterEveryWake = new List<ProgramStart>();
            this.ActionsAfterPolicyCheck = new List<ActionAfterPolicyCheck>();
            this.WakeSchedulers = new List<WakeScheduler>();
            this.UptimeSchedulers = new List<UptimeScheduler>();
            this.NetworkInterfacesToMonitor = new List<NetworkInterfaceToMonitor>();
            this.HddsToMonitor = new List<HddToMonitor>();
        }

        /// <summary>
        /// Gets or sets the timer check interval minutes.
        /// </summary>
        public uint MonitoringTimerInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the network connection should be reset after every wake.
        /// </summary>
        public bool ResetNetworkConnectionsAfterWake
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the specified services should be restarted after every wake.
        /// </summary>
        public bool EnableRestartServicesAfterEveryWake
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a string array containing all the windows services which should be restarted after every wake.
        /// </summary>
        public List<Service> RestartServicesAfterEveryWake
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the specified programs should be started after every wake.
        /// </summary>
        public bool EnableStartProgramsAfterEveryWake
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the programs which should be started after every wake.
        /// </summary>
        public List<ProgramStart> StartProgramsAfterEveryWake
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log mode.
        /// </summary>
        public LogMode LogMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets max log file size in bytes.
        /// </summary>
        public uint MaxLogFileSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the network machines should be monitored.
        /// </summary>
        public bool EnableMonitorNetworkMachines
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of network machines to monitor.
        /// When at least one of these machines is reachable over the local network, a standby of the server should be avoided.
        /// </summary>
        public List<NetworkMachine> NetworkMachinesToMonitor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the processes should be monitored.
        /// </summary>
        public bool EnableMonitorProcesses
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of processes to monitor.
        /// When at least one of these processes is running, a standby of the server is should be avoided.
        /// </summary>
        public List<ProcessToMonitor> ProcessesToMonitor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the network interfaces should be monitored.
        /// </summary>
        public bool EnableNetworkInterfacesToMonitor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the network interfaces to monitor.
        /// </summary>
        public List<NetworkInterfaceToMonitor> NetworkInterfacesToMonitor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if network resource access should be checked.
        /// </summary>
        public bool EnableCheckNetworkResourceAccess
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of network resources which will be checked when access to network resources will be checked.
        /// </summary>
        public NetworkShareAccessType CheckNetworkResourcesType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the HDDs should be monitored.
        /// </summary>
        public bool EnableHddsToMonitor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the HDDs to monitor.
        /// </summary>
        public List<HddToMonitor> HddsToMonitor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if this CPU load should be checked.
        /// </summary>
        public bool EnableCheckCpuLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if this memory load should be checked.
        /// </summary>
        public bool EnableCheckMemoryLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the CPU load in percent.
        /// </summary>
        public float CpuLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the memory load in percent.
        /// </summary>
        public float MemoryLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Get or sets a value indicating if wake timers are enabled.
        /// </summary>
        public bool EnableWakeTimers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the list of wake schedulers.
        /// </summary>
        public List<WakeScheduler> WakeSchedulers
        {
            get;
            set;
        }

        /// <summary>
        /// Get or sets a value indicating if uptimes are enabled.
        /// </summary>
        public bool EnableUptimes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the list of uptime schedulers.
        /// </summary>
        public List<UptimeScheduler> UptimeSchedulers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the active plugins.
        /// </summary>
        public List<Guid> ActivePlugins
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of disabled plugins.
        /// </summary>
        public List<Guid> DisabledPlugins
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the actions after policy check should be executed.
        /// </summary>
        public bool EnableActionsAfterPolicyCheck
        {
            get;
            set;
        }

        /// <summary>
        ///  Gets or sets the actions after policy check.
        /// </summary>
        public List<ActionAfterPolicyCheck> ActionsAfterPolicyCheck
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if remote shutdown should be enabled.
        /// </summary>
        public bool EnableRemoteShutdown
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port used for remote shut down.
        /// </summary>
        public ushort RemoteShutdownPort
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the password for remote shut down.
        /// </summary>
        public string RemoteShutdownPasswordHash
        {
            get;
            set;
        }
    }
}
