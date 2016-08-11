
namespace Wsapm.Core
{
    /// <summary>
    /// Interface for all suspend check classes.
    /// </summary>
    public interface IStandbyCheck
    {
        /// <summary>
        /// Checks if the standby should be suspended.
        /// </summary>
        /// <param name="settings">The WsapmSettings to use.</param>
        /// <returns>The result as CheckSuspendResult.</returns>
        CheckSuspendResult CheckStandby(WsapmSettings settings);
    }
}
