using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "小组", parentMenuName: "管理", orderNum: 920, onlyForAdmin: true, icons: "fa fa-sitemap")]
    public class Channel : BaseModel
    {
        [Display(Name = "顺序", Order = 10)]
        public int OrderNum { get; set; }
    }
}