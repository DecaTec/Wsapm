using System;
using System.ServiceProcess;
using System.Windows;
using System.Linq;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddServiceToRestartWindow.xaml
    /// </summary>
    public partial class AddServiceToRestartWindow : Window
    {
        private Service service;
        private Service[] allServicesToRestart;
        private Service editServiceCopy = null;

        public AddServiceToRestartWindow(Service[] allServicesToRestart)
        {
            InitializeComponent();

            this.allServicesToRestart = allServicesToRestart;
        }

        public AddServiceToRestartWindow(Service serviceToEdit, Service[] allServicesToRestart)
            : this(allServicesToRestart)
        {
            this.Title = Wsapm.Resources.Wsapm.AddServiceToRestartWindow_TitleEdit;
            this.editServiceCopy = serviceToEdit;
            this.textBoxService.Text = serviceToEdit.ServiceName;
        }

        /// <summary>
        /// Gets the chosen service.
        /// </summary>
        /// <returns></returns>
        internal Service GetService()
        {
            return this.service;
        }

        private void buttonAddService_Click(object sender, RoutedEventArgs e)
        {
            ServiceChoiceWindow scw = new ServiceChoiceWindow();

            var result = scw.ShowDialog();

            if (result == true)
                this.textBoxService.Text = scw.GetService();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxService.Focus();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var serviceStr = this.textBoxService.Text.Trim();

            if (string.IsNullOrEmpty(serviceStr))
            {
                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddServiceToRestartWindow_NoInfoError, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var serviceExists = ServiceController.GetServices().Where(s => s.ServiceName == serviceStr || s.DisplayName == serviceStr).Count() > 0;

            if (!serviceExists)
            {
                var result = MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.AddServiceToRestartWindow_ServiceDoesNotExistWarning, serviceStr, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;
            }

            // Avoid to add a element twice.
            if (this.editServiceCopy == null)
            {
                // Add new mode.
                if (this.allServicesToRestart != null)
                {
                    for (int i = 0; i < this.allServicesToRestart.Length; i++)
                    {
                        if (this.allServicesToRestart[i].ServiceName == serviceStr)
                        {
                            MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddServiceToRestartWindow_ServiceAlreadyAdded, serviceStr), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    } 
                }
            }
            else
            {
                // Edit mode.
                if (serviceStr == this.editServiceCopy.ServiceName)
                {
                    // Element was not changed.
                    this.DialogResult = false;
                    this.Close();
                    return;
                }
                else
                {
                    // Element was changed.
                    if (this.allServicesToRestart != null)
                    {
                        for (int i = 0; i < this.allServicesToRestart.Length; i++)
                        {
                            if (this.allServicesToRestart[i].ServiceName == serviceStr)
                            {
                                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddServiceToRestartWindow_ServiceAlreadyAdded, serviceStr), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        } 
                    }
                }
            }

            this.service = new Service(serviceStr);
            this.DialogResult = true;
            this.Close();
        }
    }
}
