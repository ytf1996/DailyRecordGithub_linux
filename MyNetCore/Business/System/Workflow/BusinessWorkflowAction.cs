using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace MyNetCore.Business
{
    public class BusinessWorkflowAction : BaseBusiness<WorkflowAction>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        public override IQueryable<WorkflowAction> GetList(DataTableParameters param, out int totalCount, 
            Expression<Func<WorkflowAction, bool>> predicate = null, string orderByExpression = null, 
            bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            var list = base.GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight, asNoTracking);
            if (list != null)
            {
                return list.Include(m => m.Workflow);
            }
            return list;
        }
    }
}