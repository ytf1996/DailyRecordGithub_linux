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
    public class TerritoryProfilesController : BaseManagementController<TerritoryProfiles, BusinessTerritoryProfiles>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<TerritoryProfiles> list = business.GetList(param, out total);

            object result = null;
            List<TerritoryProfiles> finalList = null;


            if (list == null)
            {
                finalList = new List<TerritoryProfiles>();
            }
            else
            {
                finalList = list.ToList();
            }

            Business.BusinessTerritory busiessTerr = new Business.BusinessTerritory();

            result = from m in finalList
                     select new
                     {
                         m.Id,
                         m.Name,
                         CreatedDate = m.CreatedDate.ToString("yyyy-MM-dd HH-mm-ss"),
                         TerritoryId = busiessTerr.GetByTerrId(m.TerritoryId).Name
                     };

            return JsonListResult(param, total, result);

        }
    }
}