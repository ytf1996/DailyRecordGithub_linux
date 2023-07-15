using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNetCore.Models
{
    /// <summary>
    /// 数据库实体基类
    /// </summary>
    [DisplayName(name: "系统日志", parentMenuName: "管理", orderNum: 990, onlyForAdmin: true, icons: "fa fa-sitemap", needAddButton: false)]
    [Table(name: "ErrorLog")]
    public class ErrorLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Application
        /// </summary>
        [MaxLength(50)]
        public string Application { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        [Display(Name = "日期")]
        public DateTime? Logged { get; set; }

        /// <summary>
        /// Level
        /// </summary>
        [Display(Name = "Level")]
        [MaxLength(255)]
        public string Level { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        [Display(Name = "Message")]
        [MaxLength(4000)]
        public string Message { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        [Display(Name = "Logger")]
        [MaxLength(4000)]
        public string Logger { get; set; }

        /// <summary>
        /// CallSite
        /// </summary>
        [Display(Name = "CallSite")]
        [MaxLength(255)]
        public string CallSite { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        [Display(Name = "Exception")]
        [MaxLength(4000)]
        public string Exception { get; set; }

    }
}