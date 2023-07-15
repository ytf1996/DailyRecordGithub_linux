using MyNetCore.Models;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Areas.WeChatMini.Business
{
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        private string appId = WeChatMiniSettingParam.MyConfig.WxOpenAppId;

        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
               : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalGlobalMessageContext.ExpireMinutes = 3。
            GlobalMessageContext.ExpireMinutes = 3;

            if (!string.IsNullOrEmpty(postModel.AppId))
            {
                appId = postModel.AppId;//通过第三方开放平台发送过来的请求
            }

            //在指定条件下，不使用消息去重
            base.OmitRepeatedMessageFunc = requestMessage =>
            {
                var textRequestMessage = requestMessage as RequestMessageText;
                if (textRequestMessage != null && textRequestMessage.Content == "容错")
                {
                    return false;
                }
                return true;
            };
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "这条消息来自DefaultResponseMessage。";
            return responseMessage;
        }
    }
}
