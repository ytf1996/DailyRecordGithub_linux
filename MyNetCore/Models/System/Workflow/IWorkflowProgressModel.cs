using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "工作流审批记录", parentMenuName: "工作流", orderNum: 800, onlyForAdmin: true, isMenu: false)]
    public class IWorkflowProgressModel : BaseModel
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        [Display(Name = "记录ID", Order = 10)]
        public int RecordId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "备注", Order = 20)]
        public string Remark { get; set; }

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
        public int? WorkflowButtonId { get; set; }

    }
}