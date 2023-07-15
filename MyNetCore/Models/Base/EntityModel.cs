using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    /// <summary>
    /// 实体信息
    /// </summary>
    public class EntityModel
    {
        public string FullName { get; set; }

        public string DisplayName { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string ParentMenuName { get; set; }

        public string Icons { get; set; }

        public int OrderNum { get; set; }

        public bool OnlyForAdmin { get; set; }

        public bool IsMenu { get; set; }

        public bool NeedAddButton { get; set; }
    }
}