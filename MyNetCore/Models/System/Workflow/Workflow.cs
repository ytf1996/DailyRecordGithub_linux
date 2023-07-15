using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "工作流", parentMenuName: "管理", orderNum: 905, onlyForAdmin: true, isMenu: true, icons: "fa fa-sitemap")]
    public class Workflow : BaseModel
    {
        /// <summary>
        /// 实体名称
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "实体名称", Order = 10)]
        public string WorkflowEntityName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "备注", Order = 20)]
        public string Remark { get; set; }
    }
}