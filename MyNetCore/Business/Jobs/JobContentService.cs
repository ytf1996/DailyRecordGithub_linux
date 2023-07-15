using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Business.Jobs
{
    //定时任务业务逻辑类
    public class JobContentService
    {
        public JobContentService()
        {

        }
        
        public void JobTest1()
        {
            BusinessLog businessLog = new BusinessLog();

            businessLog.Info("定时任务测试");
        }

        /// <summary>
        /// 系统通知
        /// </summary>
        public void SendNotice()
        {
            BusinessNotice businessNotice = new BusinessNotice();
            businessNotice.SendNotices();
        }
    }
}
