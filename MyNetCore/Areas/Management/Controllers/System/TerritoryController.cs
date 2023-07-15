using MyNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using Roim.Common;

namespace MyNetCore.Areas.Management.Controllers
{
    public class TerritoryController : BaseManagementController<Territory, BusinessTerritory>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            BusinessTerritory busiessTerr = new Business.BusinessTerritory();

            Expression<Func<Territory, bool>> predicate = null;

            if (param.ListCustomSearch != null)
            {
                string nameValue = param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "Name") == null ? null :
                    param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "Name").SearchValue;
                string territoryIdValue = param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "TerritoryId") == null ? null :
                    param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "TerritoryId").SearchValue;
                int territoryIdValueInt = 0;

                if (!string.IsNullOrWhiteSpace(territoryIdValue))
                {
                    int.TryParse(territoryIdValue, out territoryIdValueInt);
                }

                if (!string.IsNullOrWhiteSpace(nameValue))
                {
                    predicate = predicate.And(m => m.Name.Contains(nameValue));
                }

                if (!string.IsNullOrWhiteSpace(territoryIdValue))
                {
                    predicate = predicate.And(m => m.TerritoryId == territoryIdValueInt);
                }
            }

            IQueryable<Territory> list = busiessTerr.GetList(param, out total, predicate);

            object result = null;
            List<Territory> finalList = null;

            if (list == null)
            {
                finalList = new List<Territory>();
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
                         ParentTerrId = busiessTerr.GetByTerrId(m.ParentTerrId).Name,
                         m.TerritoryId
                     };

            return JsonListResult(param, total, result);
        }

        public override IActionResult Add()
        {
            BusinessTerritory businessTerritory = new BusinessTerritory();
            string errorMsg = string.Empty;
            if (!businessTerritory.CheckHasAddRight<Territory>(out errorMsg))
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", errorMsg));
            }
            string parentTerrId = Request.Query["parentTerrId"];
            int parentTerrIdInt = 0;
            Territory territory = new Territory();
            if (!string.IsNullOrWhiteSpace(parentTerrId) && int.TryParse(parentTerrId, out parentTerrIdInt))
            {
                territory.ParentTerrId = parentTerrIdInt;
            }
            return View(territory);
        }

        [HttpPost]
        public override IActionResult Add(Territory model)
        {
            Business.BusinessTerritory businessTerr = new Business.BusinessTerritory();
            businessTerr.Add(model);
            return Success("操作成功", null, "List");
        }

        [HttpPost]
        public override IActionResult Edit(Territory model)
        {
            if (model.Id != 1 && model.ParentTerrId == null)
            {
                business.ThrowErrorInfo("请选择上级区域");
            }
            base.Edit(model);
            return Success("操作成功", null, "List");
        }

    }
}