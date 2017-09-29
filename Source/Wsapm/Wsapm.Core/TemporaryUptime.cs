using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a temporary uptime.
    /// </summary>
    [Serializable]
    public sealed class TemporaryUptime
    {
        /// <summary>
        /// Initializes a new instanced of TemporaryUptime.
        /// </summary>
        public TemporaryUptime()
        {

        }

        /// <summary>
        /// Initializes a new instanced of TemporaryUptime.
        /// </summary>
        /// <param name="upTimeUntil"></param>
        public TemporaryUptime(DateTime upTimeUntil)
        {
            this.TemporaryUptimeUntil = upTimeUntil;
        }

        /// <summary>
        /// Initializes a new instanced of TemporaryUptime.
        /// </summary>
        /// <param name="timeSpanUntil"></param>
        public TemporaryUptime(TimeSpan timeSpanUntil)
        {
            this.TemporaryUptimeUntil = DateTime.Now.Add(timeSpanUntil);
        }

        /// <summary>
        /// Initializes a new instanced of TemporaryUptime.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        public TemporaryUptime(int hours, int minutes) : this(new TimeSpan(hours, minutes, 0))
        {
           
        }

        /// <summary>
        /// Gets or sets the temporary uptime until.
        /// </summary>
        public DateTime TemporaryUptimeUntil
        {
            get;
            set;
        }
    }
}
