using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "工作流按钮", parentMenuName: "工作流", orderNum: 990, onlyForAdmin: true, isMenu: false, icons: "fa fa-sitemap")]
    public class WorkflowButton : BaseModel
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
        /// 上一个状态
        /// </summary>
        [Display(Name = "上一个状态")]
        [ForeignKey("LastWorkflowStepId")]
        public virtual WorkflowStep LastWorkflowStep { get; set; }

        /// <summary>
        /// 上一个状态Id
        /// </summary>
        [Display(Name = "上一个状态Id")]
        [CustomColumn(isHide: true)]
        public int LastWorkflowStepId { get; set; }

        /// <summary>
        /// 下一个状态
        /// </summary>
        [Display(Name = "下一个状态")]
        [ForeignKey("NextWorkflowStepId")]
        public virtual WorkflowStep NextWorkflowStep { get; set; }

        /// <summary>
        /// 下一个状态Id
        /// </summary>
        [Display(Name = "下一个状态Id")]
        [CustomColumn(isHide: true)]
        public int NextWorkflowStepId { get; set; }

        /// <summary>
        /// 申请人可见
        /// </summary>
        [Display(Name = "申请人可见", Order = 20)]
        public bool OnlyViewForCreatedBy { get; set; }

        /// <summary>
        /// 上级可见
        /// </summary>
        [Display(Name = "上级可见", Order = 30)]
        public bool OnlyViewForLineManager { get; set; }


        /// <summary>
        /// 小组可见
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "小组可见", Order = 40)]
        public string ChannelIds { get; set; }

        /// <summary>
        /// 人员可见
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "人员可见", Order = 50)]
        public string UserIds { get; set; }

        /// <summary>
        /// 按钮可见条件
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "按钮可见条件", Order = 15)]
        public string CanViewCondition { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        [Display(Name = "顺序", Order = 90)]
        public int OrderNum { get; set; }
    }
}