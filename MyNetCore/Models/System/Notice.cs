using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "通知", parentMenuName: "管理", orderNum: 980, onlyForAdmin: true, icons: "fa fa-sitemap")]
    public class Notice : BaseModel
    {

        /// <summary>
        /// 提醒人
        /// </summary>
        [Display(Name = "提醒人")]
        [ForeignKey("NoticeManId")]
        public virtual Users NoticeMan { get; set; }

        /// <summary>
        /// 提醒人Id
        /// </summary>
        [Display(Name = "提醒人Id")]
        [CustomColumn(isHide: true)]
        public int NoticeManId { get; set; }

        /// <summary>
        /// 接收邮箱地址
        /// </summary>
        [Display(Name = "接收邮箱地址", Order = 50)]
        [MaxLength(50)]
        public string EmailAddress { get; set; }

        /// <summary>
        /// 企业微信账号
        /// </summary>
        [Display(Name = "企业微信账号", Order = 60)]
        [MaxLength(50)]
        public string QYWechatUserName { get; set; }

        /// <summary>
        /// 预计发送时间
        /// </summary>
        [Display(Name = "预计发送时间", Order = 70)]
        [CustomColumn(laydateType: LaydateType.datetime)]
        public DateTime PreSentTime { get; set; }

        /// <summary>
        /// 是否已发送
        /// </summary>
        [Display(Name = "是否已发送", Order = 80)]
        public bool IsSend { get; set; }

        /// <summary>
        /// 实际发送时间
        /// </summary>
        [Display(Name = "实际发送时间", Order = 90)]
        [CustomColumn(laydateType: LaydateType.datetimeNoChoose)]
        public DateTime? SendTime { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Display(Name = "内容", Order = 100)]
        [MaxLength(255)]
        public string Content { get; set; }

        /// <summary>
        /// 实体名称
        /// </summary>
        [Display(Name = "实体名称", Order = 110)]
        [MaxLength(255)]
        public string EntityFullName { get; set; }

        /// <summary>
        /// 相关记录ID
        /// </summary>
        [Display(Name = "相关记录ID", Order = 120)]
        public int RecordId { get; set; }

        /// <summary>
        /// 是否发送成功
        /// </summary>
        [Display(Name = "是否发送成功", Order = 130)]
        public bool IsSendSuccess { get; set; }

        /// <summary>
        /// 已发送次数
        /// </summary>
        [Display(Name = "已发送次数", Order = 140)]
        public int SendCount { get; set; }

        /// <summary>
        /// 发送失败原因
        /// </summary>
        [Display(Name = "发送失败原因", Order = 150)]
        [MaxLength(255)]
        public string SendFailReason { get; set; }

    }
}