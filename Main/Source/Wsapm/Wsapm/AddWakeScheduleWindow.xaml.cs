using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddWakeScheduleWindow.xaml
    /// </summary>
    public partial class AddWakeScheduleWindow : Window
    {
        private WakeScheduler wakeScheduler;
        private WakeScheduler[] allWakeSchedulers;
        private WakeScheduler editWakeSchedulerCopy;

        public AddWakeScheduleWindow(WakeScheduler[] allWakeSchedulers)
        {
            InitializeComponent();

            this.allWakeSchedulers = allWakeSchedulers;
            InitUI();
        }

        public AddWakeScheduleWindow(WakeScheduler wakeSchedulerToEdit, WakeScheduler[] allWakeSchedulers)
            : this(allWakeSchedulers)
        {
            this.Title = Wsapm.Resources.Wsapm.AddWakeScheduleWindow_TitleEdit;
            this.editWakeSchedulerCopy = wakeSchedulerToEdit.Copy();
            InitUI(this.editWakeSchedulerCopy);
        }

        private void InitUI()
        {
            this.upDownWakeInterval.Value = 1;
            FillComboBoxRepeatInterval();
            this.dateTimePickerStartTime.Value = DateTime.Now;
            this.dateTimePickerEndTime.Value = DateTime.Now;
            EnableDisableWakeOptions();
            EnableDisableRepeatOptions();
            EnableDisableEndTimeOptions();
            EnableDisableStartProgramsWhenSystemAlreadyRunning();
            this.checkBoxWake.IsChecked = true;
        }

        private void InitUI(WakeScheduler wakeScheduler)
        {
            if (this.editWakeSchedulerCopy != null)
            {
                this.checkBoxWake.IsChecked = this.editWakeSchedulerCopy.EnableWakeScheduler;
                EnableDisableWakeOptions();

                this.dateTimePickerStartTime.Value = this.editWakeSchedulerCopy.DueTime;

                this.checkBoxEnableRepeat.IsChecked = this.editWakeSchedulerCopy.EnableRepeat;

                var repeat = this.editWakeSchedulerCopy.RepeatAfter;

                if (this.editWakeSchedulerCopy.RepeatAfter.Minutes != 0)
                {
                    // Minutes.
                    this.comboBoxWakeInterval.SelectedIndex = 0;
                    this.upDownWakeInterval.Value = repeat.Minutes;
                }
                else if (this.editWakeSchedulerCopy.RepeatAfter.Hours != 0)
                {
                    // Hours.
                    this.comboBoxWakeInterval.SelectedIndex = 1;
                    this.upDownWakeInterval.Value = repeat.Hours;
                }
                else if (this.editWakeSchedulerCopy.RepeatAfter.Days != 0)
                {
                    // Days.
                    this.comboBoxWakeInterval.SelectedIndex = 2;
                    this.upDownWakeInterval.Value = repeat.Days;
                }

                EnableDisableRepeatOptions();

                this.checkBoxEnableEndTime.IsChecked = this.editWakeSchedulerCopy.EnableEndTime;
                EnableDisableEndTimeOptions();

                // Start programs setting.
                this.checkBoxStartProgramsAfterWake.IsChecked = this.editWakeSchedulerCopy.EnableStartProgramsAfterWake;
                FilldataGridStartProgramsAfterWake(this.editWakeSchedulerCopy);
                EnableDisableStartProgramsAfterWakeOptions();                

                this.checkBoxStartProgramsWhenSystemAlreadyRunning.IsChecked = this.editWakeSchedulerCopy.StartProgramsWhenSystemIsAlreadyRunning;
            }
        }

        /// <summary>
        /// Gets the wake scheduler specified.
        /// </summary>
        /// <returns></returns>
        internal WakeScheduler GetWakeScheduler()
        {
            return this.wakeScheduler;
        }

        private void FillComboBoxRepeatInterval()
        {
            this.comboBoxWakeInterval.Items.Clear();
            this.comboBoxWakeInterval.Items.Add(Wsapm.Resources.Wsapm.AddWakeScheduleWindow_ComboBoxRepeatIntervalMinutes);
            this.comboBoxWakeInterval.Items.Add(Wsapm.Resources.Wsapm.AddWakeScheduleWindow_ComboBoxRepeatIntervalHours);
            this.comboBoxWakeInterval.Items.Add(Wsapm.Resources.Wsapm.AddWakeScheduleWindow_ComboBoxRepeatIntervalDays);
            this.comboBoxWakeInterval.SelectedIndex = 2;
        }

        private void EnableDisableWakeOptions()
        {
            var enabled = this.checkBoxWake.IsChecked.Value;
            this.dateTimePickerStartTime.IsEnabled = enabled;
            this.checkBoxEnableRepeat.IsEnabled = enabled;
            this.upDownWakeInterval.IsEnabled = enabled;
            this.comboBoxWakeInterval.IsEnabled = enabled;
            this.checkBoxEnableEndTime.IsEnabled = enabled;
            this.dateTimePickerEndTime.IsEnabled = enabled;
            this.checkBoxStartProgramsAfterWake.IsEnabled = enabled;
            EnableDisableRepeatOptions();
            EnableDisableEndTimeOptions();
            EnableDisableStartProgramsAfterWakeOptions();
        }

        private void EnableDisableStartProgramsAfterWakeOptions()
        {
            var enabled = this.checkBoxStartProgramsAfterWake.IsChecked.Value;
            this.dataGridStartProgramsAfterWake.IsEnabled = enabled;
            this.buttonAddStartProgramAfterWake.IsEnabled = enabled;
            this.buttonRemoveStartProgramAfterWake.IsEnabled = enabled;
            this.buttonEditStartProgramAfterWake.IsEnabled = enabled;
        }

        private void EnableDisableStartProgramsWhenSystemAlreadyRunning()
        {
            var enabled = this.checkBoxStartProgramsAfterWake.IsChecked.Value;
            this.checkBoxStartProgramsWhenSystemAlreadyRunning.IsEnabled = enabled;
        }

        private void EnableDisableRepeatOptions()
        {
            var enabled = this.checkBoxEnableRepeat.IsChecked.Value;
            this.upDownWakeInterval.IsEnabled = enabled;
            this.comboBoxWakeInterval.IsEnabled = enabled;
        }

        private void EnableDisableEndTimeOptions()
        {
            var enabled = this.checkBoxEnableEndTime.IsChecked.Value;
            this.dateTimePickerEndTime.IsEnabled = enabled;
        }

        private void FilldataGridStartProgramsAfterWake(WakeScheduler wakeSchedulerToEdit)
        {
            this.dataGridStartProgramsAfterWake.Items.Clear();

            for (int i = 0; i < wakeSchedulerToEdit.StartProgramsAfterWake.Count; i++)
            {
                this.dataGridStartProgramsAfterWake.Items.Add(wakeSchedulerToEdit.StartProgramsAfterWake[i]);
            }
        }

        private WakeScheduler BuildWakeScheduler()
        {
            var scheduler = new WakeScheduler();
            scheduler.EnableWakeScheduler = this.checkBoxWake.IsChecked.Value;
            scheduler.DueTime = this.dateTimePickerStartTime.Value.Value;

            scheduler.EnableRepeat = this.checkBoxEnableRepeat.IsChecked.Value;

            if (this.comboBoxWakeInterval.SelectedIndex == 0)
            {
                // Minutes.
                scheduler.RepeatAfter = TimeSpan.FromMinutes((int)this.upDownWakeInterval.Value);
            }
            else if (this.comboBoxWakeInterval.SelectedIndex == 1)
            {
                // Hours.
                scheduler.RepeatAfter = TimeSpan.FromHours((int)this.upDownWakeInterval.Value);
            }
            else if (this.comboBoxWakeInterval.SelectedIndex == 2)
            {
                // Days.
                scheduler.RepeatAfter = TimeSpan.FromDays((int)this.upDownWakeInterval.Value);
            }

            scheduler.EnableEndTime = this.checkBoxEnableEndTime.IsChecked.Value;
            scheduler.EndTime = this.dateTimePickerEndTime.Value.Value;
            scheduler.EnableStartProgramsAfterWake = this.checkBoxStartProgramsAfterWake.IsChecked.Value;

            List<ProgramStart> programsToStart = new List<ProgramStart>(this.dataGridStartProgramsAfterWake.Items.Count);

            foreach (var item in this.dataGridStartProgramsAfterWake.Items)
            {
                var programToStart = item as ProgramStart;

                if (programToStart == null)
                    continue;

                programsToStart.Add(programToStart);
            }

            scheduler.StartProgramsAfterWake = programsToStart;

            scheduler.StartProgramsWhenSystemIsAlreadyRunning = this.checkBoxStartProgramsWhenSystemAlreadyRunning.IsChecked.Value;

            return scheduler;
        }

        private void checkBoxWake_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableWakeOptions();
        }

        private void checkBoxWake_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableWakeOptions();
        }

        private void dateTimePickerStartTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!this.checkBoxEnableEndTime.IsChecked.Value)
            {
                var dtTmp = DateTime.Now;

                if (this.dateTimePickerStartTime.Value.HasValue)
                    dtTmp = this.dateTimePickerStartTime.Value.Value;
                else
                    this.dateTimePickerStartTime.Value = dtTmp;

                this.dateTimePickerEndTime.Value = dtTmp + TimeSpan.FromDays(1);
            }
        }

        private void checkBoxEnableRepeat_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableRepeatOptions();
        }

        private void checkBoxEnableRepeat_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableRepeatOptions();
        }

        private void upDownWakeInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownWakeInterval.Value == null)
                this.upDownWakeInterval.Value = 1;
        }

        private void checkBoxEnableEndTime_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableEndTimeOptions();
        }

        private void checkBoxEnableEndTime_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableEndTimeOptions();
        }

        private void dateTimePickerEndTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!this.dateTimePickerEndTime.Value.HasValue)
            {
                if (DateTime.Now <= this.dateTimePickerStartTime.Value.Value)
                    this.dateTimePickerEndTime.Value = this.dateTimePickerStartTime.Value.Value + TimeSpan.FromDays(1);
                else
                    this.dateTimePickerEndTime.Value = DateTime.Now;
            }
        }

        private void checkBoxStartProgramsAfterWake_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableStartProgramsAfterWakeOptions();
            EnableDisableStartProgramsWhenSystemAlreadyRunning();
        }

        private void checkBoxStartProgramsAfterWake_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableStartProgramsAfterWakeOptions();
            EnableDisableStartProgramsWhenSystemAlreadyRunning();
        }

        private void buttonAddStartProgramAfterWake_Click(object sender, RoutedEventArgs e)
        {
            var psList = new List<ProgramStart>(this.dataGridStartProgramsAfterWake.Items.Count);

            for (int i = 0; i < this.dataGridStartProgramsAfterWake.Items.Count; i++)
            {
                psList.Add((ProgramStart)this.dataGridStartProgramsAfterWake.Items[i]);
            }

            var addForm = new AddProcessToStartWindow(psList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return;

            var p = addForm.GetProgramStart();

            if (p == null)
                return;

            this.dataGridStartProgramsAfterWake.Items.Add(p);
        }

        private void buttonRemoveStartProgramAfterWake_Click(object sender, RoutedEventArgs e)
        {
            var p = this.dataGridStartProgramsAfterWake.SelectedItem as ProgramStart;

            if (p == null)
                return;

            this.dataGridStartProgramsAfterWake.Items.Remove(p);
        }

        private void buttonEditStartProgramAfterWake_Click(object sender, RoutedEventArgs e)
        {
            EditStartProgramsAfterWake();
        }

        private void dataGridStartProgramsAfterWake_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditStartProgramsAfterWake();
        }

        private void EditStartProgramsAfterWake()
        {
            var selectedItem = this.dataGridStartProgramsAfterWake.SelectedItem as ProgramStart;
            var selectedIndex = this.dataGridStartProgramsAfterWake.SelectedIndex;

            if (selectedItem == null)
                return;

            var editItem = EditStartProgramsAfterWake(selectedItem);

            if (editItem != null)
            {
                this.dataGridStartProgramsAfterWake.Items[selectedIndex] = editItem;
            }
        }

        private ProgramStart EditStartProgramsAfterWake(ProgramStart selectedItem)
        {
            var psList = new List<ProgramStart>(this.dataGridStartProgramsAfterWake.Items.Count);

            for (int i = 0; i < this.dataGridStartProgramsAfterWake.Items.Count; i++)
            {
                psList.Add((ProgramStart)this.dataGridStartProgramsAfterWake.Items[i]);
            }

            var addForm = new AddProcessToStartWindow(selectedItem, psList.ToArray());
            var result = addForm.ShowDialog();

            if (result != true)
                return null;

            var p = addForm.GetProgramStart();

            if (p == null)
                return null;

            return p;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var ws = BuildWakeScheduler();
            this.wakeScheduler = ws;
            this.DialogResult = true;
            this.Close();
        }
    }
}
