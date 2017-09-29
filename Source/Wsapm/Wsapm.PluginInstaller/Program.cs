using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Wsapm.Core;

namespace Wsapm.PluginInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Debugger.Launch();
                //Debugger.Break();

                // Arguments:
                // Install: Wsapm.PluginInstaller.exe INSTALL <pluginZipFile> <folderToInstallPlugin> <tempFolderToDelete>
                // Uninstall: Wsapm.PluginInstaller.exe UNINSTALL <pluginFolder>
                if (args.Length == 3)
                {
                    // Uninstall.
                    string operation = args[0];
                    string pluginFolder = args[1];
                    string settingsFile = args[2];

                    if (0 == String.Compare(operation, "UNINSTALL", false))
                        UninstallPlugin(pluginFolder, settingsFile);
                }
                else if (args.Length == 4)
                {
                    // Install.
                    string operation = args[0];
                    string pluginZip = args[1];
                    string newPluginFolder = args[2];
                    string tempUnzipFolder = args[3];

                    if (0 == String.Compare(operation, "INSTALL", false))
                        InstallPlugin(pluginZip, newPluginFolder, tempUnzipFolder);
                }
                else
                {
                    Console.WriteLine(Resources.Wsapm_PluginInstaller.PluginInstaller_ArgsError);
                    Console.WriteLine(Resources.Wsapm_PluginInstaller.PluginInstaller_AnyKeyWsapmStart);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format(Resources.Wsapm_PluginInstaller.PluginInstaller_GeneralError, ex.Message));
                Console.WriteLine(Resources.Wsapm_PluginInstaller.PluginInstaller_AnyKeyWsapmStart);
                Console.ReadKey();
            }
            finally
            {
                // Start Wsapm.
                var fileToStart = Assembly.GetExecutingAssembly().Location;
                var basePath = Path.GetDirectoryName(fileToStart);
                var psi = new ProcessStartInfo();
                psi.Arguments = "-startpluginconfig";
                psi.FileName = basePath + "\\Wsapm.exe";
                Process.Start(psi);
            }
        }

        /// <summary>
        /// Installs a plugin.
        /// </summary>
        /// <param name="pluginZip">The already checked plugin.zip file.</param>
        /// <param name="folderToInstallTo">The folder to install the plugin to.</param>
        /// <param name="tmpFolderToDelete">The temp folder to delete.</param>
        private static void InstallPlugin(string pluginZip, string folderToInstallTo, string tmpFolderToDelete)
        {
            // Exit Wsapm.exe.
            WaitForWsapmShutdown();

            // Shutdown service.
            StopWsapmService();

            // Delete temp unzip folder.
            if (Directory.Exists(tmpFolderToDelete))
                Directory.Delete(tmpFolderToDelete, true);

            // Install plugin.
            Console.WriteLine(Resources.Wsapm_PluginInstaller.PluginInstaller_InstallingPlugin);
            ZipFile zipFile = ZipFile.Read(pluginZip);

            foreach (ZipEntry entry in zipFile)
            {
                entry.Extract(folderToInstallTo, ExtractExistingFileAction.OverwriteSilently);
            }

            // Start service.
            StartWsapmService();
        }

        /// <summary>
        /// Uninstalls a plugin.
        /// </summary>
        /// <param name="pluginFolder">The folder of the plugin to uninstall.</param>
        /// <param name="settingsFile">The settings folder of the plugin. Can be String.Empty when the plugin is a plugin without settings.</param>
        private static void UninstallPlugin(string pluginFolder, string settingsFolder)
        {
            // Exit Wsapm.exe.
            WaitForWsapmShutdown();

            // Shutdown service.
            StopWsapmService();

            // Uninstall plugin.
            Console.WriteLine(Resources.Wsapm_PluginInstaller.PluginInstaller_UninstallingPlugin);

            if (Directory.Exists(pluginFolder))
                Directory.Delete(pluginFolder, true);

            // Remove plugin's settings.
            if (!String.IsNullOrEmpty(settingsFolder) && Directory.Exists(settingsFolder))
            {
                Console.WriteLine((Resources.Wsapm_PluginInstaller.PluginInstaller_UninstallingPluginSettings));
                Directory.Delete(settingsFolder, true);
            }

            // Start service.
            StartWsapmService();
        }

        private static void StopWsapmService()
        {
            var service = GetWsapmService();

            if (service != null && service.Status != ServiceControllerStatus.Stopped)
            {
                Console.WriteLine(Resources.Wsapm_PluginInstaller.PluginInstaller_StoppingService);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }

        private static void StartWsapmService()
        {
            var service = GetWsapmService();

            if (service != null)
            {
                Console.WriteLine(Resources.Wsapm_PluginInstaller.PluginInstaller_StartingService);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
        }

        /// <summary>
        /// Checks if the WSAPM service is already installed.
        /// </summary>
        /// <returns>The service as ServiceController.</returns>
        private static ServiceController GetWsapmService()
        {
            var services = ServiceController.GetServices();

            for (int i = 0; i < services.Length; i++)
            {
                if (services[i].ServiceName == WsapmConstants.ServiceName)
                {
                    return services[i];
                }
            }

            return null;
        }

        private static void WaitForWsapmShutdown()
        {
            Console.WriteLine(Resources.Wsapm_PluginInstaller.PluginInstaller_WaitingForWsapmToShutdown);

            while (IsWsapmRunning())
            {
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Checks if Wsapm.exe is still running.
        /// </summary>
        /// <returns>True, if Wsapm.exe ist still running, otherwise false.</returns>
        private static bool IsWsapmRunning()
        {
            var proc = Process.GetProcessesByName("Wsapm");
            return !(proc == null || proc.Length == 0);
        }
    }
}
