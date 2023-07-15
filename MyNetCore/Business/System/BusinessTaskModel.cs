using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;
using Roim.Common;

namespace MyNetCore.Business
{
    public class BusinessTaskModel : BaseBusiness<TaskModel>
    {
        protected override bool IsTask
        {
            get
            {

                return true;
            }
        }

        /// <summary>
        /// 根据ID执行任务
        /// </summary>
        /// <param name="id"></param>
        public void RunTaskById(int id)
        {
            BusinessTaskModel businessTaskModel = new BusinessTaskModel();

            TaskModel taskModel = businessTaskModel.GetById(id, false);

            if (taskModel == null)
            {
                ThrowErrorInfo($"未找到ID为{id}的任务");
            }

            string url = taskModel.SourseFromUrl;

            if (string.IsNullOrWhiteSpace(url))
            {
                ThrowErrorInfo("未配置任务的调用Url地址");
            }

            if (!url.StartsWith("http://") || !url.StartsWith("https://"))
            {
                var urlDomain = SystemSettingParam.MyConfig.DomainUrl;
                url = $"{urlDomain}/{url}";
            }

            if (taskModel.Status == StatusEnum.Runing)
            {
                return;
            }

            var db = DB;

            taskModel.Status = StatusEnum.Runing;
            businessTaskModel.Edit(taskModel, false, db);

            var strategy = db.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        TaskHistory taskHistory = new TaskHistory
                        {
                            Name = taskModel.Name,
                            TaskModelId = taskModel.Id,
                            TerritoryId = taskModel.TerritoryId,
                            IsSuccess = true
                        };

                        try
                        {
                            if (!taskModel.StartTime.HasValue || taskModel.StartTime == DateTime.MinValue)
                            {
                                taskModel.StartTime = DateTime.Now;
                            }

                            string resultStr = new WebClientCustom().CreateHttpResponse(url);
                            taskModel.Status = StatusEnum.Waitting;

                            taskModel.LastRunTime = DateTime.Now;

                            if (taskModel.Frequency == FrequencyEnum.Manual)
                            {
                                taskModel.NextRunTime = DateTime.MinValue;
                                taskModel.Status = StatusEnum.Stoped;
                            }
                            else if (taskModel.Frequency == FrequencyEnum.Cycle)
                            {
                                if (taskModel.CycleType == CycleTypeEnum.Second)
                                {
                                    taskModel.NextRunTime = taskModel.LastRunTime.Value.AddSeconds(taskModel.CycleTypeValue);
                                }
                                else if (taskModel.CycleType == CycleTypeEnum.Minute)
                                {
                                    taskModel.NextRunTime = taskModel.LastRunTime.Value.AddMinutes(taskModel.CycleTypeValue);
                                }
                                else if (taskModel.CycleType == CycleTypeEnum.Hour)
                                {
                                    taskModel.NextRunTime = taskModel.LastRunTime.Value.AddHours(taskModel.CycleTypeValue);
                                }
                                else if (taskModel.CycleType == CycleTypeEnum.Day)
                                {
                                    taskModel.NextRunTime = taskModel.LastRunTime.Value.AddDays(taskModel.CycleTypeValue);
                                }
                                else if (taskModel.CycleType == CycleTypeEnum.Month)
                                {
                                    taskModel.NextRunTime = taskModel.LastRunTime.Value.AddMonths(taskModel.CycleTypeValue);
                                }
                            }
                            else if (taskModel.Frequency == FrequencyEnum.Timing)
                            {
                                if (taskModel.TimingType == TimingTypeEnum.Day)
                                {
                                    taskModel.NextRunTime = DateTime.Today.AddDays(1).AddHours(taskModel.PlanRunTime);
                                }
                                else if (taskModel.TimingType == TimingTypeEnum.Month)
                                {
                                    taskModel.NextRunTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
                                        .AddDays(taskModel.PlanRunDate).AddHours(taskModel.PlanRunTime);
                                }
                            }

                            taskModel.ReTryTimes = 0;
                            taskModel.ErrorMessage = null;

                            businessTaskModel.Edit(taskModel, false, db);
                        }
                        catch (Exception ex)
                        {
                            taskModel.Status = StatusEnum.Abnormal;
                            taskModel.ErrorMessage = ex.Message;
                            taskModel.ReTryTimes += 1;
                            businessTaskModel.Edit(taskModel, false);
                            taskHistory.IsSuccess = false;
                            taskHistory.ErrorMsg = ex.Message;
                        }

                        new BusinessTaskHistory().Add(taskHistory, false, true, db);

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
            });
        }

        public void StopTaskById(int id)
        {
            BusinessTaskModel businessTaskModel = new BusinessTaskModel();

            TaskModel taskModel = businessTaskModel.GetById(id, false);

            if (taskModel == null)
            {
                ThrowErrorInfo($"未找到ID为{id}的任务");
            }

            taskModel.Status = StatusEnum.Stoped;
            taskModel.NextRunTime = DateTime.MinValue;
            businessTaskModel.Edit(taskModel);
        }

        public override bool CustomValidForSave(TaskModel model, out string errorMsg)
        {
            //if (!model.SourseFromUrl.StartsWith("http://") && !model.SourseFromUrl.StartsWith("https://"))
            //{
            //    ThrowErrorInfo($"调用Url地址必须以http://或https://开头");
            //}
            return base.CustomValidForSave(model, out errorMsg);
        }
    }
}