using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyNetCore.Models
{
    /// <summary>
    /// 请假管理
    /// </summary>
    [DisplayName(name: "请假管理", parentMenuName: "请假信息管理", icons: "fa fa-ticket", orderNum: 210, onlyForAdmin: false, isMenu: false)]
    public class AbsenceInfo : BaseModel
    {
        /// <summary>
        /// 请假开始时间
        /// </summary>
        [Display(Name = "请假开始时间")]
        [CustomColumn(isRequired: true, laydateType: LaydateType.datetime)]
        public virtual DateTime BegAbsenceTime { get; set; }

        /// <summary>
        /// 请假结束时间
        /// </summary>
        [Display(Name = "请假结束时间")]
        [CustomColumn(isRequired: true, laydateType: LaydateType.datetime)]
        public virtual DateTime EndAbsenceTime { get; set; }

        /// <summary>
        /// 请假时长   (1天按8小时算)
        /// </summary>
        [Display(Name = "请假时长")]
        [CustomColumn(isRequired: true)]
        public virtual double AbsenceHours { get; set; }

        /// <summary>
        /// 请假原因
        /// </summary>
        [MaxLength(4000)]
        [Display(Name = "请假原因")]
        public virtual string AbsenceReason { get; set; }

        /// <summary>
        /// 请假备注
        /// </summary>
        [MaxLength(4000)]
        [Display(Name = "请假备注")]
        public virtual string AbsenceRemark { get; set; }



        /// <summary>
        /// 是否批准
        /// </summary>
        [Display(Name = "是否批准")]
        //[CustomColumn(isRequired: true)]
        public virtual bool? WhetherApprove { get; set; }

        /// <summary>
        /// 批复备注
        /// </summary>
        [MaxLength(4000)]
        [Display(Name = "批复备注")]
        public virtual string ReplyRemark { get; set; }
    }
}
