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
    public class AccessTokenController : BaseManagementController<AccessToken, BusinessAccessToken>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<AccessToken> list = business.GetList(param, out total);

            object result = null;
            List<AccessToken> finalList = null;


            if (list == null)
            {
                finalList = new List<AccessToken>();
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
                         CurrentUser = m.CurrentUser == null ? "" : string.Format("{0}({1})",m.CurrentUser.Name,m.CurrentUser.Code),
                         EffectDateTime = m.EffectDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                     };

            return JsonListResult(param, total, result);

        }
    }
}