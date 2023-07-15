using MyNetCore.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Business.Jobs
{
    public static class JobBaseService
    {
        public static bool IsRun { get; set; }

        public static IScheduler _scheduler { get; set; }

        /// <summary>
        /// 开启任务主线程
        /// </summary>
        /// <param name="_schedulerFactory"></param>
        /// <param name="forceRun">是否强制运行</param>
        public static async void RunTask(ISchedulerFactory _schedulerFactory, bool forceRun = false)
        {
            if(forceRun)
            {
                if(_scheduler != null)
                {
                    await _scheduler.Shutdown(true);
                }
                IsRun = false;
            }

            if (IsRun)
            {
                return;
            }

            try
            {
                IsRun = true;
                //通过工场类获得调度器
                _scheduler = await _schedulerFactory.GetScheduler();
                //开启调度器
                await _scheduler.Start();
                //创建触发器(也叫时间策略)

                int second = SystemSettingParam.MyConfig.JobIntervalInSeconds;

                if(second <= 0)
                {
                    second = 10;
                }

                var trigger = TriggerBuilder.Create()
                                .WithSimpleSchedule(x => x.WithIntervalInSeconds(second).RepeatForever())//每10秒执行一次
                                .Build();
                //创建作业实例
                //Jobs即我们需要执行的作业
                var jobDetail = JobBuilder.Create<Jobs>()
                                .WithIdentity("Myjob", "group")//我们给这个作业取了个“Myjob”的名字，并取了个组名为“group”
                                .Build();
                //将触发器和作业任务绑定到调度器中
                await _scheduler.ScheduleJob(jobDetail, trigger);
            }
            catch (Exception ex)
            {
                IsRun = false;
                throw ex;
            }
        }

        /// <summary>
        /// 关闭任务主线程
        /// </summary>
        public static async void ShutDown()
        {
            if (_scheduler != null)
            {
                await _scheduler.Shutdown(true);
            }
            IsRun = false;
        }

    }
}
