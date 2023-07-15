using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "参数", parentMenuName: "参数", icons: "", orderNum: 1100, onlyForAdmin: true, isMenu: false)]
    public class SystemParam : BaseModel
    {
        /// <summary>
        /// 分类
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "分类")]
        public string ParamType { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "值")]
        public string ParamValue { get; set; }
    }
}