using System;
using MyNetCore.Models;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Work.AdvancedAPIs.OAuth2;
using Senparc.Weixin.Work.AdvancedAPIs;
using Senparc.Weixin.Work.Containers;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using Senparc.Weixin.Work.AdvancedAPIs.MailList.Member;
using Senparc.Weixin.Work.AdvancedAPIs.Mass;
using Senparc.Weixin.Work.Entities.Menu;
using Senparc.Weixin.Work.CommonAPIs;
using MyNetCore.Business;

namespace MyNetCore.Tools
{
    public class WeChatWorkHelper : CommonBusiness
    {
        private string _CorpID { get; set; }

        private int _AgentId { get; set; }

        private string _AppSecret { get; set; }

        private string _Secret { get; set; }

        /// <summary>
        /// 企业号应用Token
        /// </summary>
        private string _AgentToken { get; set; }

        /// <summary>
        /// 企业号应用EncodingAESKey
        /// </summary>
        private string _AgentEncodingAESKey { get; set; }


        public WeChatWorkHelper()
        {
            _CorpID = WeChatWorkSettingParam.MyConfig.CorpID;
            _AgentId = WeChatWorkSettingParam.MyConfig.AgentId;
            _AppSecret = WeChatWorkSettingParam.MyConfig.AgentSecret;
            _Secret = WeChatWorkSettingParam.MyConfig.Secret;
            _AgentToken = WeChatWorkSettingParam.MyConfig.AgentToken;
            _AgentEncodingAESKey = WeChatWorkSettingParam.MyConfig.AgentEncodingAESKey;
        }

        public WeChatWorkHelper(string corpID, int agentId, string appSecret, string secret, string agentToken, string agentEncodingAESKey)
        {
            _CorpID = corpID;
            _AgentId = agentId;
            _Secret = secret;
            _AppSecret = secret;
            _AgentToken = agentToken;
            _AgentEncodingAESKey = agentEncodingAESKey;
        }

        /// <summary>
        /// 获取跳转地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetReturnUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }
            return OAuth2Api.GetCode(_CorpID, url, "", _AgentId.ToString());
        }

