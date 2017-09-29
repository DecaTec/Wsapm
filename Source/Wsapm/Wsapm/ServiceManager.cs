using System;
using System.ServiceProcess;
using Wsapm.Core;
using System.Linq;

namespace Wsapm
{
    /// <summary>
    /// Class for handling of the WSAPM service.
    /// </summary>
    internal sealed class ServiceManager : IDisposable
    {
        private ServiceController service;
        private bool serviceInstalled;
        bool disposed = false;

        /// <summary>
        /// Initializes a new instance of ServiceManager
        /// </summary>
        internal ServiceManager()
        {
            this.service = GetWsapmService();
        }

        ~ServiceManager()
        {
            // Avoid calling dispose on destruction.
            Dispose(false);
        }

        /// <summary>
        /// Gets a value indicating if the WSAPM service is currently installed.
        /// </summary>
        internal bool ServiceInstalled
        {
            get
            {
                return this.serviceInstalled;
            }
        }

        /// <summary>
        /// Gets the current status message, i.e. error message when the service could not be started.
        /// </summary>
        internal string StatusMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Checks if the WSAPM service is already installed.
        /// </summary>
        /// <returns>The WSAPM service as ServiceController or null if the WSAPM service is not installed.</returns>
        private ServiceController GetWsapmService()
        {
            var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == WsapmConstants.ServiceName);
            this.serviceInstalled = service != null;
            return service;
        }

        /// <summary>
        /// Gets a value indicating if the WSAPM service is running.
        /// </summary>
        internal bool ServiceRunning
        {
            get
            {
                if (this.service != null)
                    return this.service.Status == ServiceControllerStatus.Running;
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets a value indicating if the WSAPM service is stopped.
        /// </summary>
        internal bool ServiceStopped
        {
            get
            {
                if (this.service != null)
                    return this.service.Status == ServiceControllerStatus.Stopped;
                else
                    return false;
            }
        }

        /// <summary>
        /// Starts the WSAPM service.
        /// </summary>
        /// <returns>True if the WSAPM service war started successfiully, otherwise false.</returns>
        internal bool StartService()
        {
            if (this.service != null)
            {
                try
                {
                    this.service.Start();
                    this.service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20));
                    this.StatusMessage = Wsapm.Resources.Wsapm.ServiceManager_StatusMessageMonitoring;
                    return true;
                }
                catch (Exception ex)
                {
                    this.StatusMessage = String.Format(Wsapm.Resources.Wsapm.ServiceManager_StatusMessageStartServiceError, ex.Message);
                    return false;
                }
            }
            else
            {
                this.StatusMessage = Wsapm.Resources.Wsapm.ServiceManager_StatusMessageStartServiceNotPresentError;
                return false;
            }
        }

        /// <summary>
        /// Stops the WSAPM service.
        /// </summary>
        /// <returns>True if the WSAPM service was stopped successfully, otherwise false.</returns>
        internal bool StopService()
        {
            if (this.service != null && this.service.CanStop)
            {
                try
                {
                    this.service.Stop();
                    this.service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));
                    this.StatusMessage = Wsapm.Resources.Wsapm.ServiceManager_StatusMessageStop;
                    return true;
                }
                catch (Exception ex)
                {
                    this.StatusMessage = String.Format(Wsapm.Resources.Wsapm.ServiceManager_StatusMessageStopServiceError, ex.Message);
                    return false;
                }
            }
            else
            {
                this.StatusMessage = Wsapm.Resources.Wsapm.ServiceManager_StatusMessageStopServiceNotPresentError;
                return false;
            }
        }

        #region IDisposable interface

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);   
        }

        #endregion IDisposable interface

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (this.service != null)
                {
                    this.service.Close();
                }
            }

            disposed = true;
        }
    }
}
