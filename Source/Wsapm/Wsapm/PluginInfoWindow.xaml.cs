using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wsapm
{
    /// <summary>
    /// Interaction logic for PluginInfoWindow.xaml
    /// </summary>
    public partial class PluginInfoWindow : Window
    {
        public PluginInfoWindow(string content)
        {
            InitializeComponent();
            this.textBlockPluginInfo.Text = content;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
