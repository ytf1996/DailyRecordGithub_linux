using MyNetCore.Controllers;
using MyNetCore.Models;
using Roim.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System;

namespace MyNetCore.Areas.Management.Controllers
{
    public class WorkflowController : BaseManagementController<Workflow, BusinessWorkflow>
    {
        #region 工作流
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<Workflow> list = business.GetList(param, out total);

            object result = null;
            List<Workflow> finalList = null;


            if (list == null)
            {
                finalList = new List<Workflow>();
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
                         CreatedDate = m.CreatedDate.ToString("yyyy-MM-dd"),
                         m.Remark,
                         EntityName = BusinessHelper.GetDisplayNameByClassFullName(m.WorkflowEntityName)
                     };

            return JsonListResult(param, total, result);

        }

        /// <summary>
        /// 设置工作流
        /// </summary>
        /// <returns></returns>
        public IActionResult SetWorkflow()
        {
            return View();
        }

        /// <summary>
        /// 添加工作流
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public override IActionResult Add(Workflow model)
        {
            Business.BusinessWorkflow businessTerr = new Business.BusinessWorkflow();
            businessTerr.Add(model);
            return Success("操作成功", null, "List");
        }

        /// <summary>
        /// 编辑工作流步骤
        /// </summary>
        /// <returns></returns>
        public IActionResult EditForWorkflowStep()
        {
            string idStr = Request.Query["id"];
            int id = 0;
            if (string.IsNullOrWhiteSpace(idStr))
            {
                business.ThrowErrorInfo("参数错误");
            }
            if (!int.TryParse(idStr, out id))
            {
                business.ThrowErrorInfo("参数错误");
            }

            BusinessWorkflowStep businessWorkflowStep = new BusinessWorkflowStep();

            WorkflowStep modelWorkflowStep = businessWorkflowStep.GetById(id);

            if (modelWorkflowStep == null)
            {
                modelWorkflowStep = new WorkflowStep();
            }

            return View(modelWorkflowStep);
        }
        #endregion

        #region 工作流步骤
        /// <summary>
        /// 编辑工作流步骤
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditForWorkflowStep(WorkflowStep model)
        {
            if (model == null || model.Id == 0)
            {
                business.ThrowErrorInfo("参数错误");
            }

            BusinessWorkflowStep businessWorkflowStep = new BusinessWorkflowStep();

            businessWorkflowStep.Edit(model);

            return Success("操作成功", null, "SetWorkflow?workflowId=" + model.WorkflowId);
        }

        /// <summary>
        /// 删除工作流步骤
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteForWorkflowStep(WorkflowStep model)
        {
            return Failure("只能删除按钮");
            //BusinessWorkflowStep businessWorkflowStep = new BusinessWorkflowStep();
            //List<WorkflowStep> listWorkflowStep = null;//工作流状态集合
            //WorkflowStep modelFirstWorkflowStep = null;//第一个状态

            //listWorkflowStep = new BusinessWorkflowStep().GetListByCondition(m => m.WorkflowId == model.WorkflowId && m.Deleted == false).ToList();
            //modelFirstWorkflowStep = listWorkflowStep == null || !listWorkflowStep.Any() ? null : listWorkflowStep.OrderBy(m => m.Id).FirstOrDefault();

            //if (model.Id == modelFirstWorkflowStep.Id)
            //{
            //    return Failure("不能删除第一个状态");
            //}

            //return Success("删除成功", null, "SetWorkflow?workflowId=" + model.WorkflowId);
        }
        #endregion

