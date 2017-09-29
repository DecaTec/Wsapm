using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for ProcessChoiceWindow.xaml
    /// </summary>
    public partial class ProcessChoiceWindow : Window
    {
        private string chosenProcess;
        private ICollectionView colView;

        public ProcessChoiceWindow()
        {
            InitializeComponent();

            this.busyIndicator.BusyContent = Wsapm.Resources.Wsapm.General_BusyIndicatorBusyContent;
            this.chosenProcess = String.Empty;
        }

        /// <summary>
        /// Gets the chosen process.
        /// </summary>
        /// <returns></returns>
        internal string GetProcess()
        {
            return this.chosenProcess;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            this.busyIndicator.IsBusy = true;
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.busyIndicator.IsBusy = false;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            InitUI();
        }

        private void InitUI()
        {
            var procs = Process.GetProcesses();
            IList<SimpleProcess> processes = new List<SimpleProcess>();

            for (int i = 0; i < procs.Length; i++)
            {
                string processName = procs[i].ProcessName;
                string fileName = String.Empty;
                string processDescription = String.Empty;

                try
                {
                    fileName = procs[i].MainModule.FileName;
                    FileInfo fi = new FileInfo(fileName);
                    fileName = fi.FullName;
                    processDescription = procs[i].MainModule.FileVersionInfo.FileDescription;
                }
                catch (Exception)
                {
                }

                processes.Add(new SimpleProcess(procs[i].ProcessName, fileName, processDescription));
            }

            colView = CollectionViewSource.GetDefaultView(processes);
            Dispatcher.Invoke((Action)(() => this.processDataGrid.ItemsSource = colView));
            Dispatcher.Invoke((Action)(() => this.processDataGrid.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending))));
        }

        private bool SimpleProcessFilter(object item)
        {
            SimpleProcess proc = item as SimpleProcess;

            if (proc == null)
                return false;

            return proc.Name.ToLower().Contains(this.textBoxFilter.Text.ToLower()) || proc.FileName.ToLower().Contains(this.textBoxFilter.Text.ToLower());
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var proc = this.processDataGrid.SelectedItem as SimpleProcess;

            if (proc != null)
                this.chosenProcess = proc.Name;

            this.DialogResult = true;
            this.Close();
        }

        private void processDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var proc = this.processDataGrid.SelectedItem as SimpleProcess;

            if (proc != null)
            {
                this.chosenProcess = proc.Name;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void textBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.colView != null)
                colView.Filter = SimpleProcessFilter;
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.busyIndicator.IsBusy = true;
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }
    }

    public class SimpleProcess
    {
        internal SimpleProcess()
        {

        }

        public SimpleProcess(string name, string fileName, string description)
        {
            this.Name = name;
            this.FileName = fileName;
            this.Description = description;
        }

        public string Name
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
    }
}
