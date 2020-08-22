using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using uk.JohnCook.dotnet.StreamController.Controls;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : StyledWindow
    {
        private const string PASSWORD_PLACEHOLDER = "{20E90077-AE25-42AA-B8A9-08C56F788542}";

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
            // A GUID as a placeholder password value to avoid copying password
            //  unnecessarily and to detect PasswordBox changes on save.
            pbPassword.Password = PASSWORD_PLACEHOLDER;
        }

        /// <summary>
        /// Save user preferences.
        /// </summary>
        /// <returns>True if saved, false if validation error.</returns>
        private bool Save()
        {
            save_button.IsEnabled = false;
            Preferences.Default.obs_settings_changed = false;
            if (!WindowUtilityLibrary.DependencyObjectIsValid(this))
            {
                save_validation_error.Visibility = Visibility.Visible;
                NotifyTextBlockChanged(save_validation_error);
                save_button.IsEnabled = true;
                return false;
            }
            else
            {
                save_validation_error.Visibility = Visibility.Hidden;
            }

            ResourceManager rm = new ResourceManager("uk.JohnCook.dotnet.StreamController.Properties.Resources", typeof(PreferencesWindow).Assembly);

            UpdatePreferenceIfChanged("obs_uri_scheme", cbUriProtocol.Text);
            if (cbUsePassword.IsChecked.Value && pbPassword.Password == PASSWORD_PLACEHOLDER)
            {
                if (!Preferences.Default.obs_use_password)
                {
                    _ = MessageBox.Show(rm.GetString("new_password_placeholder_error", CultureInfo.CurrentCulture), rm.GetString("title_new_password_error", CultureInfo.CurrentCulture), MessageBoxButton.OK);
                    save_button.IsEnabled = true;
                    return false;
                }
            }
            else if (cbUsePassword.IsChecked.Value)
            {
                SharedModels.SecurePreferences securePreferences = new SharedModels.SecurePreferences();
                char[] password = pbPassword.Password.ToArray();
                string output = String.Empty;
                _ = securePreferences.StoreString(ref output, ref password);
                Preferences.Default.obs_password = output;
                Array.Clear(password, 0, password.Length);
                pbPassword.Password = PASSWORD_PLACEHOLDER;
                Preferences.Default.obs_use_password = true;

                _ = MessageBox.Show(rm.GetString("new_password_set_message", CultureInfo.CurrentCulture), rm.GetString("title_new_password_saved", CultureInfo.CurrentCulture), MessageBoxButton.OK);
            }
            else
            {
                Preferences.Default.obs_use_password = false;
                Preferences.Default.obs_password = String.Empty;
            }


            UpdatePreferenceIfChanged("obs_local_clock_source_name", ComboBoxLocalClockTimeSource.Text);
            UpdatePreferenceIfChanged("local_timezone_use_utc_offset", !CheckBoxLocalClockCustomTimezone.IsChecked.Value);
            UpdatePreferenceIfChanged("local_timezone_abbreviation", TextBoxLocalClockTimezoneNormal.Text);
            UpdatePreferenceIfChanged("local_timezone_dst_abbreviation", TextBoxLocalClockTimezoneDst.Text);
            UpdatePreferenceIfChanged("obs_local_clock_weather_json_url", TextBoxLocalClockWeatherUrl.Text);
            UpdatePreferenceIfChanged("obs_local_clock_weather_json_url_fetch_delay", TextBoxLocalClockWeatherUrlFetchDelay.Text);
            UpdatePreferenceIfChanged("obs_local_clock_location_source_name", ComboBoxLocalClockWeatherLocationSource.Text);
            UpdatePreferenceIfChanged("obs_local_clock_weather_symbol_source_name", ComboBoxLocalClockWeatherSymbolSource.Text);
            UpdatePreferenceIfChanged("obs_local_clock_weather_temp_source_name", ComboBoxLocalClockWeatherTempSource.Text);
            UpdatePreferenceIfChanged("obs_local_clock_weather_attrib1_source_name", ComboBoxLocalClockWeatherAttrib1Source.Text);
            UpdatePreferenceIfChanged("obs_local_clock_weather_attrib1_text", TextBoxLocalClockWeatherAttrib1Value.Text);
            UpdatePreferenceIfChanged("obs_local_clock_weather_attrib2_source_name", ComboBoxLocalClockWeatherAttrib2Source.Text);
            UpdatePreferenceIfChanged("obs_local_clock_weather_attrib2_text", TextBoxLocalClockWeatherAttrib2Value.Text);
            UpdatePreferenceIfChanged("obs_local_clock_cycle_delay", TextBoxLocalClockWeatherCycleDelay.Text);
            UpdatePreferenceIfChanged("obs_slideshow_source_name", ComboBoxSlideShowSource.Text);
            UpdatePreferenceIfChanged("obs_slideshow_next_scene_delay", TextBoxSlideShowSceneDelay.Text);


            Preferences.Default.Save();
            Preferences.Default.obs_settings_changed = true;
            // TODO: Update existing ObsWsClient.AutoReconnect if necessary
            return true;
        }

        private static void UpdatePreferenceIfChanged(string propertyName, object value)
        {
            if (Preferences.Default[propertyName] != value && Preferences.Default[propertyName].GetType() == value.GetType())
            {
                Preferences.Default[propertyName] = value;
            }
        }

        /// <summary>
        /// Save preferences by clicking the Save button.
        /// </summary>
        /// <param name="sender">A button.</param>
        /// <param name="e">Associated button click state information and event data.</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool saved = Save();
            e.Handled = true;
            if (saved)
            {
                this.Close();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Close preferences window using Escape key.
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            // Save preferences using Ctrl+S.
            else if (e.Key == Key.S
                && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                save_button.Focus();
                if (Save())
                {
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Raise a UI Automation event for a TextBlock.
        /// </summary>
        /// <param name="textBlock">The TextBlock that has changed content.</param>
        private static void NotifyTextBlockChanged(TextBlock textBlock)
        {
            System.Windows.Automation.Peers.AutomationPeer peer = System.Windows.Automation.Peers.UIElementAutomationPeer.FromElement(textBlock);
            if (peer == null) { return; }
            peer.RaiseAutomationEvent(System.Windows.Automation.Peers.AutomationEvents.LiveRegionChanged);
        }

        private void ValidationErrorTextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            NotifyTextBlockChanged(sender as TextBlock);
        }
    }
}
