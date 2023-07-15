using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Models
{
    /// <summary>
    /// 下周计划
    /// </summary>
    [DisplayName(name: "下周计划", parentMenuName: "下周计划管理", icons: "fa fa-ticket", orderNum: 210, onlyForAdmin: false, isMenu: false)]
    public class PlanNextWeekInfo : BaseModel
    {
        /// <summary>
        /// 自然周开始日期
        /// </summary>
        [Display(Name = "自然周开始日期")]
        [CustomColumn(isRequired: true, laydateType: LaydateType.date)]
        public virtual DateTime BegDate { get; set; }

        /// <summary>
        /// 项目分类信息
        /// </summary>
        [Display(Name = "项目分类信息")]
        [ForeignKey("ProjectClassificationInfoId")]
        public virtual ProjectClassificationInfo ProjectClassificationInfo { get; set; }

        /// <summary>
        /// 项目分类信息Id
        /// </summary>
        [Display(Name = "项目分类信息Id")]
        [CustomColumn(isRequired: true)]
        public virtual int ProjectClassificationInfoId { get; set; }

        /// <summary>
        /// 工作计划安排
        /// </summary>
        [MaxLength(4000)]
        [Display(Name = "工作计划安排")]
       // [CustomColumn(isRequired: true)]
        public virtual string JobContent { get; set; }
    }


    #region 新增下周计划
    public class PlanDto
    {
        /// <summary>
        /// 自然周开始日期（每周一）
        /// </summary>
        public virtual DateTime BegDate { get; set; }

        /// <summary>
        /// 明细项列表
        /// </summary>
        public virtual List<PlanItem> ItemList { get; set; }
    }

    public class PlanItem
    {
        /// <summary>
        /// 项目分类信息Id
        /// </summary>
        public virtual string ProjectClassificationInfoId { get; set; }

        /// <summary>
        /// 工作计划安排
        /// </summary>
        public virtual string JobContent { get; set; }
    }
    #endregion



    #region 查询方法返回的Dto
    public class PlanShowDto
    {
        /// <summary>
        /// 自然周开始日期（每周一）
        /// </summary>
        public virtual DateTime BegDate { get; set; }

        /// <summary>
        /// 项目列表
        /// </summary>
        public virtual List<WeeklyProject> WeeklyProjects { get; set; }// = new List<WeeklyProject>();

        /// <summary>
        /// 项目列表
        /// </summary>
        public virtual DataTable WeeklyData { get; set; }
    }


    public class WeeklyProject
    {
        /// <summary>
        /// 项目编号
        /// </summary>
        public virtual string ProjectClassificationInfoId { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public virtual string ClassificationName { get; set; }
    }


    public class CellDto
    {
        public virtual int? Id { get; set; }

        public virtual string JobContent { get; set; }
    }
    #endregion
}
