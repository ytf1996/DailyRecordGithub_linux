using MyNetCore.Controllers;
using MyNetCore.Models;
using Roim.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace MyNetCore.Areas.Management.Controllers
{
    public class NoticeController : BaseManagementController<Notice, BusinessNotice>
    {
        public NoticeController()
        {

        }

        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<Notice> list = business.GetList(param, out total);

            object result = null;
            List<Notice> finalList = null;

            if (list == null)
            {
                finalList = new List<Notice>();
            }
            else
            {
                finalList = list.ToList();
            }

            result = from m in finalList
                     select new
                     {
                         m.Id,
                         NoticeMan = m.NoticeMan == null ? m.EmailAddress : string.Format("{0}({1})", m.NoticeMan.Name, m.NoticeMan.Id),
                         PreSentTime = m.PreSentTime.ToString("yyyy-MM-dd HH:mm:ss"),
                         IsSend = m.IsSend ? "是" : "否",
                         IsSendSuccess = m.IsSendSuccess ? "是" : "否",
                         m.SendCount,
                         SendTime = m.SendTime.HasValue ? m.SendTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""
                     };

            return JsonListResult(param, total, result);

        }


    }
}