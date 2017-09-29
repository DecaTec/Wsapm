using System;
using System.Net;

namespace Wsapm.Wpf.Controls.SimpleFileDownloader
{
    /// <summary>
    /// Class for web related tools.
    /// </summary>
    public static class WebTools
    {
        /// <summary>
        /// Checks if a web resource exists.
        /// </summary>
        /// <param name="webResource">The web resource to check as string.</param>
        /// <returns>True if the web resource exists, otherwise false.</returns>
        public static bool WebResourceExists(string webResource)
        {
            try
            {
                Uri uri = new Uri(webResource);
                return WebResourceExists(uri);
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a web resource exists.
        /// </summary>
        /// <param name="webResource">The web resource to check as URI.</param>
        /// <returns>True if the web resource exists, otherwise false.</returns>
        public static bool WebResourceExists(Uri webResourceUri)
        {
            WebRequest request = WebRequest.Create(webResourceUri);
            request.Method = "HEAD";
            WebResponse response = null;

            try
            {
                response = request.GetResponse();
            }
            catch (WebException)
            {
                return false;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            return true;
        }
    }
}
