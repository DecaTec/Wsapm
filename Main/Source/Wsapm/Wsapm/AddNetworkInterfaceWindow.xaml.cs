using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddNetworkInterfaceWindow.xaml
    /// </summary>
    public partial class AddNetworkInterfaceWindow : Window
    {
        private NetworkInterfaceToMonitor networkInterfaceToMonitor;
        private NetworkInterfaceToMonitor[] allNetworkInterfaces;
        private NetworkInterfaceToMonitor editNetworkInterfaceToMonitor;

        private NetworkLoadCurrent networkLoad;
        private Timer networkLoadTimer;

        private readonly TimeSpan displayRefreshInterval = TimeSpan.FromSeconds(1);
        private string currentSelectedNic;
        private readonly string loadingString = Wsapm.Resources.Wsapm.AddNetworkInterfaceWindow_LabelNetworkLoadLoading;

        public AddNetworkInterfaceWindow(NetworkInterfaceToMonitor[] allNetworkInterfaces)
        {
            InitializeComponent();

            this.allNetworkInterfaces = allNetworkInterfaces;

            var nics = WsapmTools.GetAvailableNetworkInterfaces();

            foreach (var nic in nics)
            {
                this.comboBoxAvailableNetworkInterfaces.Items.Add(nic);
            }

            this.comboBoxAvailableNetworkInterfaces.Items.Add(WsapmTools.GetCommonDiaplayNameAllNetworkInterfaces());

            this.networkLoad = new NetworkLoadCurrent();
            this.networkLoadTimer = null;
            this.networkLoadTimer = new Timer(OnShowNetworkLoadsCallback, null, new TimeSpan(), this.displayRefreshInterval);
        }

        public AddNetworkInterfaceWindow(NetworkInterfaceToMonitor networkInterfaceToMonitorToEdit, NetworkInterfaceToMonitor[] allNetworkInterfaces)
            : this(allNetworkInterfaces)
        {
            this.Title = Wsapm.Resources.Wsapm.AddNetworkInterfaceWindow_TitleEdit;
            this.editNetworkInterfaceToMonitor = networkInterfaceToMonitorToEdit.Copy();
        }

        /// <summary>
        /// Gets the created NetworkInterfaceToMonitor.
        /// </summary>
        /// <returns></returns>
        internal NetworkInterfaceToMonitor GetNetworkInterface()
        {
            return this.networkInterfaceToMonitor;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var nicName = this.comboBoxAvailableNetworkInterfaces.SelectedItem as string;
            var enableNetworkLoadTotal = this.checkBoxEnableNetworkLoadTotal.IsChecked.Value;
            var enableNetworkLoadSent = this.checkBoxEnableNetworkLoadSent.IsChecked.Value;
            var enableNetworkLoadReceived = this.checkBoxEnableNetworkLoadReceived.IsChecked.Value;
            var networkLoadTotal = this.upDownNetworkLoadTotal.Value.Value;
            var networkLoadReceived = this.upDownNetworkLoadReceived.Value.Value;
            var networkLoadSent = this.upDownNetworkLoadSent.Value.Value;

            var nic = new NetworkInterfaceToMonitor();
            nic.NetworkInterface = nicName;
            nic.EnableCheckNetworkLoadDownload = enableNetworkLoadReceived;
            nic.EnableCheckNetworkLoadUpload = enableNetworkLoadSent;
            nic.EnableCheckNetworkLoadTotal = enableNetworkLoadTotal;
            nic.NetworkLoadDownload = networkLoadReceived;
            nic.NetworkLoadUpload = networkLoadSent;
            nic.NetworkLoadTotal = networkLoadTotal;

            // Avoid to add a element twice.
            if (this.editNetworkInterfaceToMonitor == null)
            {
                // Add new mode.
                if (this.allNetworkInterfaces != null)
                {
                    for (int i = 0; i < this.allNetworkInterfaces.Length; i++)
                    {
                        if (this.allNetworkInterfaces[i] == nic)
                        {
                            MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddNetworkInterfaceWindow_NetworkInterfaceAlreadyAdded, nic.NetworkInterface), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }
            else
            {
                // Edit mode.
                if (nic != this.editNetworkInterfaceToMonitor)
                {
                    // Element was changed.
                    if (this.allNetworkInterfaces != null)
                    {
                        for (int i = 0; i < this.allNetworkInterfaces.Length; i++)
                        {
                            if (this.allNetworkInterfaces[i] == nic)
                            {
                                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddNetworkInterfaceWindow_NetworkInterfaceAlreadyAdded, nic.NetworkInterface), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                }
            }

            this.networkInterfaceToMonitor = nic;
            this.DialogResult = true;
            this.Close();
        }

        private void checkBoxEnableNetworkLoadTotal_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkLoadTotalOptions();
        }

        private void checkBoxEnableNetworkLoadTotal_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkLoadTotalOptions();
        }

        private void EnableDisableNetworkLoadTotalOptions()
        {
            var enabled = this.checkBoxEnableNetworkLoadTotal.IsChecked.Value;
            this.upDownNetworkLoadTotal.IsEnabled = enabled;
        }

        private void checkBoxEnableNetworkLoadReceived_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkLoadReceivedOptions();
        }

        private void checkBoxEnableNetworkLoadReceived_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkLoadReceivedOptions();
        }

        private void EnableDisableNetworkLoadReceivedOptions()
        {
            var enabled = this.checkBoxEnableNetworkLoadReceived.IsChecked.Value;
            this.upDownNetworkLoadReceived.IsEnabled = enabled;
        }

        private void upDownNetworkLoadReceived_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownNetworkLoadReceived.Value == null)
                this.upDownNetworkLoadReceived.Value = 0;
        }

        private void checkBoxEnableNetworkLoadSent_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkLoadSentOptions();
        }

        private void checkBoxEnableNetworkLoadSent_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableNetworkLoadSentOptions();
        }

        private void EnableDisableNetworkLoadSentOptions()
        {
            var enabled = this.checkBoxEnableNetworkLoadSent.IsChecked.Value;
            this.upDownNetworkLoadSent.IsEnabled = enabled;
        }

        private void upDownNetworkLoadSent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownNetworkLoadSent.Value == null)
                this.upDownNetworkLoadSent.Value = 0;
        }

        private void upDownNetworkLoadTotal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownNetworkLoadTotal.Value == null)
                this.upDownNetworkLoadTotal.Value = 0;
        }

        private void OnShowNetworkLoadsCallback(object state)
        {
            if (string.IsNullOrEmpty(this.currentSelectedNic))
                return;

            var loadTotal = string.Empty;
            var loadSent = string.Empty;
            var loadReceived = string.Empty;
            bool allNics = false;

            allNics = this.currentSelectedNic == WsapmTools.GetCommonDiaplayNameAllNetworkInterfaces();

            try
            {
                if (allNics)
                {
                    loadTotal = WsapmConvert.ConvertByteToKBit(this.networkLoad.GetCurrentNetworkLoadTotalAllNics()).ToString("0.00") + " KBit/s";
                    loadSent = WsapmConvert.ConvertByteToKBit(this.networkLoad.GetCurrentNetworkLoadUploadAllNics()).ToString("0.00") + " KBit/s";
                    loadReceived = WsapmConvert.ConvertByteToKBit(this.networkLoad.GetCurrentNetworkLoadDownloadAllNics()).ToString("0.00") + " KBit/s";
                }
                else
                {
                    loadTotal = WsapmConvert.ConvertByteToKBit(this.networkLoad.GetCurrentNetworkLoadTotal(this.currentSelectedNic)).ToString("0.00") + " KBit/s";
                    loadSent = WsapmConvert.ConvertByteToKBit(this.networkLoad.GetCurrentNetworkLoadUpload(this.currentSelectedNic)).ToString("0.00") + " KBit/s";
                    loadReceived = WsapmConvert.ConvertByteToKBit(this.networkLoad.GetCurrentNetworkLoadDownload(this.currentSelectedNic)).ToString("0.00") + " KBit/s";
                }
            }
            catch (Exception)
            {
            }

            Dispatcher.Invoke((Action)(() =>
            {
                this.labelCurrentNetworkLoadReceived.Content = loadReceived;
                this.labelCurrentNetworkLoadSent.Content = loadSent;
                this.labelCurrentNetworkLoadTotal.Content = loadTotal;
            }));
        }

        private void comboBoxAvailableNetworkInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.currentSelectedNic = (string)this.comboBoxAvailableNetworkInterfaces.SelectedItem;

            this.labelCurrentNetworkLoadReceived.Content = this.loadingString;
            this.labelCurrentNetworkLoadSent.Content = this.loadingString;
            this.labelCurrentNetworkLoadTotal.Content = this.loadingString;

            this.checkBoxEnableNetworkLoadReceived.IsChecked = false;
            this.checkBoxEnableNetworkLoadSent.IsChecked = false;
            this.checkBoxEnableNetworkLoadTotal.IsChecked = false;
            this.upDownNetworkLoadReceived.Value = 0;
            this.upDownNetworkLoadSent.Value = 0;
            this.upDownNetworkLoadTotal.Value = 0;

            if (this.allNetworkInterfaces != null && this.allNetworkInterfaces.Length > 0)
            {
                for (int i = 0; i < this.allNetworkInterfaces.Length; i++)
                {
                    if (this.allNetworkInterfaces[i].NetworkInterface == (string)this.comboBoxAvailableNetworkInterfaces.SelectedItem)
                    {
                        this.checkBoxEnableNetworkLoadReceived.IsChecked = this.allNetworkInterfaces[i].EnableCheckNetworkLoadDownload;
                        this.checkBoxEnableNetworkLoadSent.IsChecked = this.allNetworkInterfaces[i].EnableCheckNetworkLoadUpload;
                        this.checkBoxEnableNetworkLoadTotal.IsChecked = this.allNetworkInterfaces[i].EnableCheckNetworkLoadTotal;
                        this.upDownNetworkLoadReceived.Value = (int)this.allNetworkInterfaces[i].NetworkLoadDownload;
                        this.upDownNetworkLoadSent.Value = (int)this.allNetworkInterfaces[i].NetworkLoadUpload;
                        this.upDownNetworkLoadTotal.Value = (int)this.allNetworkInterfaces[i].NetworkLoadTotal;
                        break;
                    }
                }
            }

            EnableDisableNetworkLoadReceivedOptions();
            EnableDisableNetworkLoadSentOptions();
            EnableDisableNetworkLoadTotalOptions();
        }

        private void comboBoxAvailableNetworkInterfaces_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.editNetworkInterfaceToMonitor != null && this.comboBoxAvailableNetworkInterfaces.Items.Count > 0)
            {
                for (int i = 0; i < this.comboBoxAvailableNetworkInterfaces.Items.Count; i++)
                {
                    if (this.editNetworkInterfaceToMonitor.NetworkInterface == (string)this.comboBoxAvailableNetworkInterfaces.Items[i])
                    {
                        this.comboBoxAvailableNetworkInterfaces.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
                this.comboBoxAvailableNetworkInterfaces.SelectedIndex = 0;
        }
    }
}
