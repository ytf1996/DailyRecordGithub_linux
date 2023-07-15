using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    public abstract class BaseParam : BaseModel
    {
        /// <summary>
        /// 参数类型
        /// </summary>
        [CustomColumn(isHide: true)]
        public abstract string ParamType { get; }

        [CustomColumn(isReadOnly: true, isHide: true)]
        public override int? CreatedById { get => base.CreatedById; set => base.CreatedById = value; }

        [CustomColumn(isHide: true)]
        public override string Name { get => base.Name; set => base.Name = value; }

        [CustomColumn(isHide: true)]
        public override int? TerritoryId { get => base.TerritoryId; set => base.TerritoryId = value; }
    }
}