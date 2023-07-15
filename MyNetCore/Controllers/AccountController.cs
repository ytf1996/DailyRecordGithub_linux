using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MyNetCore.Controllers
{
    public class AccountController : BaseController
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginAction()
        {
            string code = Request.Form["code"];
            string password = Request.Form["password"];
            string wechatCodeBind = Request.Form["wechatCodeBind"];
            bool remember = !string.IsNullOrWhiteSpace(Request.Form["remember"]) && Request.Form["remember"] == "on";

            if (string.IsNullOrWhiteSpace(code))
            {
                return Failure("请输入账号");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return Failure("请输入密码");
            }

            var businessUser = new BusinessUsers();

            Users modelUser = businessUser.GetByCondition(m => m.Code == code && m.Deleted == false, false);

            if (modelUser == null)
            {
                return Failure("无此账号");
            }

            if (modelUser.Disabled)
            {
                return Failure("该用户已被禁用");
            }

            if (modelUser.PassWord != Roim.Common.DEncrypt.DEncrypt.DES(password))
            {
                return Failure("密码不正确");
            }

            if (!string.IsNullOrWhiteSpace(wechatCodeBind))
            {
                if (BusinessHelper.ListWechatMPUserForBindCode == null)
                {
                    return Failure("验证码不正确或已失效");
                }

                var modelMPUserBind = BusinessHelper.ListWechatMPUserForBindCode.FirstOrDefault(m => m.Code == wechatCodeBind);
                if (modelMPUserBind == null)
                {
                    return Failure("验证码不正确或已失效");
                }

                BusinessHelper.ListWechatMPUserForBindCode.Remove(modelMPUserBind);

                if (modelMPUserBind.ExpirationDate < DateTime.Now)
                {
                    return Failure("验证码已失效");
                }

                if (businessUser.GetByCondition(m => m.WeChatCorpId == modelMPUserBind.WechatMPUserId, false) != null)
                {
                    return Failure("您已绑定账号，无需重复操作");
                }

                modelUser.WeChatCorpId = modelMPUserBind.WechatMPUserId;
            }

            modelUser.LastLogin = DateTime.Now;
            modelUser.PassWord = null;
            businessUser.Edit(modelUser, false);

            //用户标识
            var claims = new[] { new Claim(ClaimTypes.Name, code) };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(
                HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties()
                {
                    ExpiresUtc = DateTime.Now.AddMonths(1),
                    IsPersistent = remember
                });

            HttpContext.Session.SetObjectAsJson("CurrentUser", modelUser);

            string backUrl = Request.Query["historyUrl"];

            if (!string.IsNullOrWhiteSpace(backUrl))
            {
                return Success("登录成功", null, backUrl);
            }
            else
            {
                return Success("登录成功", null, "~/Management/Home/Index");
            }
        }

        [HttpPost]
        public IActionResult ChangePwd()
        {
            string myCode = Request.Form["myCode"];
            string oldPassword = Request.Form["oldPassword"];
            string newPassword = Request.Form["newPassword"];
            string confirmNewPassword = Request.Form["confirmNewPassword"];

            if (string.IsNullOrWhiteSpace(myCode))
            {
                return Failure("请输入账号");
            }

            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                return Failure("请输入老密码");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return Failure("请输入新密码");
            }

            if (newPassword != confirmNewPassword)
            {
                return Failure("两次密码输入不一致");
            }

            MyNetCore.Models.Users modelUser = new BusinessUsers().GetByCondition(m => m.Code == myCode, false);

            if (modelUser == null)
            {
                return Failure("无此账号");
            }

            if (modelUser.Disabled)
            {
                return Failure("该用户已被禁用");
            }

            if (modelUser.PassWord != Roim.Common.DEncrypt.DEncrypt.DES(oldPassword))
            {
                return Failure("老密码不正确");
            }

            modelUser.PassWord = newPassword;

            Business.BusinessUsers bu = new Business.BusinessUsers();
            bu.UpdatePassword(myCode, newPassword, modelUser.Id);

            return Success();

        }

        public IActionResult LoginForMPWechat()
        {
            return View();
        }

        public IActionResult LoginForMPWechatAction()
        {
            string wechatId = Request.Query["wechatId"];

            if (string.IsNullOrWhiteSpace(wechatId))
            {
                return Failure("参数不正确");
            }

            if (BusinessHelper.ListWechatMPUserForLogin == null)
            {
                return Failure("未获得登录信息,请重试");
            }

            var model = BusinessHelper.ListWechatMPUserForLogin.FirstOrDefault(m => m.WechatMPUserId == wechatId);

            if (model == null)
            {
                return Failure("未获得登录信息,请重试");
            }

            BusinessHelper.ListWechatMPUserForLogin.Remove(model);

            if (model.ExpirationDate < DateTime.Now)
            {
                return Failure("登录链接过期,请重新获取");
            }

            BusinessUsers businessUser = new BusinessUsers();
            Users modelUser = businessUser.GetByCondition(m => m.WeChatCorpId == wechatId, false);
            if (modelUser == null)
            {
                return Failure("未获得用户信息,请先进行绑定操作");
            }

            if (modelUser.Disabled)
            {
                return Failure("该用户已被禁用");
            }
            modelUser.PassWord = null;
            modelUser.LastLogin = DateTime.Now;
            businessUser.Edit(modelUser, false);

            //用户标识
            //var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            //identity.AddClaim(new Claim(ClaimTypes.Name, modelUser.Code));
            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            HttpContext.Session.SetObjectAsJson("CurrentUser", modelUser);

            string backUrl = Request.Query["historyUrl"];


            return Success("登录成功", null, "~/Management/Home/Index");

        }


        public IActionResult SignOut()
        {
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(HttpContext);
            return RedirectToAction("Login");
        }

        public IActionResult ForgetPassword(string email)
        {
            return JsonResult("仔细想想");
        }
    }
}