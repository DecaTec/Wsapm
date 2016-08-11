using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for EventArgs with additional data.
    /// </summary>
    /// <typeparam name="T">The type of the additional data.</typeparam>
    public sealed class DataEventArgs<T> : EventArgs
    {
        private readonly T eventData;

        /// <summary>
        /// Creates a new instance of DataEventArgs.
        /// </summary>
        /// <param name="data">The event data.</param>
        public DataEventArgs(T data)
        {
            eventData = data;
        }

        /// <summary>
        /// Gets the additional event data.
        /// </summary>
        public T Data
        {
            get
            {
                return this.eventData;
            }
        }
    }
}
