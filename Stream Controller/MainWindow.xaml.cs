using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudioWrapperLibrary;

namespace Stream_Controller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
        private readonly AudioEndpointNotificationCallback notificationCallback;
        private readonly IMMNotificationClient notificationClient;
        private readonly System.Collections.ObjectModel.ObservableCollection<String> devices = new System.Collections.ObjectModel.ObservableCollection<string>();
        private MMDevice defaultRenderDevice;
        private MMDevice defaultCaptureDevice;
        private MMDevice currentInterface;
        private readonly AudioInterfaces audioInterfaces = AudioInterfaces.Instance;
        public MainWindow()
        {
            InitializeComponent();
            notificationCallback = new AudioEndpointNotificationCallback(this);
            notificationClient = (IMMNotificationClient)notificationCallback;
            deviceEnumerator.RegisterEndpointNotificationCallback(notificationClient);
            PopulateAudioInterfaces();
        }

        private void PopulateAudioInterfaces()
        {
            foreach (MMDevice device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active | DeviceState.Unplugged | DeviceState.NotPresent))
            {
                devices.Add(device.ID);
                if (device.State != DeviceState.NotPresent)
                {
                    cb_interfaces.Items.Add(device.FriendlyName.ToString());
                    continue;
                }
                cb_interfaces.Items.Add(device.ID.ToString());
            }
            cb_interfaces.SelectionChanged += CurrentDeviceChanged;
            defaultRenderDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            DefaultRenderDeviceChanged();
            defaultCaptureDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            DefaultCaptureDeviceChanged();
        }

        private void ChangeDefaultRenderDevice(string defaultDeviceId)
        {
            defaultRenderDevice.AudioEndpointVolume.OnVolumeNotification -= OnDefaultRenderDeviceVolumeNotification;
            defaultRenderDevice = deviceEnumerator.GetDevice(defaultDeviceId);
            DefaultRenderDeviceChanged();
        }

        private void OnDefaultRenderDeviceVolumeNotification(AudioVolumeNotificationData data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                render_interface_volume.Text = Math.Round(defaultRenderDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100f).ToString() + "%";
                render_interface_muted.Text = defaultRenderDevice.AudioEndpointVolume.Mute == true ? "Yes" : "No";
            });
        }

        private void ChangeDefaultCaptureDevice(string defaultDeviceId)
        {
            defaultCaptureDevice.AudioEndpointVolume.OnVolumeNotification -= OnDefaultCaptureDeviceVolumeNotification;
            defaultCaptureDevice = deviceEnumerator.GetDevice(defaultDeviceId);
            DefaultCaptureDeviceChanged();
        }

        private void OnDefaultCaptureDeviceVolumeNotification(AudioVolumeNotificationData data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                capture_interface_volume.Text = Math.Round(defaultCaptureDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100f).ToString() + "%";
                capture_interface_muted.Text = defaultCaptureDevice.AudioEndpointVolume.Mute == true ? "Yes" : "No";
            });
        }

        private void OnCurrentInterfaceVolumeNotification(AudioVolumeNotificationData data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                interface_volume.Text = Math.Round(currentInterface.AudioEndpointVolume.MasterVolumeLevelScalar * 100f).ToString() + "%";
                interface_muted.Text = data.Muted == true ? "Yes" : "No";
            });
        }

        private class AudioEndpointNotificationCallback : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
        {
            private readonly MainWindow mWindow;
            public AudioEndpointNotificationCallback(MainWindow window)
            {
                this.mWindow = window;
            }

            public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
            {
                if (role != Role.Console)
                {
                    return;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (flow == DataFlow.Render)
                    {
                        mWindow.ChangeDefaultRenderDevice(defaultDeviceId);
                    }
                    else if (flow == DataFlow.Capture)
                    {
                        mWindow.ChangeDefaultCaptureDevice(defaultDeviceId);
                    }
                });
            }

            public void OnDeviceAdded(string pwstrDeviceId)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (mWindow.devices.Contains(pwstrDeviceId))
                    {
                        return;
                    }
                    else
                    {
                        mWindow.devices.Add(pwstrDeviceId);
                        mWindow.cb_interfaces.Items.Add(mWindow.deviceEnumerator.GetDevice(pwstrDeviceId).FriendlyName);
                    }
                });
            }

            public void OnDeviceRemoved(string deviceId)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!mWindow.devices.Contains(deviceId))
                    {
                        return;
                    }
                    else
                    {
                        int old_index = -1;
                        if (mWindow.currentInterface == mWindow.deviceEnumerator.GetDevice(deviceId))
                        {
                            old_index = mWindow.devices.IndexOf(mWindow.currentInterface.ID);
                            mWindow.currentInterface = mWindow.defaultRenderDevice;
                            mWindow.cb_interfaces.SelectedIndex = mWindow.devices.IndexOf(mWindow.currentInterface.ID);
                        }
                        mWindow.devices.RemoveAt(old_index);
                        mWindow.cb_interfaces.Items.RemoveAt(old_index);
                    }
                });
            }

            public void OnDeviceStateChanged(string deviceId, DeviceState newState)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DeviceState oldState = mWindow.deviceEnumerator.GetDevice(deviceId).State;
                    int cb_selected = mWindow.cb_interfaces.SelectedIndex;
                    int index = -1;
                    if (mWindow.cb_interfaces.Items.Contains(deviceId))
                    {
                        index = mWindow.cb_interfaces.Items.IndexOf(deviceId);
                        if (newState == DeviceState.Active || newState == DeviceState.Unplugged)
                        {
                            mWindow.cb_interfaces.Items[index] = mWindow.deviceEnumerator.GetDevice(deviceId).FriendlyName;
                        }
                    } else if (newState != DeviceState.Active && newState != DeviceState.Unplugged)
                    {
                        index = mWindow.devices.IndexOf(deviceId);
                        mWindow.cb_interfaces.Items[index] = deviceId;
                    }
                    if (cb_selected == index)
                    {
                        mWindow.cb_interfaces.SelectedIndex = index;
                    }
                });
                return;
            }

            public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
            {
                return;
                // throw new NotImplementedException();
            }
        }

        private void DefaultRenderDeviceChanged()
        {
            int device_index = devices.IndexOf(defaultRenderDevice.ID);
            cb_interfaces.SelectedIndex = device_index;
            render_interface_name.Text = defaultRenderDevice.FriendlyName;
            render_interface_type.Text = defaultRenderDevice.DataFlow.ToString();
            render_interface_state.Text = defaultRenderDevice.State.ToString();
            render_interface_volume.Text = Math.Round(defaultRenderDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100f).ToString() + "%";
            defaultRenderDevice.AudioEndpointVolume.OnVolumeNotification += OnDefaultRenderDeviceVolumeNotification;
            render_interface_muted.Text = defaultRenderDevice.AudioEndpointVolume.Mute == true ? "Yes" : "No";
        }

        private void DefaultCaptureDeviceChanged()
        {
            capture_interface_name.Text = defaultCaptureDevice.FriendlyName;
            capture_interface_type.Text = defaultCaptureDevice.DataFlow.ToString();
            capture_interface_state.Text = defaultCaptureDevice.State.ToString();
            capture_interface_volume.Text = Math.Round(defaultCaptureDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100f).ToString() + "%";
            defaultCaptureDevice.AudioEndpointVolume.OnVolumeNotification += OnDefaultCaptureDeviceVolumeNotification;
            capture_interface_muted.Text = defaultCaptureDevice.AudioEndpointVolume.Mute == true ? "Yes" : "No";
        }

        private void CurrentDeviceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            if (e.RemovedItems.Count == 1 && currentInterface.State == DeviceState.Active)
            {
                currentInterface.AudioEndpointVolume.OnVolumeNotification -= OnCurrentInterfaceVolumeNotification;
            }
            //currentInterface = deviceEnumerator.GetDevice(devices[cb_interfaces.SelectedIndex]);
            int new_index = (sender as ComboBox).SelectedIndex;
            currentInterface = deviceEnumerator.GetDevice(devices[new_index]);
            if (currentInterface.State != DeviceState.NotPresent)
            {
                interface_name.Text = currentInterface.FriendlyName;
            } else
            {
                interface_name.Text = currentInterface.ID;
            }
            
            interface_type.Text = currentInterface.DataFlow.ToString();
            interface_state.Text = currentInterface.State.ToString();
            if (currentInterface.State == DeviceState.Active)
            {
                interface_volume_label.Visibility = Visibility.Visible;
                interface_volume.Text = Math.Round(currentInterface.AudioEndpointVolume.MasterVolumeLevelScalar * 100f).ToString() + "%";
                interface_volume.Visibility = Visibility.Visible;
                interface_muted_label.Visibility = Visibility.Visible;
                interface_muted.Text = currentInterface.AudioEndpointVolume.Mute == true ? "Yes" : "No";
                interface_muted.Visibility = Visibility.Visible;
                currentInterface.AudioEndpointVolume.OnVolumeNotification += OnCurrentInterfaceVolumeNotification;
            }
            else
            {
                interface_volume_label.Visibility = Visibility.Collapsed;
                interface_volume.Visibility = Visibility.Collapsed;
                interface_muted_label.Visibility = Visibility.Collapsed;
                interface_muted.Visibility = Visibility.Collapsed;
            }
        }

        private void MenuItemPreferences_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilityLibrary.MakeWindowActive(WindowUtilityLibrary.PREFERENCES);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItemObs_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilityLibrary.MakeWindowActive(WindowUtilityLibrary.WEB_SOCKETS);
        }
    }
}
