using MyNetCore.Business.Param;
using System.ComponentModel.DataAnnotations;

namespace MyNetCore.Models
{
    [DisplayName(name: "微信小程序", parentMenuName: "参数", orderNum: 1120, onlyForAdmin: true, url: "Edit", icons: "fa fa-asterisk")]
    public class WeChatMiniSettingParam : BaseParam
    {

        private static WeChatMiniSettingParam _MyConfig;
        public static WeChatMiniSettingParam MyConfig
        {
            get
            {
                if (_MyConfig == null)
                {
                    BusinessSystemParam businessSystemParam = new BusinessSystemParam();
                    _MyConfig = businessSystemParam.Get<WeChatMiniSettingParam>(false);
                }

                return _MyConfig;
            }
            set
            {
                BusinessSystemParam businessSystemParam = new BusinessSystemParam();
                businessSystemParam.Save<WeChatMiniSettingParam>(value);
                _MyConfig = null;
            }
        }

        /// <summary>
        /// WxOpenAppId
        /// </summary>
        [Display(Name = "WxOpenAppId")]
        public string WxOpenAppId { get; set; }

        /// <summary>
        /// WxOpenAppSecret
        /// </summary>
        [Display(Name = "WxOpenAppSecret")]
        public string WxOpenAppSecret { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        [Display(Name = "Token")]
        public string Token { get; set; }

        /// <summary>
        /// EncodingAESKey
        /// </summary>
        [Display(Name = "EncodingAESKey")]
        public string EncodingAESKey { get; set; }

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
                return "WeChatMiniSettingParam";
            }
        }
    }
}