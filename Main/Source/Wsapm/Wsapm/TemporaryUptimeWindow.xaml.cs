using System;
using System.Windows;
using Wsapm.Core;
using Xceed.Wpf.Toolkit;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for TemporaryUptimeWindow.xaml
    /// </summary>
    public partial class TemporaryUptimeWindow : Window
    {
        public TemporaryUptimeWindow()
        {
            InitializeComponent();
        }

        private void upDownTemporaryUptime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var numericUpDown = (IntegerUpDown)sender;

            if (numericUpDown.Value == null)
                numericUpDown.Value = 0;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var hours = this.upDownTemporaryUptimeHours.Value.Value;
            var minutes = this.upDownTemporaryUptimeMinutes.Value.Value;

            if (hours == 0 && minutes == 0)
            {
                System.Windows.MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.TemporaryUptimeWindow_ErrorNoUptimeDefined, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var temporaryUptime = new TemporaryUptime(hours, minutes);
                var temporaryUptimeManager = new TemporaryUptimeManager();
                temporaryUptimeManager.SaveTemporaryUptime(temporaryUptime);
                this.DialogResult = true;
            }
            catch (WsapmException ex)
            {
                System.Windows.MessageBox.Show(string.Format("{1}:{0}{0}{2}", Environment.NewLine, ex.Message, ex.InnerException.Message), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
            finally
            {
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.upDownTemporaryUptimeHours.Value = 1;
            this.upDownTemporaryUptimeMinutes.Value = 0;
        }
    }
}
