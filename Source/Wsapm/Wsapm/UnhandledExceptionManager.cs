using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Wsapm.Core;
using Wsapm.Wpf.Controls;

namespace Wsapm
{
    public static class UnhandledExceptionManager
    {
        public static readonly int DefaultErrorReturnCode = -1;
        public static readonly int ReturnCodeSevereError = -256;

        /// <summary>
        /// Handles unhanded exception by showing general exception dialog.
        /// </summary>
        /// <param name="ex">The unhanded exception.</param>
        public static void HandleException(Exception ex)
        {
            ShowExceptionDialog(ex);
        }

        /// <summary>
        /// Handles unhanded exception and shuts down application afterwards.
        /// </summary>
        /// <param name="ex">The unhanded exception.</param>
        public static void HandleExceptionAndShutDownApplication(Exception ex)
        {
            HandleExceptionAndShutDownApplication(ex, DefaultErrorReturnCode);
        }

        /// <summary>
        /// Handles unhanded exception and shuts down application afterwards with the given return code.
        /// </summary>
        /// <param name="ex">The unhanded exception.</param>
        /// <param name="returnCode">The return code of the application.</param>
        public static void HandleExceptionAndShutDownApplication(Exception ex, int returnCode)
        {
            ShowExceptionDialog(ex);
            Application.Current.Shutdown(returnCode);
        }

        private static void ShowExceptionDialog(Exception ex)
        {
            try
            {
                var progInfo = new ProgramInformation("Wsapm.exe", VersionInformation.Version.ToString(3));
                var exceptionDialog = new UnhandledExceptionWindow(ex, progInfo, Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, new BitmapImage(new Uri("pack://application:,,,/Resources/app.ico")));
                exceptionDialog.ShowDialog();
            }
            catch (Exception)
            {
                // If something fails when showing dialog, the application has to be shut down.
                // Use specific return coded to signal severe error.
                Application.Current.Shutdown(ReturnCodeSevereError);
            }
        }
    }
}
