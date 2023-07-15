using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessBabyInfoDaliyProgress : BaseBusiness<BabyInfoDaliyProgress>
    {
        public override IQueryable<BabyInfoDaliyProgress> GetList(DataTableParameters param, out int totalCount, Expression<Func<BabyInfoDaliyProgress, bool>> predicate = null, string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            var list = base.GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight, asNoTracking);
            if (list != null)
            {
                return list.Include(m => m.CreatedBy).Include(m => m.WorkflowButton);
            }
            return list;
        }
    }
}