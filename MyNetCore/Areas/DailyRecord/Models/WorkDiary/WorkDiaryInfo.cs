using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyNetCore.Models
{
    /// <summary>
    /// 工作日志
    /// </summary>
    [DisplayName(name: "工作日志", parentMenuName: "工作日志管理", icons: "fa fa-ticket", orderNum: 210, onlyForAdmin: false, isMenu: false)]
    public class WorkDiaryInfo : BaseModel
    {
        /// <summary>
        /// 日期
        /// </summary>
        [Display(Name = "日期")]
        [CustomColumn(isRequired: true, laydateType: LaydateType.date)]
        public virtual DateTime Dt { get; set; }

        /// <summary>
        /// 星期几
        /// </summary>
        [Display(Name = "星期几")]
        [CustomColumn(isRequired: true)]
        public virtual int WhatDay { get; set; }

        /// <summary>
        /// 是否出差
        /// </summary>
        [Display(Name = "是否出差")]
        //[CustomColumn(isRequired: true)]
        public virtual bool? WhetherOnBusinessTrip { get; set; }

        /// <summary>
        /// 差旅地点
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "差旅地点")]
        public virtual string TravelSite { get; set; }

        /// <summary>
        /// 工作分类信息
        /// </summary>
        [Display(Name = "工作分类信息")]
        [ForeignKey("JobClassificationInfoId")]
        public virtual JobClassificationInfo JobClassificationInfo { get; set; }

        /// <summary>
        /// 工作分类信息Id
        /// </summary>
        [Display(Name = "工作分类信息Id")]
        //[CustomColumn(isRequired: true)]
        public virtual int? JobClassificationInfoId { get; set; }

        /// <summary>
        /// 工作内容
        /// </summary>
        [MaxLength(4000)]
        [Display(Name = "工作内容")]
        public virtual string JobContent { get; set; }

        /// <summary>
        /// 上班时间
        /// </summary>
        [Display(Name = "上班时间")]
        //[CustomColumn(isRequired: true, laydateType: LaydateType.dateNoChoose)]
        public virtual DateTime? BegWorkTime { get; set; }

        /// <summary>
        /// 下班时间
        /// </summary>
        [Display(Name = "下班时间")]
        //[CustomColumn(isRequired: true, laydateType: LaydateType.dateNoChoose)]
        public virtual DateTime? EndWorkTime { get; set; }

        /// <summary>
        /// 正常小时工作数（小时）
        /// </summary>
        [Display(Name = "正常小时工作数（小时）")]
        //[CustomColumn(isRequired: true)]
        public virtual double? NormalWorkHour { get; set; }

        /// <summary>
        /// 加班（小时）
        /// </summary>
        [Display(Name = "加班（小时）")]
        public virtual double? ExtraWorkHour { get; set; }

        /// <summary>
        /// 小计
        /// </summary>
        [Display(Name = "小计")]
        //[CustomColumn(isRequired: true)]
        public virtual double? SubtotalWorkHour { get; set; }

        /// <summary>
        /// 是否收费
        /// </summary>
        [Display(Name = "是否收费")]
        //[CustomColumn(isRequired: true)]
        public virtual string IsCharged => "Yes";

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(4000)]
        [Display(Name = "备注")]
        public virtual string RemarkContent { get; set; }


        #region 非DB   （用于日报导出）
        public static List<JobClassificationInfo> projectList = new List<JobClassificationInfo>();

        /// <summary>
        /// 日期
        /// </summary>
        [NotMapped]
        public virtual string DtExport => Dt.ToString("yyyy/MM/dd");

        /// <summary>
        /// 星期
        /// </summary>
        [NotMapped]
        public virtual string WhatDayDes
        {
            get
            {
                switch (WhatDay)
                {
                    case 0: return "星期日";
                    case 1: return "星期一";
                    case 2: return "星期二";
                    case 3: return "星期三";
                    case 4: return "星期四";
                    case 5: return "星期五";
                    case 6: return "星期六";
                    default: return null;
                }
            }
        }

        /// <summary>
        /// 是否出差
        /// </summary>
        [NotMapped]
        public virtual string WhetherOnBusinessTripExport => (WhetherOnBusinessTrip==false || WhetherOnBusinessTrip==null) ? "No" : "Yes";

        /// <summary>
        /// 工作分类信息Id
        /// </summary>
        [NotMapped]
        public virtual string JobClassificationInfoIdExport => projectList.Where(x => x.Id == JobClassificationInfoId).FirstOrDefault()?.ClassificationName;
        /// <summary>
        /// 上班时间
        /// </summary>
        [NotMapped]
        public virtual string BegWorkTimeExport => BegWorkTime?.ToString("HH:mm");

        /// <summary>
        /// 下班时间
        /// </summary>
        [NotMapped]
        public virtual string EndWorkTimeExport => EndWorkTime?.ToString("HH:mm");
        #endregion
    }



    public class DiaryShowDto
    {
        /// <summary>
        /// 日志信息列表
        /// </summary>
        public List<WorkDiaryInfo> DiaryList { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public Users User { get; set; }

        /// <summary>
        /// 正常小时
        /// </summary>
        public virtual double? NormalWorkHourSummary { get; set; }

        /// <summary>
        /// 加班工时
        /// </summary>
        public virtual double? ExtraWorkHourSummary { get; set; }

        /// <summary>
        /// 合计工作工时
        /// </summary>
        public virtual double? SubtotalWorkHourSummary { get; set; }

        /// <summary>
        /// 收费天数
        /// </summary>
        public virtual int? ChargeDayNum { get; set; }

        /// <summary>
        /// 出差天数
        /// </summary>
        public virtual int? BusinessTripDayNum { get; set; }
    }

    #region 日历 (工作日、节假日、周末)
    public class Calendar
    {
        public string Code { get; set; }

        public string Msg { get; set; }

        public MonthData Data { get; set; }
    }

    public class MonthData
    {
        public List<EachDayInfo> List { get; set; }

        public int Page { get; set; }

        public int Size { get; set; }

        public int Total { get; set; }
    }


    public class EachDayInfo
    {
        public string Year { get; set; }

        public string Month { get; set; }

        public string Date { get; set; }

        public string Yearweek { get; set; }

        public string Yearday { get; set; }

        public string Lunar_year { get; set; }

        public string Lunar_month { get; set; }

        public string Lunar_date { get; set; }

        public string Lunar_yearday { get; set; }

        public string Week { get; set; }

        public string Weekend { get; set; }

        public string Workday { get; set; }  //1是工作日       2是放假

        public string Holiday { get; set; }

        public string Holiday_or { get; set; }

        public string Holiday_overtime { get; set; }

        public string Holiday_today { get; set; }

        public string Holiday_legal { get; set; }

        public string Holiday_recess { get; set; }
    }


    public class WorkOrHoliday
    {
        public const string WorkDay = "1";

        public const string Holiday = "2";
    }
    #endregion


    public class DiarySummaryDto
    {
        /// <summary>
        /// 人员ID
        /// </summary>
        [Display(Name = "人员ID")]
        public int Id { get; set; }

        /// <summary>
        /// 人员序号  （日志周报功能）
        /// </summary>
        [Display(Name = "人员序号")]
        public int UserOrder { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        public string UserName { get; set; }

        /// <summary>
        /// 正常小时
        /// </summary>
        public virtual double? NormalWorkHourSummary { get; set; }

        /// <summary>
        /// 加班工时
        /// </summary>
        public virtual double? ExtraWorkHourSummary { get; set; }

        /// <summary>
        /// 合计工作工时
        /// </summary>
        public virtual double? SubtotalWorkHourSummary { get; set; }

        /// <summary>
        /// 合计请假天数
        /// </summary>
        public virtual double? SubtotalAbsenceDaySummary { get; set; }
    }
}
