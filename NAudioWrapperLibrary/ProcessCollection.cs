using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Management;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary
{
    public sealed class ProcessCollection : ObservableCollection<ObservableProcess>, IDisposable
    {
        private readonly SynchronizationContext _Context;
        private static readonly Lazy<ProcessCollection> lazySingleton =
            new Lazy<ProcessCollection>(
                () => new ProcessCollection()
            );
        private bool disposedValue;

        public static ProcessCollection Instance { get { return lazySingleton.Value; } }
        public static ObservableCollection<ObservableProcess> Processes { get; } = new ObservableCollection<ObservableProcess>();
        private static List<string> ProcessNameList { get; } = new List<string>();
        public bool ProcessesAreEnumerated { get; private set; }
        private readonly ManagementEventWatcher processCreatedEventWatcher;
        private readonly ManagementEventWatcher processDeletedEventWatcher;

        private ProcessCollection()
        {
            _Context = SynchronizationContext.Current;
            Processes.CollectionChanged += Processes_CollectionChanged;
            processCreatedEventWatcher = WindowsManagementInstrumentation.NewProcessCreatedEventWatcher();
            processCreatedEventWatcher.EventArrived += ProcessCreatedEventArrived;
            processCreatedEventWatcher.Start();
            processDeletedEventWatcher = WindowsManagementInstrumentation.ProcessDestroyedEventWatcher();
            processDeletedEventWatcher.EventArrived += ProcessDeletedEventArrived;
            processDeletedEventWatcher.Start();
            Initialise();
        }

        private async void Initialise()
        {
            await Task.Run(
                () => EnumerateProcesses()
                ).ConfigureAwait(true);
        }

        private Task EnumerateProcesses()
        {
            foreach (Process process in Process.GetProcesses())
            {
                ObservableProcess observableProcess = new ObservableProcess(process);
                if (!String.IsNullOrEmpty(observableProcess.MainWindowTitle))
                {
                    _Context.Send(
                        x => Processes.Add(observableProcess),
                        null);
                }
            }
            ProcessesAreEnumerated = true;
            NotifyCollectionEnumerated();
            return Task.CompletedTask;
        }

        public event EventHandler CollectionEnumerated;
        public new event NotifyCollectionChangedEventHandler CollectionChanged;

        void NotifyCollectionEnumerated()
        {
            _Context.Send(
                x => SortCollection(),
                null);
            _Context.Send(
                x => CollectionEnumerated?.Invoke(this, EventArgs.Empty),
                null);
        }

        void NotifyCollectionChanged(NotifyCollectionChangedEventArgs collectionChangedEventArgs)
        {
            _Context.Send(
                x => Instance.CollectionChanged?.Invoke(this, collectionChangedEventArgs),
                null);
        }

        private void Processes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs collectionChangedEventArgs)
        {
            NotifyCollectionChanged(collectionChangedEventArgs);
        }

        private static void SortCollection()
        {
            ProcessNameList.Clear();
            ProcessNameList.AddRange(Processes.Select(x => x.ProcessName).ToList());
            ProcessNameList.Sort();
            string currentProcessName = String.Empty;
            int processNameOccurance = 0;
            for (int i = 0; i < ProcessNameList.Count; i++)
            {
                if (ProcessNameList[i] == currentProcessName)
                {
                    processNameOccurance++;
                }
                else
                {
                    currentProcessName = ProcessNameList[i];
                    processNameOccurance = 0;
                }
                ObservableProcess observableProcess = Processes.Where(x => x.ProcessName == ProcessNameList[i]).ToArray()[processNameOccurance];
                Processes.Move(Processes.IndexOf(observableProcess), i);
            }
        }

        public static void ProcessCreatedEventArrived(object sender, EventArrivedEventArgs e)
        {
            if (e == null) { throw new ArgumentNullException(nameof(e)); }

            ManagementBaseObject eventProcess = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            string processName = eventProcess.Properties["Name"].Value.ToString();
            int processId = int.Parse(eventProcess.Properties["ProcessId"].Value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);
            ObservableProcess newProcess = new ObservableProcess(Process.GetProcessById(processId));
            if (!String.IsNullOrEmpty(newProcess.MainWindowTitle))
            {
                Instance._Context.Send(
                    x => Processes.Add(newProcess),
                    null);
                Instance._Context.Send(
                    x => SortCollection(),
                    null);
            }
            //Trace.WriteLine($"A new {processName} process was created with PID {processId}!");
        }

        public static void ProcessDeletedEventArrived(object sender, EventArrivedEventArgs e)
        {
            if (e == null) { throw new ArgumentNullException(nameof(e)); }

            ManagementBaseObject eventProcess = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            string processName = eventProcess.Properties["Name"].Value.ToString();
            int processId = int.Parse(eventProcess.Properties["ProcessId"].Value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);
            ObservableProcess deletedProcess = Processes.FirstOrDefault(x => x.Id == processId);
            if (deletedProcess != default)
            {
                Instance._Context.Send(
                    x => Processes.Remove(deletedProcess),
                    null);
                Instance._Context.Send(
                    x => SortCollection(),
                    null);
            }
            //Trace.WriteLine($"Process {processName} with PID {processId} has been destroyed!");
        }


        #region dispose

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    processCreatedEventWatcher.Dispose();
                    processDeletedEventWatcher.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ProcessCollection()
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
