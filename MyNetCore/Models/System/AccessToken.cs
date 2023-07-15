using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "API用户", parentMenuName: "管理", orderNum: 970, onlyForAdmin: true, icons: "fa fa-sitemap", needAddButton: false)]

    public class AccessToken : BaseModel
    {
        /// <summary>
        /// 登录人
        /// </summary>
        [Display(Name = "登录人")]
        [ForeignKey("CurrentUserId")]
        public virtual Users CurrentUser { get; set; }

        /// <summary>
        /// 登录人Id
        /// </summary>
        [Display(Name = "登录人", Order = 10)]
        [CustomColumn(isReadOnly: true)]
        public int? CurrentUserId { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        [CustomColumn(isRequired: true, laydateType: LaydateType.datetime)]
        [Display(Name = "有效期", Order = 11)]
        public DateTime EffectDateTime { get; set; }
    }
}