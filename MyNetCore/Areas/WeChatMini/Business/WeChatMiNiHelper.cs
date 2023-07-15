using MyNetCore.Models;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Entities.TemplateMessage;
using Senparc.Weixin.WxOpen.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Areas.WeChatMini.Business
{
    public class WeChatMiNiHelper
    {
        private string WxOpenAppId = WeChatMiniSettingParam.MyConfig.WxOpenAppId;

        /// <summary>
        /// 发送小程序消息
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <param name="openid">用户身份</param>
        /// <param name="contents">内容</param>
        public string SendMsg(string templateId, string openid, Dictionary<string, string> contents)
        {
            TemplateMessageData obj = new TemplateMessageData();

            if (contents?.Any() == true)
            {
                foreach (var item in contents)
                {
                    obj.Add(item.Key, new TemplateMessageDataValue(item.Value));
                }
            }

            WxJsonResult result = MessageApi.SendSubscribe(WxOpenAppId, openid
                , templateId, obj);

            if (result.errcode != ReturnCode.请求成功)
            {
                return result.errmsg;
            }

            return null;
        }
    }
}
