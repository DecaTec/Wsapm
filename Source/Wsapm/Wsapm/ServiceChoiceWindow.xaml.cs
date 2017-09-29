using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for ServiceChoiceWindow.xaml
    /// </summary>
    public partial class ServiceChoiceWindow : Window
    {
        private string chosenService;
        private ICollectionView colView;

        public ServiceChoiceWindow()
        {
            InitializeComponent();

            this.busyIndicator.BusyContent = Wsapm.Resources.Wsapm.General_BusyIndicatorBusyContent;
            this.chosenService = String.Empty;
        }

        /// <summary>
        /// Gets the chosen service.
        /// </summary>
        /// <returns></returns>
        internal string GetService()
        {
            return this.chosenService;
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
            SelectQuery sq = new SelectQuery("SELECT * FROM Win32_Service");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(sq);
            var items = mos.Get();

            IList<SimpleService> services = new List<SimpleService>();

            foreach (var item in items)
            {
                string displayName = (string)item["DisplayName"]; // Anzeigename
                string name = (string)item["Name"]; // Kurzname
                string description = (string)item["Description"];
                bool started = (bool)item["Started"];

                services.Add(new SimpleService(displayName, name, description, started));
            }

            colView = CollectionViewSource.GetDefaultView(services);
            Dispatcher.Invoke((Action)(() => this.serviceDataGrid.ItemsSource = colView));
            Dispatcher.Invoke((Action)(() => this.serviceDataGrid.Items.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending))));
        }

        private bool SimpleServiceFilter(object item)
        {
            SimpleService service = item as SimpleService;

            if (service == null)
                return false;

            return service.Name.ToLower().Contains(this.textBoxFilter.Text.ToLower()) || service.DisplayName.ToLower().Contains(this.textBoxFilter.Text.ToLower());
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var service = this.serviceDataGrid.SelectedItem as SimpleService;

            if (service != null)
                this.chosenService = service.Name;

            this.DialogResult = true;
            this.Close();
        }

        private void serviceDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var service = this.serviceDataGrid.SelectedItem as SimpleService;

            if (service != null)
            {
                this.chosenService = service.Name;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void textBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.colView != null)
                colView.Filter = SimpleServiceFilter;
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.busyIndicator.IsBusy = true;
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        public class SimpleService
        {
            internal SimpleService()
            {

            }

            public SimpleService(string displayName, string name, string description, bool started)
            {
                this.DisplayName = displayName;
                this.Name = name;
                this.Description = description;
                this.Started = started;
            }

            public string DisplayName
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public bool Started
            {
                get;
                set;
            }
        }

    }
}
