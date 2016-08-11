using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using Wsapm.Wpf.Controls.SimpleFileDownloader;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for the update mechanism.
    /// </summary>
    public sealed class WsapmUpdateManager
    {
        private static readonly Uri VersionUri = new Uri(@"http://decatec.de/ext/applications/wsapm/updater/WsapmVersionInformation/WsapmCurrentVersion.xml");
        private static readonly Uri VersionUriHttps = new Uri(@"https://decatec.de/ext/applications/wsapm/updater/WsapmVersionInformation/WsapmCurrentVersion.xml");

        /// <summary>
        /// Checks for updates for WSAPM.
        /// </summary>
        /// <returns>Trie if updates are available, otherwise false. Returns null if no internet connection could be established.</returns>
        public bool? CheckForUpdates()
        {
            var updateInformation = GetCurrentVersionInformation();

            if (updateInformation == null)
            {
                // We're offline.
                return null;
            }

            this.UpdateInformation = updateInformation;

            if (Version.Parse(updateInformation.CurrentVersion) > VersionInformation.Version)
                return true;

            return false;
        }

        /// <summary>
        /// Updates WSAPM to the current version.
        /// </summary>
        /// <returns>True if an update is available, otherwise false.</returns>
        public bool UpdateWsapm()
        {
            var updateStatus = CheckForUpdates();

            if (!updateStatus.HasValue || !updateStatus.Value)
                return false; // No update available.

            var fileName = SimpleFileDownloader.Download(this.UpdateInformation.DownloadUri, true, true);

            if (!string.IsNullOrEmpty(fileName))
            {
                // Start update...
                Process.Start(fileName);

                // ...and exit application.
                Environment.Exit(0);
                return true;
            }
            else
            {
                // Something went wrong while downloading update.
                return false;
            }
        }

        /// <summary>
        /// Gets the update information after an update check.
        /// </summary>
        public WsapmVersionUpdateInformation UpdateInformation
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets current version information,
        /// </summary>
        /// <returns>The VersionUpdateInformation of the current WSAPM version. Null if retrieving of the information fails.</returns>
        private static WsapmVersionUpdateInformation GetCurrentVersionInformation()
        {
            var versionInfo = WsapmUpdateManager.GetCurrentVersionInformation(WsapmUpdateManager.VersionUriHttps);

            if(versionInfo == null)
            {
                // HTTPS failed -> try HTTP.
                versionInfo = WsapmUpdateManager.GetCurrentVersionInformation(WsapmUpdateManager.VersionUri);
            }

            return versionInfo;           
        }

        private static WsapmVersionUpdateInformation GetCurrentVersionInformation(Uri uri)
        {
            try
            {
                WebRequest request = WebRequest.Create(uri);
                request.Timeout = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = request.Credentials;
                WebResponse response = (WebResponse)request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    var serializer = new XmlSerializer(typeof(WsapmVersionUpdateInformation));
                    WsapmVersionUpdateInformation info = serializer.Deserialize(stream) as WsapmVersionUpdateInformation;
                    serializer = null;
                    return info;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
