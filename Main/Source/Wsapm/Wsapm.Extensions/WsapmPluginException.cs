using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsapm.Extensions
{
    /// <summary>
    /// General exception class for plugins of Windows Server Advanced Power Management.
    /// </summary>
    [Serializable]
    public class WsapmPluginException : Exception
    {
        /// <summary>
        /// Initializes a new instance of WsapmPluginException.
        /// </summary>
        public WsapmPluginException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of WsapmPluginException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public WsapmPluginException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of WsapmPluginException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public WsapmPluginException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
