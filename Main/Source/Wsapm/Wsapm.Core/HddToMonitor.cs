using System;
using System.Xml.Serialization;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a logical drive to monitor (for settings).
    /// </summary>
    [Serializable]
    public sealed class HddToMonitor
    {
        /// <summary>
        /// Gets ot sets the drive's name.
        /// </summary>
        public string Drive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the HDD load check is enabled.
        /// </summary>
        public bool EnableCheckHddLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Hdd load in bytes/s.
        /// </summary>
        public float HddLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a formatted string ('xx KBit/s') for the Hdd load.
        /// </summary>
        [XmlIgnore]
        public string HddLoadFormatted
        {
            get
            {
                if (this.EnableCheckHddLoad)
                    return string.Format("{0} KB/s", this.HddLoad);
                else
                    return "-";
            }
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            HddToMonitor other = obj as HddToMonitor;
            return this.Equals(other);
        }

        public bool Equals(HddToMonitor other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(HddToMonitor a, HddToMonitor b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(HddToMonitor a, HddToMonitor b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this.Drive.GetHashCode() ^ this.EnableCheckHddLoad.GetHashCode() ^ this.HddLoad.GetHashCode();
        }

        #endregion Equals

        /// <summary>
        /// Copies the object.
        /// </summary>
        /// <returns></returns>
        public HddToMonitor Copy()
        {
            HddToMonitor hdd = new HddToMonitor();
            hdd.EnableCheckHddLoad = this.EnableCheckHddLoad;
            hdd.HddLoad = this.HddLoad;
            hdd.Drive = this.Drive;            
            return hdd;
        }
    }
}
