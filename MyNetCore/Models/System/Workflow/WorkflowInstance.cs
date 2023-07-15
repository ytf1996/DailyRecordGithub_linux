using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "工作流实例", parentMenuName: "工作流", orderNum: 990, onlyForAdmin: true, isMenu: false, icons: "fa fa-sitemap")]
    public class WorkflowInstance : BaseModel
    {
        /// <summary>
        /// 数据Id
        /// </summary>
        [Display(Name = "数据Id")]
        public int RecordId { get; set; }

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
        public int? WorkflowId { get; set; }


        /// <summary>
        /// 工作流当前状态
        /// </summary>
        [Display(Name = "工作流当前状态")]
        [ForeignKey("WorkflowStepId")]
        public virtual WorkflowStep WorkflowStep { get; set; }

        /// <summary>
        /// 工作流当前状态Id
        /// </summary>
        [Display(Name = "工作流当前状态Id")]
        [CustomColumn(isHide: true)]
        public int? WorkflowStepId { get; set; }

        [MaxLength(255)]
        [CustomColumn(isHide: true)]
        [Display(Name = "当前操作人Id集合", Order = 10000)]
        public string DealUsersIds { get; set; }
    }
}