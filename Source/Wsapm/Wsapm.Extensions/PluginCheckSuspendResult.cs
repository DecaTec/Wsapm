
namespace Wsapm.Extensions
{
    /// <summary>
    /// Class representing a suspend check result.
    /// </summary>
    public sealed class PluginCheckSuspendResult
    {
        /// <summary>
        /// Initializes a new PluginCheckSuspendResult with the given result and reason.
        /// </summary>
        /// <param name="suspendStandby">True if standby should be suspended, otherwise false.</param>
        /// <param name="reason">The reason for suspending/enabling standby.</param>
        public PluginCheckSuspendResult(bool suspendStandby, string reason)
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
    }
}
