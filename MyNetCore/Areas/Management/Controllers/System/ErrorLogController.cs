using MyNetCore.Controllers;
using MyNetCore.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyNetCore.Areas.Management.Controllers
{
    [Area(areaName: "Management")]
    public class ErrorLogController : CommonController
    {
        BusinessLog business = null;

        public ErrorLogController()
        {
            business = new BusinessLog();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null || currentUser.Id == 0)
            {
                string url = Request.GetAbsoluteUri();
                Response.Redirect(string.Format("/Account/Login?historyUrl={0}", url));
            }
        }

        public IActionResult List()
        {
            var currentUser = GetCurrentUserInfo();

            if(currentUser == null || !currentUser.IsAdmin)
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", 
                    MessageText.ErrorInfo.您无此操作权限.ToString()));
            }

            return View();
        }

        public IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<ErrorLog> list = business.GetList(param, out total);

            object result = null;
            List<ErrorLog> finalList = null;

            if(list == null)
            {
                finalList = new List<ErrorLog>();
            }
            else
            {
                finalList = list.ToList();
            }
            
            result = from m in finalList
                     select new
                     {
                         m.Id,
                         Logged = m.Logged.HasValue ? m.Logged.Value.ToString("yyyy/MM/dd HH:mm:ss") : "",
                         m.Message,
                         m.Exception,
                         m.Logger,
                         m.CallSite,
                         m.Application,
                         m.Level
                     };

            return JsonListResult(param, total, result);

        }


    }
}