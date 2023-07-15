using MyNetCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace MyNetCore.Areas.Management.Controllers
{
    public class BaseParamController : BaseManagementWithAuth
    {
        public void CheckHasAdminRight()
        {
            Users currentUser = GetCurrentUserInfo();

            if (currentUser == null || !currentUser.IsAdmin)
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", MessageText.ErrorInfo.您无此操作权限.ToString()));
            }
        }
    }
}