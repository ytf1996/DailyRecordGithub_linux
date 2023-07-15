using System;
using System.Collections.Generic;
using System.Linq;
using MyNetCore.Models;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MyNetCore.Business
{
    public class BusinessWorkflowButton : BaseBusiness<WorkflowButton>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        public override void Add(WorkflowButton model, bool needCheckRight = true, bool saveToDBNow = true, MySqlContext db = null)
        {
            if (db == null)
            {
                db = DB;
            }
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            var strategy = db.Database.CreateExecutionStrategy();

            strategy.Execute(() => {
                using (var dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        BusinessWorkflowStep businessWorkflowStep = new BusinessWorkflowStep();
                        WorkflowStep modelWorkflowStep = new WorkflowStep();
                        modelWorkflowStep.CreatedById = currentUser.Id;
                        modelWorkflowStep.CreatedDate = DateTime.Now;
                        modelWorkflowStep.WorkflowId = model.WorkflowId;
                        modelWorkflowStep.Name = "新状态";
                        WorkflowStep modelLastWorkflowStep = businessWorkflowStep.GetById(model.LastWorkflowStepId, false);
                        modelWorkflowStep.LevelPath = modelLastWorkflowStep == null ? 1 : modelLastWorkflowStep.LevelPath + 1;
                        modelWorkflowStep.TerritoryId = null;
                        businessWorkflowStep.Add(modelWorkflowStep, false);
                        model.NextWorkflowStepId = modelWorkflowStep.Id;
                        base.Add(model, needCheckRight, true, db);
                        dbTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            });
        }

        /// <summary>
        /// 获取某状态下的按钮集合
        /// </summary>
        /// <param name="stepId">工作流状态id</param>
        /// <returns></returns>
        public List<WorkflowButton> GetWorkflowButtonsByStepId(int stepId)
        {
            return GetListByCondition(m => m.LastWorkflowStepId == stepId, false).ToList();
        }

        public override IQueryable<WorkflowButton> GetList(DataTableParameters param, out int totalCount, Expression<Func<WorkflowButton, bool>> predicate = null, string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            var list = base.GetList(param, out totalCount, predicate, orderByExpression, isDESC, false, asNoTracking);
            if (list != null)
            {
                return list.Include(m => m.NextWorkflowStep).Include(m => m.LastWorkflowStep).Include(m => m.Workflow);
            }
            return list;
        }
    }
}