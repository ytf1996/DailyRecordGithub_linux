using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    /// <summary>
    /// 项目分类
    /// </summary>
    [DisplayName(name: "项目分类", parentMenuName: "项目分类管理", icons: "fa fa-ticket", orderNum: 210, onlyForAdmin: false, isMenu: false)]
    public class ProjectClassificationInfo : BaseModel
    {
        /// <summary>
        /// 分类名称
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "分类名称")]
        [CustomColumn(isRequired: true)]
        public virtual string ClassificationName { get; set; }

        /// <summary>
        /// 项目排序
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "项目排序")]
        public virtual string Order { get; set; }
    }

}