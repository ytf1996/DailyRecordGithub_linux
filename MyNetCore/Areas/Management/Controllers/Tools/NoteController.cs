using MyNetCore.Controllers;
using MyNetCore.Models;
using Roim.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;

namespace MyNetCore.Areas.Management.Controllers
{
    public class NoteController : BaseManagementController<Note, BusinessNote>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            Expression<Func<Note, bool>> predicate = null;
            var currentUser = GetCurrentUserInfo();

            if (currentUser == null)
            {
                return Failure("身份过期，请重新登陆");
            }

            if (param.ListCustomSearch != null)
            {
                string nameValue = param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "Name") == null ? null :
                    param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "Name").SearchValue;
                string isPublicStr = param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "IsPublic") == null ? null :
                    param.ListCustomSearch.FirstOrDefault(m => m.SearchName == "IsPublic").SearchValue;

                bool isPublic = false;

                if (isPublicStr == "true")
                {
                    isPublic = true;
                }

                if (!currentUser.IsAdmin)
                {
                    predicate = predicate.And(m => m.CreatedById == currentUser.Id || m.IsPublic == true);
                }

                if (!string.IsNullOrWhiteSpace(nameValue))
                {
                    predicate = predicate.And(m => m.Name.Contains(nameValue));
                }

                if (isPublic)
                {
                    predicate = predicate.And(m => m.IsPublic == true);
                }
            }

            IQueryable<Note> list = business.GetList(param, out total,
                predicate, null, null);

            object result = null;
            List<Note> finalList = null;

            if (list == null)
            {
                finalList = new List<Note>();
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

            Note model = business.GetById(id, false);

            if (model == null)
            {
                model = new Note();
            }
            return View(model);
        }

    }
}