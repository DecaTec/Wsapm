using Microsoft.Deployment.WindowsInstaller;
using System;
using System.IO;
using System.ServiceProcess;
using Wsapm.Core;

namespace Wsapm.Setup.CustomAction
{
    /// <summary>
    /// Class for custom action for Wsapm.Setup.
    /// </summary>
    public class CustomActions
    {
        /// <summary>
        /// Removes the Application data folder of Wsapm with all its contents.
        /// </summary>
        /// <param name="session"></param>
        /// <returns>ActionResult.Success when the operation succeeds, otherwise ActionResult.Failure.</returns>
        [CustomAction]
        public static ActionResult RemoveApplicationDataFolder(Session session)
        {
            session.Log("Begin RemoveApplicationDataFolder");

            try
            {
                string applicationDataFolder = WsapmTools.GetCommonApplicationDataFolder();

                if (Directory.Exists(applicationDataFolder))
                {
                    Directory.Delete(applicationDataFolder, true);
                }
            }
            catch (Exception ex)
            {
                session.Log("Failed to remove application data folder: " + ex.Message);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Removes the plugin folder of Wsapm with all its contents.
        /// </summary>
        /// <param name="session"></param>
        /// <returns>ActionResult.Success when the operation succeeds, otherwise ActionResult.Failure.</returns>
        [CustomAction]
        public static ActionResult RemovePluginFolder(Session session)
        {
            session.Log("Begin RemovePluginFolder");

            try
            {
                // Make sure service is stopped.
                var service = GetWsapmService();

                if (service != null && service.Status != ServiceControllerStatus.Stopped)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }

                string pluginFolder = session["INSTALLFOLDER"] + WsapmConstants.WsapmPluginFolder;

                if (Directory.Exists(pluginFolder))
                {
                    Directory.Delete(pluginFolder, true);
                }
            }
            catch (Exception ex)
            {
                session.Log("Failed to remove plugin folder: " + ex.Message);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
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
    }
}
