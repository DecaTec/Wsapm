using System;
using System.Runtime.InteropServices;

namespace Wsapm.Core
{
    /// <summary>
    /// Manages the standby mode: Standby can be suspended or enabled.
    /// </summary>
    /// <remarks>There is only one suspend action at a time, 
    /// i.e. when a new suspend standby request is handled, an already existing standby suspension is revoked before the new one gets handled.</remarks>
    public sealed class StandbyManager : IDisposable
    {
        private IntPtr currentPowerRequest;
        private bool powerRequestsSupported;
        private bool standbySuspended;

        private static object lockObj = new object();
        private bool disposed = false;

        // For power availability requests.
        private const int POWER_REQUEST_CONTEXT_VERSION = 0;
        private const int POWER_REQUEST_CONTEXT_SIMPLE_STRING = 0x1;
        private const int POWER_REQUEST_CONTEXT_DETAILED_STRING = 0x2;

        // For ThreadExecutionState.
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        /// <summary>
        /// Initializes a new instance of StandbyManager.
        /// </summary>
        public StandbyManager()
        {
            this.currentPowerRequest = IntPtr.Zero;
            this.powerRequestsSupported = CheckPowerAvailabilityRequestsSupported();
            this.standbySuspended = false;
        }

        /// <summary>
        /// Checks if power availability requests Supported are supported.
        /// </summary>
        /// <returns>True, if power availability requests are supported, otherwise false.</returns>
        private bool CheckPowerAvailabilityRequestsSupported()
        {
            var ptr = NativeMethods.LoadLibrary("kernel32.dll");
            var ptr2 = NativeMethods.GetProcAddress(ptr, "PowerSetRequest");

            if (ptr2 == IntPtr.Zero)
            {
                // Power availability requests NOT supported.
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.StandbyManager_PowerAvailabilityRequestsNotSupported, LogMode.Verbose);
                return false;
            }
            else
            {
                // Power availability requests supported.
                WsapmLog.Log.WriteLine(Resources.Wsapm_Core.StandbyManager_PowerAvailabilityRequestsSupported, LogMode.Verbose);
                return true;
            }
        }

        /// <summary>
        /// Suspends standby because of the given CheckSuspendResult.
        /// </summary>
        /// <param name="result">The result why the standby should be suspended.</param>
        public void SuspendStandby(CheckSuspendResult result)
        {
            if (this.powerRequestsSupported)
                SuspendStandbyPowerAvailabilityRequest(result);
            else
                SuspendStandbyThreadExecutionState(result);
        }

        /// <summary>
        /// Suspends standby without a CheckSuspendResult.
        /// </summary>
        /// <remarks>This is for internal use only!</remarks>
        public void SuspendStandby()
        {
            if (this.powerRequestsSupported)
                SuspendStandbyPowerAvailabilityRequest();
            else
                SuspendStandbyThreadExecutionState();
        }

        /// <summary>
        /// Enables standby.
        /// </summary>
        public void EnableStandby()
        {
            if (this.powerRequestsSupported)
                EnableStandbyPowerAvailabilityRequest();
            else
                EnableStandbyThreadExecutionState();
        }

        /// <summary>
        /// Suspend standby by PowerAvailabilityRequest because of a given CheckSuspendResult.
        /// </summary>
        /// <param name="result">The result why the standby should be suspended.</param>
        private void SuspendStandbyPowerAvailabilityRequest(CheckSuspendResult result)
        {
            try
            {
                lock (lockObj)
                {
                    // Clear current power request if there is any.
                    if (this.currentPowerRequest != IntPtr.Zero)
                    {
                        NativeMethods.PowerClearRequest(this.currentPowerRequest, PowerRequestType.PowerRequestSystemRequired);
                        this.currentPowerRequest = IntPtr.Zero;
                        this.standbySuspended = false;
                    }

                    POWER_REQUEST_CONTEXT pContext;
                    pContext.Flags = POWER_REQUEST_CONTEXT_SIMPLE_STRING;
                    pContext.Version = POWER_REQUEST_CONTEXT_VERSION;
                    pContext.SimpleReasonString = Resources.Wsapm_Core.StandbyManager_StandbySuspended;

                    this.currentPowerRequest = NativeMethods.PowerCreateRequest(ref pContext);

                    if (this.currentPowerRequest == IntPtr.Zero)
                    {
                        this.standbySuspended = false;
                        var ex = Win32ExceptionManager.GetWin32Exception();
                        throw new WsapmException(Resources.Wsapm_Core.StandbyManager_CreatePowerRequestFail, ex);
                    }

                    bool success = NativeMethods.PowerSetRequest(this.currentPowerRequest, PowerRequestType.PowerRequestSystemRequired);

                    if (!success)
                    {
                        this.standbySuspended = false;
                        this.currentPowerRequest = IntPtr.Zero;
                        var ex = Win32ExceptionManager.GetWin32Exception();
                        throw new WsapmException(Resources.Wsapm_Core.StandbyManager_SuspendStandbyFailed, ex);
                    }
                    else
                    {
                        this.standbySuspended = true;

                        if (!String.IsNullOrEmpty(result.Reason))
                            WsapmLog.Log.WriteLine(String.Format(Resources.Wsapm_Core.StandbyManager_StandbySuspendedWithReason, result.Reason), LogMode.Normal);
                        else
                            WsapmLog.Log.WriteLine(Resources.Wsapm_Core.StandbyManager_StandbySuspendedWithoutReason, LogMode.Verbose);
                    }
                }
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
            }
        }

