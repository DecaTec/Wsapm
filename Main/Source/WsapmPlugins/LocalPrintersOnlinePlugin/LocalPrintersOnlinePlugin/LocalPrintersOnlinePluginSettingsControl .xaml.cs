using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wsapm.Extensions;

namespace LocalPrintersOnlinePlugin
{
    /// <summary>
    /// Interaction logic for LocalPrintersOnlinePluginSettingsControl.xaml
    /// </summary>
    public partial class LocalPrintersOnlinePluginSettingsControl : UserControl, IWsapmPluginSettingsControl
    {
        public LocalPrintersOnlinePluginSettingsControl()
        {
            InitializeComponent();

            BuildUI();
        }
       
        public object GetSettingsBeforeSave()
        {
            var printerList = new List<string>();

            foreach (var item in this.stackPanel.Children)
            {
                var checkBox = item as CheckBox;

                if (checkBox == null)
                    continue;

                if(checkBox.IsChecked.Value)
                {
                    var str = checkBox.Content as string;

                    if (!string.IsNullOrEmpty(str))
                        printerList.Add(str);
                }
            }

            var pluginSettings = new LocalPrintersOnlinePluginSettings();
            pluginSettings.OnlinePrinters = printerList;
            return pluginSettings;
        }

        public void SetSettings(object settings)
        {
            var pluginSettings = settings as LocalPrintersOnlinePluginSettings;

            // Always check if the settings contain data before trying to access settings properties!
            if (pluginSettings == null || pluginSettings.OnlinePrinters == null || pluginSettings.OnlinePrinters.Count == 0)
                return;

            foreach (var item in this.stackPanel.Children)
            {
                var checkBox = item as CheckBox;

                if (checkBox == null)
                    continue;

                var str = checkBox.Content as string;

                if (string.IsNullOrEmpty(str))
                    continue;

                if (pluginSettings.OnlinePrinters.Contains(str))
                    checkBox.IsChecked = true;
            }
        }

        private void BuildUI()
        {
            this.stackPanel.Children.Clear();
            var printers = PrintHelper.GetInstalledLocalPrinters();
            var defaultPrinter = PrintHelper.GetDefaultLocalPrinterName();

            foreach (var printer in printers)
            {
                var checkBox = new CheckBox();
                checkBox.Content = printer;

                if (printer == defaultPrinter)
                    checkBox.FontWeight = FontWeights.Bold;               

                this.stackPanel.Children.Add(checkBox);
            }
        }
    }
}
