using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AddHddWindow.xaml
    /// </summary>
    public partial class AddHddWindow : Window
    {
        private HddToMonitor hddToMonitor;
        private HddToMonitor[] allDrives;
        private HddToMonitor editHddToMonitor;

        private HddLoadCurrent hddLoad;
        private Timer hddLoadTimer;

        private readonly TimeSpan displayRefreshInterval = TimeSpan.FromSeconds(1);
        private string currentSelectedHdd;
        private readonly string loadingString = Wsapm.Resources.Wsapm.AddHddWindow_LabelHddLoadLoading;

        public AddHddWindow(HddToMonitor[] allDrives)
        {
            InitializeComponent();

            this.allDrives = allDrives;

            var hdds = WsapmTools.GetAvailableLogicalVolumeNames();

            foreach (var hdd in hdds)
            {
                this.comboBoxAvailableDrives.Items.Add(hdd);
            }

            this.comboBoxAvailableDrives.Items.Add(WsapmTools.GetCommonDiaplayNameAllDrives());

            this.hddLoad = new HddLoadCurrent();
            this.hddLoadTimer = null;
            this.hddLoadTimer = new Timer(OnShowHddLoadsCallback, null, new TimeSpan(), this.displayRefreshInterval);
        }

        public AddHddWindow(HddToMonitor hddToMonitorToEdit, HddToMonitor[] allDrives)
            : this(allDrives)
        {
            this.Title = Wsapm.Resources.Wsapm.AddHddWindow_TitleEdit;
            this.editHddToMonitor = hddToMonitorToEdit.Copy();
        }

        /// <summary>
        /// Gets the created HddToMonitor.
        /// </summary>
        /// <returns></returns>
        internal HddToMonitor GetHdd()
        {
            return this.hddToMonitor;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var driveName = this.comboBoxAvailableDrives.SelectedItem as string;
            var enableHddLoad = this.checkBoxEnableHddLoad.IsChecked.Value;          
            var hddLoad = this.upDownHddLoad.Value.Value;

            var hdd = new HddToMonitor();
            hdd.Drive = driveName;
            hdd.EnableCheckHddLoad = enableHddLoad;
            hdd.HddLoad = hddLoad;            

            // Avoid to add a element twice.
            if (this.editHddToMonitor == null)
            {
                // Add new mode.
                if (this.allDrives != null)
                {
                    for (int i = 0; i < this.allDrives.Length; i++)
                    {
                        if (this.allDrives[i] == hdd)
                        {
                            MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddHddWindow_HddAlreadyAdded, hdd.Drive), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }
            else
            {
                // Edit mode.
                if (hdd != this.editHddToMonitor)
                {
                    // Element was changed.
                    if (this.allDrives != null)
                    {
                        for (int i = 0; i < this.allDrives.Length; i++)
                        {
                            if (this.allDrives[i] == hdd)
                            {
                                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.AddHddWindow_HddAlreadyAdded, hdd.Drive), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                }
            }

            this.hddToMonitor = hdd;
            this.DialogResult = true;
            this.Close();
        }

        private void OnShowHddLoadsCallback(object state)
        {
            if (string.IsNullOrEmpty(this.currentSelectedHdd))
                return;

            var hddLoad = string.Empty;            
            bool allHdds = false;

            allHdds = this.currentSelectedHdd == WsapmTools.GetCommonDiaplayNameAllDrives();

            try
            {
                if (allHdds)
                {
                    hddLoad = WsapmConvert.ConvertByteToKB(this.hddLoad.GetCurrentHddLoadTotalAllHdds()).ToString("0.00") + " KB/s";                   
                }
                else
                {
                    hddLoad = WsapmConvert.ConvertByteToKB(this.hddLoad.GetCurrentHddLoadInBytesPerSecond(this.currentSelectedHdd)).ToString("0.00") + " KB/s";                    
                }
            }
            catch (Exception)
            {
            }

            Dispatcher.Invoke((Action)(() =>
            {
                this.labelCurrentHddLoad.Content = hddLoad;
            }));
        }

        private void checkBoxEnableHddLoad_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableHddLoadOptions();
        }

        private void checkBoxEnableHddLoad_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableDisableHddLoadOptions();
        }

        private void EnableDisableHddLoadOptions()
        {
            var enabled = this.checkBoxEnableHddLoad.IsChecked.Value;
            this.upDownHddLoad.IsEnabled = enabled;
        }

        private void upDownHddLoad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.upDownHddLoad.Value == null)
                this.upDownHddLoad.Value = 0;
        }

        private void comboBoxAvailableDrives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.currentSelectedHdd = (string)this.comboBoxAvailableDrives.SelectedItem;

            this.labelCurrentHddLoad.Content = this.loadingString;

            this.checkBoxEnableHddLoad.IsChecked = false;            
            this.upDownHddLoad.Value = 0;

            if (this.allDrives != null && this.allDrives.Length > 0)
            {
                for (int i = 0; i < this.allDrives.Length; i++)
                {
                    if (this.allDrives[i].Drive == (string)this.comboBoxAvailableDrives.SelectedItem)
                    {
                        this.checkBoxEnableHddLoad.IsChecked = this.allDrives[i].EnableCheckHddLoad;                       
                        this.upDownHddLoad.Value = (int)this.allDrives[i].HddLoad;                       
                        break;
                    }
                }
            }

            EnableDisableHddLoadOptions();
        }

        private void comboBoxAvailableDrives_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.editHddToMonitor != null && this.comboBoxAvailableDrives.Items.Count > 0)
            {
                for (int i = 0; i < this.comboBoxAvailableDrives.Items.Count; i++)
                {
                    if (this.editHddToMonitor.Drive == (string)this.comboBoxAvailableDrives.Items[i])
                    {
                        this.comboBoxAvailableDrives.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
                this.comboBoxAvailableDrives.SelectedIndex = 0;
        }
    }
}
