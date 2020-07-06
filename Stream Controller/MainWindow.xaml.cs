using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using NAudioWrapperLibrary;
using StreamController.SharedModels;

namespace StreamController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly AudioInterfaces audioInterfaces = AudioInterfaces.Instance;
        private readonly ObservableCollection<AudioInterface> devices = audioInterfaces.Devices;

        public MainWindow()
        {
            InitializeComponent();
            PopulateAudioInterfaces();
        }

        private void PopulateAudioInterfaces()
        {
            cb_interfaces.ItemsSource = devices;
            audioInterfaces.DefaultDeviceChange += OnDefaultDeviceChanged;
            if (audioInterfaces.DefaultRender != null) { OnDefaultDeviceChanged(this, DataFlow.Render); }
            if (audioInterfaces.DefaultCapture != null) { OnDefaultDeviceChanged(this, DataFlow.Capture); }
        }

        private void OnDefaultDeviceChanged(object sender, DataFlow flow)
        {
            if (flow == DataFlow.Render)
            {
                cb_interfaces.SelectedItem = audioInterfaces.DefaultRender;
                group_default_render.DataContext = audioInterfaces.DefaultRender;
            }
            else if (flow == DataFlow.Capture)
            {
                group_default_capture.DataContext = audioInterfaces.DefaultCapture;
            }
        }
    }
}
