using System;
using System.IO;
using System.Reflection;

namespace Wsapm.Extensions
{
    /// <summary>
    /// Abstract base class of a simple Windows Server Advanced Power Management plugin.
    /// </summary>
    /// <remarks>A simple plugin is a plugin which does not support a graphical user interface or settings. It only has a check method to indicate that standby should be suppressed/admitted.</remarks>
    public abstract class WsapmPluginBase
    {
        /// <summary>
        /// Initializes a new instance of WsapmPluginBase.
        /// </summary>
        protected WsapmPluginBase()
        {
            if (this.PluginAttribute == null)
                throw new WsapmPluginException(Resources.Wsapm_Extensions.WsapmPluginBase_ErrorAttributeMissing);
            else if (this.PluginAttribute.PluginGuid == null)
                throw new WsapmPluginException(Resources.Wsapm_Extensions.WsapmPluginBase_ErrorAttributeGuidMissing);

            this.IsInitialized = false;
            this.IsPrepared = false;
            this.IsCheckingPolicy = false;
            this.IsTearedDown = false;
        }

        #region Methods to be called by WSAPM

        /// <summary>
        /// Initializes the plugin and sets the appropriate initialized property.
        /// </summary>
        public void InitializePlugin()
        {
            this.IsInitialized = Initialize();
        }

        /// <summary>
        /// Prepares the plugin and sets the appropriate prepared property.
        /// </summary>
        public void PreparePlugin()
        {
            this.IsPrepared = Prepare();
        }

        /// <summary>
        /// Checks the plugin's policy.
        /// </summary>
        /// <returns>A PluginCheckSuspendResult which indicates of the plugin's policy was satisfied.</returns>
        public PluginCheckSuspendResult CheckPluginPolicyForPlugin()
        {
            this.IsCheckingPolicy = true;
            var checkResult =  CheckPluginPolicy();
            this.IsCheckingPolicy = false;

            return checkResult;
        }

        /// <summary>
        /// Tears down the plugin.
        /// </summary>
        public void TearDownPlugin()
        {
            this.IsTearedDown = TearDown();
        }

        #endregion Methods to be called by WSAPM

        #region Abstract members to override

        /// <summary>
        /// Method which is called at least once after the plugin was loaded.
        /// </summary>
        /// <returns>True, if initialization was successful, otherwise false.</returns>
        /// <remarks>Use this method to execute any one-time initialization code.</remarks>
        protected abstract bool Initialize();

        /// <summary>
        /// Method which is always called just before the plugin policy is checked.
        /// </summary>
        /// <returns>True, if preparation was successful, otherwise false.</returns>
        /// <remarks>Use this method to prepare your plugin for the subsequent check of the plugin policy.</remarks>
        protected abstract bool Prepare();

        /// <summary>
        /// Method which is called to check the plugin's policy.
        /// </summary>
        /// <returns>A PluginCheckSuspendResult which indicates of the plugin's policy was satisfied.</returns>
        protected abstract PluginCheckSuspendResult CheckPluginPolicy();

        /// <summary>
        /// Method which is called after the plugin policy was checked.
        /// </summary>
        /// <returns>True, if tearing down was successful, otherwise false.</returns>
        /// <remarks>Use this method to dispose all resources which were created during the preparation or check of the plugin policy.</remarks>
        protected abstract bool TearDown();

        #endregion Abstract members to override

        #region Properties

        /// <summary>
        /// Gets the PluginAttribute of the plugin.
        /// </summary>
        public WsapmPluginAttribute PluginAttribute
        {
            get
            {
                var attributes = this.GetType().GetCustomAttributes(true);

                foreach (Attribute attr in attributes)
                {
                    if (attr is WsapmPluginAttribute)
                    {
                        var pluginAttribute = (WsapmPluginAttribute)attr;
                        return pluginAttribute;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating if the plugin is currently activated in Windows Server Advanced Power Management.
        /// </summary>
        public bool IsActivated
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating if the plugin was initialized.
        /// </summary>
        public bool IsInitialized
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating if the plugin was prepared.
        /// </summary>
        public bool IsPrepared
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating if the plugin is currently checking it's policy.
        /// </summary>
        public bool IsCheckingPolicy
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating if the plugin is already teared down.
        /// </summary>
        public bool IsTearedDown
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the plugin's manifest.
        /// </summary>
        public PluginManifest Manifest
        {
            get;
            set;
        }

        #endregion Properties

        #region Helpers

        /// <summary>
        /// Gets the directory the plugin is running in (i.e. install directory).
        /// </summary>
        /// <returns>The directory the plugin is running in.</returns>
        public string GetInstallDir()
        {
            return Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).Location);
        }

        /// <summary>
        /// Gets a string representation of the plugin.
        /// </summary>
        /// <returns>The string representation of the plugin.</returns>
        public sealed override string ToString()
        {
            if (this.PluginAttribute == null)
                return String.Empty;

            return this.PluginAttribute.ToString();
        }

        #endregion Helpers
    }
}
