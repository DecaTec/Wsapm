
namespace Wsapm.Extensions
{
    public sealed class PluginManifest
    {
        public PluginManifest()
        {

        }

        public PluginManifest(string pluginName, string description, string authorName)
        {
            this.PluginName = pluginName;
            this.Description = description;
            this.AuthorName = authorName;
        }

        public string PluginName
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string AuthorName
        {
            get;
            set;
        }
    }
}
