using MyNetCore.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using MyNetCore.Business.Signal;
using Microsoft.AspNetCore.SignalR;
using MyNetCore.Dtos.ChatRoom;

namespace MyNetCore.Areas.Management.Controllers
{
    public class ChatRoomController : BaseManagementController<ChatRoom, BusinessChatRoom>
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public ChatRoomController(IHubContext<SignalRHub> hubClients)
        {
            _hubContext = hubClients;
        }

        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            Expression<Func<ChatRoom, bool>> predicate = null;
            var currentUser = GetCurrentUserInfo();

            if (currentUser == null)
            {
                return Failure("身份过期，请重新登陆");
            }

            if (param.ListCustomSearch != null)
            {
                string nameValue = param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "Name") == null ? null :
                    param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "Name").SearchValue;

                predicate = m => (nameValue == "" || m.Name.Contains(nameValue) || m.Remark.Contains(nameValue));
            }

            IQueryable<ChatRoom> list = business.GetList(param, out total,
                predicate, null, null);

            object result = null;
            List<ChatRoom> finalList = null;

            if (list == null)
            {
                finalList = new List<ChatRoom>();
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
                         CreatedBy = m.CreatedBy == null ? "" : m.CreatedBy.Name,
                         Remark = string.IsNullOrWhiteSpace(m.Remark) ? "" : m.Remark.Replace("\r\n", "<br/>"),
                         CreatedDate = m.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                         UpdatedDate = m.UpdatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                     };

            return JsonListResult(param, total, result);

        }

        public override IActionResult Edit()
        {
            int id = 0;

            string idStr = Request.Query["id"];

            if (string.IsNullOrWhiteSpace(idStr))
            {
                business.ThrowErrorInfo("参数错误");
            }

            if (!int.TryParse(idStr, out id))
            {
                business.ThrowErrorInfo("参数错误");
            }

            ChatRoom model = business.GetById(id, false);

            if (model == null)
            {
                model = new ChatRoom();
            }
            return View(model);
        }

        /// <summary>
        /// 聊天室
        /// </summary>
        /// <returns></returns>
        public IActionResult Detail()
        {
            int id = 0;

            string idStr = Request.Query["id"];

            if (string.IsNullOrWhiteSpace(idStr))
            {
                business.ThrowErrorInfo("参数错误");
            }

            if (!int.TryParse(idStr, out id))
            {
                business.ThrowErrorInfo("参数错误");
            }

            ChatRoom model = business.GetById(id, false);

            ViewBag.RecordId = idStr;
            ViewBag.RecordName = model?.Name;

            return View();
        }

    }
}