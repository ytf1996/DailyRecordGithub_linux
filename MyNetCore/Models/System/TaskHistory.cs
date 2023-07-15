using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "定时任务运行记录", parentMenuName: "管理", orderNum: 951, onlyForAdmin: true, isMenu: false, icons: "fa fa-sitemap")]
    public class TaskHistory : BaseModel
    {
        /// <summary>
        /// 定时任务
        /// </summary>
        [Display(Name = "定时任务")]
        [ForeignKey("TaskModelId")]
        [JsonIgnore]
        public virtual TaskModel TaskModel { get; set; }

        /// <summary>
        /// 定时任务Id
        /// </summary>
        [Display(Name = "定时任务", Order = 10)]
        [CustomColumn(isReadOnly: true)]
        public int? TaskModelId { get; set; }

        /// <summary>
        /// 是否运行成功
        /// </summary>
        [Display(Name = "定时任务", Order = 11)]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [Display(Name = "错误信息", Order = 12)]
        [MaxLength(255)]
        public string ErrorMsg { get; set; }
    }
}