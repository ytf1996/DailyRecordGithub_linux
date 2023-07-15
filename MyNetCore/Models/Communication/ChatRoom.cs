using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "聊天室", parentMenuName: "交流", orderNum: 210, onlyForAdmin: false, icons: "fa fa-user")]
    public class ChatRoom : BaseModel
    {
        [Display(Name = "内容", Order = 30)]
        [MaxLength(1000)]
        public string Remark { get; set; }

        [Display(Name = "是否公开", Order = 20)]
        public bool IsPublic { get; set; }
    }
}