        /// <summary>
        /// Suspend standby by PowerAvailabilityRequest without a CheckSuspendResult.
        /// </summary>
        /// <remarks>This is for internal use only, e.g. suspending standby temporarily while checking policies.</remarks>
        private void SuspendStandbyPowerAvailabilityRequest()
        {
            SuspendStandbyPowerAvailabilityRequest(new CheckSuspendResult(true, string.Empty));
        }

        /// <summary>
        /// Suspend standby by SetThreadExecutionSTate because of a given CheckSuspendResult.
        /// </summary>
        /// <param name="result">The result why the standby should be suspended.</param>
        private void SuspendStandbyThreadExecutionState(CheckSuspendResult result)
        {
            try
            {
                lock (lockObj)
                {
                    var success = NativeMethods.SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);

                    if (success == 0)
                    {
                        this.standbySuspended = false;
                        var ex = Win32ExceptionManager.GetWin32Exception();
                        throw new WsapmException(Resources.Wsapm_Core.StandbyManager_SuspendStandbyFailed, ex);
                    }
                    else
                    {
                        this.standbySuspended = true;

                        if (!String.IsNullOrEmpty(result.Reason))
                            WsapmLog.Log.WriteLine(String.Format(Resources.Wsapm_Core.StandbyManager_StandbySuspendedWithReason, result.Reason), LogMode.Normal);
                        else
                            WsapmLog.Log.WriteLine(Resources.Wsapm_Core.StandbyManager_StandbySuspendedWithoutReason, LogMode.Verbose);
                    }
                }
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
            }
        }

        /// <summary>
        /// Suspend standby by SetThreadExecutionSTate without a CheckSuspendResult.
        /// </summary>
        /// <remarks>This is for internal use only, e.g. suspending standby temporarily while checking policies.</remarks>
        private void SuspendStandbyThreadExecutionState()
        {
            SuspendStandbyThreadExecutionState(new CheckSuspendResult(true, String.Empty));
        }

        /// <summary>
        /// Enable standby using power availability request.
        /// </summary>
        private void EnableStandbyPowerAvailabilityRequest()
        {
            try
            {
                lock (lockObj)
                {
                    bool standbyWasSuspended = this.standbySuspended;

                    if (this.currentPowerRequest != IntPtr.Zero)
                    {
                        var success = NativeMethods.PowerClearRequest(this.currentPowerRequest, PowerRequestType.PowerRequestSystemRequired);

                        if (!success)
                        {
                            this.currentPowerRequest = IntPtr.Zero;
                            var ex = Win32ExceptionManager.GetWin32Exception();
                            throw new WsapmException(Resources.Wsapm_Core.StandbyManager_StandbyEnableFailed, ex);
                        }
                        else
                        {
                            this.currentPowerRequest = IntPtr.Zero;
                            this.standbySuspended = false;
                        }
                    }

                    if (standbyWasSuspended)
                        WsapmLog.Log.WriteLine(Resources.Wsapm_Core.StandbyManager_StandbyEnabled, LogMode.Normal); // Only write log if standby was suspended before. 
                }
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
            }
        }

        /// <summary>
        /// Enable standby using ThreadExecutionState.
        /// </summary>
        private void EnableStandbyThreadExecutionState()
        {
            try
            {
                lock (lockObj)
                {
                    bool standbyWasSuspended = this.standbySuspended;
                    var success = NativeMethods.SetThreadExecutionState(ES_CONTINUOUS);

                    if (success == 0)
                    {
                        var ex = Win32ExceptionManager.GetWin32Exception();
                        throw new WsapmException(Resources.Wsapm_Core.StandbyManager_StandbyEnableFailed, ex);
                    }

                    // Standby successfully enabled.
                    this.standbySuspended = false;

                    if (standbyWasSuspended)
                        WsapmLog.Log.WriteLine(Resources.Wsapm_Core.StandbyManager_StandbyEnabled, LogMode.Normal); // Only write log if standby was suspended before. 
                }
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(ex);
            }
        }

        enum PowerRequestType
        {
            PowerRequestDisplayRequired = 0,
            PowerRequestSystemRequired,
            PowerRequestAwayModeRequired,
            PowerRequestMaximum
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct POWER_REQUEST_CONTEXT
        {
            public UInt32 Version;
            public UInt32 Flags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string SimpleReasonString;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PowerRequestContextDetailedInformation
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
            public IntPtr LocalizedReasonModule;
            public UInt32 LocalizedReasonId;
            public UInt32 ReasonStringCount;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string[] ReasonStrings;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct POWER_REQUEST_CONTEXT_DETAILED
        {
            public UInt32 Version;
            public UInt32 Flags;
            public PowerRequestContextDetailedInformation DetailedInformation;
        }

        /// <summary>
        /// Class containing native methods.
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr PowerCreateRequest(ref POWER_REQUEST_CONTEXT Context);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool PowerSetRequest(IntPtr PowerRequestHandle, PowerRequestType RequestType);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool PowerClearRequest(IntPtr PowerRequestHandle, PowerRequestType RequestType);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern uint SetThreadExecutionState(uint esFlags);

            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
            internal static extern IntPtr LoadLibrary(string dllToLoad);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                this.currentPowerRequest = IntPtr.Zero;
                disposed = true;
            }
        }

        ~StandbyManager()
        {
            Dispose(false);
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
