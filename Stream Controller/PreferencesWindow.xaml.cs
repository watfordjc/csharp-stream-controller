using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Stream_Controller
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow()
        {
            InitializeComponent();
            InitialiseWindow();
        }

        /// <summary>
        /// Initialise form values for user preferences.
        /// </summary>
        private void InitialiseWindow()
        {
            Uri uri = new Uri(Preferences.Default.obs_uri);
            int uriScheme = cbUriProtocol.Items.IndexOf(uri.Scheme);
            if (uriScheme != -1)
            {
                cbUriProtocol.SelectedIndex = uriScheme;
            }
            tbUriHostname.Text = uri.Host;
            tbUriPort.Text = uri.Port.ToString();
            cbConnectLaunch.IsChecked = Preferences.Default.obs_connect_launch;
            cbAutoReconnect.IsChecked = Preferences.Default.obs_auto_reconnect;
            tbBackoffMin.Text = Preferences.Default.obs_reconnect_min_seconds.ToString();
            tbBackoffMax.Text = Preferences.Default.obs_reconnect_max_minutes.ToString();
            cbAutoscroll.IsChecked = Preferences.Default.obs_autoscroll;
        }

        /// <summary>
        /// Save user preferences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(String.Format("{0}://{1}:{2}", cbUriProtocol.Text.Trim(), tbUriHostname.Text.Trim(), tbUriPort.Text.Trim()));
            Preferences.Default.obs_uri = uri.ToString();
            Preferences.Default.obs_connect_launch = cbConnectLaunch.IsChecked.Value;
            Preferences.Default.obs_auto_reconnect = cbAutoReconnect.IsChecked.Value;
            Preferences.Default.obs_reconnect_min_seconds = Int32.Parse(tbBackoffMin.Text);
            Preferences.Default.obs_reconnect_max_minutes = Int32.Parse(tbBackoffMax.Text);
            Preferences.Default.obs_autoscroll = cbAutoscroll.IsChecked.Value;
            Preferences.Default.Save();
            e.Handled = true;
            this.Close();
        }
    }
}
