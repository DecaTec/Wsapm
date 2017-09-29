
using Wsapm.Extensions;
namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a suspend check result.
    /// </summary>
    public sealed class CheckSuspendResult
    {
        /// <summary>
        /// Initializes a new CheckSuspendResult with the given result and reason.
        /// </summary>
        /// <param name="suspendStandby">True if standby should be suspended, otherwise false.</param>
        /// <param name="reason">The reason for suspending/enabling standby.</param>
        public CheckSuspendResult(bool suspendStandby, string reason)
        {
            this.Reason = reason;
            this.SuspendStandby = suspendStandby;
        }

        /// <summary>
        /// Gets or sets the reason for suspending standby. String,Empty when standby should not be suspended.
        /// </summary>
        public string Reason
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the standby should be suspended.
        /// </summary>
        public bool SuspendStandby
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a CheckSuspendResult from a FromPluginCheckSuspendResult.
        /// </summary>
        /// <param name="pluginCheckSuspendResult">The PluginCheckSuspendResult from which to get a CheckSuspendResult.</param>
        /// <param name="pluginSource">The plugin which is the source of the PluginCheckSuspendResult.</param>
        /// <returns>A CheckSuspendResult representing a FromPluginCheckSuspendResult.</returns>
        public static CheckSuspendResult FromPluginCheckSuspendResult(PluginCheckSuspendResult pluginCheckSuspendResult, WsapmPluginBase pluginSource)
        {
            var result = new CheckSuspendResult(false, string.Empty);

            if (pluginCheckSuspendResult == null)
                return result;

            result.SuspendStandby = pluginCheckSuspendResult.SuspendStandby;

            if(pluginSource != null)
                result.Reason = pluginCheckSuspendResult.Reason + " [" + pluginSource.PluginAttribute.Name + "]";

            return result;
        }
    }
}
