using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyNetCore.Business;

namespace MyNetCore.Areas.Work.Controllers
{
    public class WeChatWorkAuthController : WeChatWorkController
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null || currentUser.Id == 0)
            {
                string url = GetHostUrl();

                if (Request.IsAjax())
                {
                    filterContext.Result = new ContentResult()
                    {
                        Content = string.Format("{{\"result\":\"failure\",\"msg\":\"请重新登录\",\"urlRedirect\":\"{0}\"}}", url)
                    };
                    return;
                }
                else
                {
                    filterContext.Result = new ContentResult();
                    Response.Redirect(string.Format("/Work/WeChatWork/AuthPage?historyUrl={0}", url));
                    return;
                }
            }
            else if(currentUser.Disabled)
            {
                if (Request.IsAjax())
                {
                    filterContext.Result = new ContentResult()
                    {
                        Content = string.Format("{{\"result\":\"failure\",\"msg\":\"该账户已被禁用\",\"urlRedirect\":\"{0}\"}}","")
                    };
                    return;
                }
                else
                {
                    filterContext.Result = new ContentResult()
                    {
                        Content = "该账户已被禁用"
                    };
                    return;
                }
            }
        }
    }
}