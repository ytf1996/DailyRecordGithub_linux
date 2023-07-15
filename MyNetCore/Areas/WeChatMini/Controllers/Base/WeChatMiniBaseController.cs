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

namespace MyNetCore.Areas.WeChatMini.Controllers
{
    [Area("WeChatMini")]
    public class WeChatMiniBaseController : CommonController
    {
        public IActionResult LogInWxMini(string code, string nickName, string avatarUrl, string gender)
        {
            WeChatMiniHelper weChatBaseHelper = new WeChatMiniHelper(WeChatMiniSettingParam.MyConfig.WxOpenAppId, WeChatMiniSettingParam.MyConfig.WxOpenAppSecret);
            string openid = weChatBaseHelper.GetOpenId(code);

            BusinessUsers businessUsers = new BusinessUsers();

            Users user = businessUsers.GetByCondition(m => m.WeChatOpenidForMiniProgram == openid, false);

            Gender gender1 = gender == Default.One ? Gender.Male : Gender.FeMale;

            if (user == null)
            {
                user = new Users();
                user.WeChatOpenidForMiniProgram = openid;
                user.Code = businessUsers.GetNewGuid();
                user.Name = nickName;
                user.HeadImage = avatarUrl;
                user.Gender = gender1;
                user.UserType = UserType.WeChatMini;
                if (string.IsNullOrWhiteSpace(user.Name))
                {
                    user.Name = Default.NA;
                }
                user.PassWord = businessUsers.GetNewGuid();
                user.LastLogin = DateTime.Now;
                businessUsers.AddForAuto(user);
            }
            else
            {
                if (user.Disabled)
                {
                    return Failure("用户已被禁用");
                }
                if ((user.Name != nickName || user.HeadImage != avatarUrl || user.Gender != gender1) && nickName != Default.Undefined)
                {

                    user.Name = nickName;
                    user.HeadImage = avatarUrl;
                    user.Gender = gender1;
                }
            }

            BusinessAccessToken businessAccessToken = new BusinessAccessToken();

            AccessToken accessToken = businessAccessToken.GetByCondition(m => m.CurrentUserId == user.Id, false);

            if (accessToken == null)
            {
                accessToken = new AccessToken();
                accessToken.CurrentUserId = user.Id;
                accessToken.Name = businessAccessToken.GetNewGuid();
                accessToken.EffectDateTime = DateTime.Now.AddHours(4);
                businessAccessToken.Add(accessToken, false);
            }
            else if (DateTime.Now.AddMinutes(10) >= accessToken.EffectDateTime)
            {
                accessToken.EffectDateTime = DateTime.Now.AddHours(4);
                businessAccessToken.Edit(accessToken, false);
                user.LastLogin = DateTime.Now;
                user.PassWord = null;
                businessUsers.Edit(user, false);
            }

            //用户标识
            var claims = new[] { new Claim(ClaimTypes.Name, code) };

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


        /// <summary>
        /// 微信后台验证地址（使用Get），企业微信后台应用的“修改配置”的Url填写如：http://sdk.weixin.senparc.com/work
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            string signature = Request.Query["signature"].ToString(); 
            string timestamp = Request.Query["timestamp"].ToString();
            string nonce = Request.Query["nonce"].ToString();
            string echostr = Request.Query["echostr"].ToString();
            string Token = WeChatMiniSettingParam.MyConfig.Token;

            if (Request.Method == "GET")
            {
                //get method - 仅在微信后台填写URL验证时触发
                if (CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    return Content(echostr); //返回随机字符串则表示验证通过
                }
                else
                {
                    return Content("failed:" + signature + "," + CheckSignature.GetSignature(timestamp, nonce, Token));
                }

            }
            else
            {
                //post method - 当有用户想公众账号发送消息时触发
                if (!CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    return Content("参数错误！");
                }

                //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
                var buffer = new MemoryStream();
                Request.Body.CopyTo(buffer);
                var messageHandler = new CustomMessageHandler(buffer, null);
                //执行微信处理过程
                CancellationToken tokenSource = new CancellationToken();
                messageHandler.ExecuteAsync(tokenSource);//执行微信处理过程（关键） 
                //输出结果
                return Content(messageHandler.ResponseDocument.ToString());
            }
        }

    }
}
