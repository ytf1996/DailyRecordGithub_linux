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
    public class BabyInfoController : BaseManagementController<BabyInfo, BusinessBabyInfo>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<BabyInfo> list = business.GetList(param, out total);

            object result = null;
            List<BabyInfo> finalList = null;

            if (list == null)
            {
                finalList = new List<BabyInfo>();
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
                         Sex = m.Sex.GetCustomDescription(),
                         TimeOfBirth = m.TimeOfBirth.ToString("yyyy-MM-dd HH:mm:ss"),
                         m.BirthWeight
                     };

            return JsonListResult(param, total, result);

        }

    }
}