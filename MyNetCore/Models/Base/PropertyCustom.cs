using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace MyNetCore.Models
{
    public class PropertyCustom
    {
        /// <summary>
        /// 属性
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// 是否生成查询条件
        /// </summary>
        public bool IsSearch { get; set; }
    }
}