using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddProcessToStartWindow.xaml
    /// </summary>
    public partial class AddProcessToStartWindow : Window
    {
        private ProgramStart programStart;
        private ProgramStart[] allProgramsToStart;
        private ProgramStart editProgramToStartCopy = null;

        public AddProcessToStartWindow(ProgramStart[] allProgramsToStart)
        {
            InitializeComponent();

            this.allProgramsToStart = allProgramsToStart;
        }

        public AddProcessToStartWindow(ProgramStart programToEdit, ProgramStart[] allProgramsToStart)
            : this(allProgramsToStart)
        {
            this.Title = Wsapm.Resources.Wsapm.AddProcessToStartWindow_TitleEdit;
            this.editProgramToStartCopy = programToEdit.Copy();
            this.textBoxProgram.Text = programToEdit.FileName;
            this.textBoxArgs.Text = programToEdit.Args;
        }

        /// <summary>
        /// Gets the ProgramStart specified.
        /// </summary>
        /// <returns></returns>
        internal ProgramStart GetProgramStart()
        {
            return this.programStart;
        }

        private void buttonChooseProgram_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();

            ofd.Filter = Wsapm.Resources.Wsapm.AddProcessToStartWindow_OpenFileDialogFilter;
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
                this.textBoxProgram.Text = ofd.FileName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxProgram.Focus();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var program = this.textBoxProgram.Text.Trim();
            var args = this.textBoxArgs.Text.Trim();

            if (string.IsNullOrEmpty(program))
            {
                MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.AddProcessToStartWindow_NoInfoError, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(program))
            {
                if (!FileTools.ExecutableFileExistsInEnvironmentVariablePaths(program))
                {
                    var result = MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.AddProcessToStartWindow_ProgramDoesNotExistWarning, program, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return; 
                }
            }

            if (string.IsNullOrEmpty(args))
                args = null;

            ProgramStart ps = new ProgramStart(program, args);

            // Avoid to add a element twice.
            if (this.editProgramToStartCopy == null)
            {
                // Add new mode.
                if (this.allProgramsToStart != null)
                {
                    for (int i = 0; i < this.allProgramsToStart.Length; i++)
                    {
                        if (this.allProgramsToStart[i] == ps)
                        {
                            MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddProcessToStartWindow_ProcessAlreadyAdded, ps.ToString()), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }
            else
            {
                // Edit mode.
                if (ps == this.editProgramToStartCopy)
                {
                    // Element was not changed.
                    this.DialogResult = false;
                    this.Close();
                    return;
                }
                else
                {
                    // Element was changed.
                    if (this.allProgramsToStart != null)
                    {
                        for (int i = 0; i < this.allProgramsToStart.Length; i++)
                        {
                            if (this.allProgramsToStart[i] == ps)
                            {
                                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddProcessToStartWindow_ProcessAlreadyAdded, ps.ToString()), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                }
            }

            this.programStart = new ProgramStart(program, args);
            this.DialogResult = true;
            this.Close();
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            var program = this.textBoxProgram.Text.Trim();
            var args = this.textBoxArgs.Text.Trim();

            if (string.IsNullOrEmpty(program))
            {
                MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.AddProcessToStartWindow_NoInfoError, Environment.NewLine), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(args))
                args = null;

            var pStart = new ProgramStart(program, args);

            try
            {
                var psi = new ProcessStartInfo();
                psi.FileName = pStart.FileName;
                psi.Arguments = pStart.Args;
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Wsapm.Resources.Wsapm.AddProcessToStartWindow_ErrorStartProgramTest, Environment.NewLine, pStart.FileName + " " + pStart.Args, ex.Message), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
