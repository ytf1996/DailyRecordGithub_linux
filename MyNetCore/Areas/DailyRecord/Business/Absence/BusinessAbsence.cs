using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessAbsence : BaseBusiness<AbsenceInfo>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        public override IQueryable<AbsenceInfo> GetList(DataTableParameters param, out int totalCount, Expression<Func<AbsenceInfo, bool>> predicate = null,
            string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            var list = base.GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight, asNoTracking);

            return list;
        }

        public void CheckRepeat(DateTime begDate, DateTime endDate, Users currentUser, bool isUpdate = false, int updatedId = 0)
        {
            var list = GetList(null, out int beftotalCount, x => !(x.BegAbsenceTime >= endDate || x.EndAbsenceTime <= begDate) && x.CreatedById == currentUser.Id);   //这里不要带等于

            if (isUpdate)
            {
                list = list.Where(x => x.Id != updatedId);
            }
            var result = list.ToList();

            if (result.Count > 0)
            {
                throw new Exception("请假时间不能和历史填写记录交叉");
            }
        }
    }
}