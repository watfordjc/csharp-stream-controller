using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Class containing replacement entry point for the application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// A named Mutex unique to the program install folder.
        /// <para>Named Mutexes cannot contain backslashes, so replace backslashes in path with exclamation marks.</para>
        /// <para>Append install path with a hard-coded Guid.</para>
        /// </summary>
        private static readonly string uniqueProgramString = (System.AppDomain.CurrentDomain.BaseDirectory).Replace("\\", "!", StringComparison.OrdinalIgnoreCase) + "{3C8335C6-AA3B-45CC-B583-CF76B2E536DB}";

        /// <summary>
        /// Prevent instantiation of class.
        /// </summary>
        private Program()
        {
        }

        /// <summary>
        /// Main() function for the application, ensures only one instance can run at a time.
        /// </summary>
        /// <param name="args">String array of command line arguments</param>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Trace.WriteLine("Ignoring unhandled command line arguments.");
            }
            using Mutex singleInstanceMutex = new Mutex(true, uniqueProgramString, out bool isMutexOwner);
            if (isMutexOwner)
            {
                App application = new App
                {
                    StartupUri = new Uri("pack://application:,,,/AudioCheck.xaml", UriKind.Absolute),
                    Resources = new ResourceDictionary()
                };
                ResourceDictionary appMenuBarDictionary = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/Properties/Menus.xaml", UriKind.Absolute)
                };
                application.Resources.MergedDictionaries.Add(appMenuBarDictionary);
                application.Run();
            }
        }
    }
}
