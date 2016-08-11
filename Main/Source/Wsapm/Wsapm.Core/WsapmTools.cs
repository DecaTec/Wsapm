using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Wsapm.Core
{
    public static class WsapmTools
    {
        /// <summary>
        /// Gets the common application data folder for saving settings. 
        /// </summary>
        /// <returns>The application data folder.</returns>
        public static string GetCommonApplicationDataFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" + WsapmConstants.WsapmApplicationDataFolder;
        }

        /// <summary>
        /// Gets the common application data file for saving settings.
        /// </summary>
        /// <returns>The application data file name.</returns>
        public static string GetCommonApplicationDataFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" + WsapmConstants.WsapmApplicationDataFolder + @"\" + WsapmConstants.WsapmApplicationDataFile;
        }

        /// <summary>
        /// Gets the temporary contents file which contains all files/folders to delete on next startup of the application.
        /// </summary>
        /// <returns>The temporary contents file name.</returns>
        public static string GetCommonTempContentsFile()
        {
            return GetCommonApplicationDataFolder() + @"\" + WsapmConstants.TempContentsFile;
        }

        /// <summary>
        /// Gets the common application log file.
        /// </summary>
        /// <returns>The application log file name.</returns>
        public static string GetCommonApplicationLogFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" + WsapmConstants.WsapmApplicationDataFolder + @"\" + WsapmConstants.WsapmApplicationLogFile;
        }

        /// <summary>
        /// Gets the common application data error folder.
        /// </summary>
        /// <returns></returns>
        public static string GetCommonApplicationDataErrorFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" + WsapmConstants.WsapmApplicationDataFolder + @"\" + WsapmConstants.WsapmApplicationDataErrorFolder;
        }

        /// <summary>
        /// Gets the full path of the temporary uptime file
        /// </summary>
        /// <returns>The full path of the temporary uptime file.</returns>
        public static string GetTemporaryUptimeFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" + WsapmConstants.WsapmApplicationDataFolder + @"\" + WsapmConstants.WsapmTemporaryUptimeFile;
        }

        /// <summary>
        /// Gets a new error log file name.
        /// </summary>
        /// <returns>The full path to the new log error file.</returns>
        /// <remarks>This file does not exist and has to be created in case of an error.</remarks>
        public static string GetNewErrorLogFileName()
        {
            var now = DateTime.Now;
            var errorFolder = GetCommonApplicationDataErrorFolder();

            if (!Directory.Exists(errorFolder))
                Directory.CreateDirectory(errorFolder);

            var fileName = "Error_" + now.Year.ToString(CultureInfo.InvariantCulture) + now.Month.ToString(CultureInfo.InvariantCulture) + now.Day.ToString(CultureInfo.InvariantCulture) + "_" + now.Hour.ToString(CultureInfo.InvariantCulture) + now.Minute.ToString(CultureInfo.InvariantCulture) + now.Second.ToString(CultureInfo.InvariantCulture);
            var fullFileName = errorFolder + @"\" + fileName;
            var exists = File.Exists(fullFileName + ".log");
            var retryCounter = 1;

            while (exists)
            {
                fullFileName += "_" + retryCounter.ToString(CultureInfo.InvariantCulture);
                retryCounter++;
                exists = File.Exists(fileName + ".log");
            }

            return fullFileName + ".log";
        }

        /// <summary>
        /// Gets a time string for logging.
        /// </summary>
        /// <returns>A time string dd:mm:YYYY HH:mm:ss</returns>
        public static string GetTimeString()
        {
            var now = DateTime.Now;
            return now.ToShortDateString() + " - " + now.ToLongTimeString();
        }

        /// <summary>
        /// Returns true if setting are saved on the computer.
        /// </summary>
        /// <returns>True if settings are available, otherwise false.</returns>
        public static bool SettingsAvailable()
        {
            return Directory.Exists(WsapmTools.GetCommonApplicationDataFolder());
        }

        /// <summary>
        /// Gets the timer interval in milliseconds.
        /// </summary>
        /// <param name="intervalInMinutes">The timer interval in minutes.</param>
        /// <returns>The timer interval in milliseconds.</returns>
        public static double GetTimerIntervalInMilliseconds(uint intervalInMinutes)
        {
            var ts = TimeSpan.FromMinutes(intervalInMinutes);
            return ts.TotalMilliseconds;
        }

        /// <summary>
        /// Gets a string array with available network interfaces.
        /// </summary>
        /// <returns>A string array containing all network interfaces on the machine. Might be an empty array if there are no network interfaces installed.</returns>
        public static string[] GetAvailableNetworkInterfaces()
        {
            return NetworkTools.GetAvailableNetworkInterfaces();
        }

        /// <summary>
        /// Gets a string array with available logical drive names.
        /// </summary>
        /// <returns>A string array containing all logical drive names on the machine. Might be an empty array if there are no logical drives present.</returns>
        public static string[] GetAvailableLogicalVolumeNames()
        {
            return HddTools.GetAvailableLogicalVolumeNames();
        }

        /// <summary>
        /// Gets the optimal check interval in minutes based on the Windows idle timeout (AC).
        /// </summary>
        /// <returns>The optimal check interval in minutes, but at least 1.</returns>
        public static uint GetOptimalCheckIntervalInMinutes()
        {
            try
            {
                var powerPolicy = PowerSettingsManager.GetCurrentPowerPolicy();
                var winIdleTimeOutAcMinutes = powerPolicy.PowerPolicy.User.IdleTimeoutAc / 60;

                if (winIdleTimeOutAcMinutes <= 0)
                    winIdleTimeOutAcMinutes = 2; // 2 minutes.

                var optimalCheckInterval = winIdleTimeOutAcMinutes - 1;

                if (optimalCheckInterval <= 0)
                    optimalCheckInterval = 1;

                return optimalCheckInterval;
            }
            catch (Exception)
            {
                return WsapmConstants.TimerInterval;
            }
        }

        /// <summary>
        /// Gets the current windows idle timeout (AC) in minutes.
        /// </summary>
        /// <returns>The current windows idle timeout (AC) in minutes.</returns>
        public static uint GetCurrentWindowsIdleTimeoutAcMinutes()
        {
            var powerPolicy = PowerSettingsManager.GetCurrentPowerPolicy();
            var winIdleTimeOutAcMinutes = powerPolicy.PowerPolicy.User.IdleTimeoutAc / 60;

            return winIdleTimeOutAcMinutes;
        }

        /// <summary>
        /// Gets a value indicating if wake timers are allowed.
        /// </summary>
        /// <returns>True, if wake timers are allowed, otherwise false.</returns>
        public static bool GetWakeTimersAllowed()
        {
            try
            {
                var allowed = PowerSettingsManager.GetWakeTimersAllowed();
                return allowed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the state of WakeScheduler and Windows power options are valid.
        /// </summary>
        /// <param name="settings">The WsampSettings to validate.</param>
        /// <returns>True if the state of the setting's WakeScheduler is valid with the current Windows power options, otherwise false.</returns>
        public static bool ValidateWakeTimers(WsapmSettings settings)
        {
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

            return !(settings.WakeSchedulers != null && atLeastOneWakeSchedulerActive && !WsapmTools.GetWakeTimersAllowed());
        }

        /// <summary>
        /// Determines if the state of the check interval is valid with the current Windows power options.
        /// </summary>
        /// <param name="settings">The WsampSettings to validate.</param>
        /// <returns>True if the state of the check interval is valid with the current Windows power options, otherwise false.</returns>
        public static bool ValidateCheckInterval(WsapmSettings settings)
        {
            var winIdleTimeoutAcMinutes = WsapmTools.GetCurrentWindowsIdleTimeoutAcMinutes();
            return !(settings.MonitoringTimerInterval >= winIdleTimeoutAcMinutes);
        }

        /// <summary>
        /// Marks temporary resources (files and/or folders) for deletion on next application's startup.
        /// </summary>
        /// <param name="tempResources">The temporary files and/or folders to delete on next application's sartup.</param>
        public static void MarkTempResourcesForDeletion(params string[] tempResources)
        {
            if (tempResources == null || tempResources.Length == 0)
                return;

            if (!Directory.Exists(GetCommonApplicationDataFolder()))
                return;

            var fileName = GetCommonTempContentsFile();
            string tmpContents = String.Empty;

            // Read old file contents.
            if (File.Exists(fileName))
            {
                try
                {
                    FileStream fileStream = null;

                    try
                    {
                        fileStream = File.OpenRead(fileName);

                        using (var reader = new StreamReader(fileStream))
                        {
                            fileStream = null;
                            tmpContents = reader.ReadToEnd();
                        }
                    }
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Dispose();
                    }
                }
                catch (Exception)
                {
                }
            }

            // Write new temp file.
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                FileStream fileStream = null;

                try
                {
                    fileStream = File.Create(fileName);

                    using (var writer = new StreamWriter(fileStream))
                    {
                        fileStream = null;

                        if (!String.IsNullOrEmpty(tmpContents))
                            writer.Write(tmpContents);

                        foreach (var tmpItem in tempResources)
                        {
                            if (Directory.Exists(tmpItem) || File.Exists(tmpItem))
                                writer.WriteLine(tmpItem); // Only add 'real' files/folders.
                        }
                    }
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Deletes all temporary resources (files and or folders).
        /// </summary>
        /// <remarks>Should only be called upon application startup.</remarks>
        public static void DeleteTempResources()
        {
            if (!Directory.Exists(GetCommonApplicationDataFolder()))
                return;

            var fileName = GetCommonTempContentsFile();
            string tmpContents = String.Empty;

            if (!File.Exists(fileName))
                return;

            // Read old file contents.
            try
            {
                FileStream fileStream = null;

                try
                {
                    fileStream = File.OpenRead(fileName);

                    using (var reader = new StreamReader(fileStream))
                    {
                        fileStream = null;
                        tmpContents = reader.ReadToEnd();
                    }
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Dispose();
                }
            }
            catch (Exception)
            {
                return;
            }

            if (String.IsNullOrEmpty(tmpContents))
                return;

            string[] sArr = tmpContents.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var tmpItem in sArr)
            {
                try
                {
                    if (File.Exists(tmpItem))
                    {
                        File.Delete(tmpItem);
                        continue;
                    }

                    if (Directory.Exists(tmpItem))
                        Directory.Delete(tmpItem, true);
                }
                catch (Exception)
                {
                }
            }

            // Delete temp content file.
            try
            {
                File.Delete(fileName);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets an IPAddress from a given masked text box string.
        /// </summary>
        /// <param name="textBoxString"The string of a (IP) masked text box.></param>
        /// <returns>The IPAddress from the given string or null if it is not a valid IP address.</returns>
        public static IPAddress GetIPAddressFromIPTextBoxString(string textBoxString)
        {
            if (string.IsNullOrEmpty(textBoxString))
                return null;

            var sArr = textBoxString.Split('.');
            sArr[0] = sArr[0].Trim();
            sArr[1] = sArr[1].Trim();
            sArr[2] = sArr[2].Trim();
            sArr[3] = sArr[3].Trim();

            var sb = new StringBuilder();
            sb.Append(sArr[0]);
            sb.Append(".");
            sb.Append(sArr[1]);
            sb.Append(".");
            sb.Append(sArr[2]);
            sb.Append(".");
            sb.Append(sArr[3]);
            var ipStr = sb.ToString();

            try
            {
                return IPAddress.Parse(ipStr);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a string for representing all network interfaces.
        /// </summary>
        /// <returns></returns>
        public static string GetCommonDiaplayNameAllNetworkInterfaces()
        {
            return Wsapm.Core.Resources.Wsapm_Core.WsapmManager_DisplayNameAllNetworkInterfaces;
        }

        /// <summary>
        /// Gets a string for representing all HDDs.
        /// </summary>
        /// <returns></returns>
        public static string GetCommonDiaplayNameAllDrives()
        {
            return Wsapm.Core.Resources.Wsapm_Core.WsapmManager_DisplayNameAllDrives;
        }
    }
}
