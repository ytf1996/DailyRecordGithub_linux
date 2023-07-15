using System;
using System.Net.Mail;

namespace Roim.Common
{
    public class EmailBaseHelper
    {
        /// <summary>
        /// 邮件服务器IP
        /// </summary>
        private string EmailHost { get; set; }

        /// <summary>
        /// 邮件服务器端口
        /// </summary>
        private string EmailPort { get; set; }

        /// <summary>
        /// 发件人邮箱地址
        /// </summary>
        private string EmailSenderAddress { get; set; }

        /// <summary>
        /// 发件人邮箱密码
        /// </summary>
        private string EmailPassword { get; set; }


        public EmailBaseHelper(string emailHost, string emailPort, string emailSenderAddress, string emailPassword)
        {
            this.EmailHost = emailHost;
            this.EmailPort = emailPort;
            this.EmailSenderAddress = emailSenderAddress;
            this.EmailPassword = emailPassword;
        }

        #region 发送电子邮件(无附件)
        /// <summary> 
        /// 发送电子邮件(无附件)
        /// </summary> 
        /// <param name="messageFrom">发件人邮箱地址</param> 
        /// <param name="messageTo">收件人邮箱地址</param> 
        /// <param name="messageSubject">邮件主题</param> 
        /// <param name="messageBody">邮件内容</param> 
        /// <param name="emailHost">指定发送邮件的服务器地址或IP </param>
        /// <param name="emailPassword">指定登录服务器的密码</param> 
        /// <returns>返回的是错误信息</returns> 
        public string SendEmail(String messageTo, String messageToCC, String messageSubject, String messageBody)
        {
            string errorMsg = string.Empty;
            try
            {
                MailAddress emailAddress = new MailAddress(EmailSenderAddress); //发件人邮箱地址
                MailMessage message = new MailMessage();
                messageTo = messageTo.Replace(";", ",");
                message.From = emailAddress;
                message.To.Add(messageTo); //收件人邮箱地址可以是多个以实现群发
                if (!String.IsNullOrEmpty(messageToCC))
                {
                    message.CC.Add(messageToCC);
                }
                message.Subject = messageSubject;
                message.Body = messageBody;
                message.IsBodyHtml = true; //是否为html格式 
                message.Priority = MailPriority.High; //发送邮件的优先等级 

                SmtpClient sc = new SmtpClient();
                sc.Host = EmailHost; //指定发送邮件的服务器地址或IP 
                sc.Port = int.Parse(EmailPort); //指定发送邮件端口 
                sc.Credentials = new System.Net.NetworkCredential(EmailSenderAddress, EmailPassword); //指定登录服务器的账号和密码 

                sc.Send(message); //发送邮件 
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return errorMsg;
            }
            return errorMsg;
        }
        #endregion

        #region 发送电子邮件(带附件)
        /// <summary> 
        /// 发送电子邮件(带附件)
        /// </summary> 
        /// <param name="messageFrom">发件人邮箱地址</param> 
        /// <param name="messageTo">收件人邮箱地址</param> 
        /// <param name="messageSubject">邮件主题</param> 
        /// <param name="messageBody">邮件内容</param> 
        /// <param name="emailHost">指定发送邮件的服务器地址或IP </param>
        /// <param name="emailPassword">指定登录服务器的密码</param> 
        /// <param name="attachmentStr">附件路径地址</param>
        /// <returns>返回的是错误信息</returns> 
        public string SendEmail(String messageTo, String messageToCC, String messageSubject, String messageBody, String attachmentStr)
        {
            string errorMsg = string.Empty;
            MailAddress emailAddress = new MailAddress(EmailSenderAddress); //发件人邮箱地址
            MailMessage message = new MailMessage();
            messageTo = messageTo.Replace(";", ",");


            Attachment att = null;

            if (!string.IsNullOrEmpty(attachmentStr))
            {
                try
                {
                    att = new Attachment(attachmentStr);//发送附件的内容
                    message.Attachments.Add(att);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }
            }

            message.From = emailAddress;
            message.To.Add(messageTo); //收件人邮箱地址可以是多个以实现群发 
            if (!String.IsNullOrEmpty(messageToCC))
            {
                message.CC.Add(messageToCC);
            }
            message.Subject = messageSubject;
            message.Body = messageBody;
            //message.Attachments.Add(objMailAttachment);
            message.IsBodyHtml = true; //是否为html格式 
            message.Priority = MailPriority.High; //发送邮件的优先等级 

            SmtpClient sc = new SmtpClient();
            sc.Host = EmailHost; //指定发送邮件的服务器地址或IP 
            sc.Port = int.Parse(EmailPort); //指定发送邮件端口 
            sc.Credentials = new System.Net.NetworkCredential(EmailSenderAddress, EmailPassword); //指定登录服务器的账号和密码 

            try
            {
                sc.Send(message); //发送邮件 
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return errorMsg;
            }
            finally
            {
                if (att != null)
                {
                    att.Dispose();
                }
            }
            return errorMsg;

        }
        #endregion
    }
}
