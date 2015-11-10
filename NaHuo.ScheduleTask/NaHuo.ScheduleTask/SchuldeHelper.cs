using NaHuo.ScheduleTask.Core;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NaHuo.ScheduleTask
{
    public static class SchuldeHelper
    {

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="dellist"></param>
        public static void deleteJob(Quartz.IScheduler scheduler, List<Config> dellist)
        {

            IList<JobKey> jlist = new List<Quartz.JobKey>();

            foreach (var it in dellist)
            {
                JobKey key = new JobKey(it.jobname, it.groupname);
                jlist.Add(key);

            }

            scheduler.DeleteJobs(jlist);


        }


        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="modilist"></param>

        public static void modiJob(Quartz.IScheduler scheduler, List<Config> modilist)
        {
            //   scheduler.GetTrigger(new TriggerKey(

            foreach (var item in modilist)
            {


                //var old = scheduler.GetTrigger(new TriggerKey(item.jobname + "trigger", "触发器组"));
                //if (old == null)
                //    return;

                //var newtrigger = addTriger(item).Build();
                //scheduler.RescheduleJob(old.Key, newtrigger);

                JobKey key = new JobKey(item.jobname, item.groupname);
                scheduler.DeleteJob(key);
                addjob(scheduler, item);

            }
        }






        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="list"></param>
        public static void addjob(Quartz.IScheduler scheduler, List<Core.Config> list)
        {

            foreach (var item in list)
            {

                
                Assembly assembly = Assembly.Load(item.assemblyname);
                Type type = assembly.GetType(item.@namespace);


                IJobDetail job = JobBuilder.Create(type)
                           .WithIdentity(item.jobname, item.groupname).Build(); // name "myJob", group "group1"

                var trigger = addTriger(item);

              

                scheduler.ScheduleJob(job, trigger.Build());

            }
        }


        /// <summary>
        ///添加一箱
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="item"></param>
        private static void addjob(Quartz.IScheduler scheduler, Core.Config item)
        {
            Assembly assembly = Assembly.Load(item.assemblyname);
            Type type = assembly.GetType(item.@namespace);


            IJobDetail job = JobBuilder.Create(type)
                       .WithIdentity(item.jobname, item.groupname).Build(); // name "myJob", group "group1"

            var trigger = addTriger(item);



            scheduler.ScheduleJob(job, trigger.Build());
        }

        private static TriggerBuilder addTriger(Config item)
        {
            var trigger = TriggerBuilder.Create()
                          .WithIdentity(item.jobname + "trigger", "触发器组");

            //con语法
            if (!string.IsNullOrEmpty(item.cronschedule))
            {
                trigger = trigger.WithCronSchedule(item.cronschedule);
            }
            else
            {

                if (item.starttime.HasValue)
                {
                    trigger = trigger.StartAt(item.starttime.Value);
                }
                else
                {
                    trigger = trigger.StartNow();
                }


                if (item.endtime.HasValue)
                {
                    trigger = trigger.EndAt(item.endtime.Value);
                }

                //SimpleSchedule

                Action<SimpleScheduleBuilder> action;

                if (item.isrepeat)
                {
                    action = (x) => { x.WithIntervalInSeconds(item.interval.Value); x.RepeatForever(); };
                }
                else
                {
                    action = (x) => { x.WithRepeatCount(item.repeatcoount.Value); x.WithIntervalInSeconds(item.interval.Value); };
                }


                trigger = trigger.WithSimpleSchedule(action);

            }
            return trigger;
        }



    }
}
