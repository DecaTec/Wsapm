using System;
using System.Collections.Generic;
using Wsapm.Extensions;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for managing plugins.
    /// </summary>
    public class PluginManager
    {
        private PluginLoader<WsapmPluginBase> pluginLoader;

        /// <summary>
        /// Initializes a new instance of PluginManager.
        /// </summary>
        public PluginManager()
        {
            this.pluginLoader = new PluginLoader<WsapmPluginBase>();
        }

        /// <summary>
        /// Gets the loaded plugins.
        /// </summary>
        public WsapmPluginBase[] Plugins
        {
            get
            {
                return this.pluginLoader.Plugins;
            }
        }

        /// <summary>
        /// Loads the plugins.
        /// </summary>
        public void LoadAndInitialzePlugins()
        {
            try
            {
                // Load all available plugins.
                this.pluginLoader.LoadPlugins();
            }
            catch (Exception ex)
            {
                WsapmLog.Log.WriteError(Resources.Wsapm_Core.PluginManager_ErrorLoadingPlugins, ex);
            }

            InitializePlugins();
        }

        /// <summary>
        /// Initializes all loaded plugins.
        /// </summary>
        private void InitializePlugins()
        {
            if (this.pluginLoader.PluginsAvailable)
            {
                // Make sure that new plugins are initialized.
                foreach (var plugin in this.pluginLoader.Plugins)
                {
                    try
                    {
                        if (!plugin.IsInitialized)
                        {
                            plugin.InitializePlugin();
                            WsapmLog.Log.WriteLine(String.Format(Resources.Wsapm_Core.PluginManager_PluginInitialized, plugin.PluginAttribute.Name), LogMode.Verbose);
                        }
                    }
                    catch (Exception ex)
                    {
                        WsapmLog.Log.WriteError(String.Format(Resources.Wsapm_Core.PluginManager_ErrorPluginInitialize, plugin.PluginAttribute.Name), ex);
                    }
                }
            }
        }

        /// <summary>
        /// Prepares, checks the policies and tears down all loaded plugins.
        /// </summary>
        /// <param name="activatedPlugins">The list of activated plugins.</param>
        /// <returns>The result of the check as PluginCheckSuspendResult.</returns>
        public CheckSuspendResult CheckPluginPolicies(List<Guid> activatedPlugins)
        {
            var checkResult = new CheckSuspendResult(false, String.Empty);

            if (activatedPlugins == null || activatedPlugins.Count == 0 || this.pluginLoader.Plugins == null || this.pluginLoader.Plugins.Length == 0)
                return checkResult;

            foreach (var plugin in this.pluginLoader.Plugins)
            {
                // Skip non activated plugins.
                if (!activatedPlugins.Contains(plugin.PluginAttribute.PluginGuid))
                    continue;

                // Skip plugins which are not initialized.
                if (!plugin.IsInitialized)
                    continue;

                // First load current settings (if any), then prepare, then check - plugin by plugin.
                try
                {
                    // Load settings only if it is an advanced plugin.
                    WsapmPluginAdvancedBase pluginAdvanced = plugin as WsapmPluginAdvancedBase;

                    if (pluginAdvanced != null)
                        pluginAdvanced.LoadSettings();

                    // Prepare.
                    plugin.PreparePlugin();
                    WsapmLog.Log.WriteLine(String.Format(Resources.Wsapm_Core.PluginManager_PluginPrepared, plugin.PluginAttribute.Name), LogMode.Verbose);
                }
                catch (Exception ex)
                {
                    WsapmLog.Log.WriteError(String.Format(Resources.Wsapm_Core.PluginManager_ErrorPluginPrepare, plugin.PluginAttribute.Name), ex);
                    continue;
                }

                // Skip plugins which were not prepared.
                if (!plugin.IsPrepared)
                    continue;

                try
                {
                    WsapmLog.Log.WriteLine(String.Format(Resources.Wsapm_Core.PluginManager_PluginCheckPolicy, plugin.PluginAttribute.Name), LogMode.Normal);
                    var checkResultPlugin = plugin.CheckPluginPolicyForPlugin();

                    if (checkResultPlugin.SuspendStandby)
                    {
                        checkResult = CheckSuspendResult.FromPluginCheckSuspendResult(checkResultPlugin, plugin);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    WsapmLog.Log.WriteError(String.Format(Resources.Wsapm_Core.PluginManager_ErrorPluginCheckPolicy, plugin.PluginAttribute.Name), ex);
                }
                finally
                {
                    // Make sure that the plugin gets teared down.
                    try
                    {
                        WsapmLog.Log.WriteLine(String.Format(Resources.Wsapm_Core.PluginManager_PluginTearedDown, plugin.PluginAttribute.Name), LogMode.Verbose);
                        plugin.TearDownPlugin();
                    }
                    catch (Exception ex)
                    {
                        WsapmLog.Log.WriteError(String.Format(Resources.Wsapm_Core.PluginManager_ErrorPluginTearDown, plugin.PluginAttribute.Name), ex);
                    }
                }
            }

            return checkResult;
        }
    }
}
