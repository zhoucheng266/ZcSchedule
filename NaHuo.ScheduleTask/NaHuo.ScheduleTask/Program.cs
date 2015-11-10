using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using System.IO;

using Quartz.Spi;
using System.Data.SqlClient;

namespace NaHuo.ScheduleTask
{
    public class Program
    {
        private static IScheduler scheduler = null;
        private static System.Timers.Timer _timer;


        static void Main(string[] args)
        {
            var properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = "RemoteServerSchedulerClient";

            // 设置线程池
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "5";
            properties["quartz.threadPool.threadPriority"] = "Normal";

            // 远程输出配置
            properties["quartz.scheduler.exporter.type"] = "Quartz.Simpl.RemotingSchedulerExporter, Quartz";
            properties["quartz.scheduler.exporter.port"] = "556";
            properties["quartz.scheduler.exporter.bindName"] = "QuartzScheduler";
            properties["quartz.scheduler.exporter.channelType"] = "tcp";

            //工厂

            var schedulerFactory = new StdSchedulerFactory(properties);
            scheduler = schedulerFactory.GetScheduler();



            using (NaHuo.ScheduleTask.Core.nahuo_schedulerEntities entity = new Core.nahuo_schedulerEntities())
            {
                //过滤表示为删除的任务集合
                var list = entity.Config.Where(n => !n.statusid.Value.Equals((int)NaHuo.ScheduleTask.Command.E_Status.删除));





                SchuldeHelper.addjob(scheduler, list.ToList());

                if (list.Count() > 0)
                {
                    scheduler.Start();
                }

            }


            _timer = new System.Timers.Timer();
            _timer.Interval = 1000 * 20;  //大概5分钟
            _timer.Elapsed += _timer_Elapsed;

            _timer.Start();




        }




        //守护进程
        private static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("守护进程开启:" + DateTime.Now.ToString());

            //还是读库配置
            using (NaHuo.ScheduleTask.Core.nahuo_schedulerEntities entity = new Core.nahuo_schedulerEntities())
            {
                var list = entity.Config.Where(n => !n.statusid.Value.Equals((int)NaHuo.ScheduleTask.Command.E_Status.正常));

                  SchuldeHelper.deleteJob(scheduler, list.Where(n => n.statusid == (int)NaHuo.ScheduleTask.Command.E_Status.删除).ToList());
                  SchuldeHelper.addjob(scheduler, list.Where(n => n.statusid == (int)NaHuo.ScheduleTask.Command.E_Status.新增).ToList());
                  SchuldeHelper.modiJob(scheduler, list.Where(n => n.statusid == (int)NaHuo.ScheduleTask.Command.E_Status.修改).ToList());


                if (list.Count() > 0)
                {

                    string sql = "update [nahuo_scheduler].[dbo].[Config] set statusid=1 where jobname in ({0})";


                    StringBuilder sb = new StringBuilder();
                    foreach (var ot in list)
                    {
                        sb.AppendFormat("'{0}',", ot.jobname);
                    }



                    var exesql = string.Format(sql, sb.ToString().Substring(0, sb.ToString().Length - 1));
                    SqlParameter[] parameter = {
                       };

                    entity.Database.ExecuteSqlCommand(exesql, parameter);

                }


            }

        }
    }
}
