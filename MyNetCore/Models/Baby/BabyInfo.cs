using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Models
{
    [DisplayName(name: "婴儿信息", parentMenuName: "婴儿管理", icons: "fa fa-ticket", orderNum: 210, onlyForAdmin: false, isMenu: true)]
    public class BabyInfo : BaseModel
    {
        /// <summary>
        /// 性别
        /// </summary>
        [Display(Name = "性别")]
        [CustomColumn(isRequired: true)]
        public virtual Gender Sex { get; set; }

        /// <summary>
        /// 出生时间
        /// </summary>
        [Display(Name = "出生时间")]
        [CustomColumn(isRequired: true, laydateType: LaydateType.datetime)]
        public virtual DateTime TimeOfBirth { get; set; }

        /// <summary>
        /// 出生重量
        /// </summary>
        [Display(Name = "出生重量")]
        public virtual decimal? BirthWeight { get; set; }
    }
}
