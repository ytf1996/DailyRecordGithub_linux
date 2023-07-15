using MyNetCore.Models;
using Roim.Common;
using System;

namespace MyNetCore.Tools
{
    public class EmailHelper
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="messageTo">收件人</param>
        /// <param name="messageSubject">邮件标题</param>
        /// <param name="messageBody">邮件正文</param>
        /// <param name="attachmentStr">附件地址</param>
        public string SendEmail(String messageTo, String messageSubject, String messageBody, String attachmentStr = null)
        {
            EmailBaseHelper emailBaseHelper = new EmailBaseHelper(EmailSettingParam.MyConfig.EmailHost, EmailSettingParam.MyConfig.EmailPort,
                EmailSettingParam.MyConfig.EmailSenderAddress, EmailSettingParam.MyConfig.EmailPassword);

            if (string.IsNullOrWhiteSpace(attachmentStr))
            {
                return emailBaseHelper.SendEmail(messageTo, null, messageSubject, messageBody);
            }
            else
            {
                return emailBaseHelper.SendEmail(messageTo, null, messageSubject, messageBody, attachmentStr);
            }
        }
    }
}
