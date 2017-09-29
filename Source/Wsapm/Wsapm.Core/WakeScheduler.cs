using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Wsapm.Core
{
    /// <summary>
    /// Scheduler class for recurring wake events.
    /// </summary>
    [Serializable]
    public sealed class WakeScheduler
    {
        /// <summary>
        /// Creates a new instance of Scheduler.
        /// </summary>
        public WakeScheduler() : this(DateTime.Now)
        {

        }

        /// <summary>
        /// Creates a new instance of Scheduler.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        public WakeScheduler(DateTime dueTime)
        {
            this.DueTime = dueTime;
            this.EnableRepeat = false;
            this.EnableEndTime = false;
            this.EndTime = DateTime.Now;
            this.StartProgramsAfterWake = new List<ProgramStart>();
        }

        /// <summary>
        /// Creates a new instance of Scheduler.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="repeatAfter">The interval the schedule should be prepared.</param>
        public WakeScheduler(DateTime dueTime, TimeSpan repeatAfter) : this(dueTime)
        {
            this.EnableRepeat = true;
            this.RepeatAfter = repeatAfter;
        }

        /// <summary>
        /// Creates a new instance of Scheduler.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="repeatAfter">The interval the schedule should be prepared.</param>
        /// <param name="endTime">The time the schedule should be ended.</param>
        public WakeScheduler(DateTime dueTime, TimeSpan repeatAfter, DateTime endTime) : this(dueTime, repeatAfter)
        {
            this.EnableEndTime = true;
            this.EndTime = endTime;
        }

        /// <summary>
        /// Gets or sets a value indicating if the wake scheduler should be active.
        /// </summary>
        public bool EnableWakeScheduler
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
        /// Gets or sets the end time.
        /// </summary>
        public DateTime EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value if the Wake call is repeated or nor.
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
        /// Gets or sets a value indicating if the specified programs should be started after wake.
        /// </summary>
        public bool EnableStartProgramsAfterWake
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the programs which should be started after wake.
        /// </summary>
        public List<ProgramStart> StartProgramsAfterWake
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the programs should be started when the system is already running, i.e. the system was not woken by a wake timer.
        /// </summary>
        public bool StartProgramsWhenSystemIsAlreadyRunning
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
        /// Gets a value indicating if programs should be started after wake.
        /// </summary>
        [XmlIgnore]
        public bool StartProgramsAfterWakeActive
        {
            get
            {
                return this.EnableStartProgramsAfterWake && this.StartProgramsAfterWake != null && this.StartProgramsAfterWake.Count > 0;
            }          
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
                    var tmp = this.DueTime;

                    while (tmp < DateTime.Now)
                    {
                        tmp = tmp + this.RepeatAfter;

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
                    if (DateTimeTools.DateTimeIsPassedBy(this.DueTime))
                        return null;
                    else
                        return this.DueTime;
                }
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
                if (!WakeSchedulerExpired)
                    return this.NextDueTime.Value.ToString("F");
                else
                    return Resources.Wsapm_Core.WakeScheduler_WakeSchedulerExpired;
            }
        }

        /// <summary>
        /// Gets a value indicating if the WakeScheduler is already expired.
        /// </summary>
        [XmlIgnore]
        public bool WakeSchedulerExpired
        {
            get
            {
                if (!this.NextDueTime.HasValue)
                    return true;
                else
                    return DateTimeTools.DateTimeIsPassedBy(this.NextDueTime.Value);
            }
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            WakeScheduler other = obj as WakeScheduler;
            return this.Equals(other);
        }

        public bool Equals(WakeScheduler other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(WakeScheduler a, WakeScheduler b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(WakeScheduler a, WakeScheduler b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            var hashCode = this.DueTime.GetHashCode() ^ this.EnableEndTime.GetHashCode() ^ this.EnableRepeat.GetHashCode()
                ^ this.EnableStartProgramsAfterWake.GetHashCode() ^ this.EnableWakeScheduler.GetHashCode() ^ this.EndTime.GetHashCode()
                ^ this.RepeateAfterTicks.GetHashCode();

            if (this.StartProgramsAfterWake != null)
                hashCode ^= this.StartProgramsAfterWake.GetHashCode();

            return hashCode;
        }

        #endregion Equals

        /// <summary>
        /// Copies the object.
        /// </summary>
        /// <returns></returns>
        public WakeScheduler Copy()
        {
            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj as WakeScheduler;
        }
    }
}
