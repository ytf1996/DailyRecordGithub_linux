using MyNetCore.Business.Param;
using System.ComponentModel.DataAnnotations;

namespace MyNetCore.Models
{
    [DisplayName(name: "邮件服务器参数", parentMenuName: "参数", orderNum: 1160, onlyForAdmin: true, url: "Edit", icons: "fa fa-asterisk")]
    public class EmailSettingParam : BaseParam
    {
        private static EmailSettingParam _MyConfig;
        public static EmailSettingParam MyConfig
        {
            get
            {
                if (_MyConfig == null)
                {
                    BusinessSystemParam businessSystemParam = new BusinessSystemParam();
                    _MyConfig = businessSystemParam.Get<EmailSettingParam>(false);
                }

                return _MyConfig;
            }
            set
            {
                BusinessSystemParam businessSystemParam = new BusinessSystemParam();
                businessSystemParam.Save<EmailSettingParam>(value);
                _MyConfig = null;
            }
        }
        
        /// <summary>
        /// 邮件服务器IP
        /// </summary>
        [Display(Name = "邮件服务器IP")]
        public string EmailHost { get; set; }

        /// <summary>
        /// 邮件服务器端口
        /// </summary>
        [Display(Name = "邮件服务器端口")]
        public string EmailPort { get; set; }

        /// <summary>
        /// 发件人邮箱地址
        /// </summary>
        [Display(Name = "发件人邮箱地址")]
        public string EmailSenderAddress { get; set; }

        /// <summary>
        /// 发件人邮箱密码
        /// </summary>
        [Display(Name = "发件人邮箱密码")]
        public string EmailPassword { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public override string ParamType { get { return "EmailSetting"; } }

    }
}