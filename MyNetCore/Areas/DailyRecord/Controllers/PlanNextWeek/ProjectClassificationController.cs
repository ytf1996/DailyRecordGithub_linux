using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using System;
using System.Linq;

namespace MyNetCore.Areas.DailyRecord.Controllers
{
    public class ProjectClassificationController : DailyRecordBaseWithAuthController
    {
        private BusinessProjectClassification _businessProjectClassification = new BusinessProjectClassification();

        /// <summary>
        /// 获取工作项目分类
        /// </summary>
        /// <returns></returns>
        public IActionResult List()
        {
            var list = _businessProjectClassification.GetList(null, out int totalCount,null, "Order");

            var result = list.ToList();

            return Success(data: result);
        }
    }
}
