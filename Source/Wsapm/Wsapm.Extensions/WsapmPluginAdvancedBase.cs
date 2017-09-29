using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace Wsapm.Extensions
{
    /// <summary>
    /// Abstract base class of an advanced plugin for Windows Server Advanced Power Management.
    /// </summary>
    /// <remarks>An advanced plugin is a plugin which supports a graphical user interface and settings.</remarks>
    public abstract class WsapmPluginAdvancedBase : WsapmPluginBase
    {
        private string settingsFile;
        private object currentSettings;
        private XmlSerializer serializer;

        /// <summary>
        /// Initializes a new WsapmPluginAdvancedBase.
        /// </summary>
        /// <param name="settingsType">The type of the plugin's settings.</param>
        protected WsapmPluginAdvancedBase(Type settingsType)
        {
            MakeSureSettingsFolderExists();

            this.settingsFile = GetSettingsFileName();
            this.serializer = new XmlSerializer(settingsType);
        }

        #region UI related

        /// <summary>
        /// Gets the UI control of the plugin.
        /// </summary>
        abstract public object SettingsControl
        {
            get;
        }

        #endregion UI related

        #region Settings handling

        /// <summary>
        /// Gets or sets the plugin's current settings.
        /// </summary>
        /// <remarks>As a plugin developer, make sure that you settings class can be serialized with an XmlSerializer.</remarks>
        protected object CurrentSettings
        {
            get
            {
                if (this.currentSettings == null)
                    LoadSettings();

                return this.currentSettings;
            }
            private set
            {
                this.currentSettings = value;
            }
        }

        /// <summary>
        /// Loads the plugin's settings.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (!File.Exists(this.settingsFile))
                {
                    this.CurrentSettings = LoadDefaultSettings();
                }
                else
                {
                    using (FileStream fs = new FileStream(this.settingsFile, FileMode.Open, FileAccess.Read))
                    {
                        this.CurrentSettings = this.serializer.Deserialize(fs);
                    }
                }

                if (this.CurrentSettings == null)
                    LoadDefaultSettings();

                // Only inject settings to UI when the plugin is not loaded by the WSAPM service!
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                {
                    IWsapmPluginSettingsControl iSettingsControl = this.SettingsControl as IWsapmPluginSettingsControl;

                    if (iSettingsControl != null)
                        iSettingsControl.SetSettings(this.CurrentSettings);
                }
            }
            catch (Exception ex)
            {
                throw new WsapmPluginException(String.Format(Resources.Wsapm_Extensions.WsapmPluginAdvancedBase_ErrorLoadSettings, this.PluginAttribute.Name), ex);
            }
        }

        /// <summary>
        /// Abstract method to load the plugin's default settings.
        /// </summary>
        /// <returns>The plugin's default settings as object.</returns>
        /// <remarks>As a plugin developer, make sure that you settings class can be serialized with an XmlSerializer.</remarks>
        abstract protected object LoadDefaultSettings();

        /// <summary>
        /// Saves the plugin's settings.
        /// </summary>
        /// <param name="settings"></param>
        public void SaveSettings(object settings)
        {
            try
            {
                if (this.CurrentSettings == null)
                    return;

                if (File.Exists(this.settingsFile))
                    File.Delete(this.settingsFile);

                using (FileStream fs = new FileStream(this.settingsFile, FileMode.Create, FileAccess.Write))
                {
                    this.serializer.Serialize(fs, settings);
                }
            }
            catch (Exception ex)
            {
                throw new WsapmPluginException(String.Format(Resources.Wsapm_Extensions.WsapmPluginAdvancedBase_ErrorSaveSettings, this.PluginAttribute.Name), ex);
            }
        }

        #endregion Settings handling

        #region Helpers

        /// <summary>
        /// Makes sure that the plugin settings folder exists.
        /// </summary>
        private void MakeSureSettingsFolderExists()
        {
            var path = GetPluginSettingsFolder();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Gets the settings folder of the plugin.
        /// </summary>
        /// <returns></returns>
        private string GetPluginSettingsFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" + WsapmExtensionsConstants.WsapmApplicationDataFolder + @"\" + WsapmExtensionsConstants.PluginSettingsFolder + @"\" + this.PluginAttribute.Name + " (" + this.PluginAttribute.PluginGuid + ")";
        }

        /// <summary>
        /// Gets the full settings file name.
        /// </summary>
        /// <returns>The full settings file name.</returns>
        private string GetSettingsFileName()
        {
            string path = GetPluginSettingsFolder();
            return path + @"\" + WsapmExtensionsConstants.PluginSettingsFile;
        }
        
        /// <summary>
        /// Gets the settings file name.
        /// </summary>
        public string SettingsFolder
        {
            get
            {
                return Path.GetDirectoryName(this.settingsFile);
            }
        }

        #endregion Helpers
    }
}
