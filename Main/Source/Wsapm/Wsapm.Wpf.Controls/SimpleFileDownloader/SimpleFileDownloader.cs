using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace Wsapm.Wpf.Controls.SimpleFileDownloader
{
    /// <summary>
    /// Class for downloading single files with a progress bar.
    /// </summary>
    public static class SimpleFileDownloader
    {
        /// <summary>
        /// Downloads the specified file and lets the user specify the path and file name of the file to download.
        /// </summary>
        /// <param name="fileToDownload">The file to download.</param>
        /// <returns>The (local) file name of the downloaded file if the file was downloaded successfully. When the file was not downloaded successfully, this. will be null.</returns>
        public static string Download(string fileToDownload)
        {
            return Download(fileToDownload, false, false);
        }

        /// <summary>
        /// Downloads the specified file and lets the user specify the path and file name of the file to download.
        /// </summary>
        /// <param name="fileToDownload">The file to download.</param>
        /// <param name="autoExecuteFileAfterDownload">True if the file should be automatically executed after download finished successfully. Otherwise false.</param>
        /// <returns>The (local) file name of the downloaded file if the file was downloaded successfully. When the file was not downloaded successfully, this. will be null.</returns>
        public static string Download(string fileToDownload, bool autoExecuteFileAfterDownload)
        {
            return Download(fileToDownload, autoExecuteFileAfterDownload, false);
        }

        /// <summary>
        /// Downloads the specified file and lets the user specify the path and file name of the file to download.
        /// </summary>
        /// <param name="fileToDownload">The file to download.</param>
        /// <param name="autoExecuteFileAfterDownload">True if the file should be executed automatically after download finished successfully. Otherwise false.</param>
        /// <param name="shutDownApplicationWhenExecutingAfterDownload">True if the current application should be shut down after the execution of the downloaded file. Otherwise false.</param>
        /// <returns>The (local) file name of the downloaded file if the file was downloaded successfully. When the file was not downloaded successfully, this. will be null.</returns>
        public static string Download(string fileToDownload, bool autoExecuteFileAfterDownload, bool shutDownApplicationWhenExecutingAfterDownload)
        {
            if (!WebTools.WebResourceExists(fileToDownload))
                return null;

            var fileName = Path.GetFileName(fileToDownload);
            var extension = Path.GetExtension(fileToDownload);
            var extensionWithoutDot = extension.Substring(1);

            var sfd = new SaveFileDialog();
            sfd.CheckPathExists = true;
            sfd.OverwritePrompt = true;
            sfd.FileName = fileName;
            sfd.DefaultExt = extension;
            sfd.Filter = extensionWithoutDot + " (*." + extensionWithoutDot + ")|*." + extensionWithoutDot;
            var result = sfd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var downloadWindow = new DownloadWindow(fileToDownload, sfd.FileName);
                var downloadResult = downloadWindow.ShowDialog();

                if (downloadResult.HasValue && !downloadResult.Value)
                    return null;

                if (autoExecuteFileAfterDownload)
                {
                    Process.Start(sfd.FileName);

                    if (shutDownApplicationWhenExecutingAfterDownload)
                        Environment.Exit(0);
                }

                return sfd.FileName;
            }
            else
                return null;
        }
    }
}
