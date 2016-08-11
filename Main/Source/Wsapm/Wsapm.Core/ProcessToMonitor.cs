using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a process.
    /// </summary>
    [Serializable]
    public sealed class ProcessToMonitor
    {
        /// <summary>
        /// Initializes a new instance of Process.
        /// </summary>
        public ProcessToMonitor()
        {

        }

        /// <summary>
        /// Initializes a new instance of Process.
        /// </summary>
        /// <param name="processName"></param>
        public ProcessToMonitor(string processName)
        {
            this.ProcessName = processName;
        }

        /// <summary>
        /// Gets or sets the process name.
        /// </summary>
        public string ProcessName
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the string representation of the Process.
        /// </summary>
        /// <returns>The string representation of the Process.</returns>
        public override string ToString()
        {
            return this.ProcessName;
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ProcessToMonitor other = obj as ProcessToMonitor;
            return this.Equals(other);
        }

        public bool Equals(ProcessToMonitor other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(ProcessToMonitor a, ProcessToMonitor b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(ProcessToMonitor a, ProcessToMonitor b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            if (this.ProcessName == null)
                return 0;

            return this.ProcessName.GetHashCode();
        }

        #endregion Equals

        /// <summary>
        /// Copies the object.
        /// </summary>
        /// <returns></returns>
        public Service Copy()
        {
            Service service = new Service(this.ProcessName);
            return service;
        }
    }
}
