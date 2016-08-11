
namespace Wsapm.Extensions
{
    /// <summary>
    /// Interface for a plugin control of Windows Server Advanced Power Management.
    /// </summary>
    public interface IWsapmPluginSettingsControl
    {
        /// <summary>
        /// Sets the settings when the settings control gets shown.
        /// </summary>
        /// <param name="settings"></param>
        void SetSettings(object settings);

        /// <summary>
        /// Gets the plugin's settings before the settings are saved.
        /// </summary>
        /// <returns>The plugin's settings.</returns>
        object GetSettingsBeforeSave();
    }
}
