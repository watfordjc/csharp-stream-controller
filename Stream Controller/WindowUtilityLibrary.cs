using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Windows;

namespace Stream_Controller
{
    /// <summary>
    /// A utility library for Window objects within the Application.
    /// </summary>
    static class WindowUtilityLibrary
    {
        /* Constants for Types of windows in the application. */
        public const int MAIN_WINDOW = 1; // MainWindow.xaml
        public const int PREFERENCES = 2; // PreferencesWindow.xaml
        public const int WEB_SOCKETS = 3; // WebSocketTest.xaml

        /* List containing the above constants for assertion sanity checking. */
        private static readonly List<int> windowTypes = new List<int>
        {
            MAIN_WINDOW,
            PREFERENCES,
            WEB_SOCKETS
        };

        /// <summary>
        /// Get an existing Window or create a new one.
        /// </summary>
        /// <param name="windowType">A constant for the Window Type, defined in WindowUtilityLibrary.</param>
        /// <returns>A Window of the specified Type.</returns>
        public static Window GetWindow(int windowType)
        {
            Debug.Assert(windowTypes.Contains(windowType), "Unrecognised window type.");
            IEnumerable<Window> windows = windowType switch
            {
                MAIN_WINDOW => Application.Current.Windows.OfType<MainWindow>(),
                PREFERENCES => Application.Current.Windows.OfType<PreferencesWindow>(),
                WEB_SOCKETS => Application.Current.Windows.OfType<WebSocketTest>(),
                _ => null,
            };
            return (windows != null && windows.Count() > 0) ? windows.First() : GetNewWindow(windowType);
        }

        /// <summary>
        /// Creates a new Window.
        /// </summary>
        /// <param name="windowType">A constant for the Window Type, defined in WindowUtilityLibrary.</param>
        /// <returns>A new Window of the specified Type.</returns>
        private static Window GetNewWindow(int windowType)
        {
            Debug.Assert(windowTypes.Contains(windowType), "Unrecognised window type.");
            return windowType switch
            {
                MAIN_WINDOW => new MainWindow(),
                PREFERENCES => new PreferencesWindow(),
                WEB_SOCKETS => new WebSocketTest(),
                _ => null,
            };
        }
    }
}
