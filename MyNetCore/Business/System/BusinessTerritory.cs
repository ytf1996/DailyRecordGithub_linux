using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNetCore.Models;
using Roim.Common;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace MyNetCore.Business
{
    public class BusinessTerritory : BaseBusiness<Territory>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 是否为系统自动修改操作
        /// </summary>
        private bool SysUpdateAuto = false;

        public override bool CustomValidForSave(Territory model, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (!SysUpdateAuto && (model.Id == 1))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.系统保留数据不能修改);
            }
            if (DB.Territory.AsNoTracking().Any(m => m.Id != model.Id && m.Name == model.Name.Trim() && m.ParentTerrId == model.ParentTerrId && m.Deleted == false))
            {
                errorMsg = "该父区域下已有相同的名称";
                return false;
            }
            return true;
        }

        public override bool CustomValidForDelete(Territory model, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (model.Id == 1)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.系统保留数据不能删除);
            }
            BusinessHelper.ListTerritory = null;
            return true;
        }

        public Territory GetByTerrId(int? terrId)
        {
            if (!terrId.HasValue)
            {
                return new Territory() { };
            }

            var model = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == terrId);

            if (model == null)
            {
                return new Territory() { };
            }

            return model;
        }

        public override void Add(Territory model, bool needCheckRight = true, bool saveToDBNow = true, MySqlContext db = null)
        {
            Territory parentModel = null;

            if (model == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.数据提交失败);
            }

            if (!model.ParentTerrId.HasValue)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未指定上级);
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写名称);
            }

            parentModel = GetByCondition(m => m.TerritoryId == model.ParentTerrId);

            if (parentModel == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            if (db == null)
            {
                db = DB;
            }

            int parentNextRangeStart = parentModel.NextRangeStart;

            model.TerritoryId = parentNextRangeStart;
            model.Depth = parentModel.Depth + 1;
            model.NextRangeStart = model.TerritoryId.Value + 1;
            model.RangeEnd = model.NextRangeStart + parentModel.RangeIncrement - 1;
            model.RangeIncrement = parentModel.RangeIncrement / 16;

            parentNextRangeStart = parentNextRangeStart + parentModel.RangeIncrement + 1;
            parentModel.NextRangeStart = parentNextRangeStart;

            var strategy = db.Database.CreateExecutionStrategy();

            strategy.Execute(() => {
                using (var dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        base.Add(model, true, true, db);
                        SysUpdateAuto = true;
                        base.Edit(parentModel, true, db);
                        dbTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                    BusinessHelper.ListTerritory = null;
                }
            });

            SysUpdateAuto = false;
        }

        public override void Edit(Territory model, bool needCheckRight = true, MySqlContext db = null)
        {
            if(db == null)
            {
                db = DB;
            }
            base.Edit(model, needCheckRight, db);
            BusinessHelper.ListTerritory = null;
        }

        public override IQueryable<Territory> GetAllList(string orderBy = "Id", bool? isDESC = true, int length = 0, bool needCheckRight = true, bool asNoTracking = true)
        {
            var currentUser = GetCurrentUserInfo();

            if (currentUser == null)
            {
                return null;
            }

            if (currentUser.IsAdmin)
            {
                return DB.Territory.Where(m => m.Deleted == false);
            }

            if (currentUser.TerritoryId == null)
            {
                return null;
            }

            var model = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId);

            if (model == null)
            {
                return null;
            }

            return DB.Territory.Where(m => m.Deleted == false).
                Where(m => model.TerritoryId <= m.TerritoryId && model.RangeEnd >= m.TerritoryId).OrderBy(m => m.Name);
        }
    }
}