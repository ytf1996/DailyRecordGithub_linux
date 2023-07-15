using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessAccessToken : BaseBusiness<AccessToken>
    {
        public override bool CustomValidForSave(AccessToken model, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (DB.AccessToken.Any(m=>m.CurrentUserId == model.CurrentUserId && model.Id != m.Id))
            {
                errorMsg = "数据已存在";
                return false;
            }
            return true;
        }

        public override IQueryable<AccessToken> GetList(DataTableParameters param, out int totalCount, Expression<Func<AccessToken, bool>> predicate = null,
            string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            var list = base.GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight, asNoTracking);
            if (list != null)
            {
                return list.Include(m => m.CurrentUser);
            }
            return list;
        }
    }
}