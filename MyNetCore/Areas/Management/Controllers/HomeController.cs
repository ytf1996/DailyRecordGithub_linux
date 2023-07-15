using Microsoft.AspNetCore.Mvc;
using MyNetCore.Models;
using MyNetCore.Business;

namespace MyNetCore.Areas.Management.Controllers
{
    public class HomeController : BaseManagementController<Territory, BusinessTerritory>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            return NullResult();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpdatePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddForUpdatePassword(string PassWord, string RePassWord)
        {
            if (string.IsNullOrWhiteSpace(PassWord))
            {
                return Failure("新密码不能为空");
            }

            if (string.IsNullOrWhiteSpace(RePassWord))
            {
                return Failure("请填写确认输入密码");
            }

            if (PassWord != RePassWord)
            {
                return Failure("两次密码输入不一致");
            }
            var currentUser = GetCurrentUserInfo();
            new BusinessUsers().UpdatePassword(currentUser.Code, PassWord, currentUser.Id);

            return Success("操作成功");
        }

    }
}