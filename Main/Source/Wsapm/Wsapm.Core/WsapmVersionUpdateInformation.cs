using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for WSAPM version update information.
    /// </summary>
    [Serializable]
    public sealed class WsapmVersionUpdateInformation
    {
        /// <summary>
        /// Gets or sets the current version as string.
        /// </summary>
        public string CurrentVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the download URI as string for the current version.
        /// </summary>
        public string DownloadUri
        {
            get;
            set;
        }
    }
}
