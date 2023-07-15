using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "工作流按钮事件", parentMenuName: "工作流", orderNum: 990, onlyForAdmin: true, isMenu: false, icons: "fa fa-sitemap")]
    public class WorkflowAction : BaseModel
    {
        /// <summary>
        /// 工作流
        /// </summary>
        [Display(Name = "工作流")]
        [ForeignKey("WorkflowId")]
        public virtual Workflow Workflow { get; set; }

        /// <summary>
        /// 工作流Id
        /// </summary>
        [Display(Name = "工作流Id")]
        [CustomColumn(isHide: true)]
        public int WorkflowId { get; set; }

        /// <summary>
        /// 工作流按钮
        /// </summary>
        [Display(Name = "工作流按钮")]
        [ForeignKey("WorkflowButtonId")]
        public virtual WorkflowButton WorkflowButton { get; set; }

        /// <summary>
        /// 工作流按钮Id
        /// </summary>
        [Display(Name = "工作流按钮Id")]
        [CustomColumn(isHide: true)]
        public int WorkflowButtonId { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        [Display(Name = "事件类型", Order = 20)]
        public WorkflowActionType WorkflowActionType { get; set; }

        /// <summary>
        /// 修改字段名
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "修改字段名", Order = 30)]
        public string EditColumnName { get; set; }

        /// <summary>
        /// 修改字段值
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "修改字段值", Order = 40)]
        public string EditColumnValue { get; set; }

        /// <summary>
        /// 通知小组
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "通知小组", Order = 50)]
        public string NoticeChannelIds { get; set; }

        /// <summary>
        /// 通知人员
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "通知人员", Order = 60)]
        public string NoticeUserIds { get; set; }

        /// <summary>
        /// 通知文本
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "通知文本", Order = 70)]
        public string NoticeContent { get; set; }

        /// <summary>
        /// 运行SQL语句
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "运行SQL语句", Order = 80)]
        public string RunSqlText { get; set; }

        /// <summary>
        /// 运行顺序
        /// </summary>
        [Display(Name = "运行顺序", Order = 90)]
        public int OrderNum { get; set; }

        /// <summary>
        /// 是否通知上级
        /// </summary>
        [Display(Name = "是否通知上级", Order = 100)]
        public bool NoticeLineManager { get; set; }
    }

    public enum WorkflowActionType
    {
        /// <summary>
        /// 修改字段值
        /// </summary>
        [Display(Name = "修改字段值")]
        EditColumnValue = 0,
        /// <summary>
        /// 微信通知
        /// </summary>
        [Display(Name = "微信通知")]
        WeChatNotice = 10,
        /// <summary>
        /// 邮件通知
        /// </summary>
        [Display(Name = "邮件通知")]
        EmailNotice = 20,
        /// <summary>
        /// 运行Sql语句
        /// </summary>
        [Display(Name = "运行Sql语句")]
        RunSql = 30
    }

}