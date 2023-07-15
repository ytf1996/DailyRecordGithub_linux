using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "附件", parentMenuName: "管理", orderNum: 940, onlyForAdmin: true, icons: "fa fa-sitemap", needAddButton: false)]
    public class Attachment : BaseModel
    {
        /// <summary>
        /// 后缀名
        /// </summary>
        [Display(Name = "后缀名", Order = 10)]
        [MaxLength(10)]
        public string SuffixName { get; set; }

        /// <summary>
        /// 是否为图片
        /// </summary>
        [Display(Name = "是否为图片", Order = 20)]
        public bool IsPicture { get; set; }

        /// <summary>
        /// 保存路径
        /// </summary>
        [Display(Name = "保存路径", Order = 30)]
        [MaxLength(255)]
        public string Path { get; set; }

        [Display(Name = "文件类型", Order = 40)]
        [MaxLength(50)]
        public string ContentType { get; set; }

        /// <summary>
        /// 附件大小
        /// </summary>
        [Display(Name = "附件大小", Order = 50)]
        public long Size { get; set; }
    }
}