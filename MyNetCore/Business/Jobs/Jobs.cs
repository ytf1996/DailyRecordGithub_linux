using MyNetCore.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNetCore.Business.Jobs
{
    public class Jobs : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                BusinessTaskModel businessTaskModel = new BusinessTaskModel();

                DateTime nextRunTime = DateTime.Now;

                if (SystemSettingParam.MyConfig.JobIntervalInSeconds <= 0)
                {
                    nextRunTime = nextRunTime.AddSeconds(10);
                }
                else
                {
                    nextRunTime = nextRunTime.AddSeconds(SystemSettingParam.MyConfig.JobIntervalInSeconds);
                }

                var list = businessTaskModel.GetListByCondition(m => m.NextRunTime <= nextRunTime
                && m.Status != Models.StatusEnum.Runing && m.Status != Models.StatusEnum.Stoped, false).ToList();

                BusinessTaskHistory businessTaskHistory = new BusinessTaskHistory();

                foreach (var item in list)
                {
                    try
                    {
                        businessTaskModel.RunTaskById(item.Id);
                    }
                    catch
                    {

                    }
                }

            });
        }
    }
}
