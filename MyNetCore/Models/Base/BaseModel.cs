using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNetCore.Models
{
    /// <summary>
    /// 数据库实体基类
    /// </summary>
    public class BaseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [CustomColumn(isHide: true)]
        [Column(name: "Id")]
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [CustomColumn(isRequired: true)]
        [MaxLength(50)]
        [Display(Name = "名称", Order = 1)]
        public virtual string Name { get; set; }

        /// <summary>
        /// 区域业务Id
        /// </summary>
        [Display(Name = "区域", Order = 2)]
        [Column(name: "TerritoryId")]
        public virtual int? TerritoryId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Display(Name = "创建人")]
        [ForeignKey("CreatedById")]
        [JsonIgnore]
        public virtual Users CreatedBy { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        [Display(Name = "创建人", Order = 3)]
        [CustomColumn(isReadOnly: true)]
        public virtual int? CreatedById { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Display(Name = "创建时间")]
        [CustomColumn(isHide: true)]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Display(Name = "修改人")]
        [ForeignKey("UpdatedById")]
        [JsonIgnore]
        public virtual Users UpdatedBy { get; set; }

        /// <summary>
        /// 修改人Id
        /// </summary>
        [Display(Name = "修改人Id")]
        [CustomColumn(isHide: true)]
        public int? UpdatedById { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Display(Name = "修改时间")]
        [CustomColumn(isHide: true)]
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Display(Name = "是否删除")]
        [CustomColumn(isHide: true)]
        public bool Deleted { get; set; }



    }
}