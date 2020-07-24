using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.UI.ViewManagement;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// A utility library for Window objects within the Application.
    /// </summary>
    static class WindowUtilityLibrary
    {
        /// <summary>
        /// Constants for Types of windows in the application.
        /// </summary>
        public enum WindowType
        {
            MainWindow = 0,
            PreferencesWindow = 1,
            WebSocketTest = 2,
            Audiocheck = 3
        }

        /// <summary>
        /// Type to WindowType dictionary.
        /// </summary>
        private static readonly Dictionary<Type, WindowType> enumTypeDictionary = new Dictionary<Type, WindowType>()
        {
            { typeof(MainWindow), WindowType.MainWindow },
            { typeof(PreferencesWindow), WindowType.PreferencesWindow },
            { typeof(WebSocketTest), WindowType.WebSocketTest },
            { typeof(AudioCheck), WindowType.Audiocheck }
        };

        /// <summary>
        /// Show a new or existing Window and makes it active.
        /// </summary>
        /// <param name="windowType">A constant for the WindowType, defined in WindowUtilityLibrary.</param>
        public static void MakeWindowActive(WindowType windowType)
        {
            Debug.Assert(Enum.IsDefined(typeof(WindowType), windowType), "Unrecognised window type.");
            Window window = GetWindow(windowType);
            if (window == null)
            {
                return;
            }
            window.Show();
            window.Activate();
        }

        /// <summary>
        /// Get an existing Window or create a new one.
        /// </summary>
        /// <param name="windowType">A constant for the WindowType, defined in WindowUtilityLibrary.</param>
        /// <returns>A Window of the specified Type.</returns>
        public static Window GetWindow(WindowType windowType)
        {
            Debug.Assert(Enum.IsDefined(typeof(WindowType), windowType), "Unrecognised window type.");
            IEnumerable<Window> windows = windowType switch
            {
                WindowType.MainWindow => Application.Current.Windows.OfType<MainWindow>(),
                WindowType.PreferencesWindow => Application.Current.Windows.OfType<PreferencesWindow>(),
                WindowType.WebSocketTest => Application.Current.Windows.OfType<WebSocketTest>(),
                WindowType.Audiocheck => Application.Current.Windows.OfType<AudioCheck>(),
                _ => null,
            };
            return (windows != null && windows.Any()) ? windows.First() : GetNewWindow(windowType);
        }

        /// <summary>
        /// Creates a new Window.
        /// </summary>
        /// <param name="windowType">A constant for the WindowType, defined in WindowUtilityLibrary.</param>
        /// <returns>A new Window of the specified Type.</returns>
        private static Window GetNewWindow(WindowType windowType)
        {
            Debug.Assert(Enum.IsDefined(typeof(WindowType), windowType), "Unrecognised window type.");
            return windowType switch
            {
                WindowType.MainWindow => new MainWindow(),
                WindowType.PreferencesWindow => new PreferencesWindow(),
                WindowType.WebSocketTest => new WebSocketTest(),
                WindowType.Audiocheck => new AudioCheck(),
                _ => null,
            };
        }

        /// <summary>
        /// Gets the WindowType of a (Window) Type.
        /// </summary>
        /// <param name="window">The Type of a Window instance.</param>
        /// <returns>The WindowType for that window.</returns>
        public static WindowType GetWindowTypeEnum(Type window)
        {
            Debug.Assert(enumTypeDictionary.ContainsKey(window), "Unrecognised window Type.");
            return enumTypeDictionary.GetValueOrDefault(window);
        }

        /// <summary>
        /// Constants for Window theme categorisation.
        /// </summary>
        public enum WindowsTheme
        {
            Default = 0,
            Light = 1,
            Dark = 2,
            HighContrast = 3
        }

        /// <summary>
        /// Get the theme category for the current Windows Settings personalisation preferences.
        /// </summary>
        /// <param name="ApplicationTheme">Windows 10 Personalisation - true for default app mode, false for default Windows mode.</param>
        /// <returns>The current theme category.</returns>
        public static WindowsTheme CurrentWindowsTheme(bool applicationTheme)
        {
            // High Contrast is enabled
            if (SystemParameters.HighContrast)
            {
                return WindowsTheme.HighContrast;
            }
            // Windows Settings -> Personalisation -> Colours
            // -> Default Windows Mode; Default is Dark
            else if (!applicationTheme)
            {
                return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", -1) switch
                {
                    0 => WindowsTheme.Dark,
                    1 => WindowsTheme.Light,
                    _ => WindowsTheme.Default
                };
            }
            // -> Default App Mode; Default is Light
            else if (applicationTheme)
            {
                return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", -1) switch
                {
                    0 => WindowsTheme.Dark,
                    1 => WindowsTheme.Light,
                    _ => WindowsTheme.Default
                };
            }
            else
            {
                return WindowsTheme.Default;
            }
        }

        /// <summary>
        /// Get the default theme category for Default Windows Mode or Default App Mode
        /// </summary>
        /// <param name="applicationTheme">Windows 10 Personalisation - true for default app mode, false for default Windows mode.</param>
        /// <returns>The default theme category for the specified mode.</returns>
        public static WindowsTheme DefaultTheme(bool applicationTheme)
        {
            return applicationTheme ? WindowsTheme.Light : WindowsTheme.Dark;
        }

        /// <summary>
        /// Get the current user's preferred accent colour.
        /// </summary>
        /// <returns>Brush for user's preferred accent colour.</returns>
        public static Brush WindowAccentColor()
        {
            // In High Contrast mode, accents should disappear.
            if (SystemParameters.HighContrast)
            {
                return SystemColors.WindowBrush;
            }
            //  -> Choose your accent colour -> (Recent|Windows|Custom) colour(s)
            return SystemParameters.WindowGlassBrush;
        }

        /// <summary>
        /// Recursively check a DependencyObject for validation errors.
        /// </summary>
        /// <param name="dependencyObject">The DependencyObject to check for validation errors.</param>
        /// <returns>True if no validation errors, otherwise false.</returns>
        public static bool DependencyObjectIsValid(DependencyObject dependencyObject)
        {
            // Check if this DependencyObject has Validation errors.
            if (Validation.GetHasError(dependencyObject))
            {
                return false;
            }
            // Enumerate children. LogicalTreeHelper.GetChildren() returns Object[], not DependencyObject[].
            foreach (Object childObject in LogicalTreeHelper.GetChildren(dependencyObject))
            {
                // Panel objects contain controls. Control objects can be changed by user.
                if (!(childObject is Panel) && !(childObject is Control))
                {
                    continue;
                }
                // Panel and Control are types of DependencyObject and may contain DependencyObject children.
                else if (!DependencyObjectIsValid(childObject as DependencyObject))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
