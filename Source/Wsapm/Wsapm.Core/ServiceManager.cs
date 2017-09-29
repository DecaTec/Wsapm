using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for managing windows services.
    /// </summary>
    /// <remarks>This class is not designed to handle the WSAPM service exclusively, but all windows services in general.</remarks>
    public static class ServiceManager
    {
        private static readonly TimeSpan DefaultTimeoutTimeSpan = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Restarts the services specified.
        /// </summary>
        /// <param name="servicesToRestart">The services to restart (service names or display names).</param>
        /// <remarks>The services are restarted in separate threads.</remarks>
        public static void RestartServices(Service[] servicesToRestart)
        {
            if (servicesToRestart == null || servicesToRestart.Length == 0)
                return;

            foreach (var service in servicesToRestart)
            {
                // Avoid that the WSAPM service gets restarted.
                if (service.ServiceName.ToLower().Trim() == WsapmConstants.ServiceName.ToLower().Trim())
                    continue;

                Task.Factory.StartNew(() => RestartService(service, DefaultTimeoutTimeSpan));
            }
        }

        /// <summary>
        /// Restarts the service specified with a given timeout.
        /// </summary>
        /// <param name="serviceToRestart">The service name or display name of the service.</param>
        /// <param name="timeout">The desired timeout for the starting and stopping operations.</param>
        private static void RestartService(Service serviceToRestart, TimeSpan timeout)
        {
            try
            {
                WsapmLog.Log.WriteLine(string.Format(Resources.Wsapm_Core.ServiceManager_RestartService, serviceToRestart.ServiceName), LogMode.Verbose);

                using (ServiceController service = new ServiceController(serviceToRestart.ServiceName))
                {
                    service.Refresh();

                    if (service.Status == ServiceControllerStatus.Running && service.CanStop)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    }

                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(string.Format(Resources.Wsapm_Core.ServiceManager_RestartServiceFailed, serviceToRestart.ServiceName), ex);
            }
        }
    }
}
