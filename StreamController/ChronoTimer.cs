using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Themes;
using Quartz;
using Quartz.Impl;

namespace uk.JohnCook.dotnet.StreamController
{
    public class ChronoTimer : INotifyPropertyChanged
    {
        public bool DateTimeSet { get; internal set; }

        public static ChronoTimer Instance { get { return lazySingleton.Value; } }

        private readonly ITrigger chronoMinuteTrigger = TriggerBuilder.Create()
            .WithIdentity("ChronoMinute", "ChronoTimers")
            .WithCronSchedule("00 * * * * ?")
            .Build();

        private readonly ITrigger chronoSecondTrigger = TriggerBuilder.Create()
            .WithIdentity("ChronoSecond", "ChronoTimers")
            .WithCronSchedule("01-59 * * * * ?")
            .Build();

        private IScheduler Scheduler { get; set; }

        private static readonly Lazy<ChronoTimer> lazySingleton = new Lazy<ChronoTimer>(
            () => new ChronoTimer()
            );

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DateTime> MinuteChanged;
        public event EventHandler<DateTime> SecondChanged;

        private ChronoTimer()
        {
            ICronTrigger cronTrigger = (ICronTrigger)chronoMinuteTrigger;
            cronTrigger.TimeZone = TimeZoneInfo.Utc;

            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            Scheduler = schedulerFactory.GetScheduler().Result;
            Scheduler.Start();

            Scheduler.ScheduleJob(
                JobBuilder.Create<ClockMinute>()
                .WithIdentity("ClockMinute", "OBS")
                .Build(),
                chronoMinuteTrigger);

            Scheduler.ScheduleJob(JobBuilder.Create<ClockSecond>()
                .WithIdentity("ClockSecond", "OBS")
                .Build(),
                chronoSecondTrigger);
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyMinuteChanged(DateTime dateTime)
        {
            MinuteChanged?.Invoke(this, dateTime);
        }

        public void NotifySecondChanged(DateTime dateTime)
        {
            SecondChanged?.Invoke(this, dateTime);
        }
    }

    internal class ClockMinute : IJob
    {
        public ClockMinute() { }

        public Task Execute(IJobExecutionContext context)
        {
            ChronoTimer.Instance.NotifyMinuteChanged(DateTime.UtcNow);
            return Task.CompletedTask;
        }
    }

    internal class ClockSecond : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            ChronoTimer.Instance.NotifySecondChanged(DateTime.UtcNow);
            return Task.CompletedTask;
        }
    }
}
