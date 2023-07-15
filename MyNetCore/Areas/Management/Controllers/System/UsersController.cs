using MyNetCore.Controllers;
using MyNetCore.Models;
using Roim.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Senparc.Weixin.Entities;
using Senparc.Weixin.WxOpen.AdvancedAPIs;
using Senparc.Weixin.Entities.TemplateMessage;
using Senparc.Weixin;
using MyNetCore.Areas.WeChatMini.Business;

namespace MyNetCore.Areas.Management.Controllers
{
    [Area(areaName: "Management")]
    public class UsersController : CommonController
    {
        BusinessUsers business = null;

        public UsersController()
        {
            business = new BusinessUsers();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null || currentUser.Id == 0)
            {
                string url = Request.GetAbsoluteUri();
                Response.Redirect(string.Format("/Account/Login?historyUrl={0}", url));
            }
        }

        public IActionResult Add()
        {
            string errorMsg = string.Empty;
            if (!business.CheckHasAddRight(out errorMsg))
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", errorMsg));
            }
            return View();
        }

        [HttpPost]
        public IActionResult Add(Users model, List<string> channelIds)
        {
            if (channelIds != null)
            {
                model.ChannelIds = string.Join(",", channelIds);
            }
            new BusinessUsers().Add(model);
            return Success("添加成功", null, "List");
        }


        public IActionResult Edit(int id)
        {
            var model = business.GetById(id);
            if (!business.CheckHasRecordRight(model))
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}",
                    MessageText.ErrorInfo.您无此数据的操作权限.ToString()));
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(Users model, List<string> channelIds)
        {
            if (model.LineManageId.HasValue && model.LineManageId.Value == 0)
            {
                model.LineManageId = null;
            }

            if (channelIds != null)
            {
                model.ChannelIds = string.Join(",", channelIds);
            }

            business.Edit(model, true, true);
            return Success("修改成功", null, "List");
        }

        [HttpPost]
        public IActionResult Delete(Users model)
        {
            business.Delete(model.Id);
            return Success("删除成功", null, "List");
        }

        public IActionResult List()
        {
            string errorMsg = string.Empty;
            if (!business.CheckHasSelectRight(out errorMsg))
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", errorMsg));
            }
            return View();
        }

        public IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<Users> list = business.GetList(param, out total);

            object result = null;
            List<Users> finalList = null;

            if (list == null)
            {
                finalList = new List<Users>();
            }
            else
            {
                finalList = list.ToList();
            }

            result = from m in finalList
                     select new
                     {
                         m.Id,
                         m.Name,
                         m.Code,
                         m.HeadImage,
                         UserType = m.UserType.GetCustomDescription(),
                         CounselorPropertyVal = m.CounselorPropertyVal?.GetCustomDescription(),
                         ServiceUnit = m.ServiceUnit,
                         ContractedSupplier = m.ContractedSupplier,
                         Duty=m.Duty,
                         Group = m.Group,
                         UserOrder = m.UserOrder,
                         ChannelId = m.ChannelsName,
                         TerritoryId = GetTerrByTerrId(m.TerritoryId).Name,
                         Gender = m.Gender.GetCustomDescription(),
                         TerritoryProfilesId = GetTerritoryProfilesByProfileId(m.TerritoryProfilesId).Name,
                         m.IfLeave
                     };

            return JsonListResult(param, total, result);

        }

        public IActionResult GetListForSelect()
        {
            string search = Request.Query["search"];
            string page = Request.Query["page"];
            int pageInt = 0;
            if (!int.TryParse(page, out pageInt))
            {
                pageInt = 1;
            }

            int count = 0;

            Expression<Func<Users, bool>> predicate = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                predicate = m => m.Name.Contains(search);
            }

            var list = business.GetList(pageInt, 10, out count, predicate, "Name");

            var result = from m
                         in list
                         select new
                         {
                             id = m.Id,
                             text = m.Name,
                         };
            return Json(new { items = result, total_count = count });
        }

        /// <summary>
        /// 获取指定用户当月轨迹
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetThisMonthHistory(int id)
        {
            BusinessLatLngHistory businessLatLngHistory = new BusinessLatLngHistory();

            var list = businessLatLngHistory.GetThisMonthHistory(id).Select(m => new { m.Lng, m.Lat }).ToList();

            return Success(data: list);
        }


        public IActionResult SendMsg(int id)
        {
            BusinessUsers businessUsers = new BusinessUsers();

            Users user = businessUsers.GetById(id, false);

            WeChatMiNiHelper wh = new WeChatMiNiHelper();

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("phrase3", "大雾");
            keyValuePairs.Add("thing1", "目前我市已出现能见度");

            string result = wh.SendMsg("lv5O4VttH4mEkAhJVwnsQVP6pZSaqBlKa7uJ8gMQnjk", user.WeChatOpenidForMiniProgram, keyValuePairs);

            if (!string.IsNullOrWhiteSpace(result))
            {
                return Failure(result);
            }

            return Success();
        }
    }
}