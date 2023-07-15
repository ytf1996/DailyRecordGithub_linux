using MyNetCore.Business;
using Senparc.Weixin;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sns;
using System;

namespace MyNetCore.Tools
{
    /// <summary>
    /// 微信小程序
    /// </summary>
    public class WeChatMiniHelper : CommonBusiness
    {
        private string _wxOpenAppId { get; set; }

        private string _wxOpenAppSecret { get; set; }

        public WeChatMiniHelper(string wxOpenAppId, string wxOpenAppSecret)
        {
            _wxOpenAppId = wxOpenAppId;
            _wxOpenAppSecret = wxOpenAppSecret;
        }

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        public string GetAccessToken()
        {
            var result = AccessTokenContainer.TryGetAccessToken(_wxOpenAppId, _wxOpenAppSecret, false);
            return result;
        }

        /// <summary>
        /// 获取用户身份
        /// </summary>
        /// <param name="code">wx.login得到的code</param>
        /// <returns></returns>
        public string GetOpenId(string code)
        {
            var jsonResult = SnsApi.JsCode2Json(_wxOpenAppId, _wxOpenAppSecret, code);
            if (jsonResult.errcode == ReturnCode.请求成功)
            {
                return jsonResult.openid;
            }
            return null;
        }

    }
}
