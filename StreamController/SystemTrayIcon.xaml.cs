using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using uk.JohnCook.dotnet.NAudioWrapperLibrary;
using uk.JohnCook.dotnet.StreamController.Controls;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for SystemTrayIcon.xaml
    /// </summary>
    public partial class SystemTrayIcon : StyledWindow
    {
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

        private void StyledWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ThemeChanged -= SystemTrayIcon_ThemeChanged;
            AudioInterfaceCollection.Instance.CollectionEnumerated -= Devices_CollectionEnumerated;
            NotifyIcon.TrayMouseDoubleClick -= NotifyIcon_TrayMouseDoubleClick;
            NotifyIcon.Dispose();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            NotifyIcon.ContextMenu.Placement = PlacementMode.RelativePoint;
        }

        private async void SystemTrayIcon_ThemeChanged(object sender, WindowUtilityLibrary.WindowsTheme oldTheme)
        {
            if (Instance.CurrentTheme == oldTheme)
            {
                return;
            }
            SystemTrayIcon oldWindow = Instance;
            oldWindow.NotifyIcon.ContextMenu.IsOpen = false;
            oldWindow.NotifyIcon.Visibility = Visibility.Hidden;
            oldWindow.Visibility = Visibility.Collapsed;

            lazySingleton = new Lazy<SystemTrayIcon>(
                () => new SystemTrayIcon(false)
            );
            if (AudioInterfaceCollection.Instance.DevicesAreEnumerated)
            {
                Instance.Devices_CollectionEnumerated(this, EventArgs.Empty);
            }
            Instance.Visibility = Visibility.Collapsed;
            Instance.NotifyIcon.Icon = oldWindow.NotifyIcon.Icon;
            await Instance.UpdateTrayIcon().ConfigureAwait(true);
            oldWindow.Close();
            oldWindow.Dispose();
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

        public async Task UpdateTrayIcon()
        {
            try
            {
                if (NotifyIcon.Visibility != Visibility.Visible)
                {
                    await Task.Delay(1500).ConfigureAwait(true);
                    Dispatcher.Invoke(
                        () => NotifyIcon.Visibility = Visibility.Visible
                        );
                }
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

    }
}
