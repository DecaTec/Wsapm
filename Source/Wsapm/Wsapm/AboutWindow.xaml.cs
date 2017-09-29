using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Wsapm.Core;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.labelVersion.Content = String.Format(Wsapm.Resources.Wsapm.AboutWindow_Version, VersionInformation.Version.ToString(3));
            LoadChangelog();
        }

        private void LoadChangelog()
        {
            try
            {
                Stream s = null;

                try
                {
                    s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Wsapm.Resources.Wsapm.AboutWindow_ChangelogFileName);

                    using (StreamReader sr = new StreamReader(s))
                    {
                        s = null;
                        string txt = sr.ReadToEnd();
                        this.textBlockChangelog.Text = txt;
                    }
                }
                finally
                {
                    if (s != null)
                        s.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }

        private void HyperlinkHomepage_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("https://www.decatec.de/");
            Process.Start(psi);
        }

        private void HyperlinkExtendedWpfToolkit_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("https://github.com/xceedsoftware/wpftoolkit");
            Process.Start(psi);
        }

        private void HyperlinkConverticon_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("http://converticon.com/");
            Process.Start(psi);
        }

        private void HyperlinkWixToolset_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("http://wixtoolset.org/");
            Process.Start(psi);
        }

        private void HyperlinkDotNetZip_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("http://dotnetzip.codeplex.com/");
            Process.Start(psi);
        }
    }
}
