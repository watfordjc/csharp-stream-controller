using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using uk.JohnCook.dotnet.NAudioWrapperLibrary;
using System.Threading.Tasks;

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
            PopulateApplications();
        }

        private void PopulateAudioInterfaces()
        {
            cb_interfaces.ItemsSource = AudioInterfaceCollection.Devices;
            AudioInterfaceCollection.Instance.DefaultDeviceChanged += OnDefaultDeviceChanged;
            if (AudioInterfaceCollection.Instance.DefaultRender != null) { OnDefaultDeviceChanged(this, DataFlow.Render); }
            if (AudioInterfaceCollection.Instance.DefaultCapture != null) { OnDefaultDeviceChanged(this, DataFlow.Capture); }
        }

        private void PopulateApplications()
        {
            cb_applications.ItemsSource = ProcessCollection.Processes;
            if (ProcessCollection.Instance.ProcessesAreEnumerated)
            {
                Processes_CollectionEnumerated(this, EventArgs.Empty);
            } else
            {
                ProcessCollection.Instance.CollectionEnumerated += Processes_CollectionEnumerated;
            }
            ProcessCollection.Instance.CollectionChanged += Processes_CollectionChanged;
            cb_applications.SelectionChanged += ApplicationsComboBox_SelectionChanged;
        }

        private void Processes_CollectionEnumerated(object sender, EventArgs e)
        {
            int currentProcessId = Process.GetCurrentProcess().Id;
            cb_applications.SelectedItem = ProcessCollection.Processes.FirstOrDefault(x => x.Id == currentProcessId);
        }

        private void Processes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableProcess currentProcess = cb_applications.SelectedItem as ObservableProcess;
            if (e.OldItems == null) {
                return;
            }
            if (currentProcess == null && e.Action == NotifyCollectionChangedAction.Move)
            {
                currentProcess = e.NewItems[0] as ObservableProcess;
                cb_applications.SelectedItem = currentProcess;
            }
            if (currentProcess == null || (e.OldItems.Contains(currentProcess) && e.Action == NotifyCollectionChangedAction.Remove))
            {
                cb_applications.SelectedItem = ProcessCollection.Processes.FirstOrDefault(x => x.Id == Process.GetCurrentProcess().Id);
            }
        }

        private void ApplicationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                UpdateApplicationAudioDevices(e.AddedItems[0] as ObservableProcess);
            }
        }

        private void UpdateApplicationAudioDevices(ObservableProcess process)
        {
            AudioInterface applicationRender = AudioInterfaceCollection.GetDefaultApplicationDevice(DataFlow.Render, process);
            AudioInterface applicationCapture = AudioInterfaceCollection.GetDefaultApplicationDevice(DataFlow.Capture, process);
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
            ObservableProcess process = (cb_applications.SelectedItem as ObservableProcess);
            AudioInterface currentInterface = (cb_interfaces.SelectedItem as AudioInterface);
            AudioInterfaceCollection.ChangeDefaultApplicationDevice(currentInterface, process);
            UpdateApplicationAudioDevices(process);
        }

        private void BtnResetAllApplicationDefault_Click(object sender, RoutedEventArgs e)
        {
            AudioInterfaceCollection.ClearAllApplicationDefaultDevices(DataFlow.All);
            UpdateApplicationAudioDevices(cb_applications.SelectedItem as ObservableProcess);
        }

        private async void BtnToggleAllApplicationDefault_Click(object sender, RoutedEventArgs e)
        {
            (e.OriginalSource as Button).IsEnabled = false;
            await Task.Run(
                () => AudioInterfaceCollection.ToggleAllDefaultApplicationDevice()
                ).ConfigureAwait(true);
            (e.OriginalSource as Button).IsEnabled = true;
        }
    }
}
