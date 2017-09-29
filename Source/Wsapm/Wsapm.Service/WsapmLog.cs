using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wsapm.Core;

namespace Wsapm.Service
{
    /// <summary>
    /// Class for logging.
    /// </summary>
    internal sealed class WsapmLog
    {
        private string settingsFolder;
        private string logFile;
        private static object lockObj = new object();

        /// <summary>
        /// Initializes a new instance of ServiceLog.
        /// </summary>
        internal WsapmLog()
            : this(LogMode.Normal, DecaTec.Toolkit.Tools.Convert.ConvertKBToByte(0))
        {

        }

        /// <summary>
        /// Initializes a new instance of ServiceLog.
        /// </summary>
        /// <param name="logMode">The LogMode defining the level of detail of the log.</param>
        internal WsapmLog(LogMode logMode)
            : this(logMode, DecaTec.Toolkit.Tools.Convert.ConvertKBToByte(0))
        {

        }

        /// <summary>
        /// Initializes a new instance of ServiceLog.
        /// </summary>
        /// <param name="logMode">The LogMode defining the level of detail of the log.</param>
        /// <param name="maxFileSize">The max file size in byte.</param>
        internal WsapmLog(LogMode logMode, uint maxFileSize)
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
        internal void WriteLine(string msg, LogMode logCategory)
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
        internal void WriteWarning(string msg, LogMode logCategory)
        {
            if (this.LogMode == LogMode.None)
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append(Resources.Wsapm_Service.WsapmLog_WarningIndicator);
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
        internal void WriteError(string msg, Exception ex)
        {
            if (this.LogMode == LogMode.None)
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append(Resources.Wsapm_Service.WsapmLog_ErrorIndicator);
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
        internal void WriteError(Exception ex)
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
        /// Gets or sets the log mode.
        /// </summary>
        internal LogMode LogMode
        {
            get;
            set;
        }

        /// <summary>
        /// gets or sets the max. log file size.
        /// </summary>
        internal uint MaxLogFileSize
        {
            get;
            set;
        }
    }
}
