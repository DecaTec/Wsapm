using System.Windows.Controls;
using Wsapm.Extensions;

namespace TwonkyStreamingStatusPlugin
{
    /// <summary>
    /// Interaction logic for TwonkyStreamingStatusPluginSettingsControl.xaml
    /// </summary>
    public partial class TwonkyStreamingStatusPluginSettingsControl : UserControl, IWsapmPluginSettingsControl
    {
        public TwonkyStreamingStatusPluginSettingsControl()
        {
            InitializeComponent();
        }

        public object GetSettingsBeforeSave()
        {
            var pluginSettings = new TwonkyStreamingStatusPluginSettings();
            pluginSettings.TwonkyUrl = this.textBoxServerUrl.Text;
            return pluginSettings;
        }

        public void SetSettings(object settings)
        {
            var pluginSettings = settings as TwonkyStreamingStatusPluginSettings;

            if(pluginSettings == null)
                return;

            if(!string.IsNullOrEmpty(pluginSettings.TwonkyUrl))
                this.textBoxServerUrl.Text = pluginSettings.TwonkyUrl;
        }
    }
}
