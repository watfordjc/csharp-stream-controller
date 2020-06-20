using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            int schemeIndex = cbUriProtocol.Items
                .Cast<ComboBoxItem>()
                .Select(c => (string)c.Content)
                .ToList()
                .IndexOf(Preferences.Default.obs_uri_scheme);
            cbUriProtocol.SelectedIndex = schemeIndex;
        }

        /// <summary>
        /// Save user preferences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Preferences.Default.obs_uri_scheme = cbUriProtocol.Text;
            Preferences.Default.Save();
            e.Handled = true;
            this.Close();
        }

    }
}
