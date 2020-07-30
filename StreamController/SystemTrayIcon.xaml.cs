using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using uk.JohnCook.dotnet.NAudioWrapperLibrary;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for SystemTrayIcon.xaml
    /// </summary>
    public partial class SystemTrayIcon : Window, IDisposable
    {
        private bool disposedValue;
        public static SystemTrayIcon Instance { get { return lazySingleton.Value; } }

        private static readonly Lazy<SystemTrayIcon> lazySingleton =
        new Lazy<SystemTrayIcon>(
            () => new SystemTrayIcon()
        );

        public SystemTrayIcon()
        {
            WindowUtilityLibrary.WindowsTheme applicableTheme = WindowUtilityLibrary.CurrentWindowsTheme(false);
            if (applicableTheme == WindowUtilityLibrary.WindowsTheme.Default)
            {
                applicableTheme = WindowUtilityLibrary.DefaultTheme(false);
            }
            foreach (ResourceDictionary dictionary in WindowUtilityLibrary.GetStyledResourceDictionary(applicableTheme))
            {
                Resources.MergedDictionaries.Add(dictionary);
            }
            InitializeComponent();
            AudioInterfaceCollection.Instance.CollectionEnumerated += Devices_CollectionEnumerated;
            NotifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            WindowUtilityLibrary.MakeWindowActive(WindowUtilityLibrary.WindowType.Audiocheck);
        }

        private void Devices_CollectionEnumerated(object sender, EventArgs e)
        {
            taskbarRenderMenu.ItemsSource = AudioInterfaceCollection.ActiveDevices;
            taskbarCaptureMenu.ItemsSource = AudioInterfaceCollection.ActiveDevices;
        }

        public void UpdateTrayIcon()
        {
            NotifyIcon.Icon = Properties.Resources.icon;
            NotifyIcon.Visibility = Visibility.Visible;
        }

        #region System Tray Context Menu

        private void SystemTrayRenderDefault_Click(object sender, RoutedEventArgs e)
        {
            AudioInterfaceCollection.ChangeDefaultDevice(((sender as MenuItem).DataContext as AudioInterface).ID);
        }


        private void SystemTrayCaptureDefault_Click(object sender, RoutedEventArgs e)
        {
            AudioInterfaceCollection.ChangeDefaultDevice(((sender as MenuItem).DataContext as AudioInterface).ID);
        }


        private async void SystemTrayToggleCustomAudio_Click(object sender, RoutedEventArgs e)
        {
            (e.OriginalSource as MenuItem).IsEnabled = false;
            await Task.Run(
                () => AudioInterfaceCollection.ToggleAllDefaultApplicationDevice()
                ).ConfigureAwait(true);
            (e.OriginalSource as MenuItem).IsEnabled = true;
        }

        private void SystemTrayExit_Click(object sender, RoutedEventArgs e)
        {
            Instance.NotifyIcon.Visibility = Visibility.Collapsed;
            App.Current.Shutdown();
        }

        #endregion

        #region dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    NotifyIcon.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                NotifyIcon = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~SystemTrayIcon()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
