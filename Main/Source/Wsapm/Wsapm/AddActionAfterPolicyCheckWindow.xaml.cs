using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddActionAfterPolicyCheck.xaml
    /// </summary>
    public partial class AddActionAfterPolicyCheckWindow : Window
    {
        private ActionAfterPolicyCheck actionAfterPolicyCheck;
        private ActionAfterPolicyCheck[] allActionsAfterPolicyCheck;
        private ActionAfterPolicyCheck editActionAfterPolicyCheckCopy = null;
        private Guid[] guidsAllActionsAfterPolicyCheck;
        private Guid guidCurrentActionAfterPolicyCheck;

        public AddActionAfterPolicyCheckWindow(ActionAfterPolicyCheck[] allActionsAfterPolicyCheck)
        {
            InitializeComponent();

            this.allActionsAfterPolicyCheck = allActionsAfterPolicyCheck;
            FillGuids(this.allActionsAfterPolicyCheck, null);
            InitUi();
        }

        public AddActionAfterPolicyCheckWindow(ActionAfterPolicyCheck actionAfterPolicyCheckToEdit, ActionAfterPolicyCheck[] allActionsAfterPolicyCheck)
            : this(allActionsAfterPolicyCheck)
        {
            this.Title = Wsapm.Resources.Wsapm.AddActionAfterPolicyCheck_TitleEdit;
            this.editActionAfterPolicyCheckCopy = actionAfterPolicyCheckToEdit.Copy();
            FillGuids(this.allActionsAfterPolicyCheck, editActionAfterPolicyCheckCopy);

            if (actionAfterPolicyCheckToEdit.ProgramStart != null)
            {
                this.textBoxProgram.Text = actionAfterPolicyCheckToEdit.ProgramStart.FileName;
                this.textBoxArgs.Text = actionAfterPolicyCheckToEdit.ProgramStart.Args;
            }

            InitUi(this.editActionAfterPolicyCheckCopy);
        }

        private void FillGuids(ActionAfterPolicyCheck[] allActionsAfterPolicyCheck, ActionAfterPolicyCheck copiedAction)
        {
            if (copiedAction != null)
                this.guidCurrentActionAfterPolicyCheck = Guid.NewGuid();

            this.guidsAllActionsAfterPolicyCheck = new Guid[allActionsAfterPolicyCheck.Length];

            for (int i = 0; i < allActionsAfterPolicyCheck.Length; i++)
            {
                if (copiedAction != null && copiedAction == allActionsAfterPolicyCheck[i])
                    this.guidsAllActionsAfterPolicyCheck[i] = this.guidCurrentActionAfterPolicyCheck;
                else
                    this.guidsAllActionsAfterPolicyCheck[i] = Guid.NewGuid();
            }
        }

        private void InitUi()
        {
            FillComboBoxActionTrigger();
            FillComboBoxAction();
            SetVisibilityOfProgramStartOptions();
        }

        private void InitUi(ActionAfterPolicyCheck action)
        {
            if (this.editActionAfterPolicyCheckCopy != null)
            {
                this.comboBoxActionTrigger.SelectedIndex = (int)this.editActionAfterPolicyCheckCopy.ActionModeAfterPolicyCheck;
                this.comboBoxAction.SelectedIndex = (int)this.editActionAfterPolicyCheckCopy.ActionTypeAfterPolicyCheck;
            }
        }

        private void FillComboBoxActionTrigger()
        {
            this.comboBoxActionTrigger.Items.Clear();
            this.comboBoxActionTrigger.Items.Add(Wsapm.Resources.Wsapm.AddActionAfterPolicyCheckWindow_ComboBoxActionTriggerAtLeatOnePolicySatisfied);
            this.comboBoxActionTrigger.Items.Add(Wsapm.Resources.Wsapm.AddActionAfterPolicyCheckWindow_ComboBoxActionTriggerNoPolicySatisfied);
            this.comboBoxActionTrigger.SelectedIndex = 0;
        }

        private void FillComboBoxAction()
        {
            this.comboBoxAction.Items.Clear();
            this.comboBoxAction.Items.Add(Wsapm.Resources.Wsapm.AddActionAfterPolicyCheckWindow_ComboBoxActionStandby);
            this.comboBoxAction.Items.Add(Wsapm.Resources.Wsapm.AddActionAfterPolicyCheckWindow_ComboBoxActionHibernate);
            this.comboBoxAction.Items.Add(Wsapm.Resources.Wsapm.AddActionAfterPolicyCheckWindow_ComboBoxActionShutdown);
            this.comboBoxAction.Items.Add(Wsapm.Resources.Wsapm.AddActionAfterPolicyCheckWindow_ComboBoxActionStartProgram);
            this.comboBoxAction.SelectedIndex = 0;
        }

        /// <summary>
        /// Gets the wake scheduler specified.
        /// </summary>
        /// <returns></returns>
        internal ActionAfterPolicyCheck GetActionAfterPolicyCheck()
        {
            return this.actionAfterPolicyCheck;
        }

        private void buttonChooseProgram_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();

            ofd.Filter = Wsapm.Resources.Wsapm.AddProcessToStartWindow_OpenFileDialogFilter;
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
                this.textBoxProgram.Text = ofd.FileName;
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

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var action = new ActionAfterPolicyCheck();

            if (this.comboBoxActionTrigger.SelectedIndex == 0)
                action.ActionModeAfterPolicyCheck = ActionModeAfterPolicyCheck.AtLeastOnePolicySatisfied;
            else if (this.comboBoxActionTrigger.SelectedIndex == 1)
                action.ActionModeAfterPolicyCheck = ActionModeAfterPolicyCheck.NoPolicySatisfied;

            if (this.comboBoxAction.SelectedIndex == 0)
                action.ActionTypeAfterPolicyCheck = ActionTypeAfterPolicyCheck.Standby;
            else if (this.comboBoxAction.SelectedIndex == 1)
                action.ActionTypeAfterPolicyCheck = ActionTypeAfterPolicyCheck.Hibernate;
            else if (this.comboBoxAction.SelectedIndex == 2)
                action.ActionTypeAfterPolicyCheck = ActionTypeAfterPolicyCheck.Shutdown;
            else if (this.comboBoxAction.SelectedIndex == 3)
            {
                action.ActionTypeAfterPolicyCheck = ActionTypeAfterPolicyCheck.StartProgram;

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

                action.ProgramStart = new ProgramStart(program, args);
            }

            // Avoid to add a element twice.
            if (this.editActionAfterPolicyCheckCopy == null)
            {
                // Add new mode.
                if (this.allActionsAfterPolicyCheck != null)
                {
                    if (ActionAlreadyExists(action))
                        return;

                    if (AddedSecondEnergyStateChangeAction(action))
                        return;
                }
            }
            else
            {
                // Edit mode.
                if (action == this.editActionAfterPolicyCheckCopy)
                {
                    // Element was not changed.
                    this.DialogResult = false;
                    this.Close();
                    return;
                }
                else
                {
                    // Element was changed.
                    if (this.allActionsAfterPolicyCheck != null)
                    {
                        if (ActionAlreadyExists(action))
                            return;

                        if (AddedSecondEnergyStateChangeAction(action))
                            return;
                    }
                }
            }

            this.actionAfterPolicyCheck = action;
            this.DialogResult = true;
            this.Close();
        }

        private bool ActionAlreadyExists(ActionAfterPolicyCheck actionToAdd)
        {
            // Avoid adding the same action twice.
            for (int i = 0; i < this.allActionsAfterPolicyCheck.Length; i++)
            {
                if (this.allActionsAfterPolicyCheck[i] == actionToAdd)
                {
                    MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddActionAfterPolicyCheck_ActionAlreadyAdded, actionToAdd.ToString()), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }

            return false;
        }

        private bool AddedSecondEnergyStateChangeAction(ActionAfterPolicyCheck actionToAdd)
        {
            // Avoid adding more actions which will change energy state.
            if (actionToAdd.ActionTypeAfterPolicyCheck == ActionTypeAfterPolicyCheck.Hibernate
                || actionToAdd.ActionTypeAfterPolicyCheck == ActionTypeAfterPolicyCheck.Shutdown 
                || actionToAdd.ActionTypeAfterPolicyCheck == ActionTypeAfterPolicyCheck.Standby)
            {
                for (int i = 0; i < this.allActionsAfterPolicyCheck.Length; i++)
                {
                    if ((this.guidsAllActionsAfterPolicyCheck != null && this.guidsAllActionsAfterPolicyCheck.Length != 0 && this.guidCurrentActionAfterPolicyCheck != this.guidsAllActionsAfterPolicyCheck[i]) &&
                        this.allActionsAfterPolicyCheck[i].ActionModeAfterPolicyCheck == actionToAdd.ActionModeAfterPolicyCheck &&
                        (this.allActionsAfterPolicyCheck[i].ActionTypeAfterPolicyCheck == ActionTypeAfterPolicyCheck.Hibernate
                        || this.allActionsAfterPolicyCheck[i].ActionTypeAfterPolicyCheck == ActionTypeAfterPolicyCheck.Shutdown
                        || this.allActionsAfterPolicyCheck[i].ActionTypeAfterPolicyCheck == ActionTypeAfterPolicyCheck.Standby))
                    {
                        // Tried to add another action which will change energy state -> error.
                        MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddActionAfterPolicyCheck_ActionForChangeEneryStateAdded, this.allActionsAfterPolicyCheck[i].ToString()), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        return true;
                    }
                }
            }

            return false;
        }

        private void comboBoxAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVisibilityOfProgramStartOptions();
        }

        private void SetVisibilityOfProgramStartOptions()
        {
            if (this.comboBoxAction.SelectedIndex != 3)
            {
                this.groupBoxStartProgram.Visibility = System.Windows.Visibility.Collapsed;
                this.labelProgramToStart.Visibility = System.Windows.Visibility.Collapsed;
                this.textBoxProgram.Visibility = System.Windows.Visibility.Collapsed;
                this.buttonChooseProgram.Visibility = System.Windows.Visibility.Collapsed;
                this.LabelProgramToStartArguments.Visibility = System.Windows.Visibility.Collapsed;
                this.textBoxArgs.Visibility = System.Windows.Visibility.Collapsed;
                this.buttonTest.Visibility = System.Windows.Visibility.Collapsed;
                this.groupBoxStandbyHibernate.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.groupBoxStartProgram.Visibility = System.Windows.Visibility.Visible;
                this.labelProgramToStart.Visibility = System.Windows.Visibility.Visible;
                this.textBoxProgram.Visibility = System.Windows.Visibility.Visible;
                this.buttonChooseProgram.Visibility = System.Windows.Visibility.Visible;
                this.LabelProgramToStartArguments.Visibility = System.Windows.Visibility.Visible;
                this.textBoxArgs.Visibility = System.Windows.Visibility.Visible;
                this.buttonTest.Visibility = System.Windows.Visibility.Visible;
                this.groupBoxStandbyHibernate.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
