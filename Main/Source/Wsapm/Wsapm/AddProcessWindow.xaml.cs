using System;
using System.Windows;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddProcessWindow.xaml
    /// </summary>
    public partial class AddProcessWindow : Window
    {
        private ProcessToMonitor process;
        private ProcessToMonitor[] allProcesses;
        private ProcessToMonitor editProcessCopy = null;

        public AddProcessWindow(ProcessToMonitor[] allProcesses)
        {
            InitializeComponent();

            this.allProcesses = allProcesses;
        }

        public AddProcessWindow(ProcessToMonitor processToEdit, ProcessToMonitor[] allProcesses)
            : this(allProcesses)
        {
            this.Title = Wsapm.Resources.Wsapm.AddProcessWindow_TitleEdit;
            this.editProcessCopy = processToEdit;
            this.textBoxProcess.Text = processToEdit.ProcessName;
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        /// <returns></returns>
        internal ProcessToMonitor GetProcess()
        {
            return this.process;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var processStr = this.textBoxProcess.Text.Trim();

            if (string.IsNullOrEmpty(processStr))
            {
                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddProcessWindow_NoInfoError, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Avoid to add a element twice.
            if (this.editProcessCopy == null)
            {
                // Add new mode.
                if (this.allProcesses != null)
                {
                    for (int i = 0; i < this.allProcesses.Length; i++)
                    {
                        if (this.allProcesses[i].ProcessName == processStr)
                        {
                            MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddProcessWindow_ProcessAlreadyAdded, processStr), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }
            else
            {
                // Edit mode.
                if (processStr == this.editProcessCopy.ProcessName)
                {
                    // Element was not changed.
                    this.DialogResult = false;
                    this.Close();
                    return;
                }
                else
                {
                    // Element was changed.
                    if (this.allProcesses != null)
                    {
                        for (int i = 0; i < this.allProcesses.Length; i++)
                        {
                            if (this.allProcesses[i].ProcessName == processStr)
                            {
                                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddProcessWindow_ProcessAlreadyAdded, processStr), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                }
            }

            this.process = new ProcessToMonitor(processStr);
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxProcess.Focus();
        }

        private void buttonAddProcess_Click(object sender, RoutedEventArgs e)
        {
            ProcessChoiceWindow pcw = new ProcessChoiceWindow();

            var result = pcw.ShowDialog();

            if (result == true)
                this.textBoxProcess.Text = pcw.GetProcess();
        }
    }
}
