﻿using Microsoft.VisualBasic;
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

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// A utility library for Window objects within the Application.
    /// </summary>
    static class WindowUtilityLibrary
    {
        /* Constants for Types of windows in the application. */
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
