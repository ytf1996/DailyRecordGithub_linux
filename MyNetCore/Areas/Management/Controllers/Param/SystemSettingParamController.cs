using MyNetCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using System.Net;
using System.IO;
using MyNetCore.Business;

namespace MyNetCore.Areas.Management.Controllers
{
    public class SystemSettingParamController : BaseParamController
    {
        public IActionResult Edit()
        {
            CheckHasAdminRight();
            return View(SystemSettingParam.MyConfig);
        }


        [HttpPost]
        public IActionResult Edit(SystemSettingParam model)
        {
            SystemSettingParam.MyConfig = model;
            return Success("编辑成功", null, "Edit");
        }

        


        public IActionResult ResetIIS()
        {
            Process.Start("iisreset");
            return Success("重启成功", null, "Edit");
        }
    }
}