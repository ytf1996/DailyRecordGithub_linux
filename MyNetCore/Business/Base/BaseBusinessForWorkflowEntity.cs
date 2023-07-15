using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BaseBusinessForWorkflowEntity<T> : BaseBusiness<T>
        where
        T : IWorkflowModel
    {
        public override IQueryable<T> GetList(DataTableParameters param, out int totalCount,
            Expression<Func<T, bool>> predicate = null, string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true,
            bool asNoTracking = true)
        {
            var list = base.GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight, asNoTracking);
            if (list != null)
            {
                return list.Include(m => m.WorkflowInstance);
            }
            return list;
        }

        public override void Add(T model, bool needCheckRight = true, bool saveToDBNow = true, MySqlContext db = null)
        {
            if (db == null)
            {
                db = DB;
            }

            string entityFullName = typeof(T).FullName;
            Workflow modelWorkflow = DB.Workflow.FirstOrDefault(m => m.Deleted == false && m.WorkflowEntityName == entityFullName);

            if (modelWorkflow == null || modelWorkflow.Id == 0)
            {
                base.Add(model, needCheckRight, saveToDBNow);
                return;
            }

            var strategy = db.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using (var dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        base.Add(model, needCheckRight, saveToDBNow);

                        WorkflowInstance modelWorkflowInstance = new WorkflowInstance();
                        modelWorkflowInstance.CreatedById = model.CreatedById;
                        modelWorkflowInstance.CreatedDate = DateTime.Now;
                        modelWorkflowInstance.Name = model.Name;
                        modelWorkflowInstance.RecordId = model.Id;
                        modelWorkflowInstance.TerritoryId = model.TerritoryId;
                        modelWorkflowInstance.UpdatedById = model.UpdatedById;
                        modelWorkflowInstance.UpdatedDate = DateTime.Now;
                        modelWorkflowInstance.WorkflowId = modelWorkflow.Id;
                        WorkflowStep modelWorkflowStep = db.WorkflowStep.OrderBy(m => m.Id).FirstOrDefault(m => m.WorkflowId == modelWorkflow.Id);
                        if (modelWorkflowStep == null)
                        {
                            ThrowErrorInfo("工作流设置不正确，请联系管理员");
                        }
                        modelWorkflowInstance.WorkflowStepId = modelWorkflowStep.Id;
                        db.WorkflowInstance.Add(modelWorkflowInstance);
                        db.SaveChanges();
                        modelWorkflowInstance.DealUsersIds = GetShenPiRenIdsByInstanceId(modelWorkflowInstance);
                        model.WorkflowInstanceId = modelWorkflowInstance.Id;
                        db.SaveChanges();
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

        public override void Edit(T model, bool needCheckRight = true, MySqlContext db = null)
        {
            if (model.WorkflowInstanceId == null)
            {
                base.Edit(model, needCheckRight);
                return;
            }

            if (db == null)
            {
                db = DB;
            }

            WorkflowInstance modelWorkflowInstance = db.WorkflowInstance.FirstOrDefault(m => m.Id == model.WorkflowInstanceId);
            if (modelWorkflowInstance == null)
            {
                base.Edit(model, needCheckRight, db);
                return;
            }

            var modelDB = GetById(model.Id, false);

            if (modelDB == null)
            {
                ThrowErrorInfo("未在数据库中找到相应数据，修改失败");
            }

            bool terrIdHasChanged = (model.TerritoryId != modelDB.TerritoryId);

            bool nameHasChanged = (model.Name != modelDB.Name);

            CheckEditRight(modelDB, needCheckRight);
            CheckEditRight(model, needCheckRight);

            if (NeedCheckNameRepeat)
            {
                var checkRepeatModel = GetByCondition(m => m.Name == model.Name && m.Id != model.Id, needCheckRight);
                if (checkRepeatModel != null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.名称已存在);
                }
            }

            string errorMsg = string.Empty;

            if (!CustomValidForSave(model, out errorMsg))
            {
                ThrowErrorInfo(errorMsg);
            }

            var strategy = db.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using (var dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        model.UpdatedById = GetCurrentUserId();
                        model.UpdatedDate = DateTime.Now;
                        db.Entry(model).State = EntityState.Modified;
                        if (terrIdHasChanged || nameHasChanged)
                        {
                            modelWorkflowInstance.Name = model.Name;
                            modelWorkflowInstance.TerritoryId = model.TerritoryId;
                            modelWorkflowInstance.UpdatedById = model.UpdatedById;
                            modelWorkflowInstance.UpdatedDate = DateTime.Now;
                        }

                        if (terrIdHasChanged)
                        {
                            modelWorkflowInstance.DealUsersIds = GetShenPiRenIdsByInstanceId(modelWorkflowInstance);
                        }

                        db.SaveChanges();
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
    }
}
