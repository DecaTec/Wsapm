using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window, IDisposable
    {
        private DispatcherTimer timer;
        private CpuLoad cpuLoad;
        private NetworkLoad networkLoad;
        private LoadCheckType checkType;

        private readonly int numberOfProbes = 5;
        private readonly TimeSpan breakBetweenProbes = TimeSpan.FromMilliseconds(200);
        private readonly TimeSpan displayRefreshInterval = TimeSpan.FromSeconds(1);

        public LoadWindow(LoadCheckType checkType, string additionalInformation)
        {
            InitializeComponent();
            this.checkType = checkType;

            switch (this.checkType)
            {
                case LoadCheckType.Cpu:
                    this.Title = Wsapm.Resources.Wsapm.LoadWinodw_TitleCpu;
                    this.cpuLoad = new CpuLoad();
                    break;
                case LoadCheckType.NetworkTotal:
                case LoadCheckType.NetworkReceived:
                case LoadCheckType.NetworkSent:
                    this.Title = Wsapm.Resources.Wsapm.LoadWinodw_TitleNetwork;
                    this.networkLoad = new NetworkLoad(additionalInformation);
                    break;
                default:
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBlockLoad.Text = String.Empty;

            this.timer = new DispatcherTimer();
            this.timer.Interval = displayRefreshInterval;
            this.timer.Tick += timer_Tick;
            this.timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {                
                switch (this.checkType)
                {
                    case LoadCheckType.Cpu:
                        this.textBlockLoad.Text += this.cpuLoad.GetAverageCpuLoad(numberOfProbes, breakBetweenProbes).ToString("0.00")  + " %" + Environment.NewLine;
                        break;
                    case LoadCheckType.NetworkTotal:
                        this.textBlockLoad.Text += (WsapmConvert.ConvertByteToKB(this.networkLoad.GetAverageNetworkLoadTotalInBytesPerSecond(numberOfProbes, breakBetweenProbes))).ToString("0.00") + " KB/s" + Environment.NewLine;
                        break;
                    case LoadCheckType.NetworkReceived:
                        this.textBlockLoad.Text += (WsapmConvert.ConvertByteToKB(this.networkLoad.GetCurrentNetworkLoadReceivedInBytesPerSecond(numberOfProbes, breakBetweenProbes))).ToString("0.00") + " KB/s" + Environment.NewLine;
                        break;
                    case LoadCheckType.NetworkSent:
                        this.textBlockLoad.Text += (WsapmConvert.ConvertByteToKB(this.networkLoad.GetCurrentNetworkLoadSentInBytesPerSecond(numberOfProbes, breakBetweenProbes))).ToString("0.00") + " KB/s" + Environment.NewLine;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                    this.timer.Tick -= timer_Tick;
                }

                MessageBox.Show(String.Format(Wsapm.Resources.Wsapm.LoadWindow_ReadValueError, Environment.NewLine, ex.Message), Wsapm.Resources.Wsapm.General_MessageBoxErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Dispatcher.Invoke((Action)(() => this.Close()));
            }

            this.scrollViewer.ScrollToEnd();
            CommandManager.InvalidateRequerySuggested();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.timer != null)
            {
                this.timer.Tick -= timer_Tick;
                this.timer.Stop();
                this.timer = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.cpuLoad != null)
                    this.cpuLoad.Dispose();

                if (this.networkLoad != null)
                    this.networkLoad.Dispose();
            }
        }

        #region IDisposable members

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable members
    }

    /// <summary>
    /// Enum for the type of load check.
    /// </summary>
    public enum LoadCheckType
    {
        Cpu,
        NetworkTotal,
        NetworkReceived,
        NetworkSent
    }
}
