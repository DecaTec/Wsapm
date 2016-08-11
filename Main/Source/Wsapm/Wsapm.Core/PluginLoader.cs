using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Wsapm.Extensions;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for loading plugins.
    /// </summary>
    /// <typeparam name="T">The type of the plugins.</typeparam>
    public class PluginLoader<T>
    {
        private string pluginDirectory;
        private T[] pluginsImport;
        private T[] loadedPlugins;

        private static object lockObjectPluginsImport = new object();
        private static object lockObjectPluginsLoaded = new object();

        /// <summary>
        /// Initializes a new instance of PluginLoader with the standard plugin directory.
        /// </summary>
        public PluginLoader()
            : this(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + WsapmConstants.WsapmPluginFolder)
        {
        }

        /// <summary>
        /// Initializes a new instance of PluginLoader.
        /// </summary>
        /// <param name="pluginDirectory">The plugin directory.</param>
        public PluginLoader(string pluginDirectory)
        {
            this.pluginDirectory = pluginDirectory;
        }

        /// <summary>
        /// Gets or sets the plugins.
        /// </summary>
        [ImportMany]
        private T[] PluginsImport
        {
            get
            {
                lock (lockObjectPluginsImport)
                {
                    return this.pluginsImport;
                }
            }
            set
            {
                lock (lockObjectPluginsImport)
                {
                    this.pluginsImport = value;
                }
            }
        }

        /// <summary>
        /// Gets the loaded plugins.
        /// </summary>
        public T[] Plugins
        {
            get
            {
                lock (lockObjectPluginsLoaded)
                {
                    return this.loadedPlugins;
                }
            }
            private set
            {
                lock (lockObjectPluginsLoaded)
                {
                    this.loadedPlugins = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if plugins are available.
        /// </summary>
        public bool PluginsAvailable
        {
            get
            {
                return this.Plugins != null && this.Plugins.Length > 0;
            }
        }

        /// <summary>
        /// Loads the available plugins.
        /// </summary>
        public void LoadPlugins()
        {
            if (!Directory.Exists(this.pluginDirectory))
                return;

            if (String.IsNullOrEmpty(this.pluginDirectory))
                return;

            var aggregateCatalog = new AggregateCatalog();
            var subDirs = Directory.GetDirectories(this.pluginDirectory);

            foreach (var dir in subDirs)
            {
                var catalog = new DirectoryCatalog(dir);

                try
                {
                    CompositionContainer tmpContainer = new CompositionContainer(catalog);
                    tmpContainer.GetExportedValues<WsapmPluginBase>();
                }
                catch (Exception ex) // (CompositionException ex)
                {
                    // Ignore plugins that fail to load.
                    WsapmLog.Log.WriteError(Resources.Wsapm_Core.PuginLoader_CompositionError, ex);
                    continue;
                }

                aggregateCatalog.Catalogs.Add(catalog);
            }

            var compositionContainer = new CompositionContainer(aggregateCatalog);
            compositionContainer.ComposeParts(this);

            CheckForDuplicateGuidsAndManifest();
        }

        private bool CheckPluginManifests(T plugin)
        {
            if (plugin == null)
                return true;

            var pluginBase = plugin as WsapmPluginBase;

            if (pluginBase == null)
                return true;

            try
            {
                var manifest = PluginManifestReader.GetPluginManifest(pluginBase.GetInstallDir(), pluginBase.PluginAttribute.Name);

                if (manifest != null)
                {
                    pluginBase.Manifest = manifest;
                    return true;
                }
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(string.Format(Resources.Wsapm_Core.PuginLoader_ManifestMissingError, pluginBase.PluginAttribute.Name), ex);
            }

            return false;
        }

        private void CheckForDuplicateGuidsAndManifest()
        {
            if (this.PluginsImport == null || this.PluginsImport.Length == 0)
                return;

            IList<T> fullyLoadedPlugins = new List<T>();

            // Add already loaded plugins.
            if (this.Plugins != null)
            {
                foreach (var loadedPlugin in this.Plugins)
                {
                    if(CheckPluginManifests(loadedPlugin))
                        fullyLoadedPlugins.Add(loadedPlugin);
                }
            }

            // Add newly loaded plugins.
            foreach (var plugin in PluginsImport)
            {
                WsapmPluginBase pluginBase = plugin as WsapmPluginBase;

                if (pluginBase == null)
                    continue;

                bool addPlugin = true;

                foreach (var loadedPlugin in fullyLoadedPlugins)
                {
                    var loadedPluginBase = loadedPlugin as WsapmPluginBase;

                    if (loadedPluginBase != null && loadedPluginBase.PluginAttribute.PluginGuid == pluginBase.PluginAttribute.PluginGuid)
                    {
                        addPlugin = false;
                        break;
                    }
                }

                if (addPlugin && CheckPluginManifests(plugin))
                    fullyLoadedPlugins.Add(plugin);
            }

            this.Plugins = fullyLoadedPlugins.ToArray<T>();
        }

        /// <summary>
        /// Gets the plugin directory.
        /// </summary>
        public string PluginDirectory
        {
            get
            {
                return this.pluginDirectory;
            }
        }
    }
}
