using MyNetCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace MyNetCore.Areas.Management.Controllers
{
    public class EmailSettingParamController : BaseParamController
    {
        public IActionResult Edit()
        {
            CheckHasAdminRight();

            return View(EmailSettingParam.MyConfig);
        }


        [HttpPost]
        public IActionResult Edit(EmailSettingParam model)
        {
            EmailSettingParam.MyConfig = model;
            return Success("编辑成功", null, "Edit");
        }


    }
}