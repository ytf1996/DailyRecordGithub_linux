using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "工作流状态", parentMenuName: "工作流", orderNum: 990, onlyForAdmin: true, isMenu: false, icons: "fa fa-sitemap")]
    public class WorkflowStep : BaseModel
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
        /// 层级
        /// </summary>
        [Display(Name = "层级")]
        [CustomColumn(isHide: true)]
        public int LevelPath { get; set;}
    }
}