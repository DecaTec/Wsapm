using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;

namespace Wsapm.Wpf.Controls.SimpleFileDownloader
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window, IDisposable
    {
        private WebClient webClient;
        private string downloadFileName;
        private string targetFileName;
        private string windowTitle;

        /// <summary>
        /// Creates a new instance of DownloadWindow with the settings specified.
        /// </summary>
        /// <param name="fileToDownload">The file to download.</param>
        /// <param name="targetFileName">The (local) target file name.</param>
        /// <param name="windowTitle">The window title of the download window.</param>
        public DownloadWindow(string fileToDownload, string targetFileName, string windowTitle)
        {
            InitializeComponent();

            this.downloadFileName = fileToDownload;
            this.targetFileName = targetFileName;
            this.windowTitle = windowTitle;
        }

        /// <summary>
        /// Creates a new instance of DownloadWindow with the settings specified.
        /// </summary>
        /// <param name="fileToDownload">The file to download.</param>
        /// <param name="targetFileName">The (local) target file name.</param>
        public DownloadWindow(string fileToDownload, string targetFileName)
            : this(fileToDownload, targetFileName, Wsapm.Wpf.Controls.Resources.Wsapm_Wpf_Controls.SimpleFileDownloadWindow_DefaultWindowTitle)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBlockDownloadInfo.Text = this.downloadFileName;
            this.Title = this.windowTitle;
            DownladFileAsync(this.downloadFileName, this.targetFileName);
        }

        private void DownladFileAsync(string fileToDownload, string targetFileName)
        {
            this.webClient = new WebClient();
            this.webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            this.webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            this.webClient.DownloadFileAsync(new Uri(fileToDownload), targetFileName);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => SetProgress(e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive)));
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if ((e.Cancelled && File.Exists(this.targetFileName)) || e.Error != null)
            {
                File.Delete(this.targetFileName);
                this.DialogResult = false;
            }
            else
                this.DialogResult = true;

            this.webClient = null;
            this.Close();
        }

        private void SetProgress(double progressPercent, long bytesReceived, long totalBytesToReceive)
        {
            this.progressBar.Value = progressPercent;
            this.textBlockDownloadProgress.Text = DownloadWindow.ConvertByteToKB(bytesReceived) + "KB / " + DownloadWindow.ConvertByteToKB(totalBytesToReceive) + "KB";
        }

        /// <summary>
        /// Converts size from byte to KB.
        /// </summary>
        /// <param name="sizeInByte">The size in byte.</param>
        /// <returns>The size in KB.</returns>
        private static long ConvertByteToKB(long sizeInByte)
        {
            return sizeInByte / 1024;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.webClient != null)
                this.webClient.CancelAsync();

            try
            {
                // Delete temporary downloaded file.
                if (!string.IsNullOrEmpty(this.targetFileName) && File.Exists(this.targetFileName))
                    File.Delete(this.targetFileName);
            }
            catch (IOException)
            {
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.webClient != null)
                    this.webClient.Dispose();
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
