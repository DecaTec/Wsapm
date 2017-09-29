using System.IO;
using System.Linq;

namespace Wsapm.Core
{
    public static class HddTools
    {
        /// <summary>
        /// Gets the available logical volume names.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAvailableLogicalVolumeNames()
        {
            return DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed).Select(x => x.Name).ToArray();            
        }

        public static string GetPerformanceCounterInstanceName(string logicalVolume)
        {
            if (char.IsLetter(logicalVolume[0]) && logicalVolume[1] == ':')
                return logicalVolume.Substring(0, 2);
            else
                return logicalVolume;
        }
    }
}