        #region 工作流按钮
        /// <summary>
        /// 添加工作流按钮
        /// </summary>
        /// <returns></returns>
        public IActionResult AddForWorkflowButton()
        {
            string workflowStepIdStr = Request.Query["workflowStepId"];
            int workflowStepId = 0;
            if (string.IsNullOrWhiteSpace(workflowStepIdStr))
            {
                business.ThrowErrorInfo("参数错误");
            }
            if (!int.TryParse(workflowStepIdStr, out workflowStepId))
            {
                business.ThrowErrorInfo("参数错误");
            }

            BusinessWorkflowStep businessWorkflowStep = new BusinessWorkflowStep();
            WorkflowStep modelWorkflowStep = businessWorkflowStep.GetById(workflowStepId);
            if (modelWorkflowStep == null)
            {
                business.ThrowErrorInfo("参数错误");
            }

            WorkflowButton modelWorkflowButton = new WorkflowButton();
            modelWorkflowButton.WorkflowId = modelWorkflowStep.WorkflowId;
            modelWorkflowButton.LastWorkflowStepId = modelWorkflowStep.Id;
            return View(modelWorkflowButton);
        }

        /// <summary>
        /// 添加工作流按钮
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddForWorkflowButton(WorkflowButton model, List<string> ChannelIds, List<string> UserIds)
        {
            if (model == null)
            {
                business.ThrowErrorInfo("参数错误");
            }

            if (ChannelIds != null)
            {
                model.ChannelIds = string.Join(",", ChannelIds);
            }
            if (UserIds != null)
            {
                model.UserIds = string.Join(",", UserIds);
            }

            new BusinessWorkflowButton().Add(model);

            return Success("操作成功", null, "SetWorkflow?workflowId=" + model.WorkflowId);
        }

        /// <summary>
        /// 编辑工作流按钮
        /// </summary>
        /// <returns></returns>
        public IActionResult EditForWorkflowButton()
        {
            string workflowButtonIdStr = Request.Query["id"];
            int workflowButtonId = 0;
            if (string.IsNullOrWhiteSpace(workflowButtonIdStr))
            {
                business.ThrowErrorInfo("参数错误");
            }
            if (!int.TryParse(workflowButtonIdStr, out workflowButtonId))
            {
                business.ThrowErrorInfo("参数错误");
            }

            BusinessWorkflowButton businessWorkflowButton = new BusinessWorkflowButton();
            WorkflowButton modelWorkflowButton = businessWorkflowButton.GetById(workflowButtonId);
            if (modelWorkflowButton == null)
            {
                business.ThrowErrorInfo("未找到相关数据");
            }
            return View(modelWorkflowButton);
        }

        /// <summary>
        /// 编辑工作流按钮
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ChannelIds"></param>
        /// <param name="UserIds"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditForWorkflowButton(WorkflowButton model, List<string> ChannelIds, List<string> UserIds)
        {
            if (model == null)
            {
                business.ThrowErrorInfo("参数错误");
            }

            if (ChannelIds != null)
            {
                model.ChannelIds = string.Join(",", ChannelIds);
            }
            if (UserIds != null)
            {
                model.UserIds = string.Join(",", UserIds);
            }

            new BusinessWorkflowButton().Edit(model);

            return Success("操作成功", null, "SetWorkflow?workflowId=" + model.WorkflowId);
        }

        /// <summary>
        /// 删除工作流按钮
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteForWorkflowButton(WorkflowButton model)
        {
            if (model == null)
            {
                business.ThrowErrorInfo("参数错误");
            }

            new BusinessWorkflowButton().Delete(model);

            return Success("操作成功", null, "SetWorkflow?workflowId=" + model.WorkflowId);
        }
        #endregion

        #region 工作流按钮点击事件实体
        /// <summary>
        /// 添加工作流按钮点击事件
        /// </summary>
        /// <returns></returns>
        public IActionResult AddForWorkflowAction()
        {
            int workflowButtonId = Roim.Common.ConvertHelper.ConvertToInt(Request.Query["workflowButtonId"], 0).Value;

            if (workflowButtonId == 0)
            {
                business.ThrowErrorInfo("参数不正确");
            }

            WorkflowButton modelWorkflowButton = new BusinessWorkflowButton().GetById(workflowButtonId);

            if (modelWorkflowButton == null)
            {
                business.ThrowErrorInfo("未找到相应按钮的数据");
            }

            WorkflowAction modelWorkflowAction = new WorkflowAction();

            modelWorkflowAction.WorkflowId = modelWorkflowButton.WorkflowId;
            modelWorkflowAction.Workflow = modelWorkflowButton.Workflow;
            modelWorkflowAction.WorkflowActionType = WorkflowActionType.EditColumnValue;
            modelWorkflowAction.EditColumnName = "Name";
            modelWorkflowAction.WorkflowButtonId = modelWorkflowButton.Id;

            return View(modelWorkflowAction);
        }

