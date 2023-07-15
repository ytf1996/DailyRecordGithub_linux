using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    /// <summary>
    /// 自定义属性
    /// </summary>
    public class CustomColumnAttribute : Attribute
    {
        private bool _isHide;
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHide
        {
            get
            {
                return _isHide;
            }
        }

        private bool _isRequired;
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired
        {
            get
            {
                return _isRequired;
            }
        }

        private bool _isReadOnly;
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
            }
        }

        private LaydateType _laydateType;
        /// <summary>
        /// 日期选择类型
        /// </summary>
        public LaydateType LaydateType
        {
            get
            {
                return _laydateType;
            }
        }

        private string _imgWidth;
        /// <summary>
        /// 图片高度
        /// </summary>
        public string ImgWidth
        {
            get
            {
                return _imgWidth;
            }
        }

        private string _imgHeight;
        /// <summary>
        /// 图片宽度
        /// </summary>
        public string ImgHeight
        {
            get
            {
                return _imgHeight;
            }
        }

        private bool _isSearch;

        /// <summary>
        /// 是否为查询字段
        /// </summary>
        public bool IsSearch
        {
            get
            {
                return _isSearch;
            }
        }

        /// <summary>
        /// 字段自定义属性
        /// </summary>
        /// <param name="isHide">是否隐藏</param>
        /// <param name="isRequired">是否必填</param>
        /// <param name="isReadOnly">是否只读</param>
        /// <param name="laydateType">日期选择类型</param>
        /// <param name="imgHeight">图片高度</param>
        /// <param name="imgWidth">图片宽度</param>
        public CustomColumnAttribute(bool isHide = false, bool isRequired = false, bool isReadOnly = false, LaydateType laydateType = LaydateType.date
            , string imgWidth = "200", string imgHeight = "200", bool isSearch = false)
        {
            _isHide = isHide;
            _isRequired = isRequired;
            _isReadOnly = isReadOnly;
            _laydateType = laydateType;
            _imgWidth = imgWidth;
            _imgHeight = imgHeight;
            _isSearch = isSearch;
        }
    }


    public enum LaydateType
    {
        date,
        datetime,
        dateNoChoose,
        datetimeNoChoose
    }

}