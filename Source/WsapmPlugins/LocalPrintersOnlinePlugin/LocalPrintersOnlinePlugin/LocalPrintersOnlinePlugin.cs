using System.ComponentModel.Composition;
using Wsapm.Extensions;

namespace LocalPrintersOnlinePlugin
{
    [Export(typeof(WsapmPluginBase))]
    [WsapmPlugin("LocalPrintersOnline", "v1.1.0", "{58E6EB76-F93F-4819-8CCC-6BF466AC71DB}")]
    public class LocalPrintersOnlinePlugin : WsapmPluginAdvancedBase
    {
        // Private variable for the only instance of the settings control class.
        private LocalPrintersOnlinePluginSettingsControl settingsControl;

        public LocalPrintersOnlinePlugin() : base(typeof(LocalPrintersOnlinePluginSettings))
        { 
        }

        protected override object LoadDefaultSettings()
        {
            // Just return an empty instance ot the settings class as no printer should be checked by default.
            return new LocalPrintersOnlinePluginSettings();
        }

        public override object SettingsControl
        {
            get
            {
                // Always return the same instance of the settings control class here.
                if (this.settingsControl == null)
                    this.settingsControl = new LocalPrintersOnlinePluginSettingsControl();

                return this.settingsControl;
            }
        }

        protected override PluginCheckSuspendResult CheckPluginPolicy()
        {
            // You can always access the current settings with the CurrentSettings property of the base class.
            var pluginSettings = this.CurrentSettings as LocalPrintersOnlinePluginSettings;

            // Don't forget to check if the settings contains data before accessing it.
            if (pluginSettings == null || pluginSettings.OnlinePrinters == null || pluginSettings.OnlinePrinters.Count == 0)
                return new PluginCheckSuspendResult(false, string.Empty);

            foreach (var printer in pluginSettings.OnlinePrinters)
            {
                if (PrintHelper.IsPrinterOnline(printer))
                {
                    // Just return a PluginCheckSuspendResult if the first printer was found online.
                    return new PluginCheckSuspendResult(true, string.Format(Resources.LocalPrintersOnlinePlugin.LocalPrintersOnlinePlugin_PrinterSwitchedOn, printer));
                }
            }

            // No printer was to be checked, just return a PluginCheckSuspendResult so that the standby mode should not be supressed.
            return new PluginCheckSuspendResult(false, string.Empty);
        }

        protected override bool Initialize()
        {
            return true;
        }

        protected override bool Prepare()
        {
            return true;
        }

        protected override bool TearDown()
        {
            return true;
        }
    }
}
