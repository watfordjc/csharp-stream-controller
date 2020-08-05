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
using uk.JohnCook.dotnet.StreamController.Controls;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for SystemTrayIcon.xaml
    /// </summary>
    public partial class SystemTrayIcon : StyledWindow
    {
        private bool disposedValue;
        public static SystemTrayIcon Instance { get { return lazySingleton.Value; } }

        private static Lazy<SystemTrayIcon> lazySingleton =
        new Lazy<SystemTrayIcon>(
            () => new SystemTrayIcon(false)
        );

        public SystemTrayIcon(bool applicationWindow) : base(applicationWindow)
        {
            InitializeComponent();
            ThemeChanged += SystemTrayIcon_ThemeChanged;
            AudioInterfaceCollection.Instance.CollectionEnumerated += Devices_CollectionEnumerated;
            NotifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
        }

        private void SystemTrayIcon_ThemeChanged(object sender, WindowUtilityLibrary.WindowsTheme oldTheme)
        {
            if (Instance.CurrentTheme == oldTheme)
            {
                return;
            }
            SystemTrayIcon oldWindow = Instance;
            oldWindow.NotifyIcon.ContextMenu.IsOpen = false;
            oldWindow.NotifyIcon.Visibility = Visibility.Hidden;
            oldWindow.Close();

            lazySingleton = new Lazy<SystemTrayIcon>(
                () => new SystemTrayIcon(false)
            );
            if (AudioInterfaceCollection.Instance.DevicesAreEnumerated)
            {
                Instance.Devices_CollectionEnumerated(this, EventArgs.Empty);
            }
            UpdateTrayIcon();
            oldWindow.Dispose(true);
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

        public static async void UpdateTrayIcon()
        {
            try
            {
                Instance.NotifyIcon.Icon = Properties.Resources.icon;
                await Task.Delay(1500).ConfigureAwait(true);
                Instance.NotifyIcon.Visibility = Visibility.Visible;
            }
            catch (NullReferenceException)
            {
                return;
            }

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

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    ThemeChanged -= SystemTrayIcon_ThemeChanged;
                    AudioInterfaceCollection.Instance.CollectionEnumerated -= Devices_CollectionEnumerated;
                    NotifyIcon.TrayMouseDoubleClick -= NotifyIcon_TrayMouseDoubleClick;
                    NotifyIcon.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
