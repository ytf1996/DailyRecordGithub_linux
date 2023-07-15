using MyNetCore.Controllers;
using MyNetCore.Business;
using MyNetCore.Models;
using System;
using Senparc.Weixin.Work.Tencent;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using MyNetCore.Tools;

namespace MyNetCore.Areas.Work.Controllers
{
    [Area("Work")]
    public class WeChatWorkController : CommonController
    {
        #region 获取身份,企业号登录
        //获取身份链接
        public IActionResult AuthPage()
        {
            var url = string.Format("{0}{1}?historyUrl={2}", GetDomainUrl(),
                "/Work/WeChatWork/GetWxWorkUserCode", Request.Query["historyUrl"]);

            return Redirect(new WeChatWorkHelper().GetReturnUrl(url));
        }

        /// <summary>
        /// 获取身份,企业号登录
        /// </summary>
        /// <returns></returns>
        public IActionResult GetWxWorkUserCode()
        {
            string historyUrl = Request.Query["historyUrl"];

            var currentUser = GetCurrentUserInfo();

            if(currentUser != null)
            {
                if (!string.IsNullOrWhiteSpace(historyUrl))
                {
                    return Redirect(historyUrl);
                }
                else
                {
                    return Redirect("/Management/Home/Index");
                }
            }

            string code = Request.Query["code"];

            if (string.IsNullOrWhiteSpace(code))
            {
                return Failure("获取code失败");
            }

            WeChatWorkHelper weChatWorkHelper = new WeChatWorkHelper();
            string userIdWxWork = weChatWorkHelper.GetUserIdByCode(code);

            BusinessUsers businessUsers = new BusinessUsers();

            Users modelUser = businessUsers.GetByCondition(m => m.WeChatUserId == userIdWxWork, false);
            var modelWxWorkUser = weChatWorkHelper.GetUserInfoByUserId(userIdWxWork);

            if(modelUser == null)
            {
                modelUser = businessUsers.GetByCondition(m => m.Code == userIdWxWork, false);
                modelUser.WeChatUserId = userIdWxWork;
            }

            if (modelUser == null)
            {
                modelUser = new Users();
                modelUser.Code = "WORK_" + userIdWxWork;
                modelUser.CreatedById = 1;
                modelUser.CreatedDate = DateTime.Now;
                modelUser.Email = modelWxWorkUser.email;
                modelUser.HeadImage = modelWxWorkUser.avatar;
                modelUser.Name = modelWxWorkUser.name;
                modelUser.PhoneNum = modelWxWorkUser.mobile;
                modelUser.UserType = UserType.WeChatWork;
                modelUser.WeChatUserId = userIdWxWork;
                modelUser.PassWord = string.Format("{0}", Guid.NewGuid());
                modelUser.ChannelIds = WeChatWorkSettingParam.MyConfig.ChannelIds;
                modelUser.TerritoryId = WeChatWorkSettingParam.MyConfig.DefaultTerritoryId;
                modelUser.TerritoryProfilesId = WeChatWorkSettingParam.MyConfig.TerritoryProfilesId;
                businessUsers.AddForAuto(modelUser);
            }

            if (modelUser.Disabled)
            {
                return Failure("当前用户已被禁用");
            }

            modelUser.LastLogin = DateTime.Now;
            modelUser.PassWord = null;//不修改密码
            modelUser.HeadImage = modelWxWorkUser.avatar;
            businessUsers.Edit(modelUser, false);

            //用户标识
            var claims = new[] { new Claim(ClaimTypes.Name, modelUser.Code) };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(
                HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties()
                {
                    ExpiresUtc = DateTime.Now.AddMonths(1),
                    IsPersistent = true
                });

            HttpContext.Session.SetObjectAsJson("CurrentUser", modelUser);
            

            if (!string.IsNullOrWhiteSpace(historyUrl))
            {
                return Redirect(historyUrl);
            }
            else
            {
                return Redirect(string.Format("{0}{1}", GetHostUrl(), "/Management/Home/Index"));
            }
        }
        #endregion

        public IActionResult InitWeChat()
        {
            string signature = Request.Query["msg_signature"];
            string timestamp = Request.Query["timestamp"];
            string nonce = Request.Query["nonce"];
            string echostr = Request.Query["echostr"];
            string token = WeChatWorkSettingParam.MyConfig.AgentToken;
            string encodingAESKey = WeChatWorkSettingParam.MyConfig.AgentEncodingAESKey;
            string corpId = WeChatWorkSettingParam.MyConfig.CorpID;

            string decryptEchoString = string.Empty;

            if (Request.Method == "GET")
            {
                if (CheckSignature(token, signature, timestamp, nonce, corpId, encodingAESKey, echostr,
                ref decryptEchoString))
                {
                    if (!string.IsNullOrEmpty(decryptEchoString))
                    {
                        return Content(decryptEchoString);
                    }
                }
            }
            else
            {
                string encryptResponse = "";

                return Content(encryptResponse);
            }

            return Content("200");

        }

        /// <summary>
        /// 验证企业号签名
        /// </summary>
        /// <param name="token">企业号配置的Token</param>
        /// <param name="signature">签名内容</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">nonce参数</param>
        /// <param name="corpId">企业号ID标识</param>
        /// <param name="encodingAESKey">加密键</param>
        /// <param name="echostr">内容字符串</param>
        /// <param name="retEchostr">返回的字符串</param>
        /// <returns></returns>
        public bool CheckSignature(string token, string signature, string timestamp, string nonce, string corpId,
            string encodingAESKey, string echostr, ref string retEchostr)
        {
            WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(token, encodingAESKey, corpId);
            int result = wxcpt.VerifyURL(signature, timestamp, nonce, echostr, ref retEchostr);
            if (result != 0)
            {
                return false;
            }

            return true;
        }

    }
}