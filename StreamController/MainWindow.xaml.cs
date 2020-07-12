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
    }
}
