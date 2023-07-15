using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Models
{
    [DisplayName(name: "婴儿日常信息", parentMenuName: "婴儿管理", icons: "fa fa-ticket", orderNum: 211, onlyForAdmin: false, isMenu: true)]
    public class BabyInfoDaliy : IWorkflowModel
    {
        [Display(Name = "记录日期")]
        public virtual DateTime BusinessDate { get; set; }

        /// <summary>
        /// 婴儿信息
        /// </summary>
        [Display(Name = "婴儿信息")]
        [ForeignKey("BabyInfoId")]
        public virtual BabyInfo BabyInfo { get; set; }

        /// <summary>
        /// 婴儿信息Id
        /// </summary>
        [Display(Name = "婴儿信息Id")]
        public int? BabyInfoId { get; set; }
    }
}
