using System;
using System.Net;
using System.Windows;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddNetworkMachineWindows.xaml
    /// </summary>
    public partial class AddNetworkMachineWindow : Window
    {
        private NetworkMachine networkMachine;
        private NetworkMachine[] allNetworkMachines;
        private NetworkMachine editNetworkMachineCopy = null;

        public AddNetworkMachineWindow(NetworkMachine[] allNetworkMachines)
        {
            InitializeComponent();

            this.allNetworkMachines = allNetworkMachines;
        }

        public AddNetworkMachineWindow(NetworkMachine networkMachineToEdit, NetworkMachine[] allNetworkMachines)
            : this(allNetworkMachines)
        {
            this.Title = Wsapm.Resources.Wsapm.AddNetworkMachineWindow_TitleEdit;
            this.editNetworkMachineCopy = networkMachineToEdit.Copy();
            this.textBoxName.Text = networkMachineToEdit.Name;

            if (networkMachineToEdit.IPAddress != null)
                this.textBoxIPAddress.Text = networkMachineToEdit.IPAddress.ToString();
        }

        /// <summary>
        /// Gets the created NetworkMachine.
        /// </summary>
        /// <returns></returns>
        internal NetworkMachine GetNetworkMachine()
        {
            return this.networkMachine;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var name = this.textBoxName.Text.Trim();
            var ip = this.textBoxIPAddress.Text;

            var machine = new NetworkMachine();

            if (!string.IsNullOrEmpty(name))
                machine.Name = name;

            if (!string.IsNullOrEmpty(ip))
            {
                if(NetworkTools.IsStringIPAddress(ip))
                    machine.IPAddress = IPAddress.Parse(ip);
                else
                {
                    MessageBox.Show(Wsapm.Resources.Wsapm.AddNetworkMachineWindow_MessageIPNoRealIP, Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (string.IsNullOrEmpty(machine.Name) && machine.IPAddress == null)
            {
                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddNetworkMachineWindow_NoInfoError, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Avoid to add a element twice.
            if (this.editNetworkMachineCopy == null)
            {
                // Add new mode.
                if (this.allNetworkMachines != null)
                {
                    for (int i = 0; i < this.allNetworkMachines.Length; i++)
                    {
                        if (this.allNetworkMachines[i] == machine)
                        {
                            MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddNetworkMachineWindow_NetworkMachineAlreadyAdded, machine.ToString()), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }
            else
            {
                // Edit mode.
                if (machine == this.editNetworkMachineCopy)
                {
                    // Element was not changed.
                    this.DialogResult = false;
                    this.Close();
                    return;
                }
                else
                {
                    // Element was changed.
                    if (this.allNetworkMachines != null)
                    {
                        for (int i = 0; i < this.allNetworkMachines.Length; i++)
                        {
                            if (this.allNetworkMachines[i] == machine)
                            {
                                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddNetworkMachineWindow_NetworkMachineAlreadyAdded, machine.ToString()), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                }
            }

            this.networkMachine = machine;
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxName.Focus();
        }

        private void buttonChooseNetworkMachine_Click(object sender, RoutedEventArgs e)
        {
            var nmcw = new NetworkMachineChoiceWindow();

            var result = nmcw.ShowDialog();

            if (result == true)
            {
                NetworkMachine machine = nmcw.GetNetworkMachine();

                if (machine == null)
                    return;

                if (!string.IsNullOrEmpty(machine.Name))
                    this.textBoxName.Text = machine.Name;
                else if (machine.IPAddress != null)
                    this.textBoxIPAddress.Text = machine.IPAddress.ToString();
            }
        }
    }
}
