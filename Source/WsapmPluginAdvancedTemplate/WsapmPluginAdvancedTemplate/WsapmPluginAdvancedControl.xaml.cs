using System.Windows.Controls;
using Wsapm.Extensions;

namespace WsapmPluginAdvancedTemplate
{
    // This is the UI class of your plugin (in this case, it is a WPF UserControl).
    // It only has to implement the interface IWsapmPluginSettingsControl.

    /// <summary>
    /// Interaction logic for WsapmPluginAdvancedControl.xaml
    /// </summary>
    public partial class WsapmPluginAdvancedControl : UserControl, IWsapmPluginSettingsControl
    {
        public WsapmPluginAdvancedControl()
        {
            InitializeComponent();
        }

        public object GetSettingsBeforeSave()
        {
            // Build up a new instance of your settings class here 
            // and fill it with the elements from yout UI.
            var settings = new WsapmPluginAdvancedSettings();
            settings.MyString = this.textBox.Text;
            return settings;
        }

        public void SetSettings(object settings)
        {
            // In this method, the current settings of your plugin gets injected.
            // Use this method to initialize the UI, i.e. filling the UI elements from the current settings.
            var mySettings = (WsapmPluginAdvancedSettings)settings;           
            this.textBox.Text = mySettings.MyString;
        }
    }
}
