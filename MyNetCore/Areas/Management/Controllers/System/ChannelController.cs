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
    public class ChannelController : BaseManagementController<Channel, BusinessChannel>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<Channel> list = business.GetList(param, out total);

            object result = null;
            List<Channel> finalList = null;


            if (list == null)
            {
                finalList = new List<Channel>();
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
                         m.OrderNum
                     };

            return JsonListResult(param, total, result);

        }
    }
}