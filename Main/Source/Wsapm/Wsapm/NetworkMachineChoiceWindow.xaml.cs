using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for NetworkMachineChoiceWindow.xaml
    /// </summary>
    public partial class NetworkMachineChoiceWindow : Window
    {
        private NetworkMachine chosenComputer;
        private ICollectionView colView;

        private static CancellationTokenSource cancellationTokenSource;

        public NetworkMachineChoiceWindow()
        {
            InitializeComponent();

            //this.busyIndicator.BusyContent = Wsapm.Resources.Wsapm.General_BusyIndicatorBusyContent;
            this.chosenComputer = null;
        }

        /// <summary>
        /// Gets the chosen NetworkMachine.
        /// </summary>
        /// <returns></returns>
        internal NetworkMachine GetNetworkMachine()
        {
            return this.chosenComputer;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillIPTextBoxes();
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            this.busyIndicator.IsBusy = true;
            backgroundWorker.RunWorkerAsync();
        }

        private void FillIPTextBoxes()
        {

#if DEBUG
            this.textBoxIPFrom.Text = "192.168.178.1";
            this.textBoxIPTo.Text = "192.168.178.254";

#else
            var myIp = NetworkTools.GetLocalIPAddress(AddressFamily.InterNetwork);

            if (myIp == null || myIp == IPAddress.None)
                return;

            var myIPStr = myIp.ToString();           
            var strArr = myIPStr.Split('.');
            strArr[3] = "1";
            var ipFrom = string.Join(".", strArr);
            this.textBoxIPFrom.Text = ipFrom;

            strArr[3] = "254";
            var ipTo = string.Join(".", strArr);
            this.textBoxIPTo.Text = ipTo;
#endif
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.busyIndicator.IsBusy = false;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();

            InitUI();
        }

        private void InitUI()
        {
            string ipFromStr = null;
            string ipToStr = null;
            Dispatcher.Invoke((Action)(() => ipFromStr = this.textBoxIPFrom.Text));
            Dispatcher.Invoke((Action)(() => ipToStr = this.textBoxIPTo.Text));

            if(!NetworkTools.IsStringIPAddress(ipFromStr))
            {
                MessageBox.Show(Wsapm.Resources.Wsapm.NetworkMachineChoiceWindow_MessageIPFromNoRealIP, Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(!NetworkTools.IsStringIPAddress(ipToStr))
            {
                MessageBox.Show(Wsapm.Resources.Wsapm.NetworkMachineChoiceWindow_MessageIPToNoRealIP, Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var ipFrom = IPAddress.Parse(ipFromStr);
            var ipTo = IPAddress.Parse(ipToStr);

            var ipRange = NetworkTools.GetIPAddressRange(ipFrom, ipTo);
            var machines = NetworkTools.GetNetworkMachinesInLocalNetworkFromIPAddressRange(ipRange, cancellationTokenSource.Token);

            if (machines == null || machines.Length == 0)
                return;

            //var machines = DecaTec.Toolkit.Tools.NetworkTools.GetComputersInLocalNetwork();
            colView = CollectionViewSource.GetDefaultView(machines);
            Dispatcher.Invoke((Action)(() => this.networkMachineDataGrid.ItemsSource = colView));
            Dispatcher.Invoke((Action)(() => this.networkMachineDataGrid.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending))));
        }        

        private bool NetworkMachineFilter(object item)
        {
            NetworkMachine machine = item as NetworkMachine;

            if (machine == null)
                return false;

            return machine.Name.ToLower().Contains(this.textBoxFilter.Text.ToLower()) || machine.IPAddress.ToString().Contains(this.textBoxFilter.Text.ToLower());
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            var machine = this.networkMachineDataGrid.SelectedItem as NetworkMachine;

            if (machine != null)
                this.chosenComputer = machine;

            this.DialogResult = true;
            this.Close();
        }

        private void networkMachineDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var machine = this.networkMachineDataGrid.SelectedItem as NetworkMachine;

            if (machine != null)
            {
                this.chosenComputer = machine;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void textBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.colView != null)
                colView.Filter = NetworkMachineFilter;
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.busyIndicator.IsBusy = true;
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync(); 
        }

        private void buttonCancelBusyIndicator_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
