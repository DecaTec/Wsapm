using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wsapm.Extensions;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for PluginSettingsWindow.xaml
    /// </summary>
    public partial class PluginSettingsWindow : Window
    {
        WsapmPluginAdvancedBase plugin;

        public PluginSettingsWindow(WsapmPluginAdvancedBase plugin)
        {
            InitializeComponent();

            this.plugin = plugin;
            this.plugin.LoadSettings();

            if (this.plugin.SettingsControl != null && this.plugin.SettingsControl is UserControl)
            {
                UserControl control = (UserControl)this.plugin.SettingsControl;
                this.settingsWindowContentPresenter.Content = control;
                //this.settingsWindowContentPresenter.Height = control.Width;
                //this.settingsWindowContentPresenter.Width = control.Height;
            }
            else
            {
                var control = this.plugin.SettingsControl as System.Windows.Forms.UserControl;

                if (control != null)
                {
                    var host = new WindowsFormsHost();
                    host.Child = control;
                    this.settingsWindowContentPresenter.Content = host;
                    //this.settingsWindowContentPresenter.Height = width;
                    //this.settingsWindowContentPresenter.Width = height;
                }
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
