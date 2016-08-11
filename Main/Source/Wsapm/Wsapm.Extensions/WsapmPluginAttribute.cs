using System;

namespace Wsapm.Extensions
{
    public sealed class WsapmPluginAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of WsapmPluginAttribute.
        /// </summary>
        /// <param name="name">The (internal) name of the plugin.</param>
        /// <param name="version">The plugin's version.</param>      
        /// <param name="pluginGuid">The plugin's GUID.</param>
        /// <remarks>Every plugin for Windows Server Advanced Power Management needs a GUID for identification purposes.
        /// Only this GUID identifies a plugin clearly. Even if two plugins have the same name, they are distinctive through the individual GUID.</remarks>
        public WsapmPluginAttribute(string name, string version, string pluginGuid)
        {
            this.Name = name;
            this.Version = version;
            this.PluginGuid = Guid.Parse(pluginGuid);
        }

        /// <summary>
        /// Gets or sets the plugin's (internal) name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the plugin's version.
        /// </summary>
        public string Version
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the plugin's Guid.
        /// </summary>
        public Guid PluginGuid
        {
            get;
            set;
        }

        ///// <summary>
        ///// Gets a string representation of the plugin.
        ///// </summary>
        ///// <returns>The plugin's string representation.</returns>
        //public override string ToString()
        //{
        //    return this.Name + " (" + this.Version + ")";
        //}
    }
}
