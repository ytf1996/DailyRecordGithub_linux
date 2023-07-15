using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using System;
using System.Linq;

namespace MyNetCore.Areas.DailyRecord.Controllers
{
    public class JobClassificationController : DailyRecordBaseWithAuthController
    {
        private BusinessJobClassification _businessJobClassification = new BusinessJobClassification();

        /// <summary>
        /// 获取工作项目分类
        /// </summary>
        /// <returns></returns>
        public IActionResult List()
        {
            var list = _businessJobClassification.GetList(null, out int totalCount,null, "Id");

            var result = list.ToList();

            return Success(data: result);
        }
    }
}
