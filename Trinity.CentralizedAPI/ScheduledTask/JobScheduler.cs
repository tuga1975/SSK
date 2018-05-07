using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Quartz;
using Quartz.Impl;

namespace Trinity.BackendAPI.ScheduledTask
{
    public class JobScheduler
    {
        public async static void Start()
        {
            NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };


            IJobDetail jobReminder = JobBuilder.Create<JobReminder>()
                .WithIdentity("jobReminder", "JobScheduler")
                .Build();

            ITrigger triggerReminder = TriggerBuilder.Create()
           .WithIdentity("jobReminder", "JobScheduler")
           .WithDailyTimeIntervalSchedule
             (s =>
                s.WithIntervalInHours(24)
               .OnEveryDay()
               .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(21, 00))
             )
           .ForJob(jobReminder)
           .WithIdentity("Reminder")
           .Build();


            IJobDetail jobAbsent = JobBuilder.Create<JobAbsent>()
                .WithIdentity("jobAbsent", "JobScheduler")
                .Build();

            ITrigger triggerAbsent = TriggerBuilder.Create()
           .WithIdentity("jobAbsent", "JobScheduler")
           .WithDailyTimeIntervalSchedule
             (s =>
                s.WithIntervalInHours(24)
               .OnEveryDay()
               .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(21, 00))
             )
           .ForJob(jobAbsent)
           .WithIdentity("Absent")
           .Build();

            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.ScheduleJob(jobReminder, triggerReminder);
            await scheduler.ScheduleJob(jobAbsent, triggerAbsent);
            await scheduler.Start();
        }
    }
}