        public string GetUserIdByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_AppSecret))
            {
                ThrowErrorInfo("未配置企业号的AppSecret");
            }

            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _AppSecret);

            GetUserInfoResult result = OAuth2Api.GetUserId(accessToken, code);

            if (result.errcode == 0)
            {
                return result.UserId;
            }

            return null;

        }

        /// <summary>
        /// 根据企业微信userid获取用户详情
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public GetMemberResult GetUserInfoByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_AppSecret))
            {
                ThrowErrorInfo("未配置企业号的AppSecret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _AppSecret);
            return MailListApi.GetMember(accessToken, userId);
        }

        /// <summary>
        /// 获取企业部门集合
        /// </summary>
        public virtual GetDepartmentListResult GetDepartmentList()
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_AppSecret))
            {
                ThrowErrorInfo("未配置企业号的AppSecret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _AppSecret);
            GetDepartmentListResult result = MailListApi.GetDepartmentList(accessToken);
            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken">token</param>
        /// <param name="dpname">部门名称</param>
        /// <param name="parentId">父节点ID</param>
        /// <param name="order">排序</param>
        /// <param name="id">当前创建ID不传为自动生成</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        public virtual CreateDepartmentResult CreateDepartment(string dpname, int parentId)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_Secret))
            {
                ThrowErrorInfo("未配置企业号的Secret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _Secret);
            CreateDepartmentResult result = MailListApi.CreateDepartment(accessToken, dpname, parentId);
            return result;
        }
        /// <summary>
        /// 更新部门信息
        /// </summary>
        /// <param name="id">部门id</param>
        /// <param name="name">部门名称</param>
        /// <param name="parentId">父节点id</param>
        /// <returns></returns>
        public virtual WorkJsonResult UpdateDepartment(long id, string name, int parentId)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_Secret))
            {
                ThrowErrorInfo("未配置企业号的Secret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _Secret);
            var result = MailListApi.UpdateDepartment(accessToken, id, name, parentId);
            return result;
        }


        /// <summary>
        /// 根据部门id删除部门 部门下必须是空的
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual WorkJsonResult DeleteDepartment(long id)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_Secret))
            {
                ThrowErrorInfo("未配置企业号的Secret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _Secret);
            var result = MailListApi.DeleteDepartment(accessToken, id);
            return result;
        }


        /// <summary>
        ///获取部门成员只获取userid 和name
        /// </summary>
        /// <param name="departmentId">部门ID</param>
        /// <param name="fetchChild">1/0：是否递归获取子部门下面的成员</param>
        /// <param name="status">0获取全部员工，1获取已关注成员列表，2获取禁用成员列表，4获取未关注成员列表。status可叠加</param>
        /// <returns></returns>
        public virtual GetDepartmentMemberResult GetDepartmentMember(int departmentId, int fetchChild, int status)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_Secret))
            {
                ThrowErrorInfo("未配置企业号的AppSecret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _AppSecret);
            return MailListApi.GetDepartmentMember(accessToken, departmentId, fetchChild);
        }

        /// <summary>
        ///获取部门成员详细信息
        /// </summary>
        /// <param name="departmentId">部门ID</param>
        /// <param name="fetchChild">1/0：是否递归获取子部门下面的成员</param>
        /// <param name="status">0获取全部员工，1获取已关注成员列表，2获取禁用成员列表，4获取未关注成员列表。status可叠加</param>
        /// <returns></returns>
        public virtual GetDepartmentMemberInfoResult GetDepartmentMemberInfo(int departmentId, int fetchChild, int status)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_AppSecret))
            {
                ThrowErrorInfo("未配置企业号的AppSecret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _AppSecret);
            return MailListApi.GetDepartmentMemberInfo(accessToken, departmentId, fetchChild);
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        /// <param name="userId">员工工号</param>
        /// <param name="name">员工姓名</param>
        /// <param name="department">部门(企业号的部门)</param>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        public WorkJsonResult CreateMember(string userId, string name, long[] department = null, string mobile = null)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_Secret))
            {
                ThrowErrorInfo("未配置企业号的Secret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _Secret);
            return MailListApi.CreateMember(accessToken, new MemberCreateRequest()
            {
                userid = userId,
                name = name,
                department = department,
                mobile = mobile
            });
        }

        /// <summary>
        /// 更新员工
        /// </summary>
        /// <param name="userId">工号</param>
        /// <param name="name">名称</param>
        /// <param name="department">部门</param>
        /// <param name="mobile">手机</param>
        /// <returns></returns>
        public WorkJsonResult UpdateMember(string userId, string name = null, long[] department = null, string position = null,
            string mobile = null, string email = null, int enable = 1, string avatarMediaid = null, Extattr extattr = null)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_Secret))
            {
                ThrowErrorInfo("未配置企业号的Secret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _Secret);
            return MailListApi.UpdateMember(accessToken, new MemberUpdateRequest()
            {
                userid = userId,
                name = name,
                department = department,
                mobile = mobile,
                email = email,
                avatar_mediaid = avatarMediaid,
                extattr = extattr,
                position = position,
                enable = enable
            });
        }

        /// <summary>
        /// 删除成员
        /// </summary>
        /// <param name="userId">员工empcode</param>
        /// <returns></returns>
        public virtual WorkJsonResult DeleteMember(string userId)
        {
            if (string.IsNullOrWhiteSpace(_CorpID))
            {
                ThrowErrorInfo("未配置企业号的CorpID");
            }

            if (string.IsNullOrWhiteSpace(_Secret))
            {
                ThrowErrorInfo("未配置企业号的Secret");
            }
            string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _Secret);
            return MailListApi.DeleteMember(accessToken, userId);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="qyWechatUserName">员工工号,多发使用|分割  a|b|c</param>
        /// <param name="text">发送内容支持html</param>
        /// <returns></returns>
        public MassResult SendText(string qyWechatUserName, string sendText)
        {

            MassResult result = new MassResult();

            try
            {
                string accessToken = AccessTokenContainer.TryGetToken(_CorpID, _AppSecret);
                result = MassApi.SendText(accessToken, _AgentId.ToString(), sendText, qyWechatUserName, null, null);
            }
            catch (Exception ex)
            {
                result.errmsg = ex.Message;
                result.invaliduser = qyWechatUserName;
                result.errcode = Senparc.Weixin.ReturnCode_Work.系统繁忙;
                return result;
            }
            return result;


        }

        /// <summary>
        /// 获取前台JS的Ticket
        /// </summary>
        /// <returns></returns>
        //public JsApiTicketResult GetTicket()
        //{
        //    string accessToken = AccessTokenContainer.TryGetToken(CorpID, Secret);
        //    var url = string.Format("https://qyapi.weixin.qq.com/cgi-bin/get_jsapi_ticket?access_token={0}",
        //                            accessToken.AsUrlData());

        //    JsApiTicketResult result = Get.GetJson<JsApiTicketResult>(url);
        //    return result;
        //}

        /// <summary>
        /// 创建菜单
        /// </summary>
        public void InitQYMenu()
        {
            string domainUrl = GetDomainUrl();

            var accessToken = AccessTokenContainer.GetToken(_CorpID, _AppSecret);

            ButtonGroup bg = new ButtonGroup();

            //单击
            //bg.button.Add(new SingleClickButton()
            //{
            //    name = "单击测试",
            //    key = "OneClick",
            //    type = MenuButtonType.click.ToString(),//默认已经设为此类型，这里只作为演示
            //});

            //二级菜单
            var myMenuButton = new SubButton()
            {
                name = "我的"
            };
            myMenuButton.sub_button.Add(new SingleViewButton()
            {
                url = string.Format("{0}/Work/WeChatWorkTurnToPage/Index?pageName={1}", domainUrl, "zhuye"),
                name = "主页"
            });

            var jishibenMenuButton = new SubButton()
            {
                name = "记事本"
            };

            jishibenMenuButton.sub_button.Add(new SingleViewButton()
            {
                url = string.Format("{0}/Work/WeChatWorkTurnToPage/Index?pageName={1}", domainUrl, "jishiben"),
                name = "记事本"
            });

            jishibenMenuButton.sub_button.Add(new SingleViewButton()
            {
                url = string.Format("{0}/Work/WeChatWorkTurnToPage/Index?pageName={1}", domainUrl, "yinger"),
                name = "婴儿记录"
            });

            var communicationMenuButton = new SubButton()
            {
                name = "交流"
            };
            communicationMenuButton.sub_button.Add(new SingleViewButton()
            {
                url = string.Format("{0}/Work/WeChatWorkTurnToPage/Index?pageName={1}", domainUrl, "liaotianshi"),
                name = "聊天室"
            });

            var toolMenuButton = new SubButton()
            {
                name = "小工具"
            };
            toolMenuButton.sub_button.Add(new SingleViewButton()
            {
                url = string.Format("{0}/Work/WeChatWorkTurnToPage/Index?pageName={1}", domainUrl, "hebingpdf"),
                name = "合并PDF"
            });

            bg.button.Add(myMenuButton);
            bg.button.Add(jishibenMenuButton);
            bg.button.Add(toolMenuButton);

            var result = CommonApi.CreateMenu(accessToken, _AgentId, bg);
        }

    }


}