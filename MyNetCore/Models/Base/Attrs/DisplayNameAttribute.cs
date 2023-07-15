using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    /// <summary>
    /// 菜单设置
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        private readonly string _name;

        private readonly string _parentMenuName;

        private readonly string _url;

        private readonly string _icons;

        private readonly int _orderNum;

        private readonly bool _onlyForAdmin;

        private readonly bool _isMenu;

        private readonly bool _needAddButton;

        /// <summary>
        /// 菜单设置
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="parentMenuName">父级名称</param>
        /// <param name="icons">图标</param>
        /// <param name="orderNum">顺序</param>
        /// <param name="onlyForAdmin">仅管理员可见</param>
        /// <param name="url">列表路径(默认List)</param>
        /// <param name="isMenu">是否为菜单</param>
        /// <param name="needAddButton">是否需要添加按钮</param>
        public DisplayNameAttribute(string name, string parentMenuName, string icons = "", int orderNum = 1,
            bool onlyForAdmin = false, string url = "List", bool isMenu = true, bool needAddButton = true)
        {
            _name = name;
            _parentMenuName = parentMenuName;
            _icons = icons;
            _orderNum = orderNum;
            _onlyForAdmin = onlyForAdmin;
            _url = url;
            _isMenu = isMenu;
            _needAddButton = needAddButton;
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string Url
        {
            get
            {
                return this._url;
            }
        }

        public string ParentMenuName
        {
            get
            {
                return this._parentMenuName;
            }
        }

        public string Icons
        {
            get
            {
                return this._icons;
            }
        }

        public int OrderNum
        {
            get
            {
                return this._orderNum;
            }
        }

        public bool OnlyForAdmin
        {
            get
            {
                return this._onlyForAdmin;
            }
        }

        public bool IsMenu
        {
            get
            {
                return this._isMenu;
            }
        }

        public bool NeedAddButton
        {
            get
            {
                return _needAddButton;
            }
        }
    }
}