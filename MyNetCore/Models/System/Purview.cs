using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    public enum PurviewType
    {
        [Display(Name = "用户所在区域")]
        MyTerritory = 0,

        [Display(Name = "创建者")]
        Owner = 10,

        [Display(Name = "其它区域")]
        OtherTerritory = 20
    }

    public class Purview
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 权限类型
        /// </summary>
        [Display(Name = "权限类型")]
        public PurviewType PurviewType { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Display(Name = "创建人")]
        [ForeignKey("CreatedById")]
        public virtual Users CreatedBy { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        [Display(Name = "创建人Id")]
        public int CreatedById { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Display(Name = "创建时间")]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// 修改人
        /// </summary>
        [Display(Name = "修改人")]
        [ForeignKey("UpdatedById")]
        public virtual Users UpdatedBy { get; set; }

        /// <summary>
        /// 修改人Id
        /// </summary>
        [Display(Name = "修改人Id")]
        public int UpdatedById { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Display(Name = "修改时间")]
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// 权限配置文件
        /// </summary>
        [Display(Name = "权限配置文件")]
        [ForeignKey("TerritoryProfilesId")]
        public virtual TerritoryProfiles TerritoryProfiles { get; set; }

        /// <summary>
        /// 权限配置文件Id
        /// </summary>
        [Display(Name = "权限配置文件Id")]
        public int? TerritoryProfilesId { get; set; }

        /// <summary>
        /// 实体全称
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "名称")]
        public string FullName { get; set; }

        /// <summary>
        /// 可查询
        /// </summary>
        [Display(Name = "可查询")]
        public bool CanSelect { get; set; }

        /// <summary>
        /// 可新增
        /// </summary>
        [Display(Name = "可新增")]
        public bool CanInsert { get; set; }

        /// <summary>
        /// 可修改
        /// </summary>
        [Display(Name = "可修改")]
        public bool CanUpdate { get; set; }

        /// <summary>
        /// 可删除
        /// </summary>
        [Display(Name = "可删除")]
        public bool CanDelete { get; set; }

        /// <summary>
        /// 其它区域
        /// </summary>
        [Display(Name = "其它区域")]
        public int? OtherTerritoryId { get; set; }

    }
}