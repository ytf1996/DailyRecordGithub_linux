using Microsoft.EntityFrameworkCore;

namespace MyNetCore.Models
{
    public class MySqlContext : DbContext
    {
        public MySqlContext(DbContextOptions<MySqlContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Channel>()
            .HasOne(e => e.CreatedBy)
            .WithMany();

            modelBuilder.Entity<Channel>()
            .HasOne(e => e.UpdatedBy)
            .WithMany();
        }

        #region 系统工具类
        /// <summary>
        /// TSql查询数量用
        /// </summary>
        public virtual DbSet<ToolModel> ToolModel { get; set; }
        #endregion

        #region 管理
        /// <summary>
        /// 用户
        /// </summary>
        public virtual DbSet<Users> Users { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        public virtual DbSet<Territory> Territory { get; set; }

        /// <summary>
        /// 权限配置文件
        /// </summary>
        public virtual DbSet<TerritoryProfiles> TerritoryProfiles { get; set; }

        /// <summary>
        /// 小组
        /// </summary>
        public virtual DbSet<Channel> Channel { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        public virtual DbSet<Purview> Purview { get; set; }

        /// <summary>
        /// 日志
        /// </summary>
        public virtual DbSet<ErrorLog> Log { get; set; }

        /// <summary>
        /// 通知
        /// </summary>
        public virtual DbSet<Notice> Notice { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public virtual DbSet<SystemParam> SystemParam { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public virtual DbSet<Attachment> Attachment { get; set; }

        /// <summary>
        /// 工作流
        /// </summary>
        public virtual DbSet<Workflow> Workflow { get; set; }

        /// <summary>
        /// 工作流实例
        /// </summary>
        public virtual DbSet<WorkflowInstance> WorkflowInstance { get; set; }

        /// <summary>
        /// 工作流步骤
        /// </summary>
        public virtual DbSet<WorkflowStep> WorkflowStep { get; set; }

        /// <summary>
        /// 工作流按钮
        /// </summary>
        public virtual DbSet<WorkflowButton> WorkflowButton { get; set; }

        /// <summary>
        /// 工作流按钮事件
        /// </summary>
        public virtual DbSet<WorkflowAction> WorkflowAction { get; set; }

        /// <summary>
        /// 工作流实例
        /// </summary>
        public virtual DbSet<WorkflowDemo> WorkflowDemo { get; set; }

        /// <summary>
        /// 工作流审批记录
        /// </summary>
        public DbSet<WorkflowProgressDemo> WorkflowProgressDemo { get; set; }

        /// <summary>
        /// API用户
        /// </summary>
        public virtual DbSet<AccessToken> AccessToken { get; set; }

        /// <summary>
        /// 定时任务
        /// </summary>
        public virtual DbSet<TaskModel> TaskModel { get; set; }

        /// <summary>
        /// 定时任务运行历史记录
        /// </summary>
        public virtual DbSet<TaskHistory> TaskHistory { get; set; }
        #endregion

        #region 小工具
        /// <summary>
        /// 记事本
        /// </summary>
        public virtual DbSet<Note> Note { get; set; }

        /// <summary>
        /// 经纬度记录
        /// </summary>
        public virtual DbSet<LatLngHistory> LatLngHistory { get; set; }
        #endregion

        #region 聊天室
        public virtual DbSet<ChatRoom> ChatRoom { get; set; }
        #endregion

        #region 婴儿管理
        /// <summary>
        /// 婴儿信息
        /// </summary>
        public virtual DbSet<BabyInfo> BabyInfo { get; set; }

        /// <summary>
        /// 婴儿日常信息
        /// </summary>
        public DbSet<BabyInfoDaliy> BabyInfoDaliy { get; set; }

        /// <summary>
        /// 婴儿管理工作流
        /// </summary>
        public DbSet<BabyInfoDaliyProgress> WorkflowProgressBabyInfo { get; set; }
        #endregion

        #region 日志周报
        /// <summary>
        /// 工作日志
        /// </summary>
        public virtual DbSet<WorkDiaryInfo> WorkDiaryInfo { get; set; }

        /// <summary>
        /// 工作分类
        /// </summary>
        public DbSet<JobClassificationInfo> JobClassificationInfo { get; set; }

        /// <summary>
        /// 下周计划
        /// </summary>
        public DbSet<PlanNextWeekInfo> PlanNextWeekInfo { get; set; }

        /// <summary>
        /// 项目类别
        /// </summary>
        public DbSet<ProjectClassificationInfo> ProjectClassificationInfo { get; set; }

        /// <summary>
        /// 请假管理
        /// </summary>
        public DbSet<AbsenceInfo> AbsenceInfo { get; set; }
        #endregion
    }
}