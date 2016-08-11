
namespace Wsapm.Wpf.Controls
{
    public class ProgramInformation
    {
        /// <summary>
        /// Creates a new instance of ProgramInformation.
        /// </summary>
        /// <param name="programName">The program's name.</param>
        /// <param name="programVersion">The program's version.</param>
        public ProgramInformation(string programName, string programVersion)
            : this(programName, programVersion, string.Empty)
        {

        }

        /// <summary>
        /// Creates a new instance of ProgramInformation.
        /// </summary>
        /// <param name="programName">The program's name.</param>
        /// <param name="programVersion">The program's version.</param>
        /// <param name="programModule">The affected program module.</param>
        public ProgramInformation(string programName, string programVersion, string programModule)
        {
            this.ProgramName = programName;
            this.ProgramVersion = programVersion;
            this.ProgramModule = programModule;
        }

        /// <summary>
        /// The program's name.
        /// </summary>
        public string ProgramName
        {
            get;
            set;
        }

        /// <summary>
        /// The program's version.
        /// </summary>
        public string ProgramVersion
        {
            get;
            set;
        }

        /// <summary>
        /// The affected module of the program.
        /// </summary>
        public string ProgramModule
        {
            get;
            set;
        }
    }
}
