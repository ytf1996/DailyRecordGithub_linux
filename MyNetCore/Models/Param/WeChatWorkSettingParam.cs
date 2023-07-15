using MyNetCore.Business.Param;
using System.ComponentModel.DataAnnotations;

namespace MyNetCore.Models
{
    [DisplayName(name: "微信企业号", parentMenuName: "参数", orderNum: 1130, onlyForAdmin: true, url: "Edit", icons: "fa fa-asterisk")]
    public class WeChatWorkSettingParam : BaseParam
    {

        private static WeChatWorkSettingParam _MyConfig;
        public static WeChatWorkSettingParam MyConfig
        {
            get
            {
                if (_MyConfig == null)
                {
                    BusinessSystemParam businessSystemParam = new BusinessSystemParam();
                    _MyConfig = businessSystemParam.Get<WeChatWorkSettingParam>(false);
                }

                return _MyConfig;
            }
            set
            {
                BusinessSystemParam businessSystemParam = new BusinessSystemParam();
                businessSystemParam.Save<WeChatWorkSettingParam>(value);
                _MyConfig = null;
            }
        }

        /// <summary>
        /// 企业号的标识CorpID
        /// </summary>
        [Display(Name = "企业号的标识CorpID")]
        public string CorpID { get; set; }

        /// <summary>
        /// 企业号应用ID(Agentid)
        /// </summary>
        [Display(Name = "企业号应用ID(Agentid)")]
        public int AgentId { get; set; }

        /// <summary>
        /// 企业号应用Secret
        /// </summary>
        [Display(Name = "企业号应用Secret")]
        public string AgentSecret { get; set; }

        /// <summary>
        /// 企业号应用Token
        /// </summary>
        [Display(Name = "企业号应用Token")]
        public string AgentToken { get; set; }

        /// <summary>
        /// 企业号应用EncodingAESKey
        /// </summary>
        [Display(Name = "企业号应用EncodingAESKey")]
        public string AgentEncodingAESKey { get; set; }

        /// <summary>
        /// 企业号通讯录Secret
        /// </summary>
        [Display(Name = "企业号通讯录Secret")]
        public string Secret { get; set; }

        /// <summary>
        /// 默认小组
        /// </summary>
        [Display(Name = "默认小组")]
        public string ChannelIds { get; set; }

        /// <summary>
        /// 默认区域
        /// </summary>
        [Display(Name = "默认区域")]
        public int DefaultTerritoryId { get; set; }

        /// <summary>
        /// 默认区域权限
        /// </summary>
        [Display(Name = "默认区域权限")]
        public int TerritoryProfilesId { get; set; }

        public override string ParamType
        {
            get
            {
                return "WeChatWorkSettingParam";
            }
        }
    }
}