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
    public class BabyInfoDaliyController : BaseManagementController<BabyInfoDaliy, BusinessBabyInfoDaliy>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<BabyInfoDaliy> list = business.GetList(param, out total);

            object result = null;
            List<BabyInfoDaliy> finalList = null;


            if (list == null)
            {
                finalList = new List<BabyInfoDaliy>();
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

        public override IActionResult Add()
        {
            BaseBusiness<BabyInfoDaliy> business = new BaseBusiness<BabyInfoDaliy>();
            string errorMsg = string.Empty;
            if (!business.CheckHasAddRight<BabyInfoDaliy>(out errorMsg))
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", errorMsg));
            }
            BabyInfoDaliy model = new BabyInfoDaliy();
            model.Name = $"{DateTime.Today.ToString("yyyy-MM-dd")}记录";
            return View(model);
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

            BusinessBabyInfoDaliyProgress businessWorkflowProgressDemo = new BusinessBabyInfoDaliyProgress();

            IQueryable<BabyInfoDaliyProgress> list = businessWorkflowProgressDemo.GetList(param, out total, m => m.RecordId == entityIntId);

            object result = null;
            List<BabyInfoDaliyProgress> finalList = null;

            if (list == null)
            {
                finalList = new List<BabyInfoDaliyProgress>();
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
        public IActionResult EditForWorkflowButtonEvent(BabyInfoDaliy model, string workflowButtonId, string SPRemark)
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
            businessWorkflow.RunWorkflow<BabyInfoDaliy, BabyInfoDaliyProgress>(model.Id, workflowButtonIdInt, SPRemark);

            return Success("操作成功", null, "Edit?id=" + model.Id);
        }


        public IActionResult GetReport1(int id)
        {
            BusinessBabyInfoDaliyProgress businessWorkflowProgressDemo = new BusinessBabyInfoDaliyProgress();

            List<BabyInfoDaliyProgress> list = businessWorkflowProgressDemo.GetListByCondition(m => m.RecordId == id).ToList();

            List<string> xAxisDatas = list.Select(x => x.Name).Distinct().ToList();

            List<int> seriesDatas = new List<int>();

            foreach (var item in xAxisDatas)
            {
                seriesDatas.Add(list.Count(x => x.Name == item));
            }

            return Success(data:new { xAxisDatas, seriesDatas });
        }

    }
}