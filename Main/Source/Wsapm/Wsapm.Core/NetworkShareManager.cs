using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Wsapm.Core
{
    /// <summary>
    /// Class to manage network shares.
    /// </summary>
    public static class NetworkShareManager
    {
        private const int MAX_PREFERRED_LENGTH = -1;

        /// <summary>
        /// Gets all files opened in a network share on the current machine.
        /// </summary>
        /// <returns>A string array containing the full path of all opened file in a network share.</returns>
        public static string[] GetFilesOpenedInNetworkShare()
        {
            return GetResourcesOpenedInNetworkShare(NetworkShareAccessType.Files);
        }

         /// <summary>
        /// Gets all directories opened in a network share on the current machine.
        /// </summary>
        /// <returns>A string array containing the full path of all opened directories in a network share.</returns>
        public static string[] GetDirectoriesOpenedInNetworkShare()
        {
            return GetResourcesOpenedInNetworkShare(NetworkShareAccessType.Directories);
        }

        /// <summary>
        /// Gets all resources opened in a network share on the current machine.
        /// </summary>
        /// <returns>A string array containing the full path of all opened resources in a network share.</returns>
        public static string[] GetResourcesOpenedInNetworkShare()
        {
            return GetResourcesOpenedInNetworkShare(NetworkShareAccessType.Directories | NetworkShareAccessType.Files);
        }

        /// <summary>
        /// Gets all resources opened in a network share on the current machine.
        /// </summary>
        /// <returns>A string array containing the full path of all opened resources in a network share.</returns>
        public static string[] GetResourcesOpenedInNetworkShare(NetworkShareAccessType networkShareAccessType)
        {
            IList<string> openedResources = new List<string>();

            try
            {
                var openedResourcesFileInfo = GetResourcesOpenedInNetworkShareFileInfo3();

                if (openedResourcesFileInfo != null)
                {
                    for (int i = 0; i < openedResourcesFileInfo.Length; i++)
                    {
                        // Use Directory.Exists to find out if the opened resource is a directory or file.
                        bool isDirectory = Directory.Exists(openedResourcesFileInfo[i].fi3_pathname);

                        if ((networkShareAccessType & NetworkShareAccessType.Files) == NetworkShareAccessType.Files)
                        {
                            if (!isDirectory)
                            {
                                // It is a file.
                                openedResources.Add(openedResourcesFileInfo[i].fi3_pathname);
                            }
                        }
                        
                        if((networkShareAccessType & NetworkShareAccessType.Directories) == NetworkShareAccessType.Directories)
                        {
                            if (isDirectory)
                            {
                                // It is a folder.
                                openedResources.Add(openedResourcesFileInfo[i].fi3_pathname);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return openedResources.ToArray<string>();
        }

        private static FILE_INFO_3[] GetResourcesOpenedInNetworkShareFileInfo3()
        {
            IList<FILE_INFO_3> openedResources = new List<FILE_INFO_3>();
            int entriesRead;
            int totalEntries;
            IntPtr buffer = IntPtr.Zero;

            try
            {
                FILE_INFO_3 fileInfo3 = new FILE_INFO_3();
                int status = NativeMethods.NetFileEnum(null, null, null, 3, ref buffer, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, IntPtr.Zero);

                if (status != 0)
                {
                    var ex = Win32ExceptionManager.GetWin32Exception();
                    throw ex;
                }

                for (int i = 0; i < entriesRead; i++)
                {
                    IntPtr iPtr = new IntPtr(buffer.ToInt32() + (i * Marshal.SizeOf(fileInfo3)));
                    fileInfo3 = (FILE_INFO_3)Marshal.PtrToStructure(iPtr, typeof(FILE_INFO_3));

                    // Only add files/folder where the permission is at least 'read'.
                    if(fileInfo3.fi3_permission != 0)
                        openedResources.Add(fileInfo3);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                NativeMethods.NetApiBufferFree(buffer);
            }

            return openedResources.ToArray<FILE_INFO_3>();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct FILE_INFO_2
        {
            public int fi2_id;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct FILE_INFO_3
        {
            public int fi3_id;
            public int fi3_permission;
            public int fi3_num_locks;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string fi3_pathname;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string fi3_username;
        }

        /// <summary>
        /// Class for native methods.
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int NetFileEnum(string servername, string basepath, string username, int level, ref IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, IntPtr resume_handle);

            [DllImport("Netapi32.dll", SetLastError = true)]
            internal static extern int NetApiBufferFree(IntPtr Buffer);
        }
    }

    /// <summary>
    /// Enum defining the network share access type.
    /// </summary>
    [Flags]
    public enum NetworkShareAccessType
    {
        /// <summary>
        /// Files.
        /// </summary>
        Files = 1,
        /// <summary>
        /// Directories.
        /// </summary>
        Directories = 2
    }
}
