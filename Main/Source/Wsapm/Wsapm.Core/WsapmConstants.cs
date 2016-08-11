
namespace Wsapm.Core
{
    /// <summary>
    /// Class for server constants.
    /// </summary>
    public static class WsapmConstants
    {
        /// <summary>
        /// The timer interval.
        /// </summary>
        public static uint TimerInterval = 5; // 5 minutes

        /// <summary>
        /// The name of the WSAPM service.
        /// </summary>
        public static string ServiceName = @"Windows Server Advanced Power Management";

        /// <summary>
        /// Application data folder name.
        /// </summary>
        public static string WsapmApplicationDataFolder = @"Windows Server Advanced Power Management";

        /// <summary>
        /// Gets the error data folder name.
        /// </summary>
        public static string WsapmApplicationDataErrorFolder = @"Errors";

        /// <summary>
        /// Application data file name.
        /// </summary>
        public static string WsapmApplicationDataFile = @"WsapmSettings.xml";

        /// <summary>
        /// Application data file name.
        /// </summary>
        public static string WsapmApplicationLogFile = @"WsapmLog.log";

        /// <summary>
        /// Temporary contents file which contains the files/folders to delete on next start of the application.
        /// </summary>
        public static string TempContentsFile = @"TempContents.tmp";

        /// <summary>
        /// Interval in ms to wait between getting samples from performance monitors.
        /// </summary>
        public static int PerformanceCounterBreakIntervallMs = 200;

        /// <summary>
        /// Number of check probes to take when a performance counter is queried.
        /// </summary>
        public static int PerformanceCounterDefaultNumberCheckProbes = 5;

        /// <summary>
        /// Gets the plugin folder name.
        /// </summary>
        public static string WsapmPluginFolder = @"Plugins";

        /// <summary>
        /// File name for temporary uptime file.
        /// </summary>
        public static string WsapmTemporaryUptimeFile = @"TemporaryUptime.xml";

        /// <summary>
        /// Constant for identifying all network interfaces in the settings.
        /// </summary>
        public static string AllNetworkInterfacesName = "AllNetworkInterfaces";

        /// <summary>
        /// Constant for identifying all HDDs in the settings.
        /// </summary>
        public static string AllHddsName = "AllDrives";
    }
}
