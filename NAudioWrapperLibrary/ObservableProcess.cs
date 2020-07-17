using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary
{
    public class ObservableProcess : INotifyPropertyChanged
    {
        public string ProcessName { get; private set; }
        public int Id { get; private set; }
        public string MainWindowTitle { get; private set; }
        public bool AudioDeviceTogglingNeeded { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableProcess(Process process)
        {
            if (process == null) { throw new ArgumentNullException(nameof(process)); }

            ProcessName = process.ProcessName;
            Id = process.Id;
            try
            {
                MainWindowTitle = process.MainWindowTitle;
            }
            catch (InvalidOperationException)
            {
                Task.Run(
                    () => GetMainWindowTitle(process)
                    ).ConfigureAwait(true);
            }
        }

        private Task GetMainWindowTitle(Process process)
        {
            try
            {
                process.WaitForInputIdle();
                process.Refresh();
                MainWindowTitle = process.MainWindowTitle;
            }
            catch (InvalidOperationException)
            {
                MainWindowTitle = null;
            }
            NotifyPropertyChanged(nameof(MainWindowTitle));
            return Task.CompletedTask;
        }

        public Task RefreshWindowTitle()
        {
            Process process = Process.GetProcessById(Id);
            return GetMainWindowTitle(process);
        }
    }
}
