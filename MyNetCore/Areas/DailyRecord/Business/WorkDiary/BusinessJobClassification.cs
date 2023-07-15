using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessJobClassification : BaseBusiness<JobClassificationInfo>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        public override IQueryable<JobClassificationInfo> GetList(DataTableParameters param, out int totalCount, Expression<Func<JobClassificationInfo, bool>> predicate = null, 
            string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            var list = base.GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight, asNoTracking);

            return list;
        }
    }
}