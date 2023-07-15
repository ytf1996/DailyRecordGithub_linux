using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "记事本", parentMenuName: "我的", orderNum: 110, onlyForAdmin: false, icons: "fa fa-user")]
    public class Note : BaseModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        [CustomColumn(isRequired: true, isSearch: true)]
        [MaxLength(50)]
        [Display(Name = "名称", Order = 1)]
        public override string Name { get; set; }

        [Display(Name = "内容", Order = 30)]
        [MaxLength(4000)]
        public string Remark { get; set; }

        [CustomColumn(isSearch: true)]
        [Display(Name = "是否公开", Order = 20)]
        public bool IsPublic { get; set; }
    }
}