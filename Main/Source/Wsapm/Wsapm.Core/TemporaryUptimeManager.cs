using System;
using System.IO;
using System.Xml.Serialization;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for handling temporary uptimes.
    /// </summary>
    public sealed class TemporaryUptimeManager : IDisposable
    {
        private XmlSerializer serializer;
        private string settingsFolder;
        private string temporaryUptimeFile;
        private FileSystemWatcher fsWatcherTemporaryUptime;

        /// <summary>
        /// Event is raised when the temporary uptime changed
        /// </summary>
        /// <remarks>The client has to load the file manually after the event is fired.</remarks>
        public event EventHandler TemporaryUptimeChanged;

        public TemporaryUptimeManager()
        {
            this.settingsFolder = WsapmTools.GetCommonApplicationDataFolder();
            this.temporaryUptimeFile = WsapmTools.GetTemporaryUptimeFile();
            this.serializer = new XmlSerializer(typeof(TemporaryUptime));
            InitFileSystemWatcher();
        }

        #region Public methods        

        /// <summary>
        /// Saves the given DateTime as temporary uptime file.
        /// </summary>
        /// <param name="temporaryUptime"></param>
        public void SaveTemporaryUptime(TemporaryUptime temporaryUptime)
        {
            try
            {
                if (!Directory.Exists(this.settingsFolder))
                    Directory.CreateDirectory(this.settingsFolder);

                if (File.Exists(this.temporaryUptimeFile))
                    File.Delete(this.temporaryUptimeFile);

                using (FileStream fs = new FileStream(this.temporaryUptimeFile, FileMode.Create, FileAccess.Write))
                {
                    this.serializer.Serialize(fs, (object)temporaryUptime);
                }
            }
            catch (Exception ex)
            {
                throw new WsapmException(Resources.Wsapm_Core.TemporaryUptimeManager_SaveTemporaryUptimeError, ex);
            }
        }

        /// <summary>
        /// Loads the temporary uptime.
        /// </summary>
        /// <returns></returns>
        public TemporaryUptime LoadTemporaryUptime()
        {
            try
            {
                if (!Directory.Exists(WsapmTools.GetCommonApplicationDataFolder()) || !File.Exists(this.temporaryUptimeFile))
                    return null;

                using (FileStream fs = new FileStream(this.temporaryUptimeFile, FileMode.Open, FileAccess.Read))
                {
                    TemporaryUptime temporaryUptime = this.serializer.Deserialize(fs) as TemporaryUptime;
                    return temporaryUptime;
                }
            }
            catch (IOException ex)
            {
                throw new WsapmException(Resources.Wsapm_Core.TemporaryUptimeManager_LoadTemporaryUptimeError, ex);
            }
        }

        public void DeleteTemporaryUptime()
        {
            try
            {
                if (File.Exists(this.temporaryUptimeFile))
                    File.Delete(this.temporaryUptimeFile);
            }
            catch (Exception ex)
            {
                throw new WsapmException(Resources.Wsapm_Core.TemporaryUptimeManager_DeleteTemporaryUptimeError, ex);
            }
        }

        #endregion Public methods

        #region Public properties

        /// <summary>
        /// Gets a value indicating if a temporary uptime is defined and active.
        /// </summary>
        public DateTime? TemporaryUptimeDefinedAndActive
        {
            get
            {
                TemporaryUptime temporaryUptime = null;

                try
                {
                    temporaryUptime = this.LoadTemporaryUptime();
                }
                catch (WsapmException)
                {
                }

                if (temporaryUptime == null)
                    return null;

                if (DateTime.Now < temporaryUptime.TemporaryUptimeUntil)
                    return temporaryUptime.TemporaryUptimeUntil;
                else
                    return null;
            }
        }

        #endregion Public properties

        #region Private methods

        private void InitFileSystemWatcher()
        {
            if (!Directory.Exists(this.settingsFolder))
                Directory.CreateDirectory(this.settingsFolder);

            this.fsWatcherTemporaryUptime = new FileSystemWatcher(this.settingsFolder);
            this.fsWatcherTemporaryUptime.Filter = WsapmConstants.WsapmTemporaryUptimeFile;
            this.fsWatcherTemporaryUptime.Changed += FsWatcherTemporaryUptime_ChangedOrDeleted;
            this.fsWatcherTemporaryUptime.Deleted += FsWatcherTemporaryUptime_ChangedOrDeleted;
            this.fsWatcherTemporaryUptime.EnableRaisingEvents = true;
        }

        private void FsWatcherTemporaryUptime_ChangedOrDeleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                this.fsWatcherTemporaryUptime.EnableRaisingEvents = false;

                var tmp = this.TemporaryUptimeChanged;

                if (tmp != null)
                    tmp(this, EventArgs.Empty);
            }
            finally
            {
                this.fsWatcherTemporaryUptime.EnableRaisingEvents = true;
            }
        }

        #endregion Private methods

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
                if (this.fsWatcherTemporaryUptime != null)
                    this.fsWatcherTemporaryUptime.Dispose();
            }
        }

        #endregion IDisposable members
    }
}
