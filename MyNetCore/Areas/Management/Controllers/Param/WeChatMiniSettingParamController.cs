using MyNetCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using System.Net;
using System.IO;
using MyNetCore.Business;

namespace MyNetCore.Areas.Management.Controllers
{
    public class WeChatMiniSettingParamController : BaseParamController
    {
        public IActionResult Edit()
        {
            CheckHasAdminRight();
            return View(WeChatMiniSettingParam.MyConfig);
        }


        [HttpPost]
        public IActionResult Edit(WeChatMiniSettingParam model)
        {
            WeChatMiniSettingParam.MyConfig = model;
            return Success("编辑成功", null, "Edit");
        }

        


        public IActionResult ResetIIS()
        {
            Process.Start("iisreset");
            return Success("重启成功", null, "Edit");
        }
    }
}