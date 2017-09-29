using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for server side exceptions.
    /// </summary>
    [Serializable]
    public class WsapmException : Exception
    {
        /// <summary>
        /// Initializes a new instance of WolServerException.
        /// </summary>
        public WsapmException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of WolServerException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public WsapmException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of WolServerException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public WsapmException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
