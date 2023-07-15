using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "位置历史记录", parentMenuName: "我的", orderNum: 120, onlyForAdmin: false, icons: "fa fa-user", needAddButton: false)]
    public class LatLngHistory : BaseModel
    {
        /// <summary>
        /// 维度
        /// </summary>
        public string Lat { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public string Lng { get; set; }
    }
}