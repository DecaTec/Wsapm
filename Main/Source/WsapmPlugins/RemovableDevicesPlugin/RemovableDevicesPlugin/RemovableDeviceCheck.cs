using System.IO;
using System.Linq;

namespace RemovableDevicesPlugin
{
    /// <summary>
    /// Class for checking for removable devices.
    /// </summary>
    internal static class RemovableDeviceCheck
    {
        /// <summary>
        /// Checks if any removable device is connected to the computer.
        /// </summary>
        /// <returns>True, if any removable device is connected to the computer, otherwise false.</returns>
        internal static bool IsRemovableDeviceConnected()
        {
            var removableDevices = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable);
            return removableDevices.Count() > 0;
        }
    }
}
