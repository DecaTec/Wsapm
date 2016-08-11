using System;
using System.Windows;
using Wsapm.Core;
using Xceed.Wpf.Toolkit;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddUptimeScheduleWindow.xaml
    /// </summary>
    public partial class AddUptimeScheduleWindow : Window
    {
        private UptimeScheduler uptimeScheduler;
        private UptimeScheduler[] allUptimeSchedulers;
        private UptimeScheduler editUptimeSchedulerCopy;

        public AddUptimeScheduleWindow(UptimeScheduler[] allUptimeSchedulers)
        {
            InitializeComponent();

            this.allUptimeSchedulers = allUptimeSchedulers;
            InitUI();
        }

        public AddUptimeScheduleWindow(UptimeScheduler uptimeSchedulerToEdit, UptimeScheduler[] allUptimeSchedulers)
            : this(allUptimeSchedulers)
        {
            this.Title = Wsapm.Resources.Wsapm.AddUptimeScheduleWindow_TitleEdit;
            this.editUptimeSchedulerCopy = uptimeSchedulerToEdit.Copy();
            InitUI(this.editUptimeSchedulerCopy);
        }

        private void InitUI()
        {
            this.upDownUptimeDurationHours.Value = 1;
            this.upDownUptimeDurationMinutes.Value = 0;
            this.upDownUptimeInterval.Value = 1;
            FillComboBoxRepeatInterval();
            this.dateTimePickerStartTime.Value = DateTime.Now;
            this.dateTimePickerEndTime.Value = DateTime.Now;
            EnableDisableUptimeOptions();
            EnableDisableRepeatOptions();
            EnableDisableEndTimeOptions();
            this.checkBoxUptime.IsChecked = true;
        }       

        private void InitUI(UptimeScheduler uptimeScheduler)
        {
            if (this.editUptimeSchedulerCopy != null)
            {
                this.checkBoxUptime.IsChecked = this.editUptimeSchedulerCopy.EnableUptimeScheduler;
                EnableDisableUptimeOptions();

                this.dateTimePickerStartTime.Value = this.editUptimeSchedulerCopy.DueTime;

                this.checkBoxEnableRepeat.IsChecked = this.editUptimeSchedulerCopy.EnableRepeat;

                this.upDownUptimeDurationHours.Value = this.editUptimeSchedulerCopy.Duration.Hours;
                this.upDownUptimeDurationMinutes.Value = this.editUptimeSchedulerCopy.Duration.Minutes;

                var repeat = this.editUptimeSchedulerCopy.RepeatAfter;

                if (this.editUptimeSchedulerCopy.RepeatAfter.Minutes != 0)
                {
                    // Minutes.
                    this.comboBoxUptimeInterval.SelectedIndex = 0;
                    this.upDownUptimeInterval.Value = repeat.Minutes;
                }
                else if (this.editUptimeSchedulerCopy.RepeatAfter.Hours != 0)
                {
                    // Hours.
                    this.comboBoxUptimeInterval.SelectedIndex = 1;
                    this.upDownUptimeInterval.Value = repeat.Hours;
                }
                else if (this.editUptimeSchedulerCopy.RepeatAfter.Days != 0)
                {
                    // Days.
                    this.comboBoxUptimeInterval.SelectedIndex = 2;
                    this.upDownUptimeInterval.Value = repeat.Days;
                }

                EnableDisableRepeatOptions();

                this.checkBoxEnableEndTime.IsChecked = this.editUptimeSchedulerCopy.EnableEndTime;
                EnableDisableEndTimeOptions();
            }
        }

        private void FillComboBoxRepeatInterval()
        {
            this.comboBoxUptimeInterval.Items.Clear();
            this.comboBoxUptimeInterval.Items.Add(Wsapm.Resources.Wsapm.AddUptimeScheduleWindow_ComboBoxRepeatIntervalMinutes);
            this.comboBoxUptimeInterval.Items.Add(Wsapm.Resources.Wsapm.AddUptimeScheduleWindow_ComboBoxRepeatIntervalHours);
            this.comboBoxUptimeInterval.Items.Add(Wsapm.Resources.Wsapm.AddUptimeScheduleWindow_ComboBoxRepeatIntervalDays);
            this.comboBoxUptimeInterval.SelectedIndex = 1;
        }

        /// <summary>
        /// Gets the uptime scheduler specified.
        /// </summary>
        /// <returns></returns>
        internal UptimeScheduler GetUptimeScheduler()
        {
            return this.uptimeScheduler;
        }

        private void checkBoxUptime_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableUptimeOptions();
        }

        private void checkBoxUptime_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableUptimeOptions();
        }

        private void EnableDisableUptimeOptions()
        {
            var enabled = this.checkBoxUptime.IsChecked.Value;
            this.dateTimePickerStartTime.IsEnabled = enabled;
            this.checkBoxEnableRepeat.IsEnabled = enabled;
            this.upDownUptimeInterval.IsEnabled = enabled;
            this.comboBoxUptimeInterval.IsEnabled = enabled;
            this.checkBoxEnableEndTime.IsEnabled = enabled;
            this.dateTimePickerEndTime.IsEnabled = enabled;
            EnableDisableEndTimeOptions();
            EnableDisableRepeatOptions();  
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

        private void EnableDisableRepeatOptions()
        {
            var enabled = this.checkBoxEnableRepeat.IsChecked.Value;
            this.upDownUptimeInterval.IsEnabled = enabled;
            this.comboBoxUptimeInterval.IsEnabled = enabled;
        }

        private void checkBoxEnableEndTime_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableEndTimeOptions();
        }

        private void checkBoxEnableEndTime_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableEndTimeOptions();
        }

        private void EnableDisableEndTimeOptions()
        {
            var enabled = this.checkBoxEnableEndTime.IsChecked.Value;
            this.dateTimePickerEndTime.IsEnabled = enabled;
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

        private void upDownUptimeInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownUptimeInterval.Value == null)
                this.upDownUptimeInterval.Value = 1;
        }

        private void upDownUptimeDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var numericUpDown = (IntegerUpDown)sender;

            if (numericUpDown.Value == null)
                numericUpDown.Value = 0;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var us = BuildUptimeScheduler();
            this.uptimeScheduler = us;
            this.DialogResult = true;
            this.Close();
        }

        private UptimeScheduler BuildUptimeScheduler()
        {
            var scheduler = new UptimeScheduler();
            scheduler.EnableUptimeScheduler = this.checkBoxUptime.IsChecked.Value;
            scheduler.DueTime = this.dateTimePickerStartTime.Value.Value;

            scheduler.EnableRepeat = this.checkBoxEnableRepeat.IsChecked.Value;

            var hours = this.upDownUptimeDurationHours.Value.Value;
            var minutes = this.upDownUptimeDurationMinutes.Value.Value;
            var duration = new TimeSpan(hours, minutes, 0);
            scheduler.Duration = duration;

            if (this.comboBoxUptimeInterval.SelectedIndex == 0)
            {
                // Minutes.
                scheduler.RepeatAfter = TimeSpan.FromMinutes((int)this.upDownUptimeInterval.Value);
            }
            else if (this.comboBoxUptimeInterval.SelectedIndex == 1)
            {
                // Hours.
                scheduler.RepeatAfter = TimeSpan.FromHours((int)this.upDownUptimeInterval.Value);
            }
            else if (this.comboBoxUptimeInterval.SelectedIndex == 2)
            {
                // Days.
                scheduler.RepeatAfter = TimeSpan.FromDays((int)this.upDownUptimeInterval.Value);
            }

            scheduler.EnableEndTime = this.checkBoxEnableEndTime.IsChecked.Value;
            scheduler.EndTime = this.dateTimePickerEndTime.Value.Value;           

            return scheduler;
        }
    }
}
