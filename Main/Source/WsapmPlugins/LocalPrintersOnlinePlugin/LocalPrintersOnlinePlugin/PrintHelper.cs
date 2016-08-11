using System.Management;
using System.Linq;

namespace LocalPrintersOnlinePlugin
{
    /// <summary>
    /// Helper class for managing printers.
    /// </summary>
    public static class PrintHelper
    {
        /// <summary>
        /// Gets a string array containing the names of the installed local printers.
        /// </summary>
        /// <returns>A string array containing the names of the installed local printers. If no local printers are installed, null is returned.</returns>
        public static string[] GetInstalledLocalPrinters()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Printer WHERE Local = true");
            var searcher = new ManagementObjectSearcher(query);

            var printerNames = from ManagementObject printer in searcher.Get()
                               select printer["Name"].ToString();

            return printerNames.ToArray();
        }

        /// <summary>
        /// Gets the name of the local default printer installed on the computer.
        /// </summary>
        /// <returns>The name of the local default printer installed on the computer or an empty string if no default printer is installed or the default printer is not a local printer.</returns>
        public static string GetDefaultLocalPrinterName()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Printer WHERE Default = true AND Local = true");
            var searcher = new ManagementObjectSearcher(query);

            var defaultPrinterName = from ManagementObject printer in searcher.Get()
                                     select printer["Name"].ToString();

            return defaultPrinterName.FirstOrDefault();
        }

        /// <summary>
        /// Checks if the specified printer is online.
        /// </summary>
        /// <param name="printerName">The name of the printer.</param>
        /// <returns>True, if the specified printer is online, otherwise false. Also returns false if there is no printer installed at all.</returns>
        /// <remarks>This method does not work case sensitive.</remarks>
        public static bool IsPrinterOnline(string printerName)
        {
            if (string.IsNullOrEmpty(printerName))
                return false;

            var query = new ObjectQuery("SELECT * FROM Win32_Printer");
            var searcher = new ManagementObjectSearcher(query);

            var online = from ManagementObject printer in searcher.Get()
                         where printer["Name"].ToString().ToLower() == printerName.ToLower()
                         select !(bool)printer["WorkOffline"];

            return online.FirstOrDefault();
        }
    }
}
