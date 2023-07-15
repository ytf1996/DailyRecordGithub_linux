using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessLatLngHistory : BaseBusiness<LatLngHistory>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        public override IQueryable<LatLngHistory> GetList(DataTableParameters param, out int totalCount, Expression<Func<LatLngHistory, bool>> predicate = null,
            string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            var list = base.GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight, asNoTracking);
            if (list != null)
            {
                return list.Include(m => m.CreatedBy);
            }
            return list;
        }

        /// <summary>
        /// 获取指定用户当月轨迹
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public List<LatLngHistory> GetThisMonthHistory(int userId)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            var list = GetListByCondition(m => m.CreatedDate.Year == year && m.CreatedDate.Month == month && m.CreatedById == userId)
                .OrderBy(m => m.CreatedDate);

            return list.ToList();
        }

    }
}