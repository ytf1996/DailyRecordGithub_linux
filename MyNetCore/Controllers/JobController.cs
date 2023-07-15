using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyNetCore.Business.Jobs;
using Quartz;
using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyNetCore.Controllers
{
    public class JobController : JobBaseContentController
    {
        public JobController()
        {

        }

        /// <summary>
        /// 测试定时任务
        /// </summary>
        /// <returns></returns>
        [Display(Name = "测试定时任务")]
        public IActionResult Task1()
        {
            JobContentService jcs = new JobContentService();
            jcs.JobTest1();
            return Success();
        }

        /// <summary>
        /// 系统通知
        /// </summary>
        /// <returns></returns>
        [Display(Name = "系统通知")]
        public IActionResult SendNotice()
        {
            JobContentService jcs = new JobContentService();
            jcs.SendNotice();
            return Success();
        }

    }
}