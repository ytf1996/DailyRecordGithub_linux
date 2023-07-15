using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    public class MessageText
    {
        /// <summary>
        /// 错误信息提示
        /// </summary>
        public enum ErrorInfo
        {
            /// <summary>
            /// 数据提交失败
            /// </summary>
            [Display(Name = "数据提交失败")]
            数据提交失败 = 10001,

            /// <summary>
            /// 您无此操作权限
            /// </summary>
            [Display(Name = "您无此操作权限")]
            您无此操作权限 = 10002,

            /// <summary>
            /// 未指定要删除的数据
            /// </summary>
            [Display(Name = "未指定要删除的数据")]
            未找到要删除的数据 = 10003,

            /// <summary>
            /// 未指定上级
            /// </summary>
            [Display(Name = "未指定上级")]
            未指定上级 = 10004,

            /// <summary>
            /// 未填写名称
            /// </summary>
            [Display(Name = "未填写名称")]
            未填写名称 = 10005,

            /// <summary>
            /// 名称已存在
            /// </summary>
            [Display(Name = "名称已存在")]
            名称已存在 = 10006,

            /// <summary>
            /// 未填写账号
            /// </summary>
            [Display(Name = "未填写账号")]
            未填写账号 = 10007,

            /// <summary>
            /// 未填写密码
            /// </summary>
            [Display(Name = "未填写密码")]
            未填写密码 = 10008,

            /// <summary>
            /// 数据已存在
            /// </summary>
            [Display(Name = "数据已存在")]
            数据已存在 = 10009,

            /// <summary>
            /// 账号已存在
            /// </summary>
            [Display(Name = "账号已存在")]
            账号已存在 = 10010,

            /// <summary>
            /// 您无此操作权限
            /// </summary>
            [Display(Name = "您无此数据的操作权限")]
            您无此数据的操作权限 = 10011,

            /// <summary>
            /// 系统保留数据不能修改
            /// </summary>
            [Display(Name = "系统保留数据不能修改")]
            系统保留数据不能修改 = 10012,

            /// <summary>
            /// 系统保留数据不能删除
            /// </summary>
            [Display(Name = "系统保留数据不能删除")]
            系统保留数据不能删除 = 10013,

            /// <summary>
            /// 您无此操作权限
            /// </summary>
            [Display(Name = "您无此区域的修改权限")]
            您无此区域的修改权限 = 10014,

            /// <summary>
            /// 您无此操作权限
            /// </summary>
            [Display(Name = "您无此数据的删除权限")]
            您无此数据的删除权限 = 10015,

            /// <summary>
            /// 账号最大长度为30
            /// </summary>
            [Display(Name = "账号最大长度为30")]
            账号最大长度为30 = 10016,
        }
    }
}