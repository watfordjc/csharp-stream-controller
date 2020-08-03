using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace uk.JohnCook.dotnet.StreamController.Controls
{
    /// <summary>
    /// A Window that follows system preferences for themes.
    /// </summary>
    public abstract class StyledWindow : Window
    {
        /// <summary>
        /// The theme currently in use.
        /// </summary>
        public WindowUtilityLibrary.WindowsTheme CurrentTheme { get; private set; }
        /// <summary>
        /// If false, this is a "Windows" window, such as a system tray icon.
        /// </summary>
        public bool IsApplicationWindow { get; private set; }

        /// <summary>
        /// Constructor for a new StyledWindow, with resource dictionary creation.
        /// </summary>
        /// <param name="applicationWindow">Whether this is an application window (true) or a Windows window (false).</param>
        public StyledWindow(bool applicationWindow = true)
        {
            IsApplicationWindow = applicationWindow;
            CurrentTheme = GetApplicableTheme();
            CreateResourceDictionary(CurrentTheme);
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            FontSize = ((Int32)App.Current.Resources["DpiX"]) / 96.0 * SystemFonts.MessageFontSize;
            FontFamily = SystemFonts.MessageFontFamily;
        }

        /// <summary>
        /// Static constructor for StyledWindow.
        /// </summary>
        static StyledWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StyledWindow), new FrameworkPropertyMetadata(typeof(StyledWindow)));

            PropertyInfo dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            PropertyInfo dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            App.Current.Resources["DpiX"] = dpiXProperty.GetValue(null, null);
            App.Current.Resources["DpiY"] = dpiYProperty.GetValue(null, null);
        }

        /// <summary>
        /// Get the applicable theme for the current window and its app/Windows type.
        /// </summary>
        /// <returns>The current applicable Windows theme.</returns>
        private WindowUtilityLibrary.WindowsTheme GetApplicableTheme()
        {
            WindowUtilityLibrary.WindowsTheme applicableTheme = WindowUtilityLibrary.CurrentWindowsTheme(IsApplicationWindow);
            if (applicableTheme == WindowUtilityLibrary.WindowsTheme.Default)
            {
                applicableTheme = WindowUtilityLibrary.DefaultTheme(IsApplicationWindow);
            }
            return applicableTheme;
        }

        /// <summary>
        /// EventHandler for notifying of theme changes.
        /// </summary>
        public event EventHandler<WindowUtilityLibrary.WindowsTheme> ThemeChanged;

        /// <summary>
        /// Create a resource dictionary based on app/Windows type.
        /// </summary>
        private void CreateResourceDictionary(WindowUtilityLibrary.WindowsTheme theme)
        {
            foreach (ResourceDictionary dictionary in WindowUtilityLibrary.GetStyledResourceDictionary(theme).MergedDictionaries)
            {
                Resources.MergedDictionaries.Add(dictionary);
            }
        }

        /// <summary>
        /// Clear and recreate resource dictionary when user changes Windows theme preferences.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">Event data.</param>
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.General) { return; }

            WindowUtilityLibrary.WindowsTheme oldTheme = CurrentTheme;
            WindowUtilityLibrary.WindowsTheme newTheme = GetApplicableTheme();
            if (newTheme == oldTheme) { return; }

            Resources.MergedDictionaries.Clear();
            CreateResourceDictionary(newTheme);
            CurrentTheme = newTheme;
            ThemeChanged?.Invoke(this, oldTheme);
        }
    }
}
