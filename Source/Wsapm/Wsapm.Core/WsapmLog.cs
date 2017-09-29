using System;
using System.IO;
using System.Text;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for logging.
    /// </summary>
    public sealed class WsapmLog
    {
        private static WsapmLog logInstace;
        private string settingsFolder;
        private string logFile;
        private static object lockObj = new object();

        #region Events

        /// <summary>
        /// Event when a new log entry was written.
        /// </summary>
        public event EventHandler<DataEventArgs<string>> NewLogEntry;

        #endregion Events

        /// <summary>
        /// Gets the log instance.
        /// </summary>
        public static WsapmLog Log
        {
            get
            {
                if (logInstace == null)
                    logInstace = new WsapmLog();

                return logInstace;
            }
        }

        /// <summary>
        /// Initializes a new instance of ServiceLog.
        /// </summary>
        private WsapmLog()
            : this(LogMode.Normal, WsapmConvert.ConvertKBToByte(0))
        {

        }

        /// <summary>
        /// Initializes a new instance of ServiceLog.
        /// </summary>
        /// <param name="logMode">The LogMode defining the level of detail of the log.</param>
        private WsapmLog(LogMode logMode)
            : this(logMode, WsapmConvert.ConvertKBToByte(0))
        {

        }

        /// <summary>
        /// Initializes a new instance of ServiceLog.
        /// </summary>
        /// <param name="logMode">The LogMode defining the level of detail of the log.</param>
        /// <param name="maxFileSize">The max file size in byte.</param>
        private WsapmLog(LogMode logMode, uint maxFileSize)
        {
            this.settingsFolder = WsapmTools.GetCommonApplicationDataFolder();
            this.logFile = WsapmTools.GetCommonApplicationLogFile();
            this.LogMode = logMode;
            this.MaxLogFileSize = maxFileSize;
        }

        /// <summary>
        /// Writes a new line in the log.
        /// </summary>
        /// <param name="msg">The message to write in the log.</param>
        /// <param name="logCategory">The log category of the message to log.</param>
        /// <remarks>This function is thread save.</remarks>
        public void WriteLine(string msg, LogMode logCategory)
        {
            if (this.LogMode == LogMode.None || this.LogMode == LogMode.OnlyErrors)
                return;
            else if (this.LogMode == LogMode.Normal)
            {
                if (logCategory == LogMode.Verbose)
                    return;
            }

            WriteLine(msg);
        }

        /// <summary>
        /// Writes a new warning line in the log.
        /// </summary>
        /// <param name="msg">The warning message to write to the log.</param>
        /// <param name="logCategory">The log category of the message to log.</param>
        /// <remarks>This function is thread safe.</remarks>
        public void WriteWarning(string msg, LogMode logCategory)
        {
            if (this.LogMode == LogMode.None)
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append(Resources.Wsapm_Core.WsapmLog_WarningIndicator);
            sb.Append(" ");

            if (!String.IsNullOrEmpty(msg))
                sb.Append(msg);

            WriteLine(sb.ToString());
        }

        /// <summary>
        /// Writes a new error line in the log.
        /// </summary>
        /// <param name="msg">An additional message to the error.</param>
        /// <param name="ex">The Exception which was thrown.</param>
        public void WriteError(string msg, Exception ex)
        {
            if (this.LogMode == LogMode.None)
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append(Resources.Wsapm_Core.WsapmLog_ErrorIndicator);
            sb.Append(" ");

            if (!String.IsNullOrEmpty(msg))
                sb.Append(msg);

            if (ex != null)
            {
                sb.Append(": ");
                sb.Append(ex.Message);

                if (ex.InnerException != null)
                {
                    sb.Append("\t");
                    sb.Append(ex.InnerException.Message);
                }
            }

            WriteLine(sb.ToString());
        }

        /// <summary>
        /// Writes a new error line in the log.
        /// </summary>
        /// <param name="ex">The Exception which was thrown.</param>
        public void WriteError(Exception ex)
        {
            WriteError(String.Empty, ex);
        }

        /// <summary>
        /// Writes a line to the log file.
        /// This action is not dependent of the LogMode.
        /// </summary>
        /// <param name="msg">The message to write in the log.</param>
        /// <remarks>This function is thread save.</remarks>
        private void WriteLine(string msg)
        {
            // First fire the event to inform clients.
            FireNewLogEntryEvent(msg);
            
            // Wite the log message to the log file.
            FileStream fStream = null;
            StreamWriter writer = null;

            try
            {
                lock (lockObj)
                {
                    if (!Directory.Exists(this.settingsFolder))
                        Directory.CreateDirectory(this.settingsFolder);

                    if (!File.Exists(this.logFile))
                        fStream = new FileStream(this.logFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                    else
                    {
                        if (this.MaxLogFileSize != 0)
                        {
                            FileInfo fi = new FileInfo(this.logFile);
                            var currentFileSize = fi.Length;

                            if (currentFileSize > this.MaxLogFileSize)
                            {
                                File.Delete(this.logFile);
                                fStream = new FileStream(this.logFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                            }
                        }
                    }

                    writer = new StreamWriter(this.logFile, true);
                    writer.WriteLine(WsapmTools.GetTimeString() + ": " + msg); 
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                    writer = null;
                }

                if (fStream != null)
                {
                    fStream.Flush();
                    fStream.Close();
                    fStream = null;
                }
            }
        }

        /// <summary>
        /// Reads the log file.
        /// </summary>
        /// <returns>The content of the log file.</returns>
        public string ReadLogFile()
        {
            var logContent = String.Empty;
            FileStream s = null;
            StreamReader reader = null;

            try
            {
                if (!Directory.Exists(this.settingsFolder))
                    return String.Empty;

                if (!File.Exists(this.logFile))
                    return String.Empty;
                else
                {
                    lock (lockObj)
                    {
                        s = new FileStream(this.logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        reader = new StreamReader(s);
                        s = null;
                        logContent = reader.ReadToEnd();
                    }
                }

                return logContent;
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
                    // No s.Flush() because it's a readonly stream.
                    s.Close();
                    s = null;
                }
            }
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
        /// gets or sets the max. log file size.
        /// </summary>
        public uint MaxLogFileSize
        {
            get;
            set;
        }

        /// <summary>
        /// Fires the event when a new log entry is written.
        /// </summary>
        /// <param name="newLogEntry"></param>
        private void FireNewLogEntryEvent(string newLogEntry)
        {
            var tmp = this.NewLogEntry;

            if(tmp != null)
                tmp(this, new DataEventArgs<string>(newLogEntry));
        }
    }
}
