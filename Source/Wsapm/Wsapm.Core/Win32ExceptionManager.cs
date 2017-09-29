using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for handling Win32Excptions.
    /// </summary>
    public static class Win32ExceptionManager
    {
        /// <summary>
        /// Gets the last thrown Win32Exception.
        /// </summary>
        /// <returns>The last thrown Win32Exception or null if there was no Win32Exception.</returns>
        public static Win32Exception GetWin32Exception()
        {
            var error = Marshal.GetLastWin32Error();

            try
            {
                if (error != 0)
                    throw new Win32Exception(error);
            }
            catch (Win32Exception ex)
            {
                return ex;
            }

            return null;
        }
    }
}
