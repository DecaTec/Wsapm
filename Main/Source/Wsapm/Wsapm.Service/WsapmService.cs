using System.Diagnostics;
using System.ServiceProcess;
using Wsapm.Core;

namespace Wsapm.Service
{
    public partial class WsapmService : ServiceBase
    {
        private WsapmManager wsapmManager;

        /// <summary>
        /// Creates a new instance of WsapmService.
        /// </summary>
        public WsapmService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the instance of WsapmManager to work with.
        /// </summary>
        private WsapmManager WsapmManager
        {
            get
            {
                if (this.wsapmManager == null)
                    this.wsapmManager = new WsapmManager();

                return this.wsapmManager;
            }
        }

        #region Overrides

        /// <summary>
        /// OnStart method of service. Gets called when the service is started.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            this.WsapmManager.StartMonitoring();
        }

        /// <summary>
        /// OnStop method of service. Gets called when service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            this.WsapmManager.StopMonitoring();

            // Access via field...the only place to do it that way!
            this.wsapmManager = null;

            base.OnStop();
        }

        /// <summary>
        /// OnPause method of service. Gets called when the service is paused.
        /// </summary>
        protected override void OnPause()
        {
            this.WsapmManager.PauseMonitoring();

            base.OnPause();
        }

        /// <summary>
        /// OnContinue method of service. Gets called when the service is continued.
        /// </summary>
        protected override void OnContinue()
        {
            base.OnContinue();

            this.WsapmManager.ContinueMonitoring();
        }

        /// <summary>
        /// OnShutdown method of service. Gets called when the service is shut down.
        /// </summary>
        protected override void OnShutdown()
        {
            this.WsapmManager.StopMonitoring();

            base.OnShutdown();
        }

        /// <summary>
        /// OnPowerEvent method of service. Gets called when a power event is raised.
        /// </summary>
        /// <param name="powerStatus"></param>
        /// <returns></returns>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            base.OnPowerEvent(powerStatus);

            this.WsapmManager.OnPowerEvent(powerStatus);

            return base.OnPowerEvent(powerStatus);
        }

        #endregion Overrides

        #region Debug

        /// <summary>
        /// Represents an entry point for debugging service. Place a call to this method when the debugger should be forced to hit that break point.
        /// </summary>
        /// <remarks>Only available when sources were build in debug mode.</remarks>
        [Conditional("DEBUG")]
        internal static void DebugMode()
        {
            if (!Debugger.IsAttached)
                Debugger.Launch();

            Debugger.Break();
        }

        #endregion Debug
    }
}
