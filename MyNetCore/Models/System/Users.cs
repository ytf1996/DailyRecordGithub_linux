using Newtonsoft.Json;
using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "用户", parentMenuName: "管理", orderNum: 930, onlyForAdmin: true, icons: "fa fa-sitemap")]
    public class Users
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "名称")]
        public string Name { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Display(Name = "创建人")]
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
        public int? UpdatedById { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Display(Name = "修改时间")]
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// 直属上级
        /// </summary>
        [Display(Name = "直属上级")]
        public int? LineManageId { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Display(Name = "是否删除")]
        public bool Deleted { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        [Display(Name = "是否禁用")]
        public bool Disabled { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "账号")]
        public string Code { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [MaxLength(512)]
        [Display(Name = "密码")]
        public string PassWord { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        [Display(Name = "用户类型")]
        public UserType UserType { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [Display(Name = "手机号")]
        [MaxLength(30)]
        public string PhoneNum { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [Display(Name = "最后登录时间")]
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// 公众号用户身份编码
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "公众号用户身份编码")]
        public string WeChatCorpId { get; set; }

        /// <summary>
        /// 企业号员工账号
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "企业号员工账号")]
        public string WeChatUserId { get; set; }

        /// <summary>
        /// 小程序用户身份编码
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "小程序用户身份编码")]
        public string WeChatOpenidForMiniProgram { get; set; }

        /// <summary>
        /// 开放平台用户身份编码
        /// </summary>
        [MaxLength(255)]
        [Display(Name = "开放平台用户身份编码")]
        public string WeChatUnionId { get; set; }

        /// <summary>
        /// 区域业务Id
        /// </summary>
        public int? TerritoryId { get; set; }

        /// <summary>
        /// 小组Id
        /// </summary>
        [Display(Name = "小组Id")]
        public string ChannelIds { get; set; }

        /// <summary>
        /// 权限配置文件Id
        /// </summary>
        [Display(Name = "权限配置文件Id")]
        public int? TerritoryProfilesId { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [Display(Name = "性别")]
        public Gender Gender { get; set; }

        private string _HeadImage;
        /// <summary>
        /// 头像
        /// </summary>
        [Display(Name = "头像")]
        [MaxLength(255)]
        public string HeadImage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_HeadImage))
                {
                    return "/Content/images/default.png";
                }
                return _HeadImage;
            }
            set
            {
                _HeadImage = value;
            }
        }


        /// <summary>
        /// 顾问性质
        /// </summary>
        [Display(Name = "顾问性质")]
        public CounselorProperty? CounselorPropertyVal { get; set; }

        /// <summary>
        /// 服务单位
        /// </summary>
        [Display(Name = "服务单位")]
        public string ServiceUnit { get; set; }

        /// <summary>
        /// 签约供应商
        /// </summary>
        [Display(Name = "签约供应商")]
        public string ContractedSupplier { get; set; }

        /// <summary>
        /// 组别Project team
        /// </summary>
        [Display(Name = "组别Project team")]
        public string Group { get; set; }

        /// <summary>
        /// 人员序号  （日志周报功能）
        /// </summary>
        [Display(Name = "人员序号")]
        public int UserOrder { get; set; }

        /// <summary>
        /// 职责
        /// </summary>
        [Display(Name = "职责")]
        public string Duty { get; set; }

        /// <summary>
        ///  是否离职
        /// </summary>
        public int IfLeave { get; set; }

        #region 非DB
        [NotMapped]
        public string CounselorPropertyDes
        {
            get
            {
                if (CounselorPropertyVal == null) return null;
                return CounselorPropertyVal.GetCustomDescription();
            }
        }

        /// <summary>
        /// 小组(非DB)
        /// </summary>
        [Display(Name = "小组")]
        [NotMapped]
        public virtual List<Channel> Channels
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ChannelIds))
                {
                    return null;
                }

                List<Channel> listChannels = new List<Channel>();

                string[] channelIds = ChannelIds.Split(',');

                int itemInt = 0;

                foreach (string item in channelIds)
                {
                    if (!int.TryParse(item, out itemInt))
                    {
                        continue;
                    }
                    if (Business.BusinessHelper.ListChannel.Any(m => m.Id == itemInt))
                    {
                        listChannels.Add(Business.BusinessHelper.ListChannel.FirstOrDefault(m => m.Id == itemInt));
                    }
                }

                return listChannels;
            }
        }

        /// <summary>
        /// 小组名称(非DB)
        /// </summary>
        [Display(Name = "小组名称")]
        [NotMapped]
        public virtual string ChannelsName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ChannelIds))
                {
                    return "";
                }
                return string.Join(",", Channels.Select(m => m.Name));
            }
        }

        /// <summary>
        /// 是否为管理员(非DB)
        /// </summary>
        [NotMapped]
        public virtual bool IsAdmin
        {
            get
            {
                return this.UserType == UserType.Admin;
            }
        }
        #endregion

    }


    public enum UserType
    {
        /// <summary>
        /// 系统管理员
        /// </summary>
        [Display(Name = "系统管理员")]
        Admin = 0,

        /// <summary>
        /// 普通用户
        /// </summary>
        [Display(Name = "普通用户")]
        Normal = 10,

        /// <summary>
        /// 微信服务号用户
        /// </summary>
        [Display(Name = "微信服务号用户")]
        WeChatMP = 20,

        /// <summary>
        /// 微信企业号用户
        /// </summary>
        [Display(Name = "微信企业号用户")]
        WeChatWork = 30,

        /// <summary>
        /// 微信小程序
        /// </summary>
        [Display(Name = "微信小程序用户")]
        WeChatMini = 40,
    }


    //10开发    20测试    30实施
    public enum CounselorProperty
    {
        /// <summary>
        /// 实施
        /// </summary>
        [Display(Name = "实施")]
        DevelopmentEngineer = 10,

        /// <summary>
        /// 实施
        /// </summary>
        [Display(Name = "实施")]
        TestEngineer = 20,

        /// <summary>
        /// 实施
        /// </summary>
        [Display(Name = "实施")]
        ImplementationEngineer = 30
    }






    /// <summary>
    /// 性别
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// 男
        /// </summary>
        [Display(Name = "男")]
        Male = 1,
        /// <summary>
        /// 女
        /// </summary>
        [Display(Name = "女")]
        FeMale = 0
    }

}