using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class to determine the OS version
    /// </summary>
    public static class OsVersionTools
    {
        /// <summary>
        /// Determines if the OS is Windows XP or later.
        /// </summary>
        /// <returns>True if the OS is Windows XP or later, otherwise false.</returns>
        public static bool IsWindowsXPOrLater()
        {
            var version = Environment.OSVersion.Version;
            return version >= new Version(5, 1);
        }

        /// <summary>
        ///  Determines if the OS is Windows Vista or later.
        /// </summary>
        /// <returns>True if the OS is Windows Vista or later, otherwise false.</returns>
        public static bool IsWindowsVistaOrLater()
        {
            var version = Environment.OSVersion.Version;
            return version >= new Version(6, 0);
        }

        /// <summary>
        ///  Determines if the OS is Windows 7 or later.
        /// </summary>
        /// <returns>True if the OS is Windows 7 or later, otherwise false.</returns>
        public static bool IsWindows7OrLater()
        {
            var version = Environment.OSVersion.Version;
            return version >= new Version(6, 1);
        }

        /// <summary>
        ///  Determines if the OS is Windows 8 or later.
        /// </summary>
        /// <returns>True if the OS is Windows 8 or later, otherwise false.</returns>
        public static bool IsWindows8OrLater()
        {
            var version = Environment.OSVersion.Version;
            return version >= new Version(6, 2);
        }
    }
}
