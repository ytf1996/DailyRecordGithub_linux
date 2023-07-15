using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using System.Linq;

namespace MyNetCore.Controllers
{
    public class CommonController : Controller
    {

        public Users GetCurrentUserInfo(string accessToken = null)
        {
            CommonBusiness bbnt = new CommonBusiness();
            return bbnt.GetCurrentUserInfo(accessToken);
        }

        public IActionResult JsonResult(object result)
        {
            return Json(new { result });
        }

        public IActionResult Success(string msg = "操作成功", object data = null, string urlRedirect = "")
        {
            return Json(new { data = data, msg = msg, result = "success", urlRedirect = urlRedirect });
        }

        public IActionResult Failure(string msg = "", object data = null)
        {
            return Json(new { data = data, msg = msg, result = "failure" });
        }

        public IActionResult JsonListResult(int draw, int total, int filtered, object result)
        {
            return Json(new
            {
                draw = draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data = result
            });
        }

        public IActionResult JsonListResult(DataTableParameters param, int total, object result)
        {
            if (total == 0)
            {
                return NullResult();
            }

            int page = 1;

            var JsonResult = new { current = page, rowCount = param.PageSize, total = total, rows = result };

            return Json(JsonResult);
        }

        #region 返回空Json
        /// <summary>
        /// 返回空Json
        /// </summary>
        /// <returns></returns>
        protected IActionResult NullResult()
        {
            return Json(new { total = 0, rows = "" });
        }
        #endregion

        /// <summary>
        /// 根据TerritoryId获得区域记录
        /// </summary>
        /// <param name="terrId"></param>
        /// <returns></returns>
        public Territory GetTerrByTerrId(int? terrId)
        {
            Territory model = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == terrId && m.Deleted == false);
            if (model == null)
            {
                model = new Territory();
            }
            return model;
        }

        /// <summary>
        /// 根据权限配置文件ID获得记录
        /// </summary>
        /// <param name="profiledId"></param>
        /// <returns></returns>
        public TerritoryProfiles GetTerritoryProfilesByProfileId(int? profiledId)
        {
            TerritoryProfiles model = BusinessHelper.ListTerritoryProfiles.FirstOrDefault(m => m.Id == profiledId && m.Deleted == false);
            if (model == null)
            {
                model = new TerritoryProfiles();
            }
            return model;
        }

        /// <summary>
        /// 获得完整URL
        /// </summary>
        /// <returns></returns>
        public string GetHostUrl()
        {
            var url = AppContextMy.Current.Request.GetAbsoluteUri();

            return url;
        }

        /// <summary>
        /// 获得域名
        /// </summary>
        /// <returns></returns>
        public string GetDomainUrl()
        {
            var url = AppContextMy.Current.Request.GetDomainUri();

            return url;
        }

    }
}