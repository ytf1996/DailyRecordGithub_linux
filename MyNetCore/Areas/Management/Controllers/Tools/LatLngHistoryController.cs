using MyNetCore.Controllers;
using MyNetCore.Models;
using Roim.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;

namespace MyNetCore.Areas.Management.Controllers
{
    public class LatLngHistoryController : BaseManagementController<LatLngHistory, BusinessLatLngHistory>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            Expression<Func<LatLngHistory, bool>> predicate = null;

            IQueryable<LatLngHistory> list = business.GetList(param, out total,
                predicate, null, null);

            object result = null;
            List<LatLngHistory> finalList = null;

            if (list == null)
            {
                finalList = new List<LatLngHistory>();
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
                         CreatedBy = m.CreatedBy == null ? "" : m.CreatedBy.Name,
                         m.Lng,
                         m.Lat,
                         CreatedDate = m.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                     };

            return JsonListResult(param, total, result);

        }
    }
}