using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "工作流实体", parentMenuName: "工作流", orderNum: 800, onlyForAdmin: true, isMenu: false)]
    public class IWorkflowModel : BaseModel
    {
        /// <summary>
        /// 工作流状态
        /// </summary>
        [Display(Name = "工作流状态", Order = 1000)]
        [CustomColumn(isReadOnly: true)]
        public WorkflowStatus WorkflowStatus { get; set; }

        /// <summary>
        /// 工作流实例
        /// </summary>
        [Display(Name = "工作流实例")]
        [ForeignKey("WorkflowInstanceId")]
        public virtual WorkflowInstance WorkflowInstance { get; set; }

        /// <summary>
        /// 工作流实例Id
        /// </summary>
        [Display(Name = "工作流实例Id")]
        [CustomColumn(isHide: true)]
        public int? WorkflowInstanceId { get; set; }
        

    }

    /// <summary>
    /// 工作流状态
    /// </summary>
    public enum WorkflowStatus
    {
        [Display(Name = "新建")]
        New = 0,

        [Display(Name = "审批中")]
        Submit = 10,

        [Display(Name = "通过")]
        Approve = 20,

        [Display(Name = "驳回")]
        Refuse = 30
    }
}