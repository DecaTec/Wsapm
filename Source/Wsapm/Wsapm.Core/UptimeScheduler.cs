using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace Wsapm.Core
{
    /// <summary>
    /// Scheduler class for recurring wake events.
    /// </summary>
    [Serializable]
    public sealed class UptimeScheduler
    {
        /// <summary>
        /// Creates a new instance of UptimeScheduler.
        /// </summary>
        public UptimeScheduler() : this(DateTime.Now)
        {

        }

        /// <summary>
        /// Creates a new instance of UptimeScheduler.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        public UptimeScheduler(DateTime dueTime)
        {
            this.DueTime = dueTime;
            this.EnableRepeat = false;
            this.EnableEndTime = false;
            this.EndTime = DateTime.Now;
        }

        /// <summary>
        /// Creates a new instance of UptimeScheduler.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="repeatAfter">The interval the schedule should be prepared.</param>
        public UptimeScheduler(DateTime dueTime, TimeSpan repeatAfter) : this(dueTime)
        {
            this.EnableRepeat = true;
            this.RepeatAfter = repeatAfter;
        }

        /// <summary>
        /// Creates a new instance of UptimeScheduler.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="repeatAfter">The interval the schedule should be prepared.</param>
        /// <param name="endTime">The time the schedule should be ended.</param>
        public UptimeScheduler(DateTime dueTime, TimeSpan repeatAfter, DateTime endTime) : this(dueTime, repeatAfter)
        {
            this.EnableEndTime = true;
            this.EndTime = endTime;
        }

        /// <summary>
        /// Gets or sets a value indicating if the uptime scheduler should be active.
        /// </summary>
        public bool EnableUptimeScheduler
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the due time.
        /// </summary>
        public DateTime DueTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value if the WakeScheduler has an end time or not.
        /// </summary>
        public bool EnableEndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the duration of the uptime.
        /// </summary>
        [XmlIgnore]
        public TimeSpan Duration
        {
            get;
            set;
        }

        /// <summary>
        /// Duration as ticks.
        /// </summary>
        /// <remarks>This property is needed only for serialization.</remarks>
        [XmlElement("Duration")]
        public long DurationTicks
        {
            get
            {
                return Duration.Ticks;
            }
            set
            {
                Duration = new TimeSpan(value);
            }
        }

        /// <summary>
        /// Gets or sets a value if the uptime is repeated or nor.
        /// </summary>
        public bool EnableRepeat
        {
            get;
            set;
        }

        /// <summary>
        /// Get or sets the repeat interval.
        /// </summary>
        [XmlIgnore]
        public TimeSpan RepeatAfter
        {
            get;
            set;
        }

        /// <summary>
        /// Repeat interval as ticks.
        /// </summary>
        /// <remarks>This property is needed only for serialization.</remarks>
        [XmlElement("RepeatAfter")]
        public long RepeateAfterTicks
        {
            get
            {
                return RepeatAfter.Ticks;
            }
            set
            {
                RepeatAfter = new TimeSpan(value);
            }
        }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        public DateTime EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the DateTime on which the schedule should be run the next time.
        /// When the schedule expired, null is returned, i.e. there is no next due time of this WakeScheduler.
        /// </summary>
        [XmlIgnore]
        public DateTime? NextDueTime
        {
            get
            {
                if (this.EnableRepeat && this.RepeatAfter != null)
                {
                    // Should be repeated.
                    var now = DateTime.Now;
                    var tmp = this.DueTime;
                    var tmpWithDuration = tmp.Add(this.Duration);

                    while (tmp < now && now > tmpWithDuration)
                    {
                        tmp = tmp + this.RepeatAfter;
                        tmpWithDuration = tmp.Add(this.Duration);

                        if (this.EnableEndTime && this.EndTime != null)
                        {
                            // Has an end time.
                            if (tmp > this.EndTime)
                                return tmp - this.RepeatAfter;
                        }
                    }

                    return tmp;
                }
                else
                {
                    // Should not be repeated.
                    if (DateTimeTools.DateTimeIsPassedBy(this.DueTime.Add(this.Duration)))
                        return null;
                    else
                        return this.DueTime;
                }
            }
        }

        /// <summary>
        /// Gets or sets the next due time (with duration added).
        /// </summary>
        public DateTime? NextDueTimeWithDuration
        {
            get
            {
                var nextDueTime = this.NextDueTime;

                if (!nextDueTime.HasValue)
                    return null;
                else
                    return nextDueTime.Value.Add(this.Duration);
            }
        }

        /// <summary>
        /// Gets the DateTime on which the schedule should be run the next time (formatted for UI).
        /// </summary>
        [XmlIgnore]
        public string NextDueTimeFormatted
        {
            get
            {
                if (!UptimeSchedulerExpired)
                    return this.NextDueTime.Value.ToString("F");
                else
                    return Resources.Wsapm_Core.UptimeScheduler_UptimeSchedulerExpired;
            }
        }

        /// <summary>
        /// Gets a value indicating if the WakeScheduler is already expired.
        /// </summary>
        [XmlIgnore]
        public bool UptimeSchedulerExpired
        {
            get
            {
                if (!this.NextDueTimeWithDuration.HasValue)
                    return true;
                else
                    return DateTimeTools.DateTimeIsPassedBy(this.NextDueTime.Value.Add(this.Duration));
            }
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            UptimeScheduler other = obj as UptimeScheduler;
            return this.Equals(other);
        }

        public bool Equals(UptimeScheduler other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(UptimeScheduler a, UptimeScheduler b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(UptimeScheduler a, UptimeScheduler b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            var hashCode = this.DueTime.GetHashCode() ^ this.EnableEndTime.GetHashCode() ^ this.EnableRepeat.GetHashCode()
                ^ this.Duration.GetHashCode() ^ this.EnableUptimeScheduler.GetHashCode() ^ this.EndTime.GetHashCode()
                ^ this.RepeateAfterTicks.GetHashCode();

            return hashCode;
        }

        #endregion Equals

        /// <summary>
        /// Copies the object.
        /// </summary>
        /// <returns></returns>
        public UptimeScheduler Copy()
        {
            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj as UptimeScheduler;
        }
    }
}
