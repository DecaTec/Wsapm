﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Wsapm.Core.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Wsapm_Common {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Wsapm_Common() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Wsapm.Core.Resources.Wsapm.Common", typeof(Wsapm_Common).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CPU load greater than {0}%.
        /// </summary>
        internal static string CpuLoadCheck_CpuLoadCheckReason {
            get {
                return ResourceManager.GetString("CpuLoadCheck_CpuLoadCheckReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Network load (upload + download) greater than {0} KB/s.
        /// </summary>
        internal static string NetworkLoadCheck_CombinedNetworkLoadReason {
            get {
                return ResourceManager.GetString("NetworkLoadCheck_CombinedNetworkLoadReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Network load (download) greater than {0} KB/s.
        /// </summary>
        internal static string NetworkLoadCheck_DownloadNetworkLoadReason {
            get {
                return ResourceManager.GetString("NetworkLoadCheck_DownloadNetworkLoadReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Network load (upload) greater than {0} KB/s.
        /// </summary>
        internal static string NetworkLoadCheck_UploadNetworkLoadReason {
            get {
                return ResourceManager.GetString("NetworkLoadCheck_UploadNetworkLoadReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given IP address is not valid.
        /// </summary>
        internal static string NetworkMachine_InvalidIPError {
            get {
                return ResourceManager.GetString("NetworkMachine_InvalidIPError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resetting network connections.
        /// </summary>
        internal static string NetworkManager_ResetNetworkConnections {
            get {
                return ResourceManager.GetString("NetworkManager_ResetNetworkConnections", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connection &apos;{0}&apos; reset.
        /// </summary>
        internal static string NetworkManager_ResetNetworkInterface {
            get {
                return ResourceManager.GetString("NetworkManager_ResetNetworkInterface", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to reset networ connection &apos;{0}&apos;.
        /// </summary>
        internal static string NetworkManager_ResetNetworkInterfaceFailed {
            get {
                return ResourceManager.GetString("NetworkManager_ResetNetworkInterfaceFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to reset network connections.
        /// </summary>
        internal static string NetworkManager_ResetNetworkInterfaceGeneralError {
            get {
                return ResourceManager.GetString("NetworkManager_ResetNetworkInterfaceGeneralError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Network access to &apos;{0}&apos;.
        /// </summary>
        internal static string NetworkResourcesCheck_NetworkResourcesCheckReason {
            get {
                return ResourceManager.GetString("NetworkResourcesCheck_NetworkResourcesCheckReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Machine {0} online.
        /// </summary>
        internal static string PingCheck_MachineOnlineReason {
            get {
                return ResourceManager.GetString("PingCheck_MachineOnlineReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Process {0} found.
        /// </summary>
        internal static string ProcessCheck_ProcessFoundReason {
            get {
                return ResourceManager.GetString("ProcessCheck_ProcessFoundReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Settings could not be deleted.
        /// </summary>
        internal static string SettingsManager_DeleteSettingsError {
            get {
                return ResourceManager.GetString("SettingsManager_DeleteSettingsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while loading settings.
        /// </summary>
        internal static string SettingsManager_LoadSettingsError {
            get {
                return ResourceManager.GetString("SettingsManager_LoadSettingsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while saving settings.
        /// </summary>
        internal static string SettingsManager_SaveSettingsError {
            get {
                return ResourceManager.GetString("SettingsManager_SaveSettingsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to create power request.
        /// </summary>
        internal static string StandbyManager_CreatePowerRequestFail {
            get {
                return ResourceManager.GetString("StandbyManager_CreatePowerRequestFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Power availability requests not supported by operation system.
        /// </summary>
        internal static string StandbyManager_PowerAvailabilityRequestsNotSupported {
            get {
                return ResourceManager.GetString("StandbyManager_PowerAvailabilityRequestsNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Power availability requests supported by operation system.
        /// </summary>
        internal static string StandbyManager_PowerAvailabilityRequestsSupported {
            get {
                return ResourceManager.GetString("StandbyManager_PowerAvailabilityRequestsSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reset Windows idle timer.
        /// </summary>
        internal static string StandbyManager_ResetWindowsIdleTimer {
            get {
                return ResourceManager.GetString("StandbyManager_ResetWindowsIdleTimer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to reset Windows idle timer.
        /// </summary>
        internal static string StandbyManager_ResetWindowsIdleTimerFailed {
            get {
                return ResourceManager.GetString("StandbyManager_ResetWindowsIdleTimerFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Standby suspension deactivated.
        /// </summary>
        internal static string StandbyManager_StandbyEnabled {
            get {
                return ResourceManager.GetString("StandbyManager_StandbyEnabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to deactivate standby suspension.
        /// </summary>
        internal static string StandbyManager_StandbyEnableFailed {
            get {
                return ResourceManager.GetString("StandbyManager_StandbyEnableFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Windows Server Advanced Power Management: Standby suspended.
        /// </summary>
        internal static string StandbyManager_StandbySuspended {
            get {
                return ResourceManager.GetString("StandbyManager_StandbySuspended", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Standby unterdrückt.
        /// </summary>
        internal static string StandbyManager_StandbySuspendedWithoutReason {
            get {
                return ResourceManager.GetString("StandbyManager_StandbySuspendedWithoutReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Standby suspended (Reason: {0}).
        /// </summary>
        internal static string StandbyManager_StandbySuspendedWithReason {
            get {
                return ResourceManager.GetString("StandbyManager_StandbySuspendedWithReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to suspend standby mode.
        /// </summary>
        internal static string StandbyManager_SuspendStandbyFailed {
            get {
                return ResourceManager.GetString("StandbyManager_SuspendStandbyFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to cancel wake up timer.
        /// </summary>
        internal static string WakeManager_CancelWakeTimerFailed {
            get {
                return ResourceManager.GetString("WakeManager_CancelWakeTimerFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to set timer for wake up.
        /// </summary>
        internal static string WakeManager_SetWakeTimerFailed {
            get {
                return ResourceManager.GetString("WakeManager_SetWakeTimerFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wake scheduled at {0}.
        /// </summary>
        internal static string WakeManager_WakeScheduled {
            get {
                return ResourceManager.GetString("WakeManager_WakeScheduled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wake timer cancelled.
        /// </summary>
        internal static string WakeManager_WakeTimerCancelled {
            get {
                return ResourceManager.GetString("WakeManager_WakeTimerCancelled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ***ERROR***.
        /// </summary>
        internal static string WsapmLog_ErrorIndicator {
            get {
                return ResourceManager.GetString("WsapmLog_ErrorIndicator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ***WARNING***.
        /// </summary>
        internal static string WsapmLog_WarningIndicator {
            get {
                return ResourceManager.GetString("WsapmLog_WarningIndicator", resourceCulture);
            }
        }
    }
}
