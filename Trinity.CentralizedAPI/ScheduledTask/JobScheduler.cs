﻿using System;
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
            // Grab the Scheduler instance from the Factory
            NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
            
            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<EmailJob>()
                .WithIdentity("jobSendMail", "JobScheduler")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
           .WithIdentity("triggerSendMail", "JobScheduler")
           .WithDailyTimeIntervalSchedule
             (s =>
                s.WithIntervalInHours(24)
               .OnEveryDay()
               .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(13, 32))
             )
           .ForJob(job)
           .WithIdentity("EmailTrigger")
           .Build();

            //// Trigger the job to run now, and then repeat every 10 seconds
            //ITrigger trigger = TriggerBuilder.Create()
            //    .WithIdentity("triggerSendMail", "JobScheduler")
            //    .StartNow()
            //    .WithSimpleSchedule(x => x
            //        .WithIntervalInSeconds(30)
            //        .RepeatForever())
            //    .Build();

            // Tell quartz to schedule the job using our trigger

            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.ScheduleJob(job, trigger);
            
            await scheduler.Start();
        }
    }
}