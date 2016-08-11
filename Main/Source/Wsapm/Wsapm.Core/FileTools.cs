using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for file/directory tools.
    /// </summary>
    public static class FileTools
    {
        #region File exists in environment variables path

        /// <summary>
        /// Gets a value indicating of an executable file (*.exe) is present in the system directory or in one of the directories 
        /// specified in the environment variables.
        /// </summary>
        /// <param name="filename">The file name (without path) to search for. If the file name does not contain an extension, '.exe' will be added automatically.</param>
        /// <returns>True if the file exists in either the system directory or in one of the directories specified in the environment variables. Otherwise false.</returns>
        public static bool ExecutableFileExistsInEnvironmentVariablePaths(string filename)
        {
            bool found = false;

            if (!filename.EndsWith(".exe"))
                filename += ".exe";

            // Search in system directory first.
            var fullpath = Environment.SystemDirectory;

            if (!fullpath.EndsWith("\\"))
                fullpath += "\\";

            fullpath += filename;

            if (File.Exists(fullpath))
                found = true; // Found in system directory. Do not continue search.
            else
            {
                // File does not exist directly, try to find it in the environment variables.
                ProcessStartInfo psi = new ProcessStartInfo();
                var envVariables = psi.EnvironmentVariables;

                foreach (DictionaryEntry variable in envVariables)
                {
                    string value = variable.Value as string;

                    if (string.IsNullOrEmpty(value) || !Directory.Exists(value))
                        continue;

                    fullpath = value;

                    if (!fullpath.EndsWith("\\"))
                        fullpath += "\\";

                    fullpath += filename;

                    if (File.Exists(fullpath))
                    {
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }

        #endregion File exists in environment variables path

        #region Files in use

        private const string SearchPatternAll = "*";

        /// <summary>
        /// Determines if the given directory contains at least one file which is currently in use.
        /// </summary>
        /// <param name="basePath">The directory to check for files in use.</param>
        /// <returns>True, if the given directory contains at least one file which is currently in use. Otherwise false.</returns>
        public static bool ContainsFilesInUse(string basePath)
        {
            return ContainsFilesInUse(basePath, true, SearchPatternAll);
        }

        /// <summary>
        /// Determines if the given directory contains at least one file which is currently in use.
        /// </summary>
        /// <param name="basePath">The directory to check for files in use.</param>
        /// <param name="includeSubDirectories">True, if subdirectories should be included in the search for files in use, otherwise false.</param>
        /// <returns>True, if the given directory contains at least one file which is currently in use. Otherwise false.</returns>
        public static bool ContainsFilesInUse(string basePath, bool includeSubDirectories)
        {
            return ContainsFilesInUse(basePath, includeSubDirectories, SearchPatternAll);
        }

        /// <summary>
        /// Determines if the given directory contains at least one file which is currently in use.
        /// </summary>
        /// <param name="basePath">The directory to check for files in use.</param>
        /// <param name="includeSubDirectories">True, if subdirectories should be included in the search for files in use, otherwise false.</param>
        /// <param name="searchPattern">The search string. The default pattern is "*", which includes all files.</param>
        /// <returns>True, if the given directory contains at least one file which is currently in use. Otherwise false.</returns>
        public static bool ContainsFilesInUse(string basePath, bool includeSubDirectories, string searchPattern)
        {
            if (!Directory.Exists(basePath))
                throw new IOException("Directory '" + basePath + "' does not exist");

            DirectoryInfo di = new DirectoryInfo(basePath);
            var searchOption = SearchOption.TopDirectoryOnly;

            if (includeSubDirectories)
                searchOption = SearchOption.AllDirectories;

            var files = di.EnumerateFiles(searchPattern, searchOption);

            foreach (var file in files)
            {
                if (IsFileLocked(file))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all files which are in use for a given directory.
        /// </summary>
        /// <param name="basePath">The directory to check for files in use.</param>
        /// <returns>A FileInfo array containing all files which are currently in use.</returns>
        public static FileInfo[] GetFilesInUse(string basePath)
        {
            return GetFilesInUse(basePath, true, SearchPatternAll);
        }

        /// <summary>
        /// Gets all files which are in use for a given directory.
        /// </summary>
        /// <param name="basePath">The directory to check for files in use.</param>
        /// <param name="includeSubdirectories">True, if subdirectories should be included in the search for files in use, otherwise false.</param>
        /// <returns>A FileInfo array containing all files which are currently in use.</returns>
        public static FileInfo[] GetFilesInUse(string basePath, bool includeSubdirectories)
        {
            return GetFilesInUse(basePath, includeSubdirectories, SearchPatternAll);
        }

        /// <summary>
        /// Gets all files which are in use for a given directory.
        /// </summary>
        /// <param name="basePath">The directory to check for files in use.</param>
        /// <param name="includeSubdirectories">True, if subdirectories should be included in the search for files in use, otherwise false.</param>
        /// <param name="searchPattern">The search string. The default pattern is "*", which includes all files.</param>
        /// <returns>A FileInfo array containing all files which are currently in use.</returns>
        public static FileInfo[] GetFilesInUse(string basePath, bool includeSubdirectories, string searchPattern)
        {
            if (!Directory.Exists(basePath))
                throw new IOException("Directory '" + basePath + "' does not exist");

            IList<FileInfo> openedFilesList = new List<FileInfo>();

            DirectoryInfo di = new DirectoryInfo(basePath);
            var files = di.EnumerateFiles(searchPattern, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (IsFileLocked(file))
                    openedFilesList.Add(file);
            }

            return openedFilesList.ToArray<FileInfo>();
        }

        /// <summary>
        /// Determines if a given file is in use, i.e. locked by another process.
        /// </summary>
        /// <param name="file">A FileInfo representing the file to check.</param>
        /// <returns>True if the file is currently locked, otherwise false.</returns>
        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }

        #endregion Files in use
    }
}