        /// <summary>
        /// 添加工作流按钮点击事件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddForWorkflowAction(WorkflowAction model, List<string> NoticeChannelIds, List<string> NoticeUserIds)
        {
            if (model == null)
            {
                business.ThrowErrorInfo("参数错误");
            }

            if (NoticeChannelIds != null)
            {
                model.NoticeChannelIds = string.Join(",", NoticeChannelIds);
            }
            if (NoticeUserIds != null)
            {
                model.NoticeUserIds = string.Join(",", NoticeUserIds);
            }

            new BusinessWorkflowAction().Add(model);

            return Success("操作成功", null, "EditForWorkflowButton?id=" + model.WorkflowButtonId);
        }

        /// <summary>
        /// 编辑工作流按钮点击事件
        /// </summary>
        /// <returns></returns>
        public IActionResult EditForWorkflowAction()
        {
            int workflowActionId = Roim.Common.ConvertHelper.ConvertToInt(Request.Query["id"], 0).Value;
            if (workflowActionId == 0)
            {
                business.ThrowErrorInfo("参数错误");
            }

            BusinessWorkflowAction businessWorkflowButton = new BusinessWorkflowAction();
            WorkflowAction modelWorkflowAction = businessWorkflowButton.GetById(workflowActionId);
            if (modelWorkflowAction == null)
            {
                business.ThrowErrorInfo("未找到相关数据");
            }
            return View(modelWorkflowAction);
        }

        /// <summary>
        /// 编辑工作流按钮点击事件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="NoticeChannelIds"></param>
        /// <param name="NoticeUserIds"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditForWorkflowAction(WorkflowAction model, List<string> NoticeChannelIds, List<string> NoticeUserIds)
        {
            if (model == null)
            {
                business.ThrowErrorInfo("参数错误");
            }

            if (NoticeChannelIds != null)
            {
                model.NoticeChannelIds = string.Join(",", NoticeChannelIds);
            }
            if (NoticeUserIds != null)
            {
                model.NoticeUserIds = string.Join(",", NoticeUserIds);
            }

            new BusinessWorkflowAction().Edit(model);

            return Success("操作成功", null, "EditForWorkflowButton?id=" + model.WorkflowButtonId);
        }

        /// <summary>
        /// 删除工作流按钮点击事件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteForWorkflowAction(WorkflowAction model)
        {
            BusinessWorkflowAction businessWorkflowAction = new BusinessWorkflowAction();

            businessWorkflowAction.Delete(model);

            return Success("删除成功", null, "EditForWorkflowButton?id=" + model.WorkflowButtonId);
        }

        /// <summary>
        /// 获取按钮下的事件列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public IActionResult GetListDataForWorkflowAction(DataTableParameters param)
        {
            int total = 0;

            string workflowButtonId = Request.Form["entityId"];

            int workflowButtonIdInt = 0;

            if (string.IsNullOrWhiteSpace(workflowButtonId) || !int.TryParse(workflowButtonId, out workflowButtonIdInt))
            {
                return NullResult();
            }

            BusinessWorkflowAction businessWorkflowAction = new BusinessWorkflowAction();

            IQueryable<WorkflowAction> list = businessWorkflowAction.GetList(param, out total, m => m.WorkflowButtonId == workflowButtonIdInt);

            object result = null;
            List<WorkflowAction> finalList = null;

            if (list == null)
            {
                finalList = new List<WorkflowAction>();
            }
            else
            {
                finalList = list.ToList();
            }

            result = from m in finalList
                     select new
                     {
                         m.Id,
                         WorkflowActionType = m.WorkflowActionType.GetCustomDescription(),
                         m.EditColumnName,
                         m.EditColumnValue,
                         m.OrderNum
                     };

            return JsonListResult(param, total, result);
        }
        #endregion
    }
}