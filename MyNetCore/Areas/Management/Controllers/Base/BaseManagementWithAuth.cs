using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyNetCore.Controllers;
using System;
using System.Linq;
using System.Text;
using MyNetCore.Business;

namespace MyNetCore.Areas.Management.Controllers
{
    [Area("Management")]
    public class BaseManagementWithAuth : CommonController
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null || currentUser.Id == 0 || currentUser.Disabled)
            {
                string url = Request.GetAbsoluteUri();

                if (Request.IsAjax())
                {
                    filterContext.Result = new ContentResult()
                    {
                        Content = string.Format("{{\"result\":\"failure\",\"msg\":\"身份过期\",\"urlRedirect\":\"{0}\"}}", url)
                    };
                    return;
                }
                else
                {
                    Response.Redirect("/Account/Login", false);
                    return;
                }
                //Response.Redirect("/Account/Login", false);
            }
        }
    }
}