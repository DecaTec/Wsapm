using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for checking files opened over a network.
    /// </summary>
    public sealed class NetworkResourcesCheck : IStandbyCheck
    {
        private static NetworkResourcesCheck instance;

        /// <summary>
        /// Initializes a new instance of NetworkFilesCheck.
        /// </summary>
        private NetworkResourcesCheck()
        {
        }

        /// <summary>
        /// Gets the instance of NetworkResourcesCheck.
        /// </summary>
        public static NetworkResourcesCheck Instance
        {
            get
            {
                if (instance == null)
                    instance = new NetworkResourcesCheck();

                return instance;
            }
        }

        private CheckSuspendResult CheckNetworkFileAccess(WsapmSettings settings)
        {
            if (!settings.EnableCheckNetworkResourceAccess)
                return new CheckSuspendResult(false, string.Empty);

            string[] openedResources = null;

            if ((settings.CheckNetworkResourcesType & NetworkShareAccessType.Files) == NetworkShareAccessType.Files)
            {
                // Only check files.
                openedResources = NetworkShareManager.GetFilesOpenedInNetworkShare();
            }
            else if ((settings.CheckNetworkResourcesType & NetworkShareAccessType.Files) == NetworkShareAccessType.Directories)
            {
                // Only check directories.
                openedResources = NetworkShareManager.GetDirectoriesOpenedInNetworkShare();
            }
            else
            {
                // Check files and directories.
                openedResources = NetworkShareManager.GetResourcesOpenedInNetworkShare();
            }

            if (openedResources != null && openedResources.Length > 0)
                return new CheckSuspendResult(true, String.Format(Resources.Wsapm_Core.NetworkResourcesCheck_NetworkResourcesCheckReason, openedResources[0])); // Only take first file for reason.
            else
                return new CheckSuspendResult(false, string.Empty);
        }

        #region IStandbyCheck members

        /// <summary>
        /// Checks if the standby should be suspended.
        /// </summary>
        /// <param name="settings">The WsapmSettings to use.</param>
        /// <returns>The result as CheckSuspendResult.</returns>
        public CheckSuspendResult CheckStandby(WsapmSettings settings)
        {
            return this.CheckNetworkFileAccess(settings);
        }

        #endregion IStandbyCheck members
    }
}
