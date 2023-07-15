using MyNetCore.Models;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Tools;

namespace MyNetCore.Areas.Management.Controllers
{
    public class WeChatWorkSettingParamController : BaseParamController
    {
        public IActionResult Edit()
        {
            CheckHasAdminRight();
            return View(WeChatWorkSettingParam.MyConfig);
        }


        [HttpPost]
        public IActionResult Edit(WeChatWorkSettingParam model)
        {
            WeChatWorkSettingParam.MyConfig = model;
            return Success("编辑成功", null, "Edit");
        }

        /// <summary>
        /// 初始化企业号菜单
        /// </summary>
        /// <returns></returns>
        public IActionResult ChuShiHuaCaiDan()
        {
            WeChatWorkHelper businessWeChatQY = new WeChatWorkHelper();
            businessWeChatQY.InitQYMenu();
            return Success("初始化企业应用菜单完成");
        }
    }
}