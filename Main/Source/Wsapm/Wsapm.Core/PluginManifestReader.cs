using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Wsapm.Extensions;

namespace Wsapm.Core
{
    public sealed class PluginManifestReader
    {
        public static PluginManifest GetPluginManifest(string path, string pluginInternalName)
        {
            try
            {
                CultureInfo ci = CultureInfo.CurrentUICulture;
                var languageCode = ci.TwoLetterISOLanguageName.ToLower();

                var xml = XDocument.Load(Path.Combine(path, "PluginManifest.xml"));

                // Query the data and write out a subset of contacts
                var query = from c in xml.Root.Descendants("DescriptionSet")
                            where ((string)c.Attribute("lang")).ToLower() == languageCode
                            select c;

                if (query.FirstOrDefault() == null)
                {
                    // Specified language not found -> try to find English language.
                    query = from c in xml.Root.Descendants("DescriptionSet")
                            where ((string)c.Attribute("lang")).ToLower() == "en" || ((string)c.Attribute("lang")).ToLower() == "en-us"
                            select c;
                }

                if (query.FirstOrDefault() == null)
                {
                    // English language not found -> just take the first description set.
                    query = from c in xml.Root.Descendants("DescriptionSet")
                            select c;
                }

                XElement item = query.FirstOrDefault();

                if (item != null)
                {
                    var pluginNamifest = new PluginManifest();

                    var queryPluginName = from i in item.Descendants("PluginName") select i.Value;
                    string pluginName = queryPluginName.First();

                    var queryDescription = from i in item.Descendants("Description") select i.Value;
                    string pluginDescription = queryDescription.First();

                    var queryAuthorName = from i in item.Descendants("AuthorName") select i.Value;
                    string authorName = queryAuthorName.First();

                    return new PluginManifest(pluginName, pluginDescription, authorName);
                }
            }
            catch (Exception)
            {
                throw;
                //throw new WsapmException(Resources.Wsapm_Core.PluginManifestReader_ManifestMissingError, ex);
            }

            throw new WsapmException(Resources.Wsapm_Core.PluginManifestReader_InvalidManifestError);
        }
    }
}
