using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Models
{
    [DisplayName(name: "定时任务", parentMenuName: "管理", orderNum: 950, onlyForAdmin: true, icons: "fa fa-tasks")]
    public class TaskModel : BaseModel
    {
        /// <summary>
        /// 抽取频率
        /// </summary>
        [Display(Name = "抽取频率", Order = 51)]
        public virtual FrequencyEnum Frequency { get; set; }

        /// <summary>
        /// 周期单位
        /// </summary>
        [Display(Name = "周期单位", Order = 52)]
        public virtual CycleTypeEnum CycleType { get; set; }

        /// <summary>
        /// 频率
        /// </summary>
        [Display(Name = "频率", Order = 53)]
        public virtual int CycleTypeValue { get; set; }

        /// <summary>
        /// 定时单位
        /// </summary>
        [Display(Name = "定时单位", Order = 54)]
        public virtual TimingTypeEnum TimingType { get; set; }

        /// <summary>
        /// 计划日期(几号)
        /// </summary>
        [Display(Name = "计划日期(几号)", Order = 55)]
        public virtual int PlanRunDate { get; set; }

        /// <summary>
        /// 计划时间(24小时制)
        /// </summary>
        [Display(Name = "计划时间(24小时制)", Order = 56)]
        public virtual int PlanRunTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Display(Name = "状态", Order = 57)]
        [CustomColumn(isReadOnly: true)]
        public virtual StatusEnum Status { get; set; }

        /// <summary>
        /// 开始运行时间
        /// </summary>
        [Display(Name = "开始运行时间", Order = 58)]
        [CustomColumn(isReadOnly: true, laydateType: LaydateType.datetimeNoChoose)]
        public virtual DateTime? StartTime { get; set; }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        [Display(Name = "最后运行时间", Order = 59)]
        [CustomColumn(isReadOnly: true, laydateType: LaydateType.datetimeNoChoose)]
        public virtual DateTime? LastRunTime { get; set; }

        /// <summary>
        /// 下次运行时间
        /// </summary>
        [Display(Name = "下次运行时间", Order = 60)]
        [CustomColumn(isReadOnly: true, laydateType: LaydateType.datetimeNoChoose)]
        public virtual DateTime? NextRunTime { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Display(Name = "描述", Order = 61)]
        [MaxLength(255)]
        public virtual string Description { get; set; }

        /// <summary>
        /// 调用Url地址
        /// </summary>
        [Display(Name = "调用Url地址", Order = 70)]
        [MaxLength(255)]
        [CustomColumn(isRequired: true)]
        public virtual string SourseFromUrl { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        [Display(Name = "异常信息", Order = 71)]
        [CustomColumn(isReadOnly: true)]
        [MaxLength(255)]
        public virtual string ErrorMessage { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        [Display(Name = "重试次数", Order = 72)]
        [CustomColumn(isReadOnly: true)]
        public virtual int ReTryTimes { get; set; }
    }

    /// <summary>
    /// 抽取频率
    /// </summary>
    public enum FrequencyEnum
    {
        /// <summary>
        /// 手工
        /// </summary>
        [Display(Name = "手工")]
        Manual = 10,
        /// <summary>
        /// 定时
        /// </summary>
        [Display(Name = "定时")]
        Timing = 20,
        /// <summary>
        /// 周期
        /// </summary>
        [Display(Name = "周期")]
        Cycle = 30
    }

    /// <summary>
    /// 周期单位
    /// </summary>
    public enum CycleTypeEnum
    {
        /// <summary>
        /// 秒
        /// </summary>
        [Display(Name = "秒")]
        Second = 10,
        /// <summary>
        /// 分钟
        /// </summary>
        [Display(Name = "分钟")]
        Minute = 20,
        /// <summary>
        /// 小时
        /// </summary>
        [Display(Name = "小时")]
        Hour = 30,
        /// <summary>
        /// 天
        /// </summary>
        [Display(Name = "天")]
        Day = 40,
        /// <summary>
        /// 月
        /// </summary>
        [Display(Name = "月")]
        Month = 50
    }

    /// <summary>
    /// 定时单位
    /// </summary>
    public enum TimingTypeEnum
    {
        /// <summary>
        /// 每天
        /// </summary>
        [Display(Name = "每天")]
        Day = 10,
        /// <summary>
        /// 每月
        /// </summary>
        [Display(Name = "每月")]
        Month = 20
    }


    /// <summary>
    /// 状态
    /// </summary>
    public enum StatusEnum
    {
        /// <summary>
        /// 已停止
        /// </summary>
        [Display(Name = "已停止")]
        Stoped = 10,
        /// <summary>
        /// 运行中
        /// </summary>
        [Display(Name = "运行中")]
        Runing = 20,
        /// <summary>
        /// 等待中
        /// </summary>
        [Display(Name = "等待下次运行")]
        Waitting = 30,
        /// <summary>
        /// 异常
        /// </summary>
        [Display(Name = "异常")]
        Abnormal = 40
    }
}
