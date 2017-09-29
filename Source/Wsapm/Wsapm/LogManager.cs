using System;
using System.IO;

namespace Wsapm
{
    /// <summary>
    /// Class for logging WSAPM events.
    /// </summary>
    internal sealed class LogManager : IDisposable
    {
        private string folderName;
        private string fileName;
        private FileSystemWatcher fsWatcher;
        private static object lockObj = new object();

        /// <summary>
        /// LogChanged event.
        /// </summary>
        internal event EventHandler LogChanged;

        private void FireLogChanged()
        {
            var tmp = this.LogChanged;

            if (tmp != null)
                tmp(this, EventArgs.Empty);
        }

        /// <summary>
        /// Creates a new instance of LofManager.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folderName"></param>
        internal LogManager(string folderName, string fileName)
        {
            this.folderName = folderName;
            this.fileName = fileName;
            InitFileSystemWatcher();
            ReadLogFile(false); // Read existing log for the first time.
        }

        /// <summary>
        /// Gets or sets the log content.
        /// </summary>
        internal string Log
        {
            get;
            private set;
        }

        /// <summary>
        /// Starts watching the logging file.
        /// </summary>
        internal void Start()
        {
            this.fsWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Stops watching the logging file.
        /// </summary>
        internal void Stop()
        {
            this.fsWatcher.EnableRaisingEvents = false;
        }

        private void InitFileSystemWatcher()
        {
            if (!Directory.Exists(this.folderName))
                Directory.CreateDirectory(this.folderName);

            this.fsWatcher = new FileSystemWatcher(this.folderName);
            this.fsWatcher.IncludeSubdirectories = false;
            this.fsWatcher.Filter = this.fileName;
            this.fsWatcher.Changed += fsWatcher_Changed;
        }

        void fsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            ReadLogFile(true);
        }

        private void ReadLogFile(bool fireLogFileChangedEvent)
        {
            var log = String.Empty;
            FileStream s = null;
            StreamReader reader = null;

            var settingsFolder = this.folderName;
            var logFile = this.fileName;

            try
            {
                if (!Directory.Exists(settingsFolder))
                    return;

                var fullFileName = settingsFolder + @"\" + logFile;

                if (!File.Exists(fullFileName))
                    return;
                else
                {
                    lock (lockObj)
                    {
                        s = new FileStream(fullFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                }

                reader = new StreamReader(s);
                s = null;
                this.Log = reader.ReadToEnd();

                if (fireLogFileChangedEvent)
                    FireLogChanged();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }

                if (s != null)
                {
                    // No s.Flush() because it's a read only stream.
                    s.Close();
                    s = null;
                }
            }
        }

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
