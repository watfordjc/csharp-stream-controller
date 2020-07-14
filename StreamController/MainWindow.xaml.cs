using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using uk.JohnCook.dotnet.NAudioWrapperLibrary;
using uk.JohnCook.dotnet.StreamController.SharedModels;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PopulateAudioInterfaces();
        }

        private void PopulateAudioInterfaces()
        {
            cb_interfaces.ItemsSource = AudioInterfaceCollection.Devices;
            AudioInterfaceCollection.Instance.DefaultDeviceChange += OnDefaultDeviceChanged;
            if (AudioInterfaceCollection.Instance.DefaultRender != null) { OnDefaultDeviceChanged(this, DataFlow.Render); }
            if (AudioInterfaceCollection.Instance.DefaultCapture != null) { OnDefaultDeviceChanged(this, DataFlow.Capture); }
            cb_applications.ItemsSource = Process.GetProcesses();
            cb_applications.SelectionChanged += Cb_applications_SelectionChanged;
        }

        private void Cb_applications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int processId = (e.AddedItems[0] as Process).Id;
            UpdateApplicationAudioDevices(processId);
        }

        private void UpdateApplicationAudioDevices(int processId)
        {
            AudioInterface applicationRender = AudioInterfaceCollection.GetDefaultApplicationDevice(DataFlow.Render, processId);
            AudioInterface applicationCapture = AudioInterfaceCollection.GetDefaultApplicationDevice(DataFlow.Capture, processId);
            app_render.Text = applicationRender?.FriendlyName ?? "Default";
            app_capture.Text = applicationCapture?.FriendlyName ?? "Default";
        }

        private void OnDefaultDeviceChanged(object sender, DataFlow flow)
        {
            if (flow == DataFlow.Render)
            {
                cb_interfaces.SelectedItem = AudioInterfaceCollection.Instance.DefaultRender;
                group_default_render.DataContext = AudioInterfaceCollection.Instance.DefaultRender;
            }
            else if (flow == DataFlow.Capture)
            {
                group_default_capture.DataContext = AudioInterfaceCollection.Instance.DefaultCapture;
            }
        }

        private void BtnMakeDefaultRender_Click(object sender, RoutedEventArgs e)
        {
            AudioInterfaceCollection.ChangeDefaultDevice((cb_interfaces.SelectedItem as AudioInterface).ID);
        }

        private void BtnSetApplicationDefault_Click(object sender, RoutedEventArgs e)
        {
            int processId = (cb_applications.SelectedItem as Process).Id;
            AudioInterface currentInterface = (cb_interfaces.SelectedItem as AudioInterface);
            AudioInterfaceCollection.ChangeDefaultApplicationDevice(currentInterface, processId);
            UpdateApplicationAudioDevices(processId);
        }

        private void BtnResetAllApplicationDefault_Click(object sender, RoutedEventArgs e)
        {
            int processId = (cb_applications.SelectedItem as Process).Id;
            AudioInterfaceCollection.ClearAllApplicationDefaultDevices(DataFlow.All);
            UpdateApplicationAudioDevices(processId);
        }
    }
}
