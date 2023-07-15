using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Areas.WeChatMini.Business;
using MyNetCore.Business;
using MyNetCore.Controllers;
using MyNetCore.Models;
using Senparc.Weixin.MP;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using MyNetCore.Tools;

namespace MyNetCore.Areas.DailyRecord.Controllers
{
    [Area("DailyRecord")]
    public class DailyRecordBaseController : CommonController
    {
        public class LoginDto
        {
            public string JobNumber { get; set; }

            public string Password { get; set; }
        }

        [HttpPost]
        public IActionResult LogInDailyRecord(LoginDto input)
        {
            BusinessUsers businessUsers = new BusinessUsers();

            Users user = businessUsers.GetByCondition(m => m.Code == input.JobNumber, false);

            if (user == null)
            {
                return Failure("用户不存在");
            }
            else
            {
                if (user.Disabled)
                {
                    return Failure("用户已被禁用");
                }

                if (user.PassWord != Roim.Common.DEncrypt.DEncrypt.DES(input.Password))
                {
                    return Failure("密码不正确");
                }
            }

            BusinessAccessToken businessAccessToken = new BusinessAccessToken();

            AccessToken accessToken = businessAccessToken.GetByCondition(m => m.CurrentUserId == user.Id, false);

            if (accessToken == null)
            {
                accessToken = new AccessToken();
                accessToken.CurrentUserId = user.Id;
                accessToken.Name = businessAccessToken.GetNewGuid();
                accessToken.EffectDateTime = DateTime.Now.AddHours(24);
                businessAccessToken.Add(accessToken, false);
            }
            else //if (DateTime.Now.AddMinutes(10) >= accessToken.EffectDateTime)
            {
                accessToken.EffectDateTime = DateTime.Now.AddHours(24);
                businessAccessToken.Edit(accessToken, false);
                user.LastLogin = DateTime.Now;
                user.PassWord = null;
                businessUsers.Edit(user, false);
            }

            //用户标识
            var claims = new[] { new Claim(ClaimTypes.Name, input.JobNumber) };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(
                HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties()
                {
                    ExpiresUtc = DateTime.Now.AddHours(24),
                    IsPersistent = true
                });

            HttpContext.Session.SetObjectAsJson("CurrentUser", user);

            return Success(data: new { accessToken = accessToken.Name });
        }
    }
}
