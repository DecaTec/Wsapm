using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Wsapm.Core
{
    public static class ProgramManager
    {
        /// <summary>
        /// Starts the programs specified.
        /// </summary>
        /// <param name="programsToStart">The programs to start.</param>
        /// <remarks>The programs are started in separate threads.</remarks>
        public static void StartPrograms(ProgramStart[] programsToStart)
        {
            if (programsToStart == null || programsToStart.Length == 0)
                return;

            foreach (var program in programsToStart)
            {
                Task.Factory.StartNew(() => StartProgram(program.FileName, program.Args));
            }
        }

        private static void StartProgram(string fileName, string args)
        {
            try
            {
                WsapmLog.Log.WriteLine(string.Format(Resources.Wsapm_Core.ProgramManager_StartProgram, fileName + " " + args), LogMode.Verbose);

                var psi = new ProcessStartInfo();             
                psi.FileName = fileName;
                psi.Arguments = args;
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(string.Format(Resources.Wsapm_Core.ProgramManager_FailedStartProgram, fileName), ex);
            }
        }
    }
}
