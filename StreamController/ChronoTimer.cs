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
        public DateTime CurrentUtcTime { get; internal set; }
        public DateTime CurrentLocalTime { get; internal set; }
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

        public ChronoTimer()
        {
            ICronTrigger cronTrigger = (ICronTrigger)chronoMinuteTrigger;
            cronTrigger.TimeZone = TimeZoneInfo.Utc;

            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            Scheduler = schedulerFactory.GetScheduler().Result;
            Scheduler.Start();

            AddMinuteJob(JobBuilder.Create<ClockMinute>()
                .WithIdentity("ClockMinute", "OBS")
                .Build());

            AddSecondJob(JobBuilder.Create<ClockSecond>()
                .WithIdentity("ClockSecond", "OBS")
                .Build());
        }

        public void AddMinuteJob(IJobDetail jobDetail)
        {
            Scheduler.ScheduleJob(jobDetail, chronoMinuteTrigger);
        }

        public void RemoveJob(TriggerKey triggerKey)
        {
            Scheduler.UnscheduleJob(triggerKey);
        }

        public void AddSecondJob(IJobDetail jobDetail)
        {
            Scheduler.ScheduleJob(jobDetail, chronoSecondTrigger);
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class ClockMinute : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            ChronoTimer.Instance.CurrentUtcTime = DateTime.UtcNow;
            ChronoTimer.Instance.CurrentLocalTime = DateTime.Now;
            ChronoTimer.Instance.NotifyPropertyChanged(nameof(ChronoTimer.CurrentUtcTime));
            ChronoTimer.Instance.NotifyPropertyChanged(nameof(ChronoTimer.CurrentLocalTime));
            if (!ChronoTimer.Instance.DateTimeSet)
            {
                ChronoTimer.Instance.DateTimeSet = true;
                ChronoTimer.Instance.NotifyPropertyChanged(nameof(ChronoTimer.DateTimeSet));
            }
            return Task.CompletedTask;
        }
    }

    internal class ClockSecond : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            ChronoTimer.Instance.CurrentUtcTime = DateTime.UtcNow;
            ChronoTimer.Instance.CurrentLocalTime = DateTime.Now;
            ChronoTimer.Instance.NotifyPropertyChanged(nameof(ChronoTimer.CurrentUtcTime));
            ChronoTimer.Instance.NotifyPropertyChanged(nameof(ChronoTimer.CurrentLocalTime));
            if (!ChronoTimer.Instance.DateTimeSet)
            {
                ChronoTimer.Instance.DateTimeSet = true;
                ChronoTimer.Instance.NotifyPropertyChanged(nameof(ChronoTimer.DateTimeSet));
            }
            return Task.CompletedTask;
        }
    }
}
