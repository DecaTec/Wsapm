using Ionic.Zip;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a settings manager in order to save and load settings.
    /// </summary>
    public sealed class SettingsManager : IDisposable
    {
        private XmlSerializer serializer;
        private string settingsFile;
        private string settingsFolder;
        private FileSystemWatcher fsWatcher;

        /// <summary>
        /// Event is raised when the settings file was changed.
        /// </summary>
        /// <remarks>The client has to load the settings file manually after the event is fired.</remarks>
        public event EventHandler SettingsChanged;

        /// <summary>
        /// Initializes a new instance of SettingsManager.
        /// </summary>
        public SettingsManager()
        {
            this.settingsFolder = WsapmTools.GetCommonApplicationDataFolder();
            this.settingsFile = WsapmTools.GetCommonApplicationDataFile();
            this.serializer = new XmlSerializer(typeof(WsapmSettings));
            InitFileSystemWatcher();
        }

        private void InitFileSystemWatcher()
        {
            if (!Directory.Exists(this.settingsFolder))
                Directory.CreateDirectory(this.settingsFolder);

            this.fsWatcher = new FileSystemWatcher(this.settingsFolder);
            this.fsWatcher.Filter = WsapmConstants.WsapmApplicationDataFile;
            this.fsWatcher.Changed += fsWatcher_Changed;
            this.fsWatcher.EnableRaisingEvents = true;
        }

        void fsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                this.fsWatcher.EnableRaisingEvents = false;

                var tmp = this.SettingsChanged;

                if (tmp != null)
                    tmp(this, EventArgs.Empty);
            }
            finally
            {
                this.fsWatcher.EnableRaisingEvents = true;
            }
        }
        /// <summary>
        /// Loads settings from settings file.
        /// </summary>
        /// <returns>The loaded settings.</returns>
        public WsapmSettings LoadSettings()
        {
            try
            {
                if (!Directory.Exists(WsapmTools.GetCommonApplicationDataFolder()) || !File.Exists(this.settingsFile))
                {
                    var newSetings = GetDefaultSettings();
                    return newSetings;
                }

                using (FileStream fs = new FileStream(this.settingsFile, FileMode.Open, FileAccess.Read))
                {
                    WsapmSettings settings = this.serializer.Deserialize(fs) as WsapmSettings;
                    return settings;
                }
            }
            catch (IOException ex)
            {
                throw new WsapmException(Resources.Wsapm_Core.SettingsManager_LoadSettingsError, ex);
            }
        }

        /// <summary>
        /// Saves the settings to the settings file.
        /// </summary>
        /// <param name="settings">The settings to save.</param>
        public void SaveSettings(WsapmSettings settings)
        {
            if (settings == null)
                return;

            if (!Directory.Exists(this.settingsFolder))
                Directory.CreateDirectory(this.settingsFolder);

            SaveSettings(settings, this.settingsFile);
        }

        /// <summary>
        /// Saves the settings to the settings file specified.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="settingsFile"></param>
        internal void SaveSettings(WsapmSettings settings, string settingsFile)
        {
            if (settings == null || string.IsNullOrEmpty(settingsFile))
                return;

            try
            {
                if (File.Exists(settingsFile))
                    File.Delete(settingsFile);

                using (FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write))
                {
                    this.serializer.Serialize(fs, (object)settings);
                }
            }
            catch (Exception ex)
            {
                throw new WsapmException(Resources.Wsapm_Core.SettingsManager_SaveSettingsError, ex);
            }
        }

        /// <summary>
        /// Loads the default settings.
        /// </summary>
        public WsapmSettings GetDefaultSettings()
        {
            var newSettings = new WsapmSettings();
            newSettings.MonitoringTimerInterval = WsapmTools.GetOptimalCheckIntervalInMinutes();
            newSettings.MaxLogFileSize = WsapmConvert.ConvertKBToByte(100);
            newSettings.NetworkInterfacesToMonitor = new System.Collections.Generic.List<NetworkInterfaceToMonitor>();
            newSettings.HddsToMonitor = new System.Collections.Generic.List<HddToMonitor>();
            newSettings.EnableCheckNetworkResourceAccess = false;
            newSettings.CheckNetworkResourcesType = NetworkShareAccessType.Files;
            newSettings.CpuLoad = 0.0f;
            newSettings.MemoryLoad = 0.0f;
            newSettings.UptimeSchedulers = new System.Collections.Generic.List<UptimeScheduler>();
            newSettings.WakeSchedulers = new System.Collections.Generic.List<WakeScheduler>();            
            newSettings.LogMode = LogMode.Normal;
            newSettings.EnableRemoteShutdown = false;
            newSettings.RemoteShutdownPort = 9;
            newSettings.RemoteShutdownPasswordHash = string.Empty;            
            return newSettings;
        }

        /// <summary>
        /// Deletes the settings from the computer.
        /// </summary>
        public void DeleteSettings()
        {
            if (!WsapmTools.SettingsAvailable())
                return;

            try
            {
                Directory.Delete(WsapmTools.GetCommonApplicationDataFolder(), true);
            }
            catch (Exception ex)
            {
                throw new WsapmException(Resources.Wsapm_Core.SettingsManager_DeleteSettingsError, ex);
            }

            this.LoadSettings();
        }

        #region Import/Export

        /// <summary>
        /// Exports the settings as Zip file.
        /// </summary>
        /// <param name="fileToExportZip">The target zip file to export.</param>
        public void ExportSettings(string fileToExportZip)
        {
            if (string.IsNullOrEmpty(fileToExportZip))
                return;

            var path = Path.GetDirectoryName(fileToExportZip);

            if (!Directory.Exists(path))
                return;

            if (!File.Exists(this.settingsFile))
                return;

            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(this.settingsFile, string.Empty);
                zip.Save(fileToExportZip);
            }
        }

        /// <summary>
        /// Imports settings from a given Zip file.
        /// </summary>
        /// <param name="fileToImportZip"></param>
        public void ImportSettings(string fileToImportZip)
        {
            if (string.IsNullOrEmpty(fileToImportZip))
                return;

            if (!File.Exists(fileToImportZip))
                return;

            var tmpFolder = Path.GetTempPath() + Guid.NewGuid().ToString();

            try
            {
                Directory.CreateDirectory(tmpFolder);

                using (ZipFile zip = ZipFile.Read(fileToImportZip))
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(tmpFolder, ExtractExistingFileAction.OverwriteSilently);
                    }
                }

                var unzippedSettingsFile = tmpFolder + @"\" + WsapmConstants.WsapmApplicationDataFile; // The settings file name has always to be the same.

                if (!File.Exists(unzippedSettingsFile) || !IsValidSettingsFile(unzippedSettingsFile))
                    throw new WsapmException(Resources.Wsapm_Core.SettingsManager_ImportSettingsError); // No valid settings file.

                File.Copy(unzippedSettingsFile, this.settingsFile, true);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (Directory.Exists(tmpFolder))
                    Directory.Delete(tmpFolder, true);
            }
        }

        /// <summary>
        /// Checks if a given file is a valid WSAPM settings file.
        /// </summary>
        /// <param name="fileName">The file to check.</param>
        /// <returns>True, if the given file is a valid WSAPM settings file, otherwise false.</returns>
        private bool IsValidSettingsFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return false;

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    WsapmSettings settings = this.serializer.Deserialize(fs) as WsapmSettings;
                    return settings != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion Import/Export

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.fsWatcher != null)
                    this.fsWatcher.Dispose();
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
