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
    public class WorkflowDemoController : BaseManagementController<WorkflowDemo, BusinessWorkflowDemo>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<WorkflowDemo> list = business.GetList(param, out total);

            object result = null;
            List<WorkflowDemo> finalList = null;


            if (list == null)
            {
                finalList = new List<WorkflowDemo>();
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
                         WorkflowStatus = m.WorkflowStatus.GetCustomDescription(),
                         ShenPiRen = business.GetShenPiRenNamesByInstanceId(m.WorkflowInstanceId)
                     };

            return JsonListResult(param, total, result);

        }

        public IActionResult GetListDataForWorkflowProgressDemo(DataTableParameters param)
        {
            int total = 0;

            string entityId = Request.Form["entityId"];

            int entityIntId = 0;

            if (string.IsNullOrWhiteSpace(entityId) || !int.TryParse(entityId, out entityIntId))
            {
                return NullResult();
            }

            BusinessWorkflowProgressDemo businessWorkflowProgressDemo = new BusinessWorkflowProgressDemo();

            IQueryable<WorkflowProgressDemo> list = businessWorkflowProgressDemo.GetList(param, out total, m => m.RecordId == entityIntId);

            object result = null;
            List<WorkflowProgressDemo> finalList = null;

            if (list == null)
            {
                finalList = new List<WorkflowProgressDemo>();
            }
            else
            {
                finalList = list.ToList();
            }

            result = from m in finalList
                     select new
                     {
                         m.Id,
                         CreatedBy = m.CreatedBy.Name,
                         CreatedDate = m.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                         WorkflowButtonName = m.WorkflowButton == null ? "" : m.WorkflowButton.Name,
                         m.Remark
                     };

            return JsonListResult(param, total, result);
        }

        [HttpPost]
        public IActionResult EditForWorkflowButtonEvent(WorkflowDemo model, string workflowButtonId, string SPRemark)
        {
            if (model == null)
            {
                business.ThrowErrorInfo("参数错误");
            }

            int workflowButtonIdInt = 0;

            if(!int.TryParse(workflowButtonId,out workflowButtonIdInt))
            {
                business.ThrowErrorInfo("参数错误");
            }

            BusinessWorkflow businessWorkflow = new BusinessWorkflow();
            businessWorkflow.RunWorkflow<WorkflowDemo, WorkflowProgressDemo>(model.Id, workflowButtonIdInt, SPRemark);

            return Success("操作成功", null, "Edit?id=" + model.Id);
        }
    }
}