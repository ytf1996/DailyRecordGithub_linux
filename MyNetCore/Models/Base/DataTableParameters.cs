using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace MyNetCore.Models
{
    public class DataTableParameters
    {
        /// <summary>
        /// 查询参数
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// 正/倒序
        /// </summary>
        public string SortOrder { get; set; }

        /// <summary>
        /// 跳过多少条数据，从0开始
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// 页面大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 自定义搜索内容
        /// </summary>
        public string CustomSearch { get; set; }

        /// <summary>
        /// 主表ID
        /// </summary>
        public int EntityId { get; set; }

        public List<SearchContent> ListCustomSearch
        {
            get
            {
                
                if (string.IsNullOrWhiteSpace(CustomSearch))
                {
                    return null;
                }
                List<SearchContent> results = new List<SearchContent>();
                SearchContent modelSearchContent = null;
                var paramses = CustomSearch.Split('&');
                string[] temp = null;
                foreach (var item in paramses)
                {
                    temp = item.Split('=');
                    if(temp.Length > 1)
                    {
                        modelSearchContent = new SearchContent();
                        modelSearchContent.SearchName = temp[0];
                        modelSearchContent.SearchValue = WebUtility.UrlDecode(temp[1]);
                        results.Add(modelSearchContent);
                    }
                }
                return results;
            }
        }
    }

    /// <summary>
    /// 自定义搜索内容
    /// </summary>
    public class SearchContent
    {
        public string SearchName { get; set; }

        public string SearchValue { get; set; }
    }
}