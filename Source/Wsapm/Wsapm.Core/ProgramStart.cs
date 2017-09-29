using System;
using System.Text;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a program to start.
    /// </summary>
    [Serializable]
    public sealed class ProgramStart
    {
        /// <summary>
        /// Creates a new instance of ProgramStart.
        /// </summary>
        public ProgramStart()
        {
        }

        /// <summary>
        /// Creates a new instance of ProgramStart.
        /// </summary>
        /// <param name="fileName">The file name of the program to start.</param>
        public ProgramStart(string fileName) : this(fileName, string.Empty)
        {
        }

        /// <summary>
        /// Creates a new instance of ProgramStart.
        /// </summary>
        /// <param name="fileName">The file name of the program to start.</param>
        /// <param name="args">The arguments the program should be started with.</param>
        public ProgramStart(string fileName, string args)
        {
            this.FileName = fileName;
            this.Args = args;
        }

        /// <summary>
        /// Gets or sets the file name of the program.
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the arguments the program should be started with.
        /// </summary>
        public string Args
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the string representation of the ProgramStart.
        /// </summary>
        /// <returns>The string representation of the ProgramStart.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(4);

            if (!string.IsNullOrEmpty(this.FileName))
                sb.Append(this.FileName);

            if (!string.IsNullOrEmpty(this.Args))
            {
                sb.Append(" ");
                sb.Append(this.Args);
            }

            return sb.ToString();
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ProgramStart other = obj as ProgramStart;
            return this.Equals(other);
        }

        public bool Equals(ProgramStart other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(ProgramStart a, ProgramStart b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(ProgramStart a, ProgramStart b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            if (this.FileName == null && this.Args == null)
                return 0;
            else if (this.FileName == null)
                return this.Args.GetHashCode();
            else if (this.Args == null)
                return this.FileName.ToLower().GetHashCode();
            else
                return this.FileName.ToLower().GetHashCode() ^ this.Args.GetHashCode();
        }

        #endregion Equals

        /// <summary>
        /// Copies the object.
        /// </summary>
        /// <returns></returns>
        public ProgramStart Copy()
        {
            ProgramStart ps = new ProgramStart();
            ps.Args = this.Args;
            ps.FileName = this.FileName;
            return ps;
        }
    }
}
