using MyNetCore.Controllers;
using MyNetCore.Models;
using Roim.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using MyNetCore.Business.Jobs;
using Quartz;

namespace MyNetCore.Areas.Management.Controllers
{
    public class TaskModelController : BaseManagementController<TaskModel, BusinessTaskModel>
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public TaskModelController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<TaskModel> list = business.GetList(param, out total);

            object result = null;
            List<TaskModel> finalList = null;

            if (list == null)
            {
                finalList = new List<TaskModel>();
            }
            else
            {
                finalList = list.ToList();
            }

            result = from m in finalList
                     select new
                     {
                         m.Id,
                         m.Name,
                         CycleType = m.CycleType.GetCustomDescription(),
                         m.CycleTypeValue,
                         Frequency = m.Frequency.GetCustomDescription(),
                         LastRunTime = m.LastRunTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                         TimingType = m.TimingType.GetCustomDescription(),
                         Status = m.Status.GetCustomDescription(),
                         StartTime = m.StartTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                         NextRunTime = m.NextRunTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                     };

            return JsonListResult(param, total, result);

        }

        /// <summary>
        /// 开启单条任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult RunTaskById(int id)
        {
            BusinessTaskModel businessTaskModel = new BusinessTaskModel();
            businessTaskModel.RunTaskById(id);
            return Success();
        }

        /// <summary>
        /// 停止单条任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult StopTaskById(int id)
        {
            BusinessTaskModel businessTaskModel = new BusinessTaskModel();
            businessTaskModel.StopTaskById(id);
            return Success();
        }

        /// <summary>
        /// 强制启动主任务线程
        /// </summary>
        /// <returns></returns>
        public IActionResult RunTask()
        {
            JobBaseService.RunTask(_schedulerFactory, true);
            return Success();
        }

        /// <summary>
        /// 关闭主任务线程
        /// </summary>
        /// <returns></returns>
        public IActionResult ShutDownTask()
        {
            JobBaseService.ShutDown();
            return Success();
        }

        #region 任务运行历史记录
        public IActionResult GetListDataForTaskHistory(DataTableParameters param)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                return Failure("身份过期，请重新登录");
            }
            int total = 0;

            BusinessTaskHistory businessTaskHistory = new BusinessTaskHistory();

            IQueryable<TaskHistory> list = businessTaskHistory.GetList(param, out total, (m =>
            m.TaskModelId == param.EntityId)).WhereIf(!string.IsNullOrWhiteSpace(param.Search), x => x.ErrorMsg.Contains(param.Search));

            object result = null;
            List<TaskHistory> finalList = null;


            if (list == null)
            {
                finalList = new List<TaskHistory>();
            }
            else
            {
                finalList = list.ToList();
            }


            result = from m in finalList
                     select new
                     {
                         m.Id,
                         CreatedDate = m.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                         IsSuccess = m.IsSuccess ? "是" : "否",
                         m.ErrorMsg
                     };

            return JsonListResult(param, total, result);

        }
        #endregion
    }
}