using System;
using System.Runtime.InteropServices;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for windows power settings.
    /// </summary>
    public static class PowerSettingsManager
    {
        private static Guid SleepCategoryGuid = new Guid("238c9fa8-0aad-41ed-83f4-97be242c8f20");
        private static Guid WakeTimersGuid = new Guid("bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d");

        /// <summary>
        /// Gets a value indicating of wake timers are allowed.
        /// </summary>
        /// <returns>True, if wake timers are allowed, otherwise false.</returns>
        public static bool GetWakeTimersAllowed()
        {
            if (OsVersionTools.IsWindowsVistaOrLater())
            {
                bool wakeTimersAllowed = false;
                IntPtr activeGuidPtr = IntPtr.Zero;

                if (NativeMethods.PowerGetActiveScheme(IntPtr.Zero, ref activeGuidPtr) != 0)
                {
                    var ex = Win32ExceptionManager.GetWin32Exception();
                    throw ex;
                }

                Guid activeSchemeGuid = (Guid)Marshal.PtrToStructure(activeGuidPtr, typeof(Guid));
                uint bufferSize = 1024;
                IntPtr valuePtr = Marshal.AllocHGlobal((int)bufferSize);
                IntPtr type = IntPtr.Zero;
                try
                {
                    var success = NativeMethods.PowerReadACValue(IntPtr.Zero, ref activeSchemeGuid, ref SleepCategoryGuid, ref WakeTimersGuid, type, valuePtr, ref bufferSize);

                    if (success == 0)
                    {
                        byte[] bArr = new byte[bufferSize];
                        Marshal.Copy(valuePtr, bArr, 0, (int)bufferSize);
                        wakeTimersAllowed = BitConverter.ToBoolean(bArr, 0);
                    }
                    else
                        wakeTimersAllowed = false;
                }
                catch (Exception)
                {
                    var ex = Win32ExceptionManager.GetWin32Exception();
                    throw ex;
                }
                finally
                {
                    Marshal.FreeHGlobal(valuePtr);
                }

                return wakeTimersAllowed;
            }
            else
                return true;
        }

        /// <summary>
        /// Gets the current power policy.
        /// </summary>
        /// <returns>A PowerPolicies struct representing the current power policy.</returns>
        public static PowerPolicies GetCurrentPowerPolicy()
        {
            GLOBAL_POWER_POLICY globalPowerPolicy = new GLOBAL_POWER_POLICY();
            POWER_POLICY powerPolicy = new POWER_POLICY();

            if (!NativeMethods.GetCurrentPowerPolicies(out globalPowerPolicy, out powerPolicy))
            {
                var ex = Win32ExceptionManager.GetWin32Exception();
                throw ex;
            }

            PowerPolicies ppRet = new PowerPolicies();
            ppRet.GlobalPowerPolicy = globalPowerPolicy;
            ppRet.PowerPolicy = powerPolicy;
            return ppRet;
        }

        #region Interop

        /// <summary>
        /// Struct representing the global power policy options.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GLOBAL_POWER_POLICY
        {
            /// <summary>
            /// The global power policy (user specific).
            /// </summary>
            public GLOBAL_USER_POWER_POLICY UserPolicy;
            /// <summary>
            /// The global power policy (machine specific).
            /// </summary>
            public GLOBAL_MACHINE_POWER_POLICY MachinePolicy;
        }

        /// <summary>
        /// Struct representing the power options unique for the current power scheme.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct POWER_POLICY
        {
            /// <summary>
            /// The current power scheme's policy (user specific).
            /// </summary>
            public USER_POWER_POLICY User;
            /// <summary>
            /// The current power scheme's policy (machine specific).
            /// </summary>
            public MACHINE_POWER_POLICY Machine;
        }

        /// <summary>
        /// Struct representing the global power policy (user specific).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GLOBAL_USER_POWER_POLICY
        {
            /// <summary>
            /// The current structure revision level. Set this value by calling GetCurrentPowerPolicies or ReadGlobalPwrPolicy before using a GLOBAL_USER_POWER_POLICY structure to set power policy.
            /// </summary>
            public uint Revision;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take when the power button is pressed and the system is running on AC power.
            /// </summary>
            public POWER_ACTION_POLICY PowerButtonAc;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take when the power button is pressed and the system is running on battery power.
            /// </summary>
            public POWER_ACTION_POLICY PowerButtonDc;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take when the sleep button is pressed and the system is running on AC power.
            /// </summary>
            public POWER_ACTION_POLICY SleepButtonAc;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take when the sleep button is pressed and the system is running on battery power.
            /// </summary>
            public POWER_ACTION_POLICY SleepButtonDc;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take when the lid is closed and the system is running on AC power.
            /// </summary>
            public POWER_ACTION_POLICY LidCloseAc;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take when the lid is closed and the system is running on battery power.
            /// </summary>
            public POWER_ACTION_POLICY LidCloseDc;
            /// <summary>
            /// An array of SYSTEM_POWER_LEVEL structures that defines the actions to take at system battery discharge events.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NUM_DISCHARGE_POLICIES)]
            public SYSTEM_POWER_LEVEL[] DischargePolicy;
            /// <summary>
            /// A flag that enables or disables miscellaneous user power policy settings. This member can be one or more of the values described in Global Flags Constants.
            /// </summary>
            public GlobalPowerPolicyFlags GlobalFlags;
            private const int NUM_DISCHARGE_POLICIES = 4;
        }

        /// <summary>
        /// Struct representing the global power policy (machine specific).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GLOBAL_MACHINE_POWER_POLICY
        {
            /// <summary>
            /// The current structure revision level. Set this value by calling GetCurrentPowerPolicies or ReadGlobalPwrPolicy before using a GLOBAL_MACHINE_POWER_POLICY structure to set power policy.
            /// </summary>
            public uint Revision;
            /// <summary>
            /// The maximum power state (highest Sx value) from which a lid-open event should wake the system when running on AC power. This member must be one of the SYSTEM_POWER_STATE enumeration type values. A value of PowerSystemUnspecified indicates that a lid-open event does not wake the system.
            /// </summary>
            public SYSTEM_POWER_STATE LidOpenWakeAc;
            /// <summary>
            /// The maximum power state (highest Sx value) from which a lid-open event should wake the system when running on battery. This member must be one of the SYSTEM_POWER_STATE enumeration type values. A value of PowerSystemUnspecified indicates that a lid-open event does not wake the system.
            /// </summary>
            public SYSTEM_POWER_STATE LidOpenWakeDc;
            /// <summary>
            /// The resolution of change in the current battery capacity that should cause the system to be notified of a system power state changed event.
            /// </summary>
            public uint BroadcastCapacityResolution;
        }

        /// <summary>
        /// Struct representing current power scheme's policy (user specific).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct USER_POWER_POLICY
        {
            /// <summary>
            /// The current structure revision level. Set this value by calling GetCurrentPowerPolicies or ReadPwrScheme before using a USER_POWER_POLICY structure to set power policy.
            /// </summary>
            public uint Revision;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the system power action to initiate when the system is running on AC (utility) power and the system idle timer expires.
            /// </summary>
            public POWER_ACTION_POLICY IdleAc;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the system power action to initiate when the system is running on battery power and the system idle timer expires.
            /// </summary>
            public POWER_ACTION_POLICY IdleDc;
            /// <summary>
            /// The time that the level of system activity must remain below the idle detection threshold before the system idle timer expires when running on AC (utility) power, in seconds.
            /// This member is ignored if the system is performing an automated resume because there is no user present. To temporarily keep the system running while an application is performing a task, use the SetThreadExecutionState function.
            /// </summary>
            public uint IdleTimeoutAc;
            /// <summary>
            /// The time that the level of system activity must remain below the idle detection threshold before the system idle timer expires when running on battery power, in seconds.
            /// This member is ignored if the system is performing an automated resume because there is no user present. To temporarily keep the system running while an application is performing a task, use the SetThreadExecutionState function.
            /// </summary>
            public uint IdleTimeoutDc;
            /// <summary>
            /// The level of system activity that defines the threshold for idle detection when the system is running on AC (utility) power, expressed as a percentage.
            /// </summary>
            public byte IdleSensitivityAc;
            /// <summary>
            /// The level of system activity that defines the threshold for idle detection when the system is running on battery power, expressed as a percentage.
            /// </summary>
            public byte IdleSensitivityDc;
            /// <summary>
            /// The processor dynamic throttling policy to use when the system is running on AC (utility) power.
            /// </summary>
            public byte ThrottlePolicyAc;
            /// <summary>
            /// The processor dynamic throttling policy to use when the system is running on battery power.
            /// </summary>
            public byte ThrottlePolicyDc;
            /// <summary>
            /// The maximum system sleep state when the system is running on AC (utility) power. This member must be one of the SYSTEM_POWER_STATE enumeration type values.
            /// </summary>
            public SYSTEM_POWER_STATE MaxSleepAc;
            /// <summary>
            /// The maximum system sleep state when the system is running on AC (utility) power. This member must be one of the SYSTEM_POWER_STATE enumeration type values.
            /// </summary>
            public SYSTEM_POWER_STATE MaxSleepDc;
            /// <summary>
            /// Reserved.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] Reserved;
            /// <summary>
            /// The time before the display is turned off when the system is running on AC (utility) power, in seconds.
            /// </summary>
            public uint VideoTimeoutAc;
            /// <summary>
            /// The time before the display is turned off when the system is running on battery power, in seconds.
            /// </summary>
            public uint VideoTimeoutDc;
            /// <summary>
            /// The time before power to fixed disk drives is turned off when the system is running on AC (utility) power, in seconds.
            /// </summary>
            public uint SpindownTimeoutAc;
            /// <summary>
            /// The time before power to fixed disk drives is turned off when the system is running on battery power, in seconds.
            /// </summary>
            public uint SpindownTimeoutDc;
            /// <summary>
            /// If this member is TRUE, the system will turn on cooling fans and run the processor at full speed when passive cooling is specified and the system is running on AC (utility) power. This causes the operating system to be biased toward using the fan and running the processor at full speed.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool OptimizeForPowerAc;
            /// <summary>
            /// If this member is TRUE, the system will turn on cooling fans and run the processor at full speed when passive cooling is specified and the system is running on battery power. This causes the operating system to be biased toward using the fan and running the processor at full speed.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool OptimizeForPowerDc;
            /// <summary>
            /// The lower limit that the processor may be throttled down to prior to turning on system fans in response to a thermal event while the system is operating on AC (utility) power, expressed as a percentage.
            /// </summary>
            public byte FanThrottleToleranceAc;
            /// <summary>
            /// The lower limit that the processor may be throttled down to prior to turning on system fans in response to a thermal event while the system is operating on battery power, expressed as a percentage.
            /// </summary>
            public byte FanThrottleToleranceDc;
            /// <summary>
            /// The processor throttle level to be imposed by the system while the computer is running on AC (utility) power, expressed as a percentage.
            /// </summary>
            public byte ForcedThrottleAc;
            /// <summary>
            /// The processor throttle level to be imposed by the system while the computer is running on battery power, expressed as a percentage.
            /// </summary>
            public byte ForcedThrottleDc;
        }

        /// <summary>
        /// struct representing the current power scheme's policy (machine specific).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MACHINE_POWER_POLICY
        {
            /// <summary>
            /// The current structure revision level. Set this value by calling GetCurrentPowerPolicies or ReadPwrScheme before using a MACHINE_POWER_POLICY structure to set power policy.
            /// </summary>
            public uint Revision;
            /// <summary>
            /// The minimum system power state (lowest Sx value) to enter on a system sleep action when running on AC power. This member must be one of the SYSTEM_POWER_STATE enumeration type values.
            /// </summary>
            public SYSTEM_POWER_STATE MinSleepAc;
            /// <summary>
            /// The minimum system power state (lowest Sx value) to enter on a system sleep action when running on battery power. This member must be one of the SYSTEM_POWER_STATE enumeration type values.
            /// </summary>
            public SYSTEM_POWER_STATE MinSleepDc;
            /// <summary>
            /// The maximum system power state (highest Sx value) to enter on a system sleep action when running on AC power, and when there are outstanding latency requirements. This member must be one of the SYSTEM_POWER_STATE enumeration type values. If an application calls RequestWakeupLatency with LT_LOWEST_LATENCY, ReducedLatencySleepAc is used in place of MaxSleepAc.
            /// </summary>
            public SYSTEM_POWER_STATE ReducedLatencySleepAc;
            /// <summary>
            /// The maximum system power state (highest Sx value) to enter on a system sleep action when running on battery power, and when there are outstanding latency requirements. This member must be one of the SYSTEM_POWER_STATE enumeration type values. If an application calls RequestWakeupLatency with LT_LOWEST_LATENCY, ReducedLatencySleepAc is used in place of MaxSleepAc.
            /// </summary>
            public SYSTEM_POWER_STATE ReducedLatencySleepDc;
            /// <summary>
            /// This member is ignored.
            /// </summary>
            public uint DozeTimeoutAc;
            /// <summary>
            /// This member is ignored.
            /// </summary>
            public uint DozeTimeoutDc;
            /// <summary>
            /// Time to wait between entering the suspend state and entering the hibernate sleeping state when the system is running on AC power, in seconds. A value of zero indicates never hibernate.
            /// </summary>
            public uint DozeS4TimeoutAc;
            /// <summary>
            /// Time to wait between entering the suspend state and entering the hibernate sleeping state when the system is running on battery power, in seconds. A value of zero indicates never hibernate.
            /// </summary>
            public uint DozeS4TimeoutDc;
            /// <summary>
            /// The minimum throttle setting allowed before being overthrottled when the system is running on AC power. Thermal conditions would be the only reason for going below the minimum setting. When the processor is overthrottled, the system will initiate the OverThrottledAc policy. Note that the power policy manager has a hard-coded policy to initiate a CriticalShutdownOff whenever any thermal zone indicates a critical thermal condition. Range: 0-100.
            /// </summary>
            public byte MinThrottleAc;
            /// <summary>
            /// The minimum throttle setting allowed before being overthrottled when the system is running on battery power. Thermal conditions would be the only reason for going below the minimum setting. When the processor is overthrottled, the system will initiate the OverThrottledDc policy. Note that the power policy manager has a hard-coded policy to initiate a CriticalShutdownOff whenever any thermal zone indicates a critical thermal condition. Range: 0-100.
            /// </summary>
            public byte MinThrottleDc;
            /// <summary>
            /// Reserved.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] pad1;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take when a processor has become overthrottled (as defined by the MinThrottleAc member) when the system is running on AC power.
            /// </summary>
            public POWER_ACTION_POLICY OverThrottledAc;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take when a processor has become overthrottled (as defined by the MinThrottleDc member) when the system is running on battery power.
            /// </summary>
            public POWER_ACTION_POLICY OverThrottledDc;
        }

        /// <summary>
        /// Struct representing power action policies.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct POWER_ACTION_POLICY
        {
            /// <summary>
            /// The requested system power state. This member must be one of the POWER_ACTION enumeration type values.
            /// </summary>
            public POWER_ACTION Action;
            /// <summary>
            /// A flag that controls how to switch the power state.
            /// </summary>
            public PowerActionFlags Flags;
            /// <summary>
            /// The level of user notification.
            /// </summary>
            public PowerActionEventCode EventCode;
        }

        /// <summary>
        /// Struct representing system power levels.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_POWER_LEVEL
        {
            /// <summary>
            /// If this member is TRUE, the alarm should be activated when the battery discharges below the value set in BatteryLevel.
            /// </summary>
            public bool Enable;
            /// <summary>
            /// Reserved.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Spare;
            /// <summary>
            /// The battery capacity for this battery discharge policy, expressed as a percentage.
            /// </summary>
            public uint BatteryLevel;
            /// <summary>
            /// A POWER_ACTION_POLICY structure that defines the action to take for this battery discharge policy.
            /// </summary>
            public POWER_ACTION_POLICY PowerPolicy;
            /// <summary>
            /// The minimum system sleep state to enter when the battery discharges below the value set in BatteryLevel. This member must be one of the SYSTEM_POWER_STATE enumeration type values.
            /// </summary>
            public SYSTEM_POWER_STATE MinSystemState;
        }

        /// <summary>
        /// Enum representing global power policy flags.
        /// </summary>
        [Flags]
        public enum GlobalPowerPolicyFlags : uint
        {
            /// <summary>
            /// Enables or disables the battery meter icon in the system tray. When this flag is cleared, the battery meter icon is not displayed.
            /// </summary>
            EnableSysTrayBatteryMeter = 0x01,
            /// <summary>
            /// Enables or disables multiple battery display in the system Power Meter.
            /// </summary>
            EnableMultiBatteryDisplay = 0x02,
            /// <summary>
            /// Enables or disables requiring password logon when the system resumes from standby or hibernate.
            /// </summary>
            EnablePasswordLogon = 0x04,
            /// <summary>
            /// Enables or disables wake on ring support.
            /// </summary>
            EnableWakeOnRing = 0x08,
            /// <summary>
            /// Enables or disables support for dimming the video display when the system changes from running on AC power to running on battery power.
            /// </summary>
            EnableVideoDimDisplay = 0x10,
        }

        /// <summary>
        /// Enum representing power action flags.
        /// </summary>
        [Flags]
        public enum PowerActionFlags : uint
        {
            /// <summary>
            /// Has no effect. 
            /// Windows Server 2003 and Windows XP:  Broadcasts a PBT_APMQUERYSUSPEND event to each application to request permission to suspend operation.
            /// </summary>
            POWER_ACTION_QUERY_ALLOWED = 0x00000001,
            /// <summary>
            /// Applications can prompt the user for directions on how to prepare for suspension. Sets bit 0 in the Flags parameter passed in the lParam parameter of WM_POWERBROADCAST.
            /// </summary>
            POWER_ACTION_UI_ALLOWED = 0x00000002,
            /// <summary>
            /// Has no effect. 
            /// Windows Server 2003 and Windows XP:  Ignores applications that do not respond to the PBT_APMQUERYSUSPEND event broadcast in the WM_POWERBROADCAST message.
            /// </summary>
            POWER_ACTION_OVERRIDE_APPS = 0x00000004,
            /// <summary>
            /// Uses the first lightest available sleep state.
            /// </summary>
            POWER_ACTION_LIGHTEST_FIRST = 0x10000000,
            /// <summary>
            /// Requires entry of the system password upon resume from one of the system standby states.
            /// </summary>
            POWER_ACTION_LOCK_CONSOLE = 0x20000000,
            /// <summary>
            /// Disables all wake events.
            /// </summary>
            POWER_ACTION_DISABLE_WAKES = 0x40000000,
            /// <summary>
            /// Forces a critical suspension.
            /// </summary>
            POWER_ACTION_CRITICAL = 0x80000000,
        }

        /// <summary>
        /// Enum representing power action event codes.
        /// </summary>
        [Flags]
        public enum PowerActionEventCode : uint
        {
            /// <summary>
            /// User notified using the UI.
            /// </summary>
            POWER_LEVEL_USER_NOTIFY_TEXT = 0x00000001,
            /// <summary>
            /// User notified using sound.
            /// </summary>
            POWER_LEVEL_USER_NOTIFY_SOUND = 0x00000002,
            /// <summary>
            /// Specifies a program to be executed.
            /// </summary>
            POWER_LEVEL_USER_NOTIFY_EXEC = 0x00000004,
            /// <summary>
            /// Indicates that the power action is in response to a user power button press.
            /// </summary>
            POWER_USER_NOTIFY_BUTTON = 0x00000008,
            /// <summary>
            /// Indicates a power action of shutdown/off.
            /// </summary>
            POWER_USER_NOTIFY_SHUTDOWN = 0x00000010,
            /// <summary>
            /// Clears a user power button press.
            /// </summary>
            POWER_FORCE_TRIGGER_RESET = 0x80000000,
        }

        /// <summary>
        /// Enum representing system power states.
        /// </summary>
        public enum SYSTEM_POWER_STATE
        {
            /// <summary>
            /// A lid-open event does not wake the system.
            /// </summary>
            PowerSystemUnspecified = 0,
            /// <summary>
            /// Specifies system power state S0.
            /// </summary>
            PowerSystemWorking = 1,
            /// <summary>
            /// Specifies system power state S1.
            /// </summary>
            PowerSystemSleeping1 = 2,
            /// <summary>
            /// Specifies system power state S2.
            /// </summary>
            PowerSystemSleeping2 = 3,
            /// <summary>
            /// Specifies system power state S3.
            /// </summary>
            PowerSystemSleeping3 = 4,
            /// <summary>
            /// Specifies system power state S4 (HIBERNATE).
            /// </summary>
            PowerSystemHibernate = 5,
            /// <summary>
            /// Specifies system power state S5 (OFF).
            /// </summary>
            PowerSystemShutdown = 6,
            /// <summary>
            /// Specifies the maximum enumeration value.
            /// </summary>
            PowerSystemMaximum = 7
        }

        /// <summary>
        /// Enum representing power actions.
        /// </summary>
        public enum POWER_ACTION : uint
        {
            /// <summary>
            /// No system power action.
            /// </summary>
            PowerActionNone = 0,
            /// <summary>
            /// Reserved; do not use.
            /// </summary>
            PowerActionReserved,
            /// <summary>
            /// Sleep.
            /// </summary>
            PowerActionSleep,
            /// <summary>
            /// Hibernate.
            /// </summary>
            PowerActionHibernate,
            /// <summary>
            /// Shutdown.
            /// </summary>
            PowerActionShutdown,
            /// <summary>
            /// Shutdown and reset.
            /// </summary>
            PowerActionShutdownReset,
            /// <summary>
            /// Shutdown and power off.
            /// </summary>
            PowerActionShutdownOff,
            /// <summary>
            /// Warm eject.
            /// </summary>
            PowerActionWarmEject
        }

        /// <summary>
        /// Class containing native methods.
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("powrprof.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.I1)]
            internal static extern bool GetCurrentPowerPolicies(out GLOBAL_POWER_POLICY pGlobalPowerPolicy, out POWER_POLICY pPowerPolicy);

            [DllImport("powrprof.dll")]
            internal static extern UInt32 PowerReadFriendlyName(IntPtr RootPowerKey, ref Guid SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, IntPtr PowerSettingGuid, IntPtr Buffer, ref UInt32 BufferSize);

            [DllImport("powrprof.dll")]
            internal static extern UInt32 PowerGetActiveScheme(IntPtr UserRootPowerKey, ref IntPtr ActivePolicyGuid);

            [DllImport("powrprof.dll")]
            internal static extern uint PowerReadACValue(IntPtr RootPowerKey, ref Guid SchemeGuid, ref Guid SubGroupOfPowerSettingGuid, ref Guid PowerSettingGuid, IntPtr Type, IntPtr Buffer, ref UInt32 BufferSize);
        }

        #endregion Interop
    }

    /// <summary>
    /// Struct representing a power policy.
    /// </summary>
    public struct PowerPolicies
    {
        /// <summary>
        /// Gets the global power policy settings.
        /// </summary>
        public PowerSettingsManager.GLOBAL_POWER_POLICY GlobalPowerPolicy
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the power policy settings that are unique to the active power scheme.
        /// </summary>
        public PowerSettingsManager.POWER_POLICY PowerPolicy
        {
            get;
            internal set;
        }
    }
}
