using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "区域", parentMenuName: "管理", orderNum: 910, onlyForAdmin: true, icons: "fa fa-sitemap")]
    public class Territory : BaseModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        [CustomColumn(isRequired: true, isSearch: true)]
        [MaxLength(50)]
        [Display(Name = "名称", Order = 1)]
        public override string Name { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int TheOrder { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// 父级业务Id
        /// </summary>
        public int? ParentTerrId { get; set; }

        /// <summary>
        /// 本区域最大业务Id
        /// </summary>
        public int RangeEnd { get; set; }

        /// <summary>
        /// 下一个下级区域业务Id
        /// </summary>
        public int NextRangeStart { get; set; }

        /// <summary>
        /// 平级的下一个区域业务Id增长量,默认为上级的增长量/16
        /// </summary>
        public int RangeIncrement { get; set; }

        [NotMapped]
        [CustomColumn(isHide: true)]
        private string _NameForSelect;

        [NotMapped]
        [CustomColumn(isHide: true)]
        public virtual string NameForSelect
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_NameForSelect))
                {
                    return Name;
                }
                return _NameForSelect;
            }
            set
            {
                _NameForSelect = value;
            }
        }
    }
}