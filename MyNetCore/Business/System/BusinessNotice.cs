using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNetCore.Models;
using MyNetCore.Tools;
using Senparc.Weixin.Work.AdvancedAPIs.Mass;

namespace MyNetCore.Business
{
    public class BusinessNotice : BaseBusiness<Notice>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        public override void Add(Notice model, bool needCheckRight = true, bool saveToDBNow = true, MySqlContext db = null)
        {
            if (model.NoticeManId == 0)
            {
                model.NoticeManId = -1;
            }
            base.Add(model, needCheckRight, saveToDBNow, db);
        }


        public void SendNotices()
        {
            List<Notice> notices = GetListByCondition(m => m.IsSendSuccess == false, false).ToList();

            WeChatWorkHelper weChatWorkHelper = new WeChatWorkHelper();
            EmailHelper emailHeper = new EmailHelper();

            foreach (var item in notices)
            {
                SendNotice(item, weChatWorkHelper, emailHeper);
            }
        }

        public void SendNotice(Notice item, WeChatWorkHelper weChatWorkHelper, EmailHelper emailHeper)
        {
            try
            {
                item.SendTime = DateTime.Now;

                //企业号通知
                if (!string.IsNullOrWhiteSpace(item.QYWechatUserName))
                {
                    if (weChatWorkHelper == null)
                    {
                        weChatWorkHelper = new WeChatWorkHelper();
                    }

                    item.IsSend = true;
                    var massResult = weChatWorkHelper.SendText(item.QYWechatUserName, item.Content);
                    if (massResult.errcode != Senparc.Weixin.ReturnCode_Work.请求成功)
                    {
                        item.IsSendSuccess = false;
                        item.SendFailReason = massResult.errmsg;
                        item.SendCount += 1;
                    }
                    else
                    {
                        item.IsSendSuccess = true;
                    }

                }
                //邮件通知
                else if (!string.IsNullOrWhiteSpace(item.EmailAddress))
                {
                    if (emailHeper == null)
                    {
                        emailHeper = new EmailHelper();
                    }
                    item.IsSend = true;
                    string errorStr = emailHeper.SendEmail(item.EmailAddress, item.Content, item.Content);
                    if (!string.IsNullOrWhiteSpace(errorStr))
                    {
                        item.IsSendSuccess = false;
                        item.SendFailReason = errorStr;
                        item.SendCount += 1;
                    }
                    else
                    {
                        item.IsSendSuccess = true;
                    }
                }

                Edit(item, false);
            }
            catch (Exception e)
            {
                item.IsSendSuccess = false;
                item.SendFailReason = e.Message;
                item.SendCount += 1;
                Edit(item, false);
            }
        }
    }
}