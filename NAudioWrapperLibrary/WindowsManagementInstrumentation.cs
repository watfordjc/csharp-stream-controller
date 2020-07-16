using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Text;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary
{
    public static class WindowsManagementInstrumentation
    {
        public static ManagementEventWatcher NewProcessCreatedEventWatcher()
        {
            ManagementEventWatcher processCreationWatcher = new ManagementEventWatcher()
            {
                Query = new WqlEventQuery(
                    "__InstanceCreationEvent",
                    new TimeSpan(0, 0, 1),
                    "TargetInstance isa \"Win32_Process\""
                    ),
                Options =
                {
                    Timeout = new TimeSpan(0, 0, 5)
                }
            };
            return processCreationWatcher;
        }

        public static ManagementEventWatcher ProcessDestroyedEventWatcher()
        {
            ManagementEventWatcher processDestroyedWatcher = new ManagementEventWatcher()
            {
                Query = new WqlEventQuery(
                    "__InstanceDeletionEvent",
                    new TimeSpan(0, 0, 1),
                    "TargetInstance isa \"Win32_Process\""
                    ),
                Options =
                {
                    Timeout = new TimeSpan(0, 0, 5)
                }
            };
            return processDestroyedWatcher;
        }
    }
}
