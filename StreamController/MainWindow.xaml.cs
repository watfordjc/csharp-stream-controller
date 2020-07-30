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
using System.Windows.Input;
using uk.JohnCook.dotnet.StreamController.Controls;
using System.Windows.Automation.Peers;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : StyledWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            PopulateAudioInterfaces();
            PopulateApplications();
        }

        private void PopulateAudioInterfaces()
        {
            cb_interfaces.ItemsSource = AudioInterfaceCollection.ActiveDevices;
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
            }
            else
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
            if (e.OldItems == null)
            {
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

        private void NotifyLiveRegionChanged(DataGrid sender)
        {

            AutomationPeer peer = UIElementAutomationPeer.FromElement(sender as UIElement);

            if (peer == null) { return; }
            Dispatcher.Invoke(
                () => peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)
                );
        }



        private void OnDefaultDeviceChanged(object sender, DataFlow flow)
        {
            if (flow == DataFlow.Render)
            {
                cb_interfaces.SelectedItem = AudioInterfaceCollection.Instance.DefaultRender;
                group_default_render.Items.Clear();
                int newRow = group_default_render.Items.Add(AudioInterfaceCollection.Instance.DefaultRender);
                if (newRow != -1)
                {
                    NotifyLiveRegionChanged(group_default_render);
                }
            }
            else if (flow == DataFlow.Capture)
            {
                group_default_capture.DataContext = AudioInterfaceCollection.Instance.DefaultCapture;
            }
        }

        private void MakeInterfaceDefaultRenderDevice()
        {
            AudioInterfaceCollection.ChangeDefaultDevice((cb_interfaces.SelectedItem as AudioInterface).ID);
        }

        private void SetApplicationDefaultDevice()
        {
            ObservableProcess process = (cb_applications.SelectedItem as ObservableProcess);
            AudioInterface currentInterface = (cb_interfaces.SelectedItem as AudioInterface);
            AudioInterfaceCollection.ChangeDefaultApplicationDevice(currentInterface, process);
            UpdateApplicationAudioDevices(process);
        }

        private void ResetCustomAudioDevices()
        {
            AudioInterfaceCollection.ClearAllApplicationDefaultDevices(DataFlow.All);
            UpdateApplicationAudioDevices(cb_applications.SelectedItem as ObservableProcess);
        }

        private static async Task ToggleAllCustomAudioDevices()
        {
            await Task.Run(
                () => AudioInterfaceCollection.ToggleAllDefaultApplicationDevice()
                ).ConfigureAwait(true);
        }

        private void BtnMakeDefaultRender_Click(object sender, RoutedEventArgs e)
        {
            MakeInterfaceDefaultRenderDevice();
        }

        private void BtnSetApplicationDefault_Click(object sender, RoutedEventArgs e)
        {
            SetApplicationDefaultDevice();
        }

        private void BtnResetAllApplicationDefault_Click(object sender, RoutedEventArgs e)
        {
            ResetCustomAudioDevices();
        }

        private async void BtnToggleAllApplicationDefault_Click(object sender, RoutedEventArgs e)
        {
            (e.OriginalSource as Button).IsEnabled = false;
            await ToggleAllCustomAudioDevices().ConfigureAwait(true);
            (e.OriginalSource as Button).IsEnabled = true;
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                    && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    App.Current.Shutdown();
                }
            }
            else if (e.Key == Key.F5)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    ResetCustomAudioDevices();
                }
            }
            else if (e.Key == Key.M)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    MakeInterfaceDefaultRenderDevice();
                }
            }
            else if (e.Key == Key.T)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    await ToggleAllCustomAudioDevices().ConfigureAwait(true);
                }
            }
            else if (e.Key == Key.U)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    SetApplicationDefaultDevice();
                }
            }
        }
    }
}
