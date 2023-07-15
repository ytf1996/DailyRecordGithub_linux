using MyNetCore.Business.Param;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNetCore.Models
{
    [DisplayName(name: "系统参数", parentMenuName: "参数", orderNum: 1110, onlyForAdmin: true, url: "Edit", icons: "fa fa-asterisk")]
    public class SystemSettingParam : BaseParam
    {

        private static SystemSettingParam _MyConfig;
        public static SystemSettingParam MyConfig
        {
            get
            {
                if (_MyConfig == null)
                {
                    BusinessSystemParam businessSystemParam = new BusinessSystemParam();
                    _MyConfig = businessSystemParam.Get<SystemSettingParam>(false);
                }

                return _MyConfig;
            }
            set
            {
                BusinessSystemParam businessSystemParam = new BusinessSystemParam();
                businessSystemParam.Save<SystemSettingParam>(value);
                _MyConfig = null;
            }
        }

        /// <summary>
        /// 网站标题
        /// </summary>
        [Display(Name = "网站标题")]
        public string WebSiteTitle { get; set; }

        /// <summary>
        /// 网站域名(带Http://)
        /// </summary>
        [Display(Name = "网站域名(带Http://)")]
        public string DomainUrl { get; set; }

        /// <summary>
        /// 定时器主进程运行间隔(秒)
        /// </summary>
        [Display(Name = "定时器主进程运行间隔(秒)")]
        public int JobIntervalInSeconds { get; set; }

        public override string ParamType
        {
            get
            {
                return "SystemSettingParam";
            }
        }

        [Display(Name = "测试")]
        [ForeignKey("TestId")]
        public virtual Attachment Test { get; set; }

        [Display(Name = "测试Id")]
        public int? TestId { get; set; }

        [Display(Name = "测试2")]
        [ForeignKey("TestId2")]
        [CustomColumn(imgWidth: "200", imgHeight: "200")]
        public virtual Attachment Test2 { get; set; }

        [Display(Name = "测试Id2")]
        [CustomColumn(imgWidth: "100", imgHeight: "50")]
        public int? TestId2 { get; set; }
    }